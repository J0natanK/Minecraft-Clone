using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
	public float maxViewDst;
	public float colliderRange;
	public int numLods;

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
					chunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkGenerator, maxViewDst, colliderRange, numLods));
				}
			}
		}
	}

	public class TerrainChunk
	{
		Mesh terrainMesh;
		Mesh waterMesh;
		Mesh[] lodTerrainMeshes;
		Mesh[] lodWaterMeshes;

		RenderParams terrainParams;
		RenderParams waterParams;

		Bounds bounds;

		Vector3 meshPosition;

		float maxViewDstSqr;
		float colliderRangeSqr;

		bool hasCollider;

		float newLod;
		float lod;

		int numLods;

		public TerrainChunk(Vector2Int coord, ChunkGenerator chunkGen, float maxViewDst, float colliderRange, int numLods)
		{
			Vector2Int chunkDimensions = ChunkGenerator.ChunkDimensions;

			Vector2Int offset = coord * chunkDimensions.x;
			bounds = new Bounds(new Vector2(offset.x, offset.y), Vector2.one * chunkDimensions.x);

			terrainMesh = new();
			waterMesh = new();

			lodTerrainMeshes = new Mesh[numLods];
			lodWaterMeshes = new Mesh[numLods];
			for (int i = 0; i < numLods; i++)
			{
				lodTerrainMeshes[i] = new Mesh();
				lodWaterMeshes[i] = new Mesh();
				chunkGen.RequestMesh(lodTerrainMeshes[i], lodWaterMeshes[i], offset, i);
			}

			meshPosition = new Vector3(offset.x - (chunkDimensions.x / 2), 0, offset.y - (chunkDimensions.x / 2));

			terrainParams = new RenderParams(chunkGen.terrainMaterial);
			waterParams = new RenderParams(chunkGen.waterMaterial);

			maxViewDstSqr = maxViewDst * maxViewDst;
			colliderRangeSqr = colliderRange * colliderRange;

			this.numLods = numLods;
		}

		public void UpdateTerrainChunk()
		{
			float viewerDstFromNearestEdge = bounds.SqrDistance(viewerPosition);

			if (viewerDstFromNearestEdge < colliderRangeSqr && !hasCollider)
			{
				hasCollider = true;
			}

			bool visible = viewerDstFromNearestEdge <= maxViewDstSqr;

			if (visible)
			{
				newLod = viewerDstFromNearestEdge / maxViewDstSqr * numLods;

				if (lod != newLod)
				{
					terrainMesh = lodTerrainMeshes[(int)newLod];
					waterMesh = lodWaterMeshes[(int)newLod];

					lod = newLod;
				}
			}

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