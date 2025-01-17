using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ChunkGenerator))]

public class MeshEditor : Editor
{
	public override void OnInspectorGUI()
	{
		ChunkGenerator meshGenerator = (ChunkGenerator)target;

		DrawDefaultInspector();

		if (GUILayout.Button("Generate"))
		{
			if (Application.isPlaying)
			{
				meshGenerator.Initialize();
				meshGenerator.CreateChunkObject(Vector2Int.zero, 1, false, false);
				meshGenerator.CreateChunkObject(Vector2Int.zero, 0, false, false);
			}
			else
			{
				meshGenerator.Initialize();
				meshGenerator.CreateChunkObject(Vector2Int.zero, 0, true, true);
				meshGenerator.Dispose();
			}

		}
	}
}