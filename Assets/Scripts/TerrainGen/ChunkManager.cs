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

	public static VoxelGenerator VoxelGen;
	public static bool LoadedTerrain;

	public static Dictionary<Vector2Int, TerrainChunk> ChunkMap;
	public static Dictionary<Vector2Int, NativeArray<byte>> VoxelGridMap;

	List<MeshBuilder> meshBuildList;
	List<MeshBuilder> meshBuildQueue;

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
		MeshBuilder meshBuilder = new(terrainMesh, waterMesh, position, customVoxelGrid);

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

	void AddVoxelGrids(MeshBuilder builder)
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

		Vector2Int right = position + new Vector2Int(TerrainConstants.ChunkSize.x, 0);
		if (!VoxelGridMap.ContainsKey(right))
			VoxelGridMap.Add(right, VoxelGen.GenerateVoxelGrid(right));

		Vector2Int left = position - new Vector2Int(TerrainConstants.ChunkSize.x, 0);
		if (!VoxelGridMap.ContainsKey(left))
			VoxelGridMap.Add(left, VoxelGen.GenerateVoxelGrid(left));

		Vector2Int front = position + new Vector2Int(0, TerrainConstants.ChunkSize.x);
		if (!VoxelGridMap.ContainsKey(front))
			VoxelGridMap.Add(front, VoxelGen.GenerateVoxelGrid(front));

		Vector2Int back = position - new Vector2Int(0, TerrainConstants.ChunkSize.x);
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
			MeshBuilder chunkBuilder = meshBuildList[i];
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

		VoxelGen = new VoxelGenerator(properties);

		TerrainConstants.Init();
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

		TerrainConstants.Dispose();
		VoxelGen.Dispose();
	}
}