using UnityEngine;

public static class RectMeshGenerator
{
    public static MeshData GenerateMeshData(Vector3 position, float width, float height, int widthSegments = 1, int heightSegments = 1)
    {
        MeshData meshData = new MeshData();
        
        float widthStep = width / widthSegments;
        float heightStep = height / heightSegments;

        for (int y = 0; y <= heightSegments; y++) {
            for (int x = 0; x <= widthSegments; x++) {
                meshData.AddVertex(new Vector3(position.x + x * widthStep, 0, position.z + y * heightStep));
            }
        }

        for (int y = 0; y < heightSegments; y++) {
            for (int x = 0; x < widthSegments; x++) {
                int topLeft = y * (widthSegments + 1) + x;
                int bottomLeft = (y + 1) * (widthSegments + 1) + x;
                int topRight = topLeft + 1;
                int bottomRight = bottomLeft + 1;

                meshData.AddTriangle(topLeft, bottomLeft, topRight);
                meshData.AddTriangle(topRight,bottomLeft,bottomRight);
            }
        }

        return meshData;
    }
    
    public static MeshData GenerateMeshData(Vector3 position, float width, float height, int widthSegments = 1, int heightSegments = 1, NoiseSettings noiseSettings = null, float heightmultiplier = 25)
    {
        MeshData meshData = new MeshData();
        
        float widthStep = width / widthSegments;
        float heightStep = height / heightSegments;

        for (int y = 0; y <= heightSegments; y++) {
            for (int x = 0; x <= widthSegments; x++)
            {
                Vector3 vert = new Vector3(position.x + x * widthStep, 0, position.z + y * heightStep);
                vert.y = Noise.Evaluate((vert + noiseSettings.offset) / width * noiseSettings.scale, noiseSettings) * heightmultiplier;
                meshData.AddVertex(vert);
            }
        }

        for (int y = 0; y < heightSegments; y++) {
            for (int x = 0; x < widthSegments; x++) {
                int topLeft = y * (widthSegments + 1) + x;
                int bottomLeft = (y + 1) * (widthSegments + 1) + x;
                int topRight = topLeft + 1;
                int bottomRight = bottomLeft + 1;

                meshData.AddTriangle(topLeft, bottomLeft, topRight);
                meshData.AddTriangle(topRight,bottomLeft,bottomRight);
            }
        }

        return meshData;
    }
    
    public static MeshData GenerateMeshData(Texture2D noise, Vector3 position, float width, float height, int widthSegments = 1, int heightSegments = 1, NoiseSettings noiseSettings = null, float heightmultiplier = 25)
    {
        MeshData meshData = new MeshData();
        
        float widthStep = width / widthSegments;
        float heightStep = height / heightSegments;

        for (int y = 0; y <= heightSegments; y++) {
            for (int x = 0; x <= widthSegments; x++)
            {
                Vector3 vert = new Vector3(position.x + x * widthStep, 0, position.z + y * heightStep);
                vert.y = noise.GetPixel((int)(x * widthStep), (int)(y * heightStep)).r * heightmultiplier;
                //vert.y = Noise.Evaluate((vert + noiseSettings.offset) / width * noiseSettings.scale, noiseSettings) * heightmultiplier;
                meshData.AddVertex(vert);
            }
        }

        for (int y = 0; y < heightSegments; y++) {
            for (int x = 0; x < widthSegments; x++) {
                int topLeft = y * (widthSegments + 1) + x;
                int bottomLeft = (y + 1) * (widthSegments + 1) + x;
                int topRight = topLeft + 1;
                int bottomRight = bottomLeft + 1;

                meshData.AddTriangle(topLeft, bottomLeft, topRight);
                meshData.AddTriangle(topRight,bottomLeft,bottomRight);
            }
        }

        return meshData;
    }
}

