using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.Accessibility;

public class EndlessTerrain : MonoBehaviour
{
	public float maxViewDst;
	public float colliderRange;
	public float lifetimeSeconds;
	public Transform viewer;
	public ChunkManager chunkManager;

	public static Vector2 ViewerPosition;
	public static float MaxViewDstSqr;
	public static float ColliderRangeSqr;

	int chunksVisibleInViewDst;

	void Start()
	{
		chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / ChunkManager.ChunkDimensions.x);
		MaxViewDstSqr = maxViewDst * maxViewDst;
		ColliderRangeSqr = colliderRange * colliderRange;
	}

	void Update()
	{
		ViewerPosition = new Vector2(viewer.position.x, viewer.position.z);
		UpdateVisibleChunks();
	}

	void UpdateVisibleChunks()
	{
		int currentChunkCoordX = (int)Mathf.Round(ViewerPosition.x / ChunkManager.ChunkDimensions.x);
		int currentChunkCoordY = (int)Mathf.Round(ViewerPosition.y / ChunkManager.ChunkDimensions.x);

		for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
		{
			for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
			{
				Vector2Int viewedChunkCoord = new Vector2Int(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

				if (ChunkManager.ChunkMap.ContainsKey(viewedChunkCoord))
				{
					TerrainChunk viewedChunk = ChunkManager.ChunkMap[viewedChunkCoord];
					viewedChunk.UpdateVisibilty();

					if (!viewedChunk.visible && !viewedChunk.requestingMesh)
					{
						if (viewedChunk.terrainMesh != null)
							Destroy(viewedChunk.terrainMesh);
						if (viewedChunk.waterMesh != null)
							Destroy(viewedChunk.waterMesh);
					}
				}
				else
				{
					Vector2Int chunkPosition = viewedChunkCoord * ChunkManager.ChunkDimensions.x;
					ChunkManager.ChunkMap.Add(viewedChunkCoord, chunkManager.CreateChunk(chunkPosition));
				}
			}
		}
	}
}