using Unity.Collections;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Rendering;
using System.Data;

public class BlockBreakingAndPlacing : MonoBehaviour
{
    public float maxReach;
    public LayerMask mask;

    private RaycastHit hit;
    private Camera cam;
    private ChunkGenerator chunkGen;
    private GameObject chunkGenObj;
    private int2 chunkDimensions;

    private int2 coordinates;
    private int childIndex;

    private int itemIndex;

    private void Start()
    {
        cam = GameObject.Find("Camera").GetComponent<Camera>();
        chunkGenObj = GameObject.Find("ChunkGenerator");
        chunkGen = chunkGenObj.GetComponent<ChunkGenerator>();
        chunkDimensions = new int2(ChunkGenerator.ChunkDimensions.x, ChunkGenerator.ChunkDimensions.y);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleBlockInteraction(false); // Left-click for breaking blocks
        }

        if (Input.GetMouseButtonDown(1))
        {
            HandleBlockInteraction(true); // Right-click for placing blocks
        }

        //itemIndex = (itemIndex + (int)Input.mouseScrollDelta.y + 8) % 8;


        itemIndex += (int)Input.mouseScrollDelta.y;

        itemIndex = (itemIndex > 7) ? 1 : (itemIndex < 1) ? 7 : itemIndex;

        if (itemIndex == 5)
        {
            itemIndex = Input.mouseScrollDelta.y == 1 ? 6 : 4;
        }
    }

    private void HandleBlockInteraction(bool placeBlock)
    {
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, maxReach, mask))
        {
            coordinates = new int2((int)hit.collider.transform.position.x + (ChunkGenerator.ChunkDimensions.x / 2), (int)hit.collider.transform.position.z + (ChunkGenerator.ChunkDimensions.x / 2));
            NativeArray<int> voxelValues = ChunkGenerator.ChunkDataMap[coordinates];

            int x = Mathf.Abs((int)(hit.collider.transform.position.x - Mathf.Round(hit.point.x - 0.5f)));
            int y = placeBlock ? Mathf.Abs(Mathf.RoundToInt(hit.point.y)) : Mathf.Abs(Mathf.RoundToInt(hit.point.y)) - 1;
            int z = Mathf.Abs((int)(hit.collider.transform.position.z - Mathf.Round(hit.point.z - 0.5f)));

            Debug.Log(x);
            Debug.Log(y);
            Debug.Log(z);

            voxelValues[Utils.VoxelIndex(x, y, z)] = placeBlock ? itemIndex : 0;

            Mesh landMesh = new Mesh();
            Mesh waterMesh = new Mesh();

            UpdateChunkMesh(coordinates, hit.collider.gameObject, landMesh, waterMesh, voxelValues);

            childIndex = int.Parse(hit.collider.name);

            if (x == 0)
                UpdateNeighbouringChunk("Left");
            if (x == chunkDimensions.x - 1)
                UpdateNeighbouringChunk("Right");
            if (z == 0)
                UpdateNeighbouringChunk("Forward");
            if (z == chunkDimensions.x - 1)
                UpdateNeighbouringChunk("Back");
        }
    }

    private void UpdateChunkMesh(int2 coordinates, GameObject obj, Mesh landMesh, Mesh waterMesh, NativeArray<int> voxelValues = default)
    {
        landMesh.indexFormat = IndexFormat.UInt32;
        waterMesh.indexFormat = IndexFormat.UInt32;

        //chunkGen.RequestMesh(landMesh, waterMesh, new Vector2Int(coordinates.x, coordinates.y), true, voxelValues);

        var hitCollider = obj.GetComponent<MeshCollider>();
        var hitMeshFilter = obj.GetComponent<MeshFilter>();
        var waterMeshFilter = obj.transform.GetChild(0).GetComponent<MeshFilter>();

        hitMeshFilter.mesh = landMesh;
        waterMeshFilter.mesh = waterMesh;

        // Reset collider
        hitCollider.sharedMesh = null;
        hitCollider.sharedMesh = landMesh;
    }

    private void UpdateNeighbouringChunk(string direction)
    {
        int2 coordOffset = new int2();
        int childOffset = 0;

        switch (direction)
        {
            case "Right":
                coordOffset = new int2(chunkDimensions.x, 0);
                childOffset = 1;
                break;
            case "Left":
                coordOffset = new int2(-chunkDimensions.x, 0);
                childOffset = -1;
                break;
            case "Forward":
                coordOffset = new int2(0, chunkDimensions.x);
                childOffset = 39;
                break;
            case "Back":
                coordOffset = new int2(0, -chunkDimensions.x);
                childOffset = -39;
                break;
        }

        Mesh landMesh = new Mesh();
        Mesh waterMesh = new Mesh();

        Transform chunk = chunkGenObj.transform.GetChild(childIndex + childOffset);

        UpdateChunkMesh(coordinates + coordOffset, chunk.gameObject, landMesh, waterMesh);

        chunk.GetComponent<MeshFilter>().mesh = landMesh;
        chunk.transform.GetChild(0).GetComponent<MeshFilter>().mesh = waterMesh;
    }
}
