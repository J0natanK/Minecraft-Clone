using UnityEngine;

public class TerrainChunk
{
	public Mesh terrainMesh;
	public Mesh waterMesh;
	public Vector3 position;
	public Vector2Int gridCoords;

	RenderParams terrainParams;
	RenderParams waterParams;

	Bounds bounds;

	bool hasCollider;
	GameObject collider;

	public TerrainChunk(Vector2Int coord, ChunkManager chunkGen)
	{
		Vector2Int chunkDimensions = ChunkManager.ChunkDimensions;

		Vector2Int offset = coord * chunkDimensions.x;
		gridCoords = offset;
		bounds = new Bounds(new Vector2(offset.x, offset.y), Vector2.one * chunkDimensions.x);

		terrainMesh = new();
		waterMesh = new();
		chunkGen.RequestMesh(terrainMesh, waterMesh, offset);

		position = new Vector3(offset.x - (chunkDimensions.x / 2), 0, offset.y - (chunkDimensions.x / 2));

		terrainParams = new RenderParams(chunkGen.terrainMaterial);
		waterParams = new RenderParams(chunkGen.waterMaterial);
		terrainParams.receiveShadows = true;
		terrainParams.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
	}

	public void UpdateChunk()
	{
		float viewerDstFromNearestEdge = bounds.SqrDistance(EndlessTerrain.ViewerPosition);

		if (viewerDstFromNearestEdge < EndlessTerrain.ColliderRangeSqr && !hasCollider)
		{
			if (collider == null)
			{
				InitCollider();
			}
			else
			{
				collider.SetActive(true);
			}

			hasCollider = true;
		}
		if (hasCollider && viewerDstFromNearestEdge > EndlessTerrain.ColliderRangeSqr)
		{
			collider.SetActive(false);
			hasCollider = false;
		}

		bool visible = viewerDstFromNearestEdge <= EndlessTerrain.MaxViewDstSqr;
		if (visible) Render();
	}

	public void UpdateCollider()
	{
		collider.GetComponent<MeshCollider>().sharedMesh = terrainMesh;
	}

	void Render()
	{
		if (terrainMesh != null)
		{
			Graphics.RenderMesh(terrainParams, terrainMesh, 0, Matrix4x4.TRS(position, Quaternion.identity, Vector3.one));
		}

		if (waterMesh != null)
		{
			Graphics.RenderMesh(waterParams, waterMesh, 0, Matrix4x4.TRS(position, Quaternion.identity, Vector3.one));
		}
	}

	void InitCollider()
	{
		collider = new GameObject("Collider");
		collider.tag = "Chunk";
		collider.transform.position = position;
		collider.AddComponent<MeshCollider>().sharedMesh = terrainMesh;
	}
}