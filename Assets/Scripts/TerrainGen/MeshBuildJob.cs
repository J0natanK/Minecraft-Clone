using System.Diagnostics;
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
	[ReadOnly] public NativeArray<int> thisChunkData;
	[ReadOnly] public NativeArray<int> leftChunkData;
	[ReadOnly] public NativeArray<int> rightChunkData;
	[ReadOnly] public NativeArray<int> frontChunkData;
	[ReadOnly] public NativeArray<int> backChunkData;

	public void Execute()
	{
		for (int x = 0; x < chunkDimensions.x; x++)
		{
			for (int y = 0; y < chunkDimensions.y; y++)
			{
				for (int z = 0; z < chunkDimensions.x; z++)
				{
					int blockIndex = thisChunkData[Utils.VoxelIndex(x, y, z)];
					if (blockIndex != 0)
					{
						AddVoxel(x, y, z, blockIndex);
					}
				}
			}

		}
	}

	private void AddVoxel(int x, int y, int z, int blockIndex)
	{
		FaceUVs blockUVs = GetBlockUVs(blockIndex);

		if (blockIndex == 5)
		{
			AddWaterQuad(new int3(x, y, z), 5);
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

	private void AddQuad(int3 quadPos, int i, float2 bottomLeftUV)
	{
		int vertCount = vertices.Length;

		for (int j = 0; j < 4; j++)
		{
			vertices.Add(faceVertices[(i * 4) + j] + quadPos);
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

	private void AddWaterQuad(int3 quadPos, int i)
	{
		int vertCount = waterVertices.Length;

		for (int j = 0; j < 4; j++)
		{
			waterVertices.Add(faceVertices[(i * 4) + j] + quadPos);
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
			return leftChunkData[Utils.VoxelIndex(chunkDimensions.x - 1, neighbor.y, neighbor.z)] == 0 ||
				leftChunkData[Utils.VoxelIndex(chunkDimensions.x - 1, neighbor.y, neighbor.z)] == 5;

		if (neighbor.x >= chunkDimensions.x)
			return rightChunkData[Utils.VoxelIndex(0, neighbor.y, neighbor.z)] == 0 ||
				rightChunkData[Utils.VoxelIndex(0, neighbor.y, neighbor.z)] == 5;

		if (neighbor.z < 0)
			return backChunkData[Utils.VoxelIndex(neighbor.x, neighbor.y, chunkDimensions.x - 1)] == 0 ||
				backChunkData[Utils.VoxelIndex(neighbor.x, neighbor.y, chunkDimensions.x - 1)] == 5;

		if (neighbor.z >= chunkDimensions.x)
			return frontChunkData[Utils.VoxelIndex(neighbor.x, neighbor.y, 0)] == 0 ||
				frontChunkData[Utils.VoxelIndex(neighbor.x, neighbor.y, 0)] == 5;

		return thisChunkData[Utils.VoxelIndex(neighbor.x, neighbor.y, neighbor.z)] == 0 ||
			thisChunkData[Utils.VoxelIndex(neighbor.x, neighbor.y, neighbor.z)] == 5;
	}

	//Get the bottom left UV for all 6 faces
	public FaceUVs GetBlockUVs(int blockIndex)
	{
		FaceUVs uvs = new FaceUVs();
		switch (blockIndex)
		{
			case 1:
				uvs = new FaceUVs()
				{
					uv0 = new float2(0, 0),
					uv1 = new float2(0, 0),
					uv2 = new float2(0, 0),
					uv3 = new float2(0, 0),
					uv4 = new float2(0, 0),
					uv5 = new float2(0, 0)
				};

				break;

			case 2:
				uvs = new FaceUVs()
				{
					uv0 = new float2(0, .4f),
					uv1 = new float2(0, .4f),
					uv2 = new float2(0, .4f),
					uv3 = new float2(0, .4f),
					uv4 = new float2(0, .4f),
					uv5 = new float2(0, .4f)
				};
				break;
			case 3:
				uvs = new FaceUVs()
				{
					uv0 = new float2(0, .4f),
					uv1 = new float2(.5f, .4f),
					uv2 = new float2(.5f, .4f),
					uv3 = new float2(.5f, .4f),
					uv4 = new float2(.5f, .4f),
					uv5 = new float2(0, .2f)
				};
				break;
			case 4:
				uvs = new FaceUVs()
				{
					uv0 = new float2(.5f, 0),
					uv1 = new float2(.5f, 0),
					uv2 = new float2(.5f, 0),
					uv3 = new float2(.5f, 0),
					uv4 = new float2(.5f, 0),
					uv5 = new float2(.5f, 0)
				};
				break;
			case 5:
				uvs = new FaceUVs()
				{
					uv0 = new float2(.5f, .2f),
					uv1 = new float2(.5f, .2f),
					uv2 = new float2(.5f, .2f),
					uv3 = new float2(.5f, .2f),
					uv4 = new float2(.5f, .2f),
					uv5 = new float2(.5f, .2f)
				};
				break;
			case 6:
				uvs = new FaceUVs()
				{
					uv0 = new float2(0, .8f),
					uv1 = new float2(.5f, .6f),
					uv2 = new float2(.5f, .6f),
					uv3 = new float2(.5f, .6f),
					uv4 = new float2(.5f, .6f),
					uv5 = new float2(0, .8f)
				};
				break;
			case 7:
				uvs = new FaceUVs()
				{
					uv0 = new float2(0, .6f),
					uv1 = new float2(0, .6f),
					uv2 = new float2(0, .6f),
					uv3 = new float2(0, .6f),
					uv4 = new float2(0, .6f),
					uv5 = new float2(0, .6f)
				};
				break;
		}

		return uvs;
	}

	public struct FaceUVs
	{
		public float2 uv0;
		public float2 uv1;
		public float2 uv2;
		public float2 uv3;
		public float2 uv4;
		public float2 uv5;

		public float2 GetUV(int i)
		{
			switch (i)
			{
				case 0: return uv0;
				case 1: return uv1;
				case 2: return uv2;
				case 3: return uv3;
				case 4: return uv4;
				case 5: return uv5;
				default: return new float2(0, 0);
			}
		}
	}
}