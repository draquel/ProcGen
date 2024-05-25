using System.Collections.Generic;
using UnityEngine;

public class InfiniteTerrain : MonoBehaviour
{
   Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
   List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

   public Material material;

   public int chunkSize = 64;
   public int chunkRenderDistance = 3;

   public Transform viewer;
   private Vector3 viewerLastPos;

   public NoiseSettings noiseSettings;
   public QuadTreeSettings quadTreeSettings;

   public bool autoUpdate;
   private void Start()
   {
      setPlayerPos();
      UpdateChunks();
   }

   private void Update()
   {
      if (viewerDistanceCheck())
      {
         setPlayerPos();
         UpdateChunks();
      }
   }

   public void setPlayerPos()
   {
      viewerLastPos = viewer.transform.position;
      quadTreeSettings.viewerPosition = new Vector2(viewerLastPos.x, viewerLastPos.z);
   }

   public bool viewerDistanceCheck()
   {
      return Vector3.Distance(viewerLastPos,viewer.transform.position) >
             quadTreeSettings.minSize * (quadTreeSettings.distanceModifier == 1 ? 1 : quadTreeSettings.distanceModifier-1);
   }

   public void UpdateChunks()
   {
      foreach (TerrainChunk item in terrainChunksVisibleLastUpdate) { item.setVisibility(false); }
      terrainChunksVisibleLastUpdate.Clear();
      
      int currentChunkCoordX = Mathf.RoundToInt(viewer.transform.position.x / chunkSize);
      int currentChunkCoordZ = Mathf.RoundToInt(viewer.transform.position.z / chunkSize);

      for (int zOffset = -chunkRenderDistance; zOffset < chunkRenderDistance; zOffset++) {
         for (int xOffset = -chunkRenderDistance; xOffset < chunkRenderDistance; xOffset++) {
            Vector2 chunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordZ + zOffset);
            if (terrainChunkDictionary.ContainsKey(chunkCoord)) {
               terrainChunkDictionary[chunkCoord].UpdateChunk(noiseSettings);
            } else
            {
               GameObject chunkGO = new GameObject();
               TerrainChunk chunk = chunkGO.AddComponent<TerrainChunk>();
               chunk.Init(chunkCoord,chunkSize,quadTreeSettings);

               terrainChunkDictionary.Add(chunkCoord,chunk);
               terrainChunkDictionary[chunkCoord].Build(gameObject.transform,material); 
               terrainChunkDictionary[chunkCoord].UpdateChunk(noiseSettings); 
            }
            terrainChunkDictionary[chunkCoord].setVisibility(true);
            terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[chunkCoord]);
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
         //item.Value.OnDrawGizmos();
         
         //Chunk Borders
         Vector3 gsize = item.Value.quadTree.size;
         gsize.y *= item.Value.quadTree.settings.heightMultiplier/2;

         Vector3 gcenter = item.Value.quadTree.center;
         gcenter.y *= item.Value.quadTree.settings.heightMultiplier/2; 
        
         Gizmos.color = Color.red;
         Gizmos.DrawWireCube(gcenter, gsize);
      }
   }
}
