using Unity.Collections;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Jobs;
using Unity.Jobs;

public class Noise
{
	TerrainProperties properties;
	NativeArray<float> curve;

	NativeArray<float> frequencies;
	NativeArray<float> amplitudes;

	public Noise(TerrainProperties properties)
	{
		this.properties = properties;
		frequencies = new NativeArray<float>(properties.numOctaves, Allocator.Persistent);
		amplitudes = new NativeArray<float>(properties.numOctaves, Allocator.Persistent);
		curve = Utils.SampleCurve(properties.heightCurve, 256);

		for (int i = 0; i < properties.numOctaves; i++)
		{
			frequencies[i] = math.pow(properties.lacunarity, i) * properties.frequency;
			amplitudes[i] = math.pow(properties.persistance, i);
		}
	}

	public void Dispose()
	{
		frequencies.Dispose();
		amplitudes.Dispose();
		curve.Dispose();
	}

	public NativeArray<int> GenerateVoxelGrid(Vector2Int offset)
	{
		int size = ChunkGenerator.ChunkDimensions.x * ChunkGenerator.ChunkDimensions.y * ChunkGenerator.ChunkDimensions.x;
		NativeArray<int> voxelValues = new(size, Allocator.Persistent);

		NoiseJob job = new()
		{
			voxelValues = voxelValues,
			chunkDimensions = new int2(ChunkGenerator.ChunkDimensions.x, ChunkGenerator.ChunkDimensions.y),
			offset = new int2(offset.x, offset.y),
			curve = curve,
			frequencies = frequencies,
			amplitudes = amplitudes,
			terrainHeight = properties.terrainHeight,
			sandDensity = properties.sandDensity,
			maxTreeHeight = properties.maxTreeHeight,
			treeDensity = properties.treeDensity,
			surfaceLevel = properties.surfaceLevel,
			seaLevel = properties.seaLevel,
			stoneDepth = properties.stoneDepth,
			numOctaves = properties.numOctaves,
			seed = properties.noiseSeed,
			lacunarity = properties.lacunarity,
			persistance = properties.persistance,
			frequency = properties.frequency,
			noise3DContribution = properties.noise3DContribution,
			random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(uint.MinValue, uint.MaxValue))

		};

		job.Schedule(ChunkGenerator.ChunkDimensions.x * ChunkGenerator.ChunkDimensions.x, 1).Complete();

		return voxelValues;
	}
}