using UnityEngine;

public class ThreadQueueManager : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        if (Time.frameCount % 2 == 0) {
            // NoiseMapGenerator.ProcessThreadQueue();
            NoiseTextureGenerator.ProcessThreadQueue();
            PoissonDiscSampling.ProcessThreadQueue(); 
        } else {
            RectMeshGenerator.ProcessThreadQueue();
            QuadTreeMeshGenerator.ProcessThreadQueue(); 
        }
    }
}
