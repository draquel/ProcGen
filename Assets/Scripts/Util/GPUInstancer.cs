using System.Collections.Generic;
using UnityEngine;

public class GPUInstancer : MonoBehaviour
{
    public int Instances;

    public Mesh mesh;

    public Material[] Materials;

    private List<List<Matrix4x4>> Batches = new List<List<Matrix4x4>>();

    private void RenderBatches()
    {
        foreach (var batch in Batches)
        {
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                Graphics.DrawMeshInstanced(mesh, i, Materials[i], batch);
            }
        }
    }

    public void Update()
    {
        RenderBatches();
    }

    private void Start()
    {
        int addedMatricies = 0;

        
        Batches.Add(new List<Matrix4x4>());
        for (int i = 0; i < Instances; i++)
        {
            if (addedMatricies < 1000)
            {
                Batches[Batches.Count - 1]
                    .Add(Matrix4x4.TRS(new Vector3(Random.Range(0, 50), Random.Range(0, 50), Random.Range(0, 50)),
                        Random.rotation,
                        new Vector3(x: Random.Range(1, 3), y: Random.Range(1, 3), z: Random.Range(1, 3))));
                addedMatricies += 1;
            } else {
                Batches.Add(new List<Matrix4x4>());
                addedMatricies = 0;
            }
        }
    }
}
