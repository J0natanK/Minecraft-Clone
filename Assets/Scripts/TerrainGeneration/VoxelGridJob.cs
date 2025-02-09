using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

//TODO: Make more readable

[BurstCompile]
struct VoxelGridJob : IJobParallelFor
{
	[NativeDisableParallelForRestriction]
	public NativeArray<byte> voxelValues;

	[ReadOnly] public int2 chunkDimensions;
	[ReadOnly] public int2 offset;

	[ReadOnly] public NativeArray<float> curve;

	[ReadOnly] public NativeArray<float> frequencies;
	[ReadOnly] public NativeArray<float> amplitudes;

	[ReadOnly] public float terrainHeight;
	[ReadOnly] public float sandDensity;
	[ReadOnly] public float maxTreeHeight;
	[ReadOnly] public float treeDensity;
	[ReadOnly] public int stoneDepth;
	[ReadOnly] public float noise3DContribution;
	[ReadOnly] public int surfaceLevel;
	[ReadOnly] public int seaLevel;

	[ReadOnly] public int numOctaves;
	[ReadOnly] public float seed;
	[ReadOnly] public float lacunarity;
	[ReadOnly] public float persistance;
	[ReadOnly] public float frequency;

	[ReadOnly] public float skipThreshold;

	[ReadOnly] public Random random;

	public void Execute(int i)
	{
		int x = i % chunkDimensions.x;
		int z = i / chunkDimensions.x;

		float noiseValue = Noise2D(x + offset.x, z + offset.y);
		float noise01 = Utils.Remap(noiseValue, -1, 1, 0, 1);
		int curveIndex = (int)math.clamp(noise01 * curve.Length, 0, curve.Length - 1);

		noiseValue *= terrainHeight * curve[curveIndex];
		noiseValue += surfaceLevel;

		for (int y = 0; y < chunkDimensions.y; y++)
		{
			float noise3D = Noise3D(x + offset.x, y, z + offset.y);
			float combinedNoise = noiseValue + (noise3D * terrainHeight * noise3DContribution);

			if (y < combinedNoise)
			{
				voxelValues[Utils.VoxelIndex(x, y, z)] = Blocks.Stone;

				if (y + stoneDepth < chunkDimensions.y)
				{
					bool belowSeaLevel = y < seaLevel - stoneDepth;
					float sandNoise = noise.snoise(new float2(x * .1f, z * .1f));
					bool isSand = (y + 1 < seaLevel && sandNoise < sandDensity) || belowSeaLevel;

					for (int j = 0; j < stoneDepth; j++)
					{
						byte block;
						if (isSand) block = Blocks.Sand;
						else if (j == stoneDepth - 1) block = Blocks.Grass;
						else block = Blocks.Dirt;

						voxelValues[Utils.VoxelIndex(x, y + j + 1, z)] = block;
					}

				}

				float random = this.random.NextFloat();
				if (random < treeDensity && IsValidTreePosition(x, y, z))
				{
					AddTree(x, y + stoneDepth + 1, z);
				}

				continue;
			}

			//Water
			if (y == seaLevel)
			{
				voxelValues[Utils.VoxelIndex(x, y, z)] = Blocks.Water;
			}
		}
	}

	void AddTree(int x, int y, int z)
	{
		//Trunk
		for (int i = 0; i < 6; i++)
		{
			voxelValues[Utils.VoxelIndex(x, y + i, z)] = Blocks.Log;
		}

		//Leaves
		for (int yOffset = -2; yOffset <= 1; yOffset++)
		{
			for (int xOffset = -2; xOffset <= 2; xOffset++)
			{
				for (int zOffset = -2; zOffset <= 2; zOffset++)
				{
					//Shaping the leaves like minecraft
					if (yOffset == 1 || yOffset == 0)
					{
						if (math.abs(xOffset) == 2 || math.abs(zOffset) == 2)
						{
							continue;
						}

						if (math.abs(xOffset) == 1 && math.abs(zOffset) == 1)
						{
							continue;
						}

					}
					else
					{
						if (math.abs(xOffset) == 2 && math.abs(zOffset) == 2 && random.NextFloat() < .5f)
						{
							continue;
						}
					}

					voxelValues[Utils.VoxelIndex(x + xOffset, y + yOffset + 6, z + zOffset)] = Blocks.Leaves;
				}
			}
		}
	}



	bool IsValidTreePosition(int x, int y, int z)
	{
		//Checks if the height is within range and if the tree is far enough inside the chunk too not get cut off
		bool validHeight = y < maxTreeHeight && y > seaLevel;
		bool insideChunk = x > 2 && x < chunkDimensions.x - 2 && z > 2 && z < chunkDimensions.x - 2;

		return validHeight && insideChunk;
	}

	float Noise3D(int x, int y, int z)
	{
		float sum = 0;

		for (int i = 0; i < numOctaves; i++)
		{
			float frequency = frequencies[i] * 10;
			float amplitude = amplitudes[i];

			sum += noise.snoise(new float3((x + seed) * frequency, (y - seed) * frequency, (z - seed) * frequency)) * amplitude;
		}

		return sum;
	}

	float Noise2D(int x, int y)
	{
		float sum = 0;
		for (int i = 0; i < numOctaves; i++)
		{
			float frequency = frequencies[i];
			float amplitude = amplitudes[i];

			sum += noise.snoise(new float2((x + seed) * frequency, (y - seed) * frequency)) * amplitude;
		}

		return sum;
	}
}
