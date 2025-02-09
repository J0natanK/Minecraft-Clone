using Unity.Collections;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Jobs;
using Unity.Jobs;

public class VoxelGenerator
{
	TerrainProperties properties;
	NativeArray<float> curve;

	NativeArray<float> frequencies;
	NativeArray<float> amplitudes;

	public VoxelGenerator(TerrainProperties properties)
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

	public NativeArray<byte> GenerateVoxelGrid(Vector2Int offset)
	{
		int size = TerrainConstants.ChunkSize.x * TerrainConstants.ChunkSize.y * TerrainConstants.ChunkSize.x;
		NativeArray<byte> voxelGrid = new(size, Allocator.Persistent);

		VoxelGridJob job = new()
		{
			voxelValues = voxelGrid,
			chunkDimensions = new int2(TerrainConstants.ChunkSize.x, TerrainConstants.ChunkSize.y),
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

		job.Schedule(TerrainConstants.ChunkSize.x * TerrainConstants.ChunkSize.x, 1).Complete();

		return voxelGrid;
	}
}