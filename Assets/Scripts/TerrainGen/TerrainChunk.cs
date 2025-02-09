using Unity.Collections;
using UnityEditor;
using UnityEngine;

public class TerrainChunk
{
	public bool visible;
	public bool requestingMesh;

	public Mesh terrainMesh;
	public Mesh waterMesh;

	RenderParams terrainParams;
	RenderParams waterParams;

	Vector3 meshPosition;
	Vector2Int chunkPosition;

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

		meshPosition = new Vector3(position.x - (TerrainConstants.ChunkSize.x / 2), 0, position.y - (TerrainConstants.ChunkSize.x / 2));
		chunkPosition = position;

		terrainParams = new RenderParams(chunkManager.terrainMaterial);
		waterParams = new RenderParams(chunkManager.waterMaterial);
		terrainParams.receiveShadows = true;
		terrainParams.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
	}

	public void UpdateVisibilty()
	{
		float playerDstSqr = bounds.SqrDistance(EndlessTerrain.ViewerPosition);
		visible = playerDstSqr <= EndlessTerrain.MaxViewDstSqr;

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

			chunkManager.RequestChunkMesh(terrainMesh, waterMesh, chunkPosition);
			requestingMesh = true;
		}

		if (terrainMesh != null)
		{
			requestingMesh = false;
			Graphics.RenderMesh(terrainParams, terrainMesh, 0, Matrix4x4.TRS(meshPosition, Quaternion.identity, Vector3.one));
		}

		if (waterMesh != null)
		{
			requestingMesh = false;
			Graphics.RenderMesh(waterParams, waterMesh, 0, Matrix4x4.TRS(meshPosition, Quaternion.identity, Vector3.one));
		}
	}


	void UpdateCollider(float playerDstSqr)
	{
		bool activeCollider = playerDstSqr <= EndlessTerrain.ColliderRangeSqr;

		if (activeCollider && colliderObj == null)
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
		colliderObj.transform.position = meshPosition;
		collider = colliderObj.AddComponent<MeshCollider>();
		colliderObj.SetActive(false);
	}
}
