using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

public static class TerrainConstants
{
	public static readonly Vector2Int ChunkSize = new Vector2Int(32, 192);

	public static NativeArray<int3> FaceVertices;
	public static NativeArray<int3> FaceDirections;
	public static NativeArray<float2> UvCoordinates;

	public static void Init()
	{
		FaceVertices = new NativeArray<int3>(24, Allocator.Persistent);

		//Bottom face
		FaceVertices[0] = new int3(1, 0, 0);
		FaceVertices[1] = new int3(1, 0, 1);
		FaceVertices[2] = new int3(0, 0, 1);
		FaceVertices[3] = new int3(0, 0, 0);

		//Front face
		FaceVertices[4] = new int3(1, 0, 1);
		FaceVertices[5] = new int3(1, 1, 1);
		FaceVertices[6] = new int3(0, 1, 1);
		FaceVertices[7] = new int3(0, 0, 1);

		//Back face
		FaceVertices[8] = new int3(0, 0, 0);
		FaceVertices[9] = new int3(0, 1, 0);
		FaceVertices[10] = new int3(1, 1, 0);
		FaceVertices[11] = new int3(1, 0, 0);

		//Left face
		FaceVertices[12] = new int3(0, 0, 1);
		FaceVertices[13] = new int3(0, 1, 1);
		FaceVertices[14] = new int3(0, 1, 0);
		FaceVertices[15] = new int3(0, 0, 0);

		//Right face
		FaceVertices[16] = new int3(1, 0, 0);
		FaceVertices[17] = new int3(1, 1, 0);
		FaceVertices[18] = new int3(1, 1, 1);
		FaceVertices[19] = new int3(1, 0, 1);

		//Top face
		FaceVertices[20] = new int3(0, 1, 0);
		FaceVertices[21] = new int3(0, 1, 1);
		FaceVertices[22] = new int3(1, 1, 1);
		FaceVertices[23] = new int3(1, 1, 0);

		UvCoordinates = new NativeArray<float2>(6, Allocator.Persistent);

		UvCoordinates[0] = new float2(.5f, 0);
		UvCoordinates[1] = new float2(.5f, .5f);
		UvCoordinates[2] = new float2(.5f, .5f);
		UvCoordinates[3] = new float2(.5f, .5f);
		UvCoordinates[4] = new float2(.5f, .5f);
		UvCoordinates[5] = new float2(0, 0);

		FaceDirections = new NativeArray<int3>(6, Allocator.Persistent);

		FaceDirections[0] = new int3(0, -1, 0); // Down
		FaceDirections[1] = new int3(0, 0, 1);  // Forward
		FaceDirections[2] = new int3(0, 0, -1); // Back
		FaceDirections[3] = new int3(-1, 0, 0); // Left
		FaceDirections[4] = new int3(1, 0, 0);  // Right
		FaceDirections[5] = new int3(0, 1, 0);  // Up
	}

	public static void Dispose()
	{
		FaceVertices.Dispose();
		FaceDirections.Dispose();
		UvCoordinates.Dispose();
	}
}
