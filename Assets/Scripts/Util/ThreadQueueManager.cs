using UnityEngine;

public class ThreadQueueManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        NoiseMapGenerator.ProcessThreadQueue();
        PoissonDiscSampling.ProcessThreadQueue();
        RectMeshGenerator.ProcessThreadQueue();
        QuadTreeMeshGenerator.ProcessThreadQueue();
    }
}
