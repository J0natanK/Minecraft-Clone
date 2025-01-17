using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterAnimation : MonoBehaviour
{
    public Material waterMaterial;
    public Texture2D waterTextureAtlas;

    public float animationUpdateTime;

    Color[] pixels;
    Texture2D[] textures;

    int textureIndex;

    private void Start()
    {
        pixels = waterTextureAtlas.GetPixels();
        textures = new Texture2D[32];

        for (int i = 0; i < 32; i++)
        {
            Color[] colors = new Color[16 * 16];

            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    colors[GetIndex(x, y)] = pixels[GetIndex(x, y + (i * 16))];
                }
            }

            Texture2D tex = new Texture2D(16, 16);
            tex.filterMode = FilterMode.Point;
            tex.SetPixels(colors);
            tex.Apply();
            textures[i] = tex;
        }      

        InvokeRepeating("UpdateTexture", 0, animationUpdateTime);
    }

    void UpdateTexture()
    {
        waterMaterial.mainTexture = textures[textureIndex];
        textureIndex = textureIndex == textures.Length - 1 ? 0 : textureIndex + 1;
    }

    int GetIndex(int x, int y)
    {
        return x + (y * 16);
    }
}
