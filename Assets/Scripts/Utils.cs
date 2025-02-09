using UnityEngine;
using Unity.Collections;

public static class Utils
{
	//Converts 3D index into 1D
	public static int VoxelIndex(int x, int y, int z)
	{
		return x * TerrainConstants.ChunkSize.y * TerrainConstants.ChunkSize.x + y * TerrainConstants.ChunkSize.x + z;
	}

	//Sampled curve that can be used in job
	public static NativeArray<float> SampleCurve(AnimationCurve curve, int resolution)
	{
		NativeArray<float> curveArr = new NativeArray<float>(resolution, Allocator.Persistent);

		for (int i = 0; i < resolution; i++)
		{
			curveArr[i] = curve.Evaluate((float)i / resolution);
		}

		return curveArr;
	}

	public static float Remap(float value, float from1, float to1, float from2, float to2)
	{
		return from2 + (value - from1) * (to2 - from2) / (to1 - from1);
	}
}
