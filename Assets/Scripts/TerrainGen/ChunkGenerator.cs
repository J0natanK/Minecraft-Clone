using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Rendering;

public class ChunkGenerator : MonoBehaviour
{
	public Material terrainMaterial;
	public Material waterMaterial;
	public TerrainProperties properties;

	[Header("Performance")]
	public int maxJobsPerFrame;

	public static readonly Vector2Int ChunkDimensions = new Vector2Int(32, 200);
	public static Noise Noise;
	public static bool LoadedTerrain;
	public static NativeArray<int3> FaceVertices;
	public static NativeArray<int3> FaceDirections;
	public static NativeArray<float2> UvCoordinates;
	public static NativeHashMap<int2, NativeArray<int>> VoxelGridMap;

	List<JobData> jobList;
	List<JobData> jobQueue;

	void Start()
	{
		Initialize();
	}

	void Update()
	{
		for (int i = 0; i < jobList.Count; i++)
		{
			JobData jobData = jobList[i];
			if (jobData.scheduled && jobData.handle.IsCompleted)
			{
				jobData.Complete();
				jobList.RemoveAt(i);
			}
			if (!jobData.scheduled)
			{
				jobData.CreateAndSchedule();
			}
		}

		if (jobList.Count < maxJobsPerFrame && jobQueue.Count > 0)
		{
			int length = Mathf.Min(maxJobsPerFrame, jobQueue.Count);

			for (int i = 0; i < length; i++)
			{
				jobList.Add(jobQueue[i]);
			}

			jobQueue.RemoveRange(0, length);
		}
	}

	void LateUpdate()
	{
		if (jobQueue.Count == 0)
		{
			LoadedTerrain = true;
		}
	}

	private void OnDestroy()
	{
		Dispose();
	}

	public GameObject CreateChunkObject(Vector2Int offset, int lod, bool instantCompletion, bool logTime)
	{
		GameObject chunk = new GameObject(transform.childCount.ToString());
		var meshRenderer = chunk.AddComponent<MeshRenderer>();
		meshRenderer.sharedMaterial = terrainMaterial;

		Mesh landMesh = new Mesh
		{
			indexFormat = IndexFormat.UInt32
		};
		Mesh waterMesh = new Mesh
		{
			indexFormat = IndexFormat.UInt32
		};

		RequestMesh(landMesh, waterMesh, offset, instantCompletion, logTime);

		chunk.AddComponent<MeshFilter>().mesh = landMesh;

		//Water is seperate object so it can have transparency
		GameObject water = new GameObject("Water");
		water.transform.parent = chunk.transform;
		water.AddComponent<MeshRenderer>().sharedMaterial = waterMaterial;
		water.AddComponent<MeshFilter>().mesh = waterMesh;

		return chunk;
	}

	public void RequestMesh(Mesh landMesh, Mesh waterMesh, Vector2Int offset, bool instantCompletion = false, bool logTime = false, NativeArray<int> customVoxelValues = default)
	{
		if (instantCompletion)
		{
			var jobData = new JobData
			{
				landMesh = landMesh,
				waterMesh = waterMesh,
				noiseOffset = offset,
				customVoxelGrid = customVoxelValues,
				logTime = logTime
			};

			jobData.CreateAndSchedule();
			jobData.Complete();
		}
		else
		{
			jobQueue.Add(new JobData
			{
				landMesh = landMesh,
				waterMesh = waterMesh,
				noiseOffset = offset,
				customVoxelGrid = customVoxelValues,
				logTime = logTime
			});
		}
	}

	public void Initialize()
	{
		VoxelGridMap = new NativeHashMap<int2, NativeArray<int>>(1024, Allocator.Persistent);
		jobList = new();
		jobQueue = new();

		Noise = new Noise(properties);

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

	public void Dispose()
	{
		foreach (var job in jobList)
		{
			if (job.scheduled)
			{
				job.Complete();
			}
		}

		foreach (var chunk in VoxelGridMap)
		{
			chunk.Value.Dispose();
		}
		VoxelGridMap.Dispose();

		FaceVertices.Dispose();
		FaceDirections.Dispose();
		UvCoordinates.Dispose();
		Noise.Dispose();
	}
}