using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile(CompileSynchronously = true)]
struct MeshBuildJob : IJob
{
	public NativeList<float3> vertices;
	public NativeList<int> triangles;
	public NativeList<float2> uvs;

	public NativeList<float3> waterVertices;
	public NativeList<int> waterTriangles;
	public NativeList<float2> waterUvs;

	[ReadOnly] public NativeArray<int3> faceDirections;
	[ReadOnly] public NativeArray<float2> uvCordinates;
	[ReadOnly] public NativeArray<int3> faceVertices;

	[ReadOnly] public int2 chunkDimensions;

	//Neighbouring chunks
	[ReadOnly] public NativeArray<int> thisVoxelGrid;
	[ReadOnly] public NativeArray<int> leftVoxelGrid;
	[ReadOnly] public NativeArray<int> rightVoxelGrid;
	[ReadOnly] public NativeArray<int> frontVoxelGrid;
	[ReadOnly] public NativeArray<int> backVoxelGrid;

	public void Execute()
	{
		for (int x = 0; x < chunkDimensions.x; x++)
		{
			for (int y = 0; y < chunkDimensions.y; y++)
			{
				for (int z = 0; z < chunkDimensions.x; z++)
				{
					int blockIndex = thisVoxelGrid[Utils.VoxelIndex(x, y, z)];
					if (blockIndex != Blocks.Air)
					{
						AddVoxel(x, y, z, blockIndex);
					}
				}
			}

		}
	}

	private void AddVoxel(int x, int y, int z, int blockIndex)
	{
		FaceUVs blockUVs = Blocks.GetUVs(blockIndex);

		if (blockIndex == Blocks.Water)
		{
			AddWaterQuad(new int3(x, y, z));
			return;
		}

		for (int i = 0; i < 6; i++)
		{
			if (IsFaceVisible(x, y, z, i))
			{
				AddQuad(new int3(x, y, z), i, blockUVs.GetUV(i));
			}
		}
	}

	private void AddQuad(int3 pos, int faceIndex, float2 bottomLeftUV)
	{
		int vertCount = vertices.Length;

		for (int j = 0; j < 4; j++)
		{
			vertices.Add(faceVertices[(faceIndex * 4) + j] + pos);
		}

		triangles.Add(vertCount);
		triangles.Add(vertCount + 1);
		triangles.Add(vertCount + 3);
		triangles.Add(vertCount + 3);
		triangles.Add(vertCount + 1);
		triangles.Add(vertCount + 2);

		uvs.Add(bottomLeftUV + new float2(.5f, 0));
		uvs.Add(bottomLeftUV + new float2(.5f, .2f));
		uvs.Add(bottomLeftUV + new float2(0, .2f));
		uvs.Add(bottomLeftUV);
	}

	private void AddWaterQuad(int3 quadPos)
	{
		int vertCount = waterVertices.Length;
		int vertexIndex = 20;

		for (int j = 0; j < 4; j++)
		{
			waterVertices.Add(faceVertices[vertexIndex + j] + quadPos);
		}

		waterTriangles.Add(vertCount);
		waterTriangles.Add(vertCount + 1);
		waterTriangles.Add(vertCount + 3);
		waterTriangles.Add(vertCount + 3);
		waterTriangles.Add(vertCount + 1);
		waterTriangles.Add(vertCount + 2);

		waterUvs.Add(new float2(1, 0));
		waterUvs.Add(new float2(1, 1));
		waterUvs.Add(new float2(0, 1));
		waterUvs.Add(new float2(0, 0));
	}

	private bool IsFaceVisible(int x, int y, int z, int faceIndex)
	{
		int3 direction = faceDirections[faceIndex];
		int3 neighbor = new int3(x + direction.x, y + direction.y, z + direction.z);

		if (neighbor.y < 0 || neighbor.y >= chunkDimensions.y)
			return false;

		if (neighbor.x < 0)
			return leftVoxelGrid[Utils.VoxelIndex(chunkDimensions.x - 1, neighbor.y, neighbor.z)] == Blocks.Air ||
				leftVoxelGrid[Utils.VoxelIndex(chunkDimensions.x - 1, neighbor.y, neighbor.z)] == Blocks.Water;

		if (neighbor.x >= chunkDimensions.x)
			return rightVoxelGrid[Utils.VoxelIndex(0, neighbor.y, neighbor.z)] == Blocks.Air ||
				rightVoxelGrid[Utils.VoxelIndex(0, neighbor.y, neighbor.z)] == Blocks.Water;

		if (neighbor.z < 0)
			return backVoxelGrid[Utils.VoxelIndex(neighbor.x, neighbor.y, chunkDimensions.x - 1)] == Blocks.Air ||
				backVoxelGrid[Utils.VoxelIndex(neighbor.x, neighbor.y, chunkDimensions.x - 1)] == Blocks.Water;

		if (neighbor.z >= chunkDimensions.x)
			return frontVoxelGrid[Utils.VoxelIndex(neighbor.x, neighbor.y, 0)] == Blocks.Air ||
				frontVoxelGrid[Utils.VoxelIndex(neighbor.x, neighbor.y, 0)] == Blocks.Water;

		return thisVoxelGrid[Utils.VoxelIndex(neighbor.x, neighbor.y, neighbor.z)] == Blocks.Air ||
			thisVoxelGrid[Utils.VoxelIndex(neighbor.x, neighbor.y, neighbor.z)] == Blocks.Water;
	}
}