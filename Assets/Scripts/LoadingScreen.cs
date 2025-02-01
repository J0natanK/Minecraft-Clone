using UnityEngine;
using UnityEngine.UI;
using UnityTemplateProjects;

public class LoadingScreen : MonoBehaviour
{
	public Image loadingScreen;
	public SimpleCameraController player;

	new bool enabled;

	void Start()
	{
		player.enabled = false;
		player.transform.rotation = Quaternion.Euler(-90, 0, 0);
		loadingScreen.enabled = true;
		enabled = false;
	}
	void Update()
	{
		if (ChunkManager.LoadedTerrain && !enabled)
		{
			player.enabled = true;
			loadingScreen.enabled = false;
			player.transform.rotation = Quaternion.Euler(0, 0, 0);

			enabled = true;
		}
	}
}
