using System.Collections;
using System.Collections.Generic;
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
        QuadTreeMeshGenerator.ProcessThreadQueue();
    }
}
