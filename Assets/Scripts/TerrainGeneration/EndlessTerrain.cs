using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
	[SerializeField] float renderDst;
	[SerializeField] float colliderDst;
	[SerializeField] Transform viewer;
	[SerializeField] ChunkManager chunkManager;

	public static Vector2 ViewerPosition;
	public static float RenderDstSqr;
	public static float ColliderDstSqr;

	int chunksVisibleInViewDst;

	void Start()
	{
		chunksVisibleInViewDst = Mathf.RoundToInt(renderDst / TerrainConstants.ChunkSize.x);
		RenderDstSqr = renderDst * renderDst;
		ColliderDstSqr = colliderDst * colliderDst;
	}

	void Update()
	{
		ViewerPosition = new Vector2(viewer.position.x, viewer.position.z);
		UpdateVisibleChunks();
	}

	void UpdateVisibleChunks()
	{
		int currentChunkCoordX = (int)Mathf.Round(ViewerPosition.x / TerrainConstants.ChunkSize.x);
		int currentChunkCoordY = (int)Mathf.Round(ViewerPosition.y / TerrainConstants.ChunkSize.x);

		// Loop through all surrounding chunks
		for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
		{
			for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
			{
				Vector2Int viewedChunkCoord = new Vector2Int(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

				if (ChunkManager.ChunkMap.ContainsKey(viewedChunkCoord))
				{
					TerrainChunk viewedChunk = ChunkManager.ChunkMap[viewedChunkCoord];
					viewedChunk.UpdateVisibilty();

					//Destroy non visible chunks to save memory
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
					Vector2Int chunkPosition = viewedChunkCoord * TerrainConstants.ChunkSize.x;
					ChunkManager.ChunkMap.Add(viewedChunkCoord, chunkManager.CreateChunk(chunkPosition));
				}
			}
		}
	}
}