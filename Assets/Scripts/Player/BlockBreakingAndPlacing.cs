using System;
using Unity.Collections;
using UnityEngine;

public class BlockBreakingAndPlacing : MonoBehaviour
{
	public int reach = 5;
	public Transform player;
	public string chunkTag = "Chunk";
	public ChunkManager chunkManager;

	void Update()
	{
		if (Input.GetMouseButtonDown(0)) HandleBlockModification(false);
		if (Input.GetMouseButtonDown(1)) HandleBlockModification(true);
	}

	private void HandleBlockModification(bool placeBlock)
	{
		if (Physics.Raycast(player.position, player.forward, out RaycastHit hit, reach) && hit.collider.CompareTag(chunkTag))
		{
			ModifyBlock(placeBlock, hit);
		}
	}

	private void ModifyBlock(bool placeBlock, RaycastHit hit)
	{
		Vector3Int chunkPosition = Vector3Int.FloorToInt(hit.collider.transform.position);
		Vector2Int chunkCoord = new Vector2Int(
			chunkPosition.x + (ChunkManager.ChunkDimensions.x / 2),
			chunkPosition.z + (ChunkManager.ChunkDimensions.x / 2)
		);

		Vector2Int chunkKey = new Vector2Int(chunkCoord.x / ChunkManager.ChunkDimensions.x, chunkCoord.y / ChunkManager.ChunkDimensions.x);
		if (!EndlessTerrain.ChunkDictionary.TryGetValue(chunkKey, out TerrainChunk chunk))
		{
			Debug.LogWarning("Chunk doesnt exist!");
			return;
		}

		NativeArray<byte> voxelGrid = ChunkManager.VoxelGridMap[chunkKey * ChunkManager.ChunkDimensions.x];

		Vector3Int offset = new Vector3Int(Mathf.Min((int)hit.normal.x, 0), Mathf.Min((int)hit.normal.y, 0), Mathf.Min((int)hit.normal.z, 0));
		offset = placeBlock ? offset : new Vector3Int(Mathf.Min(-(int)hit.normal.x, 0), Mathf.Min(-(int)hit.normal.y, 0), Mathf.Min(-(int)hit.normal.z, 0));

		int x = Mathf.FloorToInt(hit.point.x) - chunkPosition.x + offset.x;
		int y = Mathf.FloorToInt(hit.point.y) + offset.y;
		int z = Mathf.FloorToInt(hit.point.z) - chunkPosition.z + offset.z;

		x = Mathf.Clamp(x, 0, ChunkManager.ChunkDimensions.x);
		y = Mathf.Clamp(y, 0, ChunkManager.ChunkDimensions.y);
		z = Mathf.Clamp(z, 0, ChunkManager.ChunkDimensions.x);

		voxelGrid[Utils.VoxelIndex(x, y, z)] = placeBlock ? Blocks.Grass : Blocks.Air;

		chunkManager.RequestMesh(chunk.terrainMesh, chunk.waterMesh, chunkCoord, true, false, voxelGrid);
		chunk.UpdateCollider();

		if (x == 0)
			UpdateChunk(chunkKey + Vector2Int.left, chunkCoord + new Vector2Int(-ChunkManager.ChunkDimensions.x, 0));
		if (x == ChunkManager.ChunkDimensions.x - 1)
			UpdateChunk(chunkKey + Vector2Int.right, chunkCoord + new Vector2Int(ChunkManager.ChunkDimensions.x, 0));
		if (z == 0)
			UpdateChunk(chunkKey + Vector2Int.down, chunkCoord + new Vector2Int(0, -ChunkManager.ChunkDimensions.x));
		if (z == ChunkManager.ChunkDimensions.x - 1)
			UpdateChunk(chunkKey + Vector2Int.up, chunkCoord + new Vector2Int(0, ChunkManager.ChunkDimensions.x));
	}

	void UpdateChunk(Vector2Int key, Vector2Int coord)
	{
		TerrainChunk chunk = EndlessTerrain.ChunkDictionary[key];
		NativeArray<byte> voxelGrid = ChunkManager.VoxelGridMap[key * ChunkManager.ChunkDimensions.x];
		chunkManager.RequestMesh(chunk.terrainMesh, chunk.waterMesh, coord, true, false, voxelGrid);
		chunk.UpdateCollider();
	}
}
