using UnityEngine;

public class ImageEffect : MonoBehaviour
{
	void Start()
	{
		Camera cam = GetComponent<Camera>();
		cam.depthTextureMode = cam.depthTextureMode | DepthTextureMode.Depth;
	}
	public Material effectMaterial;

	void OnRenderImage(RenderTexture src, RenderTexture dst)
	{
		Graphics.Blit(src, dst, effectMaterial);
	}
}
