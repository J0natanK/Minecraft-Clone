using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Rendering;

class MeshBuilder
{
	public Mesh terrainMesh { get; private set; }
	public Mesh waterMesh { get; private set; }
	public Vector2Int position { get; private set; }

	public NativeArray<byte> customVoxelGrid { get; private set; }

	public bool scheduled { get; private set; }

	public JobHandle handle { get; private set; }

	NativeList<float3> vertices;
	NativeList<int> triangles;
	NativeList<float2> uvs;

	NativeList<float3> waterVertices;
	NativeList<int> waterTriangles;
	NativeList<float2> waterUvs;

	NativeArray<byte> thisVoxelGrid;
	NativeArray<byte> rightVoxelGrid;
	NativeArray<byte> leftVoxelGrid;
	NativeArray<byte> frontVoxelGrid;
	NativeArray<byte> backVoxelGrid;

	public MeshBuilder(Mesh terrainMesh, Mesh waterMesh, Vector2Int position, NativeArray<byte> customVoxelGrid = default)
	{
		this.terrainMesh = terrainMesh;
		this.waterMesh = waterMesh;
		this.position = position;
		this.customVoxelGrid = customVoxelGrid;
	}

	public void AssignVoxelGrids(
		NativeArray<byte> thisVoxelGrid,
		NativeArray<byte> rightVoxelGrid,
		NativeArray<byte> leftVoxelGrid,
		NativeArray<byte> frontVoxelGrid,
		NativeArray<byte> backVoxelGrid)
	{
		this.thisVoxelGrid = thisVoxelGrid;
		this.rightVoxelGrid = rightVoxelGrid;
		this.leftVoxelGrid = leftVoxelGrid;
		this.frontVoxelGrid = frontVoxelGrid;
		this.backVoxelGrid = backVoxelGrid;
	}

	public void ScheduleBuild()
	{
		vertices = new NativeList<float3>(Allocator.Persistent);
		triangles = new NativeList<int>(Allocator.Persistent);
		uvs = new NativeList<float2>(Allocator.Persistent);

		waterVertices = new NativeList<float3>(Allocator.Persistent);
		waterTriangles = new NativeList<int>(Allocator.Persistent);
		waterUvs = new NativeList<float2>(Allocator.Persistent);

		Vector2Int chunkDimensions = TerrainConstants.ChunkSize;

		MeshBuildJob job = new MeshBuildJob
		{
			vertices = vertices,
			triangles = triangles,
			uvs = uvs,

			waterVertices = waterVertices,
			waterTriangles = waterTriangles,
			waterUvs = waterUvs,

			thisVoxelGrid = thisVoxelGrid,
			rightVoxelGrid = rightVoxelGrid,
			leftVoxelGrid = leftVoxelGrid,
			frontVoxelGrid = frontVoxelGrid,
			backVoxelGrid = backVoxelGrid,

			faceDirections = TerrainConstants.FaceDirections,
			uvCordinates = TerrainConstants.UvCoordinates,
			faceVertices = TerrainConstants.FaceVertices,
			chunkDimensions = new int2(chunkDimensions.x, chunkDimensions.y),
		};

		handle = job.Schedule();
		scheduled = true;
	}

	public void CompleteBuild()
	{
		handle.Complete();

		NativeArray<Vector3> vertexArray = vertices.AsArray().Reinterpret<Vector3>();
		NativeArray<int> triangleArray = triangles.AsArray();
		NativeArray<Vector2> uvArray = uvs.AsArray().Reinterpret<Vector2>();

		AssignMeshData(terrainMesh, vertexArray, triangleArray, uvArray);

		if (waterVertices.Length > 0)
		{
			NativeArray<Vector3> wVertexArray = waterVertices.AsArray().Reinterpret<Vector3>();
			NativeArray<int> wTriangleArray = waterTriangles.AsArray();
			NativeArray<Vector2> wUvArray = waterUvs.AsArray().Reinterpret<Vector2>();

			AssignMeshData(waterMesh, wVertexArray, wTriangleArray, wUvArray);

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
	}

	void AssignMeshData(Mesh mesh, NativeArray<Vector3> vertices, NativeArray<int> indices, NativeArray<Vector2> uvs)
	{
		if (mesh != null)
		{
			mesh.SetVertexBufferParams(vertices.Length, new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3));
			mesh.SetVertexBufferData(vertices, 0, 0, vertices.Length, 0, MeshUpdateFlags.Default);
			mesh.SetIndexBufferParams(indices.Length, IndexFormat.UInt32);
			mesh.SetIndexBufferData(indices, 0, 0, indices.Length, MeshUpdateFlags.Default);
			mesh.SetSubMesh(0, new SubMeshDescriptor(0, indices.Length), MeshUpdateFlags.Default);
			mesh.SetUVs(0, uvs);

			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
		}
	}
}