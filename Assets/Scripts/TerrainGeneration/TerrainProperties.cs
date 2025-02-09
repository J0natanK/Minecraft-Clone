using UnityEngine;

[CreateAssetMenu(fileName = "New Terrain", menuName = "Terrain Properties")]
public class TerrainProperties : ScriptableObject
{
	public int surfaceLevel;
	public int seaLevel;

	public float sandDensity;

	public int maxTreeHeight;
	public float treeDensity;

	public int stoneDepth;

	public float persistance;
	public float frequency;
	public float lacunarity;
	public int numOctaves;
	public Vector2Int offset;
	public float noise3DContribution;
	public int noiseSeed;

	public float terrainHeight;
	public AnimationCurve heightCurve;
}
