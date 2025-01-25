using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
	public float maxViewDst;
	public float colliderRange;

	public Transform viewer;
	public ChunkGenerator chunkGenerator;

	public static Vector2 viewerPosition;

	int chunksVisibleInViewDst;

	Dictionary<Vector2, TerrainChunk> chunkDictionary = new Dictionary<Vector2, TerrainChunk>();

	void Start()
	{
		chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / ChunkGenerator.ChunkDimensions.x);
	}

	void Update()
	{
		viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
		UpdateVisibleChunks();
	}

	void UpdateVisibleChunks()
	{
		int currentChunkCoordX = (int)Mathf.Round(viewerPosition.x / ChunkGenerator.ChunkDimensions.x);
		int currentChunkCoordY = (int)Mathf.Round(viewerPosition.y / ChunkGenerator.ChunkDimensions.x);

		for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
		{
			for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
			{
				Vector2Int viewedChunkCoord = new Vector2Int(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

				if (chunkDictionary.ContainsKey(viewedChunkCoord))
				{
					TerrainChunk viewedChunk = chunkDictionary[viewedChunkCoord];
					viewedChunk.UpdateTerrainChunk();
				}
				else
				{
					chunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkGenerator, maxViewDst, colliderRange));
				}
			}
		}
	}

	public class TerrainChunk
	{
		Mesh terrainMesh;
		Mesh waterMesh;

		RenderParams terrainParams;
		RenderParams waterParams;

		Bounds bounds;

		Vector3 meshPosition;

		float maxViewDstSqr;
		float colliderRangeSqr;

		bool hasCollider;
		GameObject collider;

		public TerrainChunk(Vector2Int coord, ChunkGenerator chunkGen, float maxViewDst, float colliderRange)
		{
			Vector2Int chunkDimensions = ChunkGenerator.ChunkDimensions;

			Vector2Int offset = coord * chunkDimensions.x;
			bounds = new Bounds(new Vector2(offset.x, offset.y), Vector2.one * chunkDimensions.x);

			terrainMesh = new();
			waterMesh = new();
			chunkGen.RequestMesh(terrainMesh, waterMesh, offset);

			meshPosition = new Vector3(offset.x - (chunkDimensions.x / 2), 0, offset.y - (chunkDimensions.x / 2));

			terrainParams = new RenderParams(chunkGen.terrainMaterial);
			waterParams = new RenderParams(chunkGen.waterMaterial);

			maxViewDstSqr = maxViewDst * maxViewDst;
			colliderRangeSqr = colliderRange * colliderRange;
		}

		public void UpdateTerrainChunk()
		{
			float viewerDstFromNearestEdge = bounds.SqrDistance(viewerPosition);

			if (viewerDstFromNearestEdge < colliderRangeSqr && !hasCollider)
			{
				collider = new GameObject("Collider");
				collider.transform.position = meshPosition;
				collider.AddComponent<MeshCollider>().sharedMesh = terrainMesh;

				hasCollider = true;
			}
			if (hasCollider && viewerDstFromNearestEdge > colliderRangeSqr)
			{
				collider.SetActive(false);
				hasCollider = false;
			}

			bool visible = viewerDstFromNearestEdge <= maxViewDstSqr;

			if (visible) Render();
		}
		void Render()
		{
			if (terrainMesh != null)
			{
				Graphics.RenderMesh(terrainParams, terrainMesh, 0, Matrix4x4.TRS(meshPosition, Quaternion.identity, Vector3.one));
			}

			if (waterMesh != null)
			{
				Graphics.RenderMesh(waterParams, waterMesh, 0, Matrix4x4.TRS(meshPosition, Quaternion.identity, Vector3.one));
			}
		}
	}
}