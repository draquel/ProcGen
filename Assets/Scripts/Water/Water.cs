using UnityEditor;
using UnityEngine;

public class Water : MonoBehaviour
{
    public Material material;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    public bool isInit = false;
    public int meshSize = 20;

    public Transform followTarget;
    
    private void Start()
    {
        Init();
        CreateMesh();
    }

    private void Update()
    {
        SetPos();
    }

    public void Init()
    {
        if(isInit){ return; }
        _meshFilter = gameObject.AddComponent<MeshFilter>();
        _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        _meshRenderer.material = new Material(material);
        isInit = true;
    }

    public void SetPos()
    {
        if(followTarget)
            transform.position = new Vector3(followTarget.position.x, transform.position.y, followTarget.position.z);
            material.SetVector("_playerpos",new Vector2(followTarget.position.x,followTarget.position.z));
    }

    public void Reset()
    {
        if (Application.isPlaying) {
            Destroy(gameObject.GetComponent<MeshFilter>()); 
            Destroy(gameObject.GetComponent<Renderer>()); 
        } else {
            Undo.DestroyObjectImmediate(gameObject.GetComponent<Renderer>());
            Undo.DestroyObjectImmediate(gameObject.GetComponent<MeshFilter>());
        }

        isInit = false;
    }
    
    public void CreateMesh()
    {
        Vector3 pos = new Vector3(-meshSize/2f, 0, -meshSize/2f);
        _meshFilter.mesh = RectMeshGenerator.GenerateMeshData(pos, meshSize, meshSize, meshSize, meshSize).CreateMesh();
    }
}


[CustomEditor(typeof(Water))]
public class WaterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Water water = (Water)target;

        if (GUILayout.Button("Create Mesh"))
        {
            if(!water.isInit)
                water.Init();
            water.CreateMesh();
        }

        if (GUILayout.Button("Rest"))
        {
           water.Reset(); 
        }

    }
}