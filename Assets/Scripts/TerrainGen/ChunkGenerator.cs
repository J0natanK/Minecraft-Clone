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
	public static Noise noise;
	public static bool loadedTerrain;
	public static NativeArray<int3> faceVertices;
	public static NativeArray<int3> faceDirections;
	public static NativeArray<float2> uvCoordinates;
	public static NativeHashMap<int2, NativeArray<int>> ChunkDataMap;

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
			loadedTerrain = true;
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

		RequestMesh(landMesh, waterMesh, offset, lod, instantCompletion, logTime);

		chunk.AddComponent<MeshFilter>().mesh = landMesh;

		//Water is seperate object so it can have transparency
		GameObject water = new GameObject("Water");
		water.transform.parent = chunk.transform;
		water.AddComponent<MeshRenderer>().sharedMaterial = waterMaterial;
		water.AddComponent<MeshFilter>().mesh = waterMesh;

		return chunk;
	}

	public void RequestMesh(Mesh landMesh, Mesh waterMesh, Vector2Int offset, int levelOfDetail, bool instantCompletion = false, bool logTime = false, NativeArray<int> customVoxelValues = default)
	{
		levelOfDetail = levelOfDetail == 0 ? 1 : levelOfDetail * 2;

		if (instantCompletion)
		{
			var jobData = new JobData
			{
				landMesh = landMesh,
				waterMesh = waterMesh,
				noiseOffset = offset,
				lod = levelOfDetail,
				customVoxelValues = customVoxelValues,
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
				lod = levelOfDetail,
				customVoxelValues = customVoxelValues,
				logTime = logTime
			});
		}
	}

	public void Initialize()
	{
		ChunkDataMap = new NativeHashMap<int2, NativeArray<int>>(1024, Allocator.Persistent);
		jobList = new();
		jobQueue = new();

		noise = new Noise(properties);

		faceVertices = new NativeArray<int3>(24, Allocator.Persistent);

		//Bottom face
		faceVertices[0] = new int3(1, 0, 0);
		faceVertices[1] = new int3(1, 0, 1);
		faceVertices[2] = new int3(0, 0, 1);
		faceVertices[3] = new int3(0, 0, 0);

		//Front face
		faceVertices[4] = new int3(1, 0, 1);
		faceVertices[5] = new int3(1, 1, 1);
		faceVertices[6] = new int3(0, 1, 1);
		faceVertices[7] = new int3(0, 0, 1);

		//Back face
		faceVertices[8] = new int3(0, 0, 0);
		faceVertices[9] = new int3(0, 1, 0);
		faceVertices[10] = new int3(1, 1, 0);
		faceVertices[11] = new int3(1, 0, 0);

		//Left face
		faceVertices[12] = new int3(0, 0, 1);
		faceVertices[13] = new int3(0, 1, 1);
		faceVertices[14] = new int3(0, 1, 0);
		faceVertices[15] = new int3(0, 0, 0);

		//Right face
		faceVertices[16] = new int3(1, 0, 0);
		faceVertices[17] = new int3(1, 1, 0);
		faceVertices[18] = new int3(1, 1, 1);
		faceVertices[19] = new int3(1, 0, 1);

		//Top face
		faceVertices[20] = new int3(0, 1, 0);
		faceVertices[21] = new int3(0, 1, 1);
		faceVertices[22] = new int3(1, 1, 1);
		faceVertices[23] = new int3(1, 1, 0);

		uvCoordinates = new NativeArray<float2>(6, Allocator.Persistent);

		uvCoordinates[0] = new float2(.5f, 0);
		uvCoordinates[1] = new float2(.5f, .5f);
		uvCoordinates[2] = new float2(.5f, .5f);
		uvCoordinates[3] = new float2(.5f, .5f);
		uvCoordinates[4] = new float2(.5f, .5f);
		uvCoordinates[5] = new float2(0, 0);

		faceDirections = new NativeArray<int3>(6, Allocator.Persistent);

		faceDirections[0] = new int3(0, -1, 0); // Down
		faceDirections[1] = new int3(0, 0, 1);  // Forward
		faceDirections[2] = new int3(0, 0, -1); // Back
		faceDirections[3] = new int3(-1, 0, 0); // Left
		faceDirections[4] = new int3(1, 0, 0);  // Right
		faceDirections[5] = new int3(0, 1, 0);  // Up
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

		foreach (var chunk in ChunkDataMap)
		{
			chunk.Value.Dispose();
		}
		ChunkDataMap.Dispose();

		faceVertices.Dispose();
		faceDirections.Dispose();
		uvCoordinates.Dispose();
		noise.Dispose();
	}
}