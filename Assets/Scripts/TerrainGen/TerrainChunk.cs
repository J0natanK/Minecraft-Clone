using UnityEditor;
using UnityEngine;

public class TerrainChunk
{
	public Mesh terrainMesh;
	public Mesh waterMesh;
	public Vector3 position;
	public Vector2Int voxelGridCoords;
	public float secSinceVisible;
	public bool requestingMesh;
	public bool destroyMesh;

	RenderParams terrainParams;
	RenderParams waterParams;

	Bounds bounds;

	bool hasCollider;
	GameObject collider;

	ChunkManager chunkManager;

	public TerrainChunk(Vector2Int coord, ChunkManager chunkManager)
	{
		Vector2Int chunkDimensions = ChunkManager.ChunkDimensions;

		Vector2Int offset = coord * chunkDimensions.x;
		voxelGridCoords = offset;
		bounds = new Bounds(new Vector2(offset.x, offset.y), Vector2.one * chunkDimensions.x);

		position = new Vector3(offset.x - (chunkDimensions.x / 2), 0, offset.y - (chunkDimensions.x / 2));

		terrainParams = new RenderParams(chunkManager.terrainMaterial);
		waterParams = new RenderParams(chunkManager.waterMaterial);
		terrainParams.receiveShadows = true;
		terrainParams.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

		this.chunkManager = chunkManager;
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
		if (visible)
		{
			destroyMesh = false;
			Render();
			secSinceVisible = 0;
		}
		else
		{
			destroyMesh = true;
		}

		secSinceVisible += Time.deltaTime;
	}

	public void UpdateCollider()
	{
		collider.GetComponent<MeshCollider>().sharedMesh = terrainMesh;
	}

	void Render()
	{
		// if (terrainMesh == null && !requestingMesh)
		// {
		// 	terrainMesh = new();
		// 	waterMesh = new();

		// 	chunkManager.RequestMesh(terrainMesh, waterMesh, voxelGridCoords);
		// 	requestingMesh = true;
		// }

		if (terrainMesh == null && !requestingMesh)
		{
			terrainMesh = new();
			waterMesh = new();

			chunkManager.RequestMesh(terrainMesh, waterMesh, voxelGridCoords);
			requestingMesh = true;
		}

		if (terrainMesh != null)
		{
			requestingMesh = false;
			Graphics.RenderMesh(terrainParams, terrainMesh, 0, Matrix4x4.TRS(position, Quaternion.identity, Vector3.one));
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