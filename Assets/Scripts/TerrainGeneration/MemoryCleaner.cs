using UnityEngine;

// Should be responisble for cleaning up VoxelGridMap 
// if it gets too large

public class MemoryCleaner : MonoBehaviour
{
	[SerializeField] float maxVoxelGridMapSizeMB;

	float voxelGridSizeMB;

	void Start()
	{
		voxelGridSizeMB = TerrainConstants.ChunkSize.x * TerrainConstants.ChunkSize.y * TerrainConstants.ChunkSize.x / 1000000;
	}

	void Update()
	{
		float currentSize = ChunkManager.VoxelGridMap.Count * voxelGridSizeMB;

		if (currentSize > maxVoxelGridMapSizeMB)
		{
			// TODO: Load far away chunks to disk?
		}
	}
}
