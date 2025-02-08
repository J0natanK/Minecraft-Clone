using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryCleaner : MonoBehaviour
{
	public float maxVoxelGridMapSizeMB;

	float voxelGridSizeMB;

	void Start()
	{
		voxelGridSizeMB = ChunkManager.ChunkDimensions.x * ChunkManager.ChunkDimensions.y * ChunkManager.ChunkDimensions.x / 1000000;
	}

	void Update()
	{
		float currentSize = ChunkManager.VoxelGridMap.Count * voxelGridSizeMB;

		if (currentSize > maxVoxelGridMapSizeMB)
		{
			//Load far away chunks to disk
		}
	}
}
