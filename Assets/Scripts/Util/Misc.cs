using UnityEngine;
public static class Misc
{
    public static Texture2D RenderTextureToTexture2D(RenderTexture rt)
    {
        Texture2D texture = new Texture2D(rt.width, rt.height, TextureFormat.RGBAFloat, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
    
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = rt;
    
        texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        texture.Apply();
    
        RenderTexture.active = currentRT;
    
        return texture;
    } 
}