using System;
using Unity.Collections;
using UnityEngine;

public class BlockBreakingAndPlacing : MonoBehaviour
{
	public int reach = 5;
	public Transform player;
	public string chunkTag = "Chunk";
	public ChunkManager chunkManager;

	byte selectedBlock;

	void Update()
	{
		if (Input.GetMouseButtonDown(0)) HandleBlockModification(false);
		if (Input.GetMouseButtonDown(1)) HandleBlockModification(true);

		for (int i = 0; i <= 9; i++)
		{
			if (Input.GetKeyDown(i.ToString()))
			{
				selectedBlock = (byte)Mathf.Clamp(i, 1, 7);
			}
		}
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
			chunkPosition.x + (TerrainConstants.ChunkSize.x / 2),
			chunkPosition.z + (TerrainConstants.ChunkSize.x / 2)
		);

		Vector2Int chunkKey = new Vector2Int(chunkCoord.x / TerrainConstants.ChunkSize.x, chunkCoord.y / TerrainConstants.ChunkSize.x);
		if (!ChunkManager.ChunkMap.TryGetValue(chunkKey, out TerrainChunk chunk))
		{
			Debug.LogWarning("Chunk doesnt exist!");
			return;
		}

		NativeArray<byte> voxelGrid = ChunkManager.VoxelGridMap[chunkKey * TerrainConstants.ChunkSize.x];

		Vector3Int offset = new Vector3Int(Mathf.Min((int)hit.normal.x, 0), Mathf.Min((int)hit.normal.y, 0), Mathf.Min((int)hit.normal.z, 0));
		offset = placeBlock ? offset : new Vector3Int(Mathf.Min(-(int)hit.normal.x, 0), Mathf.Min(-(int)hit.normal.y, 0), Mathf.Min(-(int)hit.normal.z, 0));

		int x = Mathf.FloorToInt(hit.point.x) - chunkPosition.x + offset.x;
		int y = Mathf.FloorToInt(hit.point.y) + offset.y;
		int z = Mathf.FloorToInt(hit.point.z) - chunkPosition.z + offset.z;

		x = Mathf.Clamp(x, 0, TerrainConstants.ChunkSize.x);
		y = Mathf.Clamp(y, 0, TerrainConstants.ChunkSize.y);
		z = Mathf.Clamp(z, 0, TerrainConstants.ChunkSize.x);

		voxelGrid[Utils.VoxelIndex(x, y, z)] = placeBlock ? selectedBlock : Blocks.Air;

		chunkManager.RequestChunkMesh(chunk.terrainMesh, chunk.waterMesh, chunkCoord, voxelGrid, true);
		chunk.UpdateColliderMesh();

		if (x == 0)
			UpdateChunk(chunkKey + Vector2Int.left, chunkCoord + new Vector2Int(-TerrainConstants.ChunkSize.x, 0));
		if (x == TerrainConstants.ChunkSize.x - 1)
			UpdateChunk(chunkKey + Vector2Int.right, chunkCoord + new Vector2Int(TerrainConstants.ChunkSize.x, 0));
		if (z == 0)
			UpdateChunk(chunkKey + Vector2Int.down, chunkCoord + new Vector2Int(0, -TerrainConstants.ChunkSize.x));
		if (z == TerrainConstants.ChunkSize.x - 1)
			UpdateChunk(chunkKey + Vector2Int.up, chunkCoord + new Vector2Int(0, TerrainConstants.ChunkSize.x));
	}

	void UpdateChunk(Vector2Int key, Vector2Int coord)
	{
		TerrainChunk chunk = ChunkManager.ChunkMap[key];
		NativeArray<byte> voxelGrid = ChunkManager.VoxelGridMap[key * TerrainConstants.ChunkSize.x];
		chunkManager.RequestChunkMesh(chunk.terrainMesh, chunk.waterMesh, coord, voxelGrid, true);
		chunk.UpdateColliderMesh();
	}
}
