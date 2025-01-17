using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Rendering;
using System.Diagnostics;

class JobData
{
	public Mesh landMesh, waterMesh;
	public Vector2Int noiseOffset;
	public int lod;

	public bool scheduled;
	public bool logTime;

	public Stopwatch watch;

	public JobHandle handle;

	public NativeArray<int> customVoxelValues;

	public NativeList<float3> vertices;
	public NativeList<int> triangles;
	public NativeList<float2> uvs;

	public NativeList<float3> waterVertices;
	public NativeList<int> waterTriangles;
	public NativeList<float2> waterUvs;

	public void CreateAndSchedule()
	{
		if (logTime)
		{
			watch = new();
			watch.Start();
		}
		vertices = new NativeList<float3>(Allocator.Persistent);
		triangles = new NativeList<int>(Allocator.Persistent);
		uvs = new NativeList<float2>(Allocator.Persistent);

		waterVertices = new NativeList<float3>(Allocator.Persistent);
		waterTriangles = new NativeList<int>(Allocator.Persistent);
		waterUvs = new NativeList<float2>(Allocator.Persistent);

		int2 intOffset = new int2(noiseOffset.x, noiseOffset.y);
		Vector2Int chunkDimensions = ChunkGenerator.ChunkDimensions;

		if (customVoxelValues == default)
		{
			if (!ChunkGenerator.ChunkDataMap.ContainsKey(intOffset))
				ChunkGenerator.ChunkDataMap.Add(intOffset, ChunkGenerator.noise.GenerateVoxelValues(noiseOffset));
		}
		else
		{
			if (!ChunkGenerator.ChunkDataMap.ContainsKey(intOffset))
				ChunkGenerator.ChunkDataMap.Add(intOffset, customVoxelValues);
		}

		AddNeighboringChunkData(intOffset, noiseOffset);

		MeshBuildJob job = new MeshBuildJob
		{
			vertices = vertices,
			triangles = triangles,
			uvs = uvs,
		
			waterVertices = waterVertices,
			waterTriangles = waterTriangles,
			waterUvs = waterUvs,
		
			thisChunkData = ChunkGenerator.ChunkDataMap[intOffset],
			rightChunkData = ChunkGenerator.ChunkDataMap[intOffset + new int2(chunkDimensions.x, 0)],
			leftChunkData = ChunkGenerator.ChunkDataMap[intOffset + new int2(-chunkDimensions.x, 0)],
			frontChunkData = ChunkGenerator.ChunkDataMap[intOffset + new int2(0, chunkDimensions.x)],
			backChunkData = ChunkGenerator.ChunkDataMap[intOffset + new int2(0, -chunkDimensions.x)],
		
			faceDirections = ChunkGenerator.faceDirections,
			uvCordinates = ChunkGenerator.uvCoordinates,
			faceVertices = ChunkGenerator.faceVertices,
			chunkDimensions = new int2(chunkDimensions.x, chunkDimensions.y),
		
			lod = lod
		};
		
		handle = job.Schedule();
		scheduled = true;
	}

	public void Complete()
	{
		handle.Complete();

		NativeArray<Vector3> vertexArray = vertices.AsArray().Reinterpret<Vector3>();
		NativeArray<int> triangleArray = triangles.AsArray();
		NativeArray<Vector2> uvArray = uvs.AsArray().Reinterpret<Vector2>();

		// Assign data to mesh
		landMesh.SetVertexBufferParams(vertexArray.Length, new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3));
		landMesh.SetVertexBufferData(vertexArray, 0, 0, vertexArray.Length, 0, MeshUpdateFlags.Default);
		landMesh.SetIndexBufferParams(triangleArray.Length, IndexFormat.UInt32);
		landMesh.SetIndexBufferData(triangleArray, 0, 0, triangleArray.Length, MeshUpdateFlags.Default);
		landMesh.SetSubMesh(0, new SubMeshDescriptor(0, triangleArray.Length), MeshUpdateFlags.Default);
		landMesh.SetUVs(0, uvArray);

		landMesh.RecalculateBounds();
		landMesh.RecalculateNormals();

		//Water mesh
		if (waterVertices.Length > 0)
		{
			NativeArray<Vector3> wVertexArray = waterVertices.AsArray().Reinterpret<Vector3>();
			NativeArray<int> wTriangleArray = waterTriangles.AsArray();
			NativeArray<Vector2> wUvArray = waterUvs.AsArray().Reinterpret<Vector2>();

			// Assign data to mesh
			waterMesh.SetVertexBufferParams(wVertexArray.Length, new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3));
			waterMesh.SetVertexBufferData(wVertexArray, 0, 0, wVertexArray.Length, 0, MeshUpdateFlags.Default);
			waterMesh.SetIndexBufferParams(wTriangleArray.Length, IndexFormat.UInt32);
			waterMesh.SetIndexBufferData(wTriangleArray, 0, 0, wTriangleArray.Length, MeshUpdateFlags.Default);
			waterMesh.SetSubMesh(0, new SubMeshDescriptor(0, wTriangleArray.Length), MeshUpdateFlags.Default);
			waterMesh.SetUVs(0, wUvArray);

			waterMesh.RecalculateBounds();
			waterMesh.RecalculateNormals();

			wVertexArray.Dispose();
			wTriangleArray.Dispose();
			wUvArray.Dispose();
		}

		waterTriangles.Dispose();
		waterVertices.Dispose();
		waterUvs.Dispose();

		vertices.Dispose();
		triangles.Dispose();
		uvs.Dispose();

		vertexArray.Dispose();
		triangleArray.Dispose();
		uvArray.Dispose();

		if (logTime)
		{
			watch.Stop();
			UnityEngine.Debug.Log("Execution Time: " + watch.ElapsedMilliseconds + " ms");
		}
	}

	private void AddNeighboringChunkData(int2 intOffset, Vector2Int offset)
	{
		Vector2Int chunkDimensions = ChunkGenerator.ChunkDimensions;
		Noise noise = ChunkGenerator.noise;

		if (!ChunkGenerator.ChunkDataMap.ContainsKey(intOffset + new int2(chunkDimensions.x, 0)))
			ChunkGenerator.ChunkDataMap.Add(intOffset + new int2(chunkDimensions.x, 0), noise.GenerateVoxelValues(offset + Vector2Int.right * chunkDimensions.x));

		if (!ChunkGenerator.ChunkDataMap.ContainsKey(intOffset + new int2(-chunkDimensions.x, 0)))
			ChunkGenerator.ChunkDataMap.Add(intOffset + new int2(-chunkDimensions.x, 0), noise.GenerateVoxelValues(offset + Vector2Int.left * chunkDimensions.x));

		if (!ChunkGenerator.ChunkDataMap.ContainsKey(intOffset + new int2(0, chunkDimensions.x)))
			ChunkGenerator.ChunkDataMap.Add(intOffset + new int2(0, chunkDimensions.x), noise.GenerateVoxelValues(offset + Vector2Int.up * chunkDimensions.x));

		if (!ChunkGenerator.ChunkDataMap.ContainsKey(intOffset + new int2(0, -chunkDimensions.x)))
			ChunkGenerator.ChunkDataMap.Add(intOffset + new int2(0, -chunkDimensions.x), noise.GenerateVoxelValues(offset + Vector2Int.down * chunkDimensions.x));
	}
}

