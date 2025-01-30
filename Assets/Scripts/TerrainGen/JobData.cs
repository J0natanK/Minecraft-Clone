using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Rendering;
using System.Diagnostics;

class JobData
{
	public Mesh landMesh, waterMesh;
	public Vector2Int offset;

	public bool scheduled;
	public bool logTime;

	public Stopwatch watch;

	public JobHandle handle;

	public NativeArray<int> customVoxelGrid;

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

		Vector2Int chunkDimensions = ChunkGenerator.ChunkDimensions;

		if (customVoxelGrid == default)
		{
			if (!ChunkGenerator.VoxelGridMap.ContainsKey(offset))
				ChunkGenerator.VoxelGridMap.Add(offset, ChunkGenerator.Noise.GenerateVoxelGrid(offset));
		}
		else
		{
			if (!ChunkGenerator.VoxelGridMap.ContainsKey(offset))
			{
				ChunkGenerator.VoxelGridMap.Add(offset, customVoxelGrid);
			}
			else
			{
				ChunkGenerator.VoxelGridMap[offset] = customVoxelGrid;
			}
		}

		AddNeighboringChunkData(offset);

		MeshBuildJob job = new MeshBuildJob
		{
			vertices = vertices,
			triangles = triangles,
			uvs = uvs,

			waterVertices = waterVertices,
			waterTriangles = waterTriangles,
			waterUvs = waterUvs,

			thisVoxelGrid = ChunkGenerator.VoxelGridMap[offset],
			rightVoxelGrid = ChunkGenerator.VoxelGridMap[offset + new Vector2Int(chunkDimensions.x, 0)],
			leftVoxelGrid = ChunkGenerator.VoxelGridMap[offset + new Vector2Int(-chunkDimensions.x, 0)],
			frontVoxelGrid = ChunkGenerator.VoxelGridMap[offset + new Vector2Int(0, chunkDimensions.x)],
			backVoxelGrid = ChunkGenerator.VoxelGridMap[offset + new Vector2Int(0, -chunkDimensions.x)],

			faceDirections = ChunkGenerator.FaceDirections,
			uvCordinates = ChunkGenerator.UvCoordinates,
			faceVertices = ChunkGenerator.FaceVertices,
			chunkDimensions = new int2(chunkDimensions.x, chunkDimensions.y),
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

	private void AddNeighboringChunkData(Vector2Int offset)
	{
		Vector2Int chunkDimensions = ChunkGenerator.ChunkDimensions;
		Noise noise = ChunkGenerator.Noise;

		if (!ChunkGenerator.VoxelGridMap.ContainsKey(offset + new Vector2Int(chunkDimensions.x, 0)))
			ChunkGenerator.VoxelGridMap.Add(offset + new Vector2Int(chunkDimensions.x, 0), noise.GenerateVoxelGrid(offset + Vector2Int.right * chunkDimensions.x));

		if (!ChunkGenerator.VoxelGridMap.ContainsKey(offset + new Vector2Int(-chunkDimensions.x, 0)))
			ChunkGenerator.VoxelGridMap.Add(offset + new Vector2Int(-chunkDimensions.x, 0), noise.GenerateVoxelGrid(offset + Vector2Int.left * chunkDimensions.x));

		if (!ChunkGenerator.VoxelGridMap.ContainsKey(offset + new Vector2Int(0, chunkDimensions.x)))
			ChunkGenerator.VoxelGridMap.Add(offset + new Vector2Int(0, chunkDimensions.x), noise.GenerateVoxelGrid(offset + Vector2Int.up * chunkDimensions.x));

		if (!ChunkGenerator.VoxelGridMap.ContainsKey(offset + new Vector2Int(0, -chunkDimensions.x)))
			ChunkGenerator.VoxelGridMap.Add(offset + new Vector2Int(0, -chunkDimensions.x), noise.GenerateVoxelGrid(offset + Vector2Int.down * chunkDimensions.x));
	}
}

