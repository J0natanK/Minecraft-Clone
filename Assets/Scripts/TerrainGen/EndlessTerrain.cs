using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

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
	public static Dictionary<Vector2, TerrainChunk> ChunkDictionary = new Dictionary<Vector2, TerrainChunk>();

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
		Resources.UnloadUnusedAssets();

	}

	void UpdateVisibleChunks()
	{
		int activeMeshes = 0;

		int currentChunkCoordX = (int)Mathf.Round(ViewerPosition.x / ChunkManager.ChunkDimensions.x);
		int currentChunkCoordY = (int)Mathf.Round(ViewerPosition.y / ChunkManager.ChunkDimensions.x);

		for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
		{
			for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
			{
				Vector2Int viewedChunkCoord = new Vector2Int(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

				if (ChunkDictionary.ContainsKey(viewedChunkCoord))
				{
					TerrainChunk viewedChunk = ChunkDictionary[viewedChunkCoord];
					viewedChunk.UpdateChunk();

					if (viewedChunk.destroyMesh && viewedChunk.terrainMesh != null && !viewedChunk.requestingMesh)
					{
						Destroy(viewedChunk.terrainMesh);
						Destroy(viewedChunk.waterMesh);
					}
					if (viewedChunk.terrainMesh != null) activeMeshes++;
				}
				else
				{
					ChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkManager));
				}
			}
		}

		//Debug.Log(activeMeshes);
	}
}