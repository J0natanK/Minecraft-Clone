using UnityEngine;

public class TerrainChunk
{
	public bool visible { get; private set; }
	public bool requestingMesh { get; private set; }

	public Mesh terrainMesh;
	public Mesh waterMesh;

	RenderParams terrainParams;
	RenderParams waterParams;

	Vector3 chunkCenter;
	Vector2Int chunkCorner;

	MeshCollider collider;
	GameObject colliderObj;

	Bounds bounds;

	ChunkManager chunkManager;

	public TerrainChunk(Vector2Int position, Mesh terrainMesh, Mesh waterMesh, ChunkManager chunkManager)
	{
		this.terrainMesh = terrainMesh;
		this.waterMesh = waterMesh;
		this.chunkManager = chunkManager;

		bounds = new Bounds(new Vector2(position.x, position.y), Vector2.one * TerrainConstants.ChunkSize.x);

		chunkCenter = new Vector3(position.x - (TerrainConstants.ChunkSize.x / 2), 0, position.y - (TerrainConstants.ChunkSize.x / 2));
		chunkCorner = position;

		terrainParams = new RenderParams(chunkManager.terrainMaterial);
		waterParams = new RenderParams(chunkManager.waterMaterial);
	}

	public void UpdateVisibilty()
	{
		float playerDstSqr = bounds.SqrDistance(EndlessTerrain.ViewerPosition);
		visible = playerDstSqr <= EndlessTerrain.RenderDstSqr;

		if (visible) Render();

		UpdateCollider(playerDstSqr);
	}

	public void UpdateColliderMesh()
	{
		collider.sharedMesh = terrainMesh;
	}

	void Render()
	{
		if (terrainMesh == null && !requestingMesh)
		{
			terrainMesh = new();
			waterMesh = new();

			chunkManager.RequestChunkMesh(terrainMesh, waterMesh, chunkCorner);
			requestingMesh = true;
		}

		if (terrainMesh != null)
		{
			requestingMesh = false;
			Graphics.RenderMesh(terrainParams, terrainMesh, 0, Matrix4x4.TRS(chunkCenter, Quaternion.identity, Vector3.one));
		}

		if (waterMesh != null)
		{
			requestingMesh = false;
			Graphics.RenderMesh(waterParams, waterMesh, 0, Matrix4x4.TRS(chunkCenter, Quaternion.identity, Vector3.one));
		}
	}

	// TODO: Make collider logic less horrible
	void UpdateCollider(float playerDstSqr)
	{
		bool activeCollider = playerDstSqr <= EndlessTerrain.ColliderDstSqr;

		if (activeCollider && colliderObj == null && terrainMesh.vertexCount > 0)
		{
			InitCollider();
		}

		if (colliderObj == null)
		{
			return;
		}

		if (colliderObj.activeSelf != activeCollider)
		{
			colliderObj.SetActive(activeCollider);
		}

		if (activeCollider && collider.sharedMesh != terrainMesh)
		{
			collider.sharedMesh = terrainMesh;
		}
	}

	void InitCollider()
	{
		colliderObj = new GameObject("TerrainCollider");
		colliderObj.tag = "Chunk";
		colliderObj.transform.parent = chunkManager.gameObject.transform;
		colliderObj.transform.position = chunkCenter;
		collider = colliderObj.AddComponent<MeshCollider>();
		collider.sharedMesh = terrainMesh;
		colliderObj.SetActive(false);
	}
}
