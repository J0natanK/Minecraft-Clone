using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Mathematics;

public class ChunkManager : MonoBehaviour
{
	public Material terrainMaterial;
	public Material waterMaterial;
	public TerrainProperties properties;

	[Header("Performance")]
	public int maxJobsPerFrame;

	public static readonly Vector2Int ChunkDimensions = new Vector2Int(32, 192);
	public static VoxelGen VoxelGen;
	public static bool LoadedTerrain;

	public static NativeArray<int3> FaceVertices;
	public static NativeArray<int3> FaceDirections;
	public static NativeArray<float2> UvCoordinates;

	public static Dictionary<Vector2Int, TerrainChunk> ChunkMap;
	public static Dictionary<Vector2Int, NativeArray<byte>> VoxelGridMap;

	List<ChunkBuilder> meshBuildList;
	List<ChunkBuilder> meshBuildQueue;

	public TerrainChunk CreateChunk(Vector2Int position, NativeArray<byte> customVoxelGrid = default, bool instantMesh = false)
	{
		Mesh terrainMesh = new();
		Mesh waterMesh = new();

		RequestChunkMesh(terrainMesh, waterMesh, position, customVoxelGrid, instantMesh);

		TerrainChunk chunk = new TerrainChunk(position, terrainMesh, waterMesh, this);

		return chunk;
	}

	public void RequestChunkMesh(Mesh terrainMesh, Mesh waterMesh, Vector2Int position, NativeArray<byte> customVoxelGrid = default, bool instantCompletion = false)
	{
		if (terrainMesh == null)
		{
			terrainMesh = new();
		}
		if (waterMesh == null)
		{
			waterMesh = new();
		}
		ChunkBuilder meshBuilder = new(terrainMesh, waterMesh, position, customVoxelGrid);

		if (instantCompletion)
		{
			meshBuildList.Add(meshBuilder);
			ScheduleMeshBuild(meshBuildList.Count - 1);
			CompleteMeshBuild(meshBuildList.Count - 1);
		}
		else
		{
			meshBuildQueue.Add(meshBuilder);
		}
	}

	void AddVoxelGrids(ChunkBuilder builder)
	{
		Vector2Int position = builder.position;

		if (builder.customVoxelGrid == default)
		{
			if (!VoxelGridMap.ContainsKey(position))
				VoxelGridMap.Add(position, VoxelGen.GenerateVoxelGrid(position));
		}
		else
		{
			if (VoxelGridMap.ContainsKey(position))
				VoxelGridMap[position] = builder.customVoxelGrid;
		}

		Vector2Int right = position + new Vector2Int(ChunkDimensions.x, 0);
		if (!VoxelGridMap.ContainsKey(right))
			VoxelGridMap.Add(right, VoxelGen.GenerateVoxelGrid(right));

		Vector2Int left = position - new Vector2Int(ChunkDimensions.x, 0);
		if (!VoxelGridMap.ContainsKey(left))
			VoxelGridMap.Add(left, VoxelGen.GenerateVoxelGrid(left));

		Vector2Int front = position + new Vector2Int(0, ChunkDimensions.x);
		if (!VoxelGridMap.ContainsKey(front))
			VoxelGridMap.Add(front, VoxelGen.GenerateVoxelGrid(front));

		Vector2Int back = position - new Vector2Int(0, ChunkDimensions.x);
		if (!VoxelGridMap.ContainsKey(back))
			VoxelGridMap.Add(back, VoxelGen.GenerateVoxelGrid(back));

		builder.AssignVoxelGrids(
			VoxelGridMap[position],
			VoxelGridMap[right],
			VoxelGridMap[left],
			VoxelGridMap[front],
			VoxelGridMap[back]
		);
	}


	void Start()
	{
		Initialize();
	}

	void Update()
	{
		for (int i = 0; i < meshBuildList.Count; i++)
		{
			ChunkBuilder chunkBuilder = meshBuildList[i];
			if (chunkBuilder.scheduled && chunkBuilder.handle.IsCompleted)
			{
				CompleteMeshBuild(i);
			}
			if (!chunkBuilder.scheduled)
			{
				ScheduleMeshBuild(i);
			}
		}

		if (meshBuildList.Count < maxJobsPerFrame && meshBuildQueue.Count > 0)
		{
			int length = Mathf.Min(maxJobsPerFrame, meshBuildQueue.Count);

			for (int i = 0; i < length; i++)
			{
				meshBuildList.Add(meshBuildQueue[i]);
			}

			meshBuildQueue.RemoveRange(0, length);
		}
	}

	void ScheduleMeshBuild(int i)
	{
		AddVoxelGrids(meshBuildList[i]);
		meshBuildList[i].ScheduleBuild();
	}

	void CompleteMeshBuild(int i)
	{
		meshBuildList[i].CompleteBuild();
		meshBuildList.RemoveAt(i);
	}

	void LateUpdate()
	{
		if (meshBuildQueue.Count == 0)
		{
			LoadedTerrain = true;
		}
	}

	private void OnDestroy()
	{
		Dispose();
	}

	public void Initialize()
	{
		ChunkMap = new();
		VoxelGridMap = new();
		meshBuildList = new();
		meshBuildQueue = new();

		VoxelGen = new VoxelGen(properties);

		FaceVertices = new NativeArray<int3>(24, Allocator.Persistent);

		//Bottom face
		FaceVertices[0] = new int3(1, 0, 0);
		FaceVertices[1] = new int3(1, 0, 1);
		FaceVertices[2] = new int3(0, 0, 1);
		FaceVertices[3] = new int3(0, 0, 0);

		//Front face
		FaceVertices[4] = new int3(1, 0, 1);
		FaceVertices[5] = new int3(1, 1, 1);
		FaceVertices[6] = new int3(0, 1, 1);
		FaceVertices[7] = new int3(0, 0, 1);

		//Back face
		FaceVertices[8] = new int3(0, 0, 0);
		FaceVertices[9] = new int3(0, 1, 0);
		FaceVertices[10] = new int3(1, 1, 0);
		FaceVertices[11] = new int3(1, 0, 0);

		//Left face
		FaceVertices[12] = new int3(0, 0, 1);
		FaceVertices[13] = new int3(0, 1, 1);
		FaceVertices[14] = new int3(0, 1, 0);
		FaceVertices[15] = new int3(0, 0, 0);

		//Right face
		FaceVertices[16] = new int3(1, 0, 0);
		FaceVertices[17] = new int3(1, 1, 0);
		FaceVertices[18] = new int3(1, 1, 1);
		FaceVertices[19] = new int3(1, 0, 1);

		//Top face
		FaceVertices[20] = new int3(0, 1, 0);
		FaceVertices[21] = new int3(0, 1, 1);
		FaceVertices[22] = new int3(1, 1, 1);
		FaceVertices[23] = new int3(1, 1, 0);

		UvCoordinates = new NativeArray<float2>(6, Allocator.Persistent);

		UvCoordinates[0] = new float2(.5f, 0);
		UvCoordinates[1] = new float2(.5f, .5f);
		UvCoordinates[2] = new float2(.5f, .5f);
		UvCoordinates[3] = new float2(.5f, .5f);
		UvCoordinates[4] = new float2(.5f, .5f);
		UvCoordinates[5] = new float2(0, 0);

		FaceDirections = new NativeArray<int3>(6, Allocator.Persistent);

		FaceDirections[0] = new int3(0, -1, 0); // Down
		FaceDirections[1] = new int3(0, 0, 1);  // Forward
		FaceDirections[2] = new int3(0, 0, -1); // Back
		FaceDirections[3] = new int3(-1, 0, 0); // Left
		FaceDirections[4] = new int3(1, 0, 0);  // Right
		FaceDirections[5] = new int3(0, 1, 0);  // Up
	}

	public void Dispose()
	{
		foreach (var job in meshBuildList)
		{
			if (job.scheduled)
			{
				job.CompleteBuild();
			}
		}

		foreach (var chunk in VoxelGridMap)
		{
			chunk.Value.Dispose();
		}

		FaceVertices.Dispose();
		FaceDirections.Dispose();
		UvCoordinates.Dispose();
		VoxelGen.Dispose();
	}
}