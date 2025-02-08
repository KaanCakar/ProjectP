using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ChromaticAberrationEffect : MonoBehaviour
{
    public Material chromaticMaterial;
    
    [Range(0f, 1f)]
    public float intensity = 1f;
    
    public Vector2 redOffset = new Vector2(0.01f, 0f);
    public Vector2 blueOffset = new Vector2(-0.01f, 0f);

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (chromaticMaterial != null)
        {
            chromaticMaterial.SetFloat("_Intensity", intensity);
            chromaticMaterial.SetVector("_RedOffset", redOffset);
            chromaticMaterial.SetVector("_BlueOffset", blueOffset);
            Graphics.Blit(source, destination, chromaticMaterial);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }
}