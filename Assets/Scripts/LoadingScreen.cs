using UnityEngine;
using UnityEngine.UI;
using UnityTemplateProjects;

public class LoadingScreen : MonoBehaviour
{
	public Image loadingScreen;
	public SimpleCameraController player;

	void Start()
	{
		player.enabled = false;
		player.transform.rotation = Quaternion.Euler(-90, 0, 0);
		loadingScreen.enabled = true;
	}
	void Update()
	{
		if (ChunkGenerator.LoadedTerrain)
		{
			player.enabled = true;
			loadingScreen.enabled = false;
			player.transform.rotation = Quaternion.Euler(0, 0, 0);
		}
	}
}
