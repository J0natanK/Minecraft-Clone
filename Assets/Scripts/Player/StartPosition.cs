using System.Collections;
using UnityEngine;
using UnityTemplateProjects;

public class StartPosition : MonoBehaviour
{
	public SimpleCameraController player;

	void Start()
	{
		StartCoroutine(WaitForTerrain());
	}

	IEnumerator WaitForTerrain()
	{
		while (true)
		{
			if (ChunkManager.LoadedTerrain)
			{
				player.enabled = false;
				player.transform.position = FindStartPosition();
				player.enabled = true;
				break;
			}

			yield return null;
		}
	}

	Vector3 FindStartPosition()
	{
		Ray ray = new(new Vector3(0, 1000, 0), Vector3.down);
		Physics.Raycast(ray, out RaycastHit hit);

		return hit.point + new Vector3(0, 10, 0);
	}
}
