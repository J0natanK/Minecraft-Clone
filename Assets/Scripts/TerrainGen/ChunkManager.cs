using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
	public float maxViewDst;
	public float colliderRange;
	public Transform viewer;
	public ChunkGenerator chunkGenerator;

	public static Vector2 ViewerPosition;
	public static float MaxViewDstSqr;
	public static float ColliderRangeSqr;
	public static Dictionary<Vector2, TerrainChunk> ChunkDictionary = new Dictionary<Vector2, TerrainChunk>();

	int chunksVisibleInViewDst;

	void Start()
	{
		chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / ChunkGenerator.ChunkDimensions.x);
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
		int currentChunkCoordX = (int)Mathf.Round(ViewerPosition.x / ChunkGenerator.ChunkDimensions.x);
		int currentChunkCoordY = (int)Mathf.Round(ViewerPosition.y / ChunkGenerator.ChunkDimensions.x);

		for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
		{
			for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
			{
				Vector2Int viewedChunkCoord = new Vector2Int(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

				if (ChunkDictionary.ContainsKey(viewedChunkCoord))
				{
					TerrainChunk viewedChunk = ChunkDictionary[viewedChunkCoord];
					viewedChunk.UpdateChunk();
				}
				else
				{
					ChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkGenerator));
				}
			}
		}
	}
}