using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ChunkManager))]

public class MeshEditor : Editor
{
	public override void OnInspectorGUI()
	{
		ChunkManager meshGenerator = (ChunkManager)target;

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