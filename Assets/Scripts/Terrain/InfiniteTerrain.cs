using System.Collections.Generic;
using UnityEngine;

public class InfiniteTerrain : MonoBehaviour
{
   Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
   List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

   [Header("Chunk")]
   public GameObject chunkPrefab;
   public int chunkRenderDistance = 3;
   
   [Header("Camera")]
   public Camera mainCam;
   public Transform viewer;
   private Vector3 viewerLastPos;
   private Quaternion viewerLastRot;
   public bool enableChunkOcclusion = false;

   [Header("Settings")]
   public TerrainChunkSettings chunkSettings;
   
   public bool autoUpdate; 
   
   private void Start()
   {
      setPlayerPos();
      UpdateChunks();
   }

   private void Update()
   {
      if ((!chunkSettings.quadTreeSettings.enableOcclusion && !enableChunkOcclusion && viewerDistanceCheck()) ||
          ((chunkSettings.quadTreeSettings.enableOcclusion || enableChunkOcclusion) && (viewerRotationCheck() || viewerDistanceCheck())))
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
      
      chunkSettings.quadTreeSettings.viewerPosition = new Vector2(viewerLastPos.x, viewerLastPos.z);
      chunkSettings.quadTreeSettings.viewerForward = t.forward;
   }

   public bool viewerDistanceCheck() {
      return Vector3.Distance(viewerLastPos,viewer.transform.position) >
             chunkSettings.quadTreeSettings.minSize * (chunkSettings.quadTreeSettings.distanceModifier == 1 ? 1 : chunkSettings.quadTreeSettings.distanceModifier-1);
   }

   public bool viewerRotationCheck()
   {
      float deg = Quaternion.Angle(viewerLastRot, viewer.transform.rotation);
      return deg > 5f ;
   }

   public TerrainChunk CreateChunk(Vector2 chunkCoord)
   {
      GameObject chunkGO = Instantiate(chunkPrefab, transform);
      TerrainChunk chunk = chunkGO.GetComponent<TerrainChunk>();
      chunk.Init(chunkCoord,chunkSettings);
      return chunk;
   }

   public void UpdateChunks()
   {
      foreach (TerrainChunk item in terrainChunksVisibleLastUpdate) { item.setVisibility(false); }
      terrainChunksVisibleLastUpdate.Clear();
      
      int currentChunkCoordX = Mathf.FloorToInt(viewer.transform.position.x / chunkSettings.size);
      int currentChunkCoordZ = Mathf.FloorToInt(viewer.transform.position.z / chunkSettings.size);

      for (int zOffset = -chunkRenderDistance; zOffset < chunkRenderDistance; zOffset++) {
         for (int xOffset = -chunkRenderDistance; xOffset < chunkRenderDistance; xOffset++) {
            Vector2 chunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordZ + zOffset);
            if (terrainChunkDictionary.ContainsKey(chunkCoord)) {
               if (!enableChunkOcclusion || (enableChunkOcclusion && terrainChunkDictionary[chunkCoord].VisibilityCheck())) {
                  terrainChunkDictionary[chunkCoord].UpdateChunk();
                  terrainChunkDictionary[chunkCoord].setVisibility(true);
                  terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[chunkCoord]);
               }
            } else {
               terrainChunkDictionary.Add(chunkCoord,CreateChunk(chunkCoord));
               if (!enableChunkOcclusion || (enableChunkOcclusion && terrainChunkDictionary[chunkCoord].VisibilityCheck())) {
                  terrainChunkDictionary[chunkCoord].UpdateChunk();
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
