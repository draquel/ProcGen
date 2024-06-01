using System.Collections.Generic;
using UnityEngine;

public class InfiniteTerrain : MonoBehaviour
{
   Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
   List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

   public GameObject TerrainChunkPrefab;
   public Material material;

   public int chunkSize = 64;
   public int chunkRenderDistance = 3;

   public Camera mainCam;
   public Transform viewer;
   private Vector3 viewerLastPos;
   private Quaternion viewerLastRot;


   public ComputeShader noiseComputeShader;
   public NoiseSettings noiseSettings;
   public QuadTreeSettings quadTreeSettings;

   public bool enableChunkOcclusion = false;
   public bool enableCaching = false;
   public bool autoUpdate;
   private void Start()
   {
      setPlayerPos();
      UpdateChunks();
   }

   private void FixedUpdate()
   {
      if ((!quadTreeSettings.enableOcclusion && !enableChunkOcclusion && viewerDistanceCheck()) ||
          ((quadTreeSettings.enableOcclusion || enableChunkOcclusion) && (viewerRotationCheck() || viewerDistanceCheck())))
      {
         setPlayerPos();
         UpdateChunks();
      }
   }

   public void setPlayerPos()
   {
      Transform t = viewer.transform;
      viewerLastPos = t.position;
      viewerLastRot = t.rotation;
      
      quadTreeSettings.viewerPosition = new Vector2(viewerLastPos.x, viewerLastPos.z);
      quadTreeSettings.viewerForward = t.forward;
   }

   public bool viewerDistanceCheck() {
      return Vector3.Distance(viewerLastPos,viewer.transform.position) >
             quadTreeSettings.minSize * (quadTreeSettings.distanceModifier == 1 ? 1 : quadTreeSettings.distanceModifier-1);
   }

   public bool viewerRotationCheck()
   {
      float deg = Quaternion.Angle(viewerLastRot, viewer.transform.rotation);
      return deg > 5f ;
   }

   public TerrainChunk CreateChunk(Vector2 chunkCoord)
   {
      GameObject chunkGO = Instantiate(TerrainChunkPrefab, transform);
      TerrainChunk chunk = chunkGO.GetComponent<TerrainChunk>();
      chunk.Init(chunkCoord,chunkSize,material,quadTreeSettings);
      return chunk;
   }

   public void UpdateChunks()
   {
      foreach (TerrainChunk item in terrainChunksVisibleLastUpdate) { item.setVisibility(false); }
      terrainChunksVisibleLastUpdate.Clear();
      
      int currentChunkCoordX = Mathf.FloorToInt(viewer.transform.position.x / chunkSize);
      int currentChunkCoordZ = Mathf.FloorToInt(viewer.transform.position.z / chunkSize);

      for (int zOffset = -chunkRenderDistance; zOffset < chunkRenderDistance; zOffset++) {
         for (int xOffset = -chunkRenderDistance; xOffset < chunkRenderDistance; xOffset++) {
            Vector2 chunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordZ + zOffset);
            if (terrainChunkDictionary.ContainsKey(chunkCoord)) {
               if (!enableChunkOcclusion || (enableChunkOcclusion && terrainChunkDictionary[chunkCoord].isVisible())) {
                  terrainChunkDictionary[chunkCoord].UpdateChunk(noiseComputeShader, noiseSettings);
                  terrainChunkDictionary[chunkCoord].setVisibility(true);
                  terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[chunkCoord]);
               }
            } else {
               terrainChunkDictionary.Add(chunkCoord,CreateChunk(chunkCoord));
               if (!enableChunkOcclusion || (enableChunkOcclusion && terrainChunkDictionary[chunkCoord].isVisible())) {
                  terrainChunkDictionary[chunkCoord].UpdateChunk(noiseComputeShader, noiseSettings);
                  terrainChunkDictionary[chunkCoord].setVisibility(true);
                  terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[chunkCoord]);
               }
            }
         }
      }
   }

   public void ClearChunks()
   {
      var children = new List<GameObject>();
      foreach (Transform child in transform) children.Add(child.gameObject);
      if (Application.isPlaying) {
         children.ForEach(child => Destroy(child));
      } else {
         children.ForEach(child => DestroyImmediate(child)); 
      }

      terrainChunkDictionary.Clear();
      terrainChunksVisibleLastUpdate.Clear();
   }

   public void OnDrawGizmos()
   {
      foreach (var item in terrainChunkDictionary)
      {
         item.Value.DrawChunkBorder();
         item.Value.DrawPositionMarker();
      }
   }
}
