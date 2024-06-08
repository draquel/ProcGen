using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public struct Direction {
    public const int East = 0, West = 1, North = 2, South = 3;
    /// <summary> East = 0, West = 1, North = 2, South = 3 </summary>
    public const int E = 0, W = 1, N = 2, S = 3;
}

public struct Quadrant {
    public const int NorthWest = 0, NorthEast = 1, SouthEast = 2, SouthWest = 3;
    /// <summary> North West = 0, North East = 1, South East = 2, South West = 3 </summary>
    public const int NW = 0, NE = 1, SE = 2, SW = 3;
}

public class QuadTree
{
    public QuadTreeNode rootNode;
    public List<QuadTreeNode> leaves;

    public Vector3 position;
    public Vector3 center;
    public Vector3 size;
    public QuadTreeSettings settings;

    public QuadTree(Vector3 position, Vector3 size, QuadTreeSettings settings)
    {
        size.z = size.x;
        this.size = size;
        this.position = position;
        leaves = new List<QuadTreeNode>();
        this.settings = settings;
        
        center = position + (size / 2);
        center.y -= settings.heightMultiplier/2;
    }

    public void GenerateTree()
    {
        leaves.Clear();
        rootNode = new QuadTreeNode(center, size, settings);
        rootNode.GenerateNode(leaves);
        foreach (QuadTreeNode leaf in leaves) {
            leaf.CheckNeighbors(); 
        }
    }

    public int GetDepth()
    {
        int depth = 0;
        foreach (QuadTreeNode leaf in leaves)
        {
            if(depth < leaf.depth)
                depth = leaf.depth;
        }
        return depth;
    }

    public void DrawLeafBounds()
    {
        foreach (QuadTreeNode leaf in leaves) {
            leaf.DrawBounds();
        }
    }

    public NativeArray<QuadTreeLeaf> LeavesStruct()
    {
        NativeArray<QuadTreeLeaf> leavesNA = new NativeArray<QuadTreeLeaf>();
        
        for(int i = 0; i < leaves.Count; i++)
        {
            leavesNA[i] = new QuadTreeLeaf(leaves[i].center, leaves[i].size, leaves[i].NeighborsStruct());
        }

        return leavesNA;
    }
}

public class QuadTreeNode
{
    public Bounds bounds;
    public Vector3 center;
    public Vector3 size;
    
    private uint hash;
    public int depth;
    public int corner;
    private int maxDepth;
    private int minSize;

    private QuadTreeNode rootNode;
    public bool[] neighbors;
    private QuadTreeNode[] children; 

    private QuadTreeSettings settings;

    //private Camera cam;
    private Plane[] planes;

    public QuadTreeNode(Vector3 center, Vector3 size, QuadTreeSettings settings, QuadTreeNode rootNode = null, uint hash = 1, int depth = 0, int corner = 0)
    {
        children = null;
        this.rootNode = rootNode == null && depth == 0 ? this : rootNode;
        this.hash = hash;
        this.center = center;
        this.size = size;
        this.depth = depth;
        this.corner = corner;
        maxDepth = settings.maxDepth;
        minSize = settings.minSize;
        bounds = new Bounds(this.center,this.size);
        this.settings = settings;
        
        planes = GeometryUtility.CalculateFrustumPlanes(this.settings.camera);
    }

    public void GenerateNode(List<QuadTreeNode> leaves)
    {
        if (NodeCheck() && DistanceCheck()) {
            Split();
            GenerateChildren(leaves);
        } else {
            if (!settings.enableOcclusion || (settings.enableOcclusion && VisibilityCheck()))
                leaves.Add(this);
        }
    }

    private void GenerateChildren(List<QuadTreeNode> leaves)
    {
        foreach (QuadTreeNode node in children) {
            node.GenerateNode(leaves);
        }
    }

    public bool VisibilityCheck()
    {
        Plane[] planes = FrustrumUtility.ScalePlanes(this.planes,1.2f);
        planes = FrustrumUtility.MovePlanes(planes,settings.minSize*settings.minSize*settings.distanceModifier,-settings.viewerForward);
        return FrustrumUtility.IsPartiallyInFrustum(planes, bounds);
    }

    public bool DistanceCheck()
    {
        float dist = Vector2.Distance(settings.viewerPosition, new Vector2(center.x,center.z));
        if (dist < size.x * settings.distanceModifier) {
            return true;
        }
        return false;
    }
    
    public bool NodeCheck()
    {
        if (size.x / 2f >= minSize && depth < maxDepth) {
            return true;
        }
        return false;
    }
    
    public void Split()
    {
        Vector3 halfSize = new Vector3(size.x * 0.5f, size.y, size.z * 0.5f);
        Vector3 qtrSize = new Vector3(halfSize.x * 0.5f, halfSize.y, halfSize.z * 0.5f);
        children = new QuadTreeNode[4];

        children[0] = new QuadTreeNode(center + new Vector3(-qtrSize.x, 0, qtrSize.z), halfSize, settings, rootNode, hash * 4, depth + 1,Quadrant.NW);
        children[1] = new QuadTreeNode(center + new Vector3(qtrSize.x, 0, qtrSize.z), halfSize, settings, rootNode, hash * 4 + 1, depth + 1,Quadrant.NE);
        children[2] = new QuadTreeNode(center + new Vector3(qtrSize.x, 0, -qtrSize.z), halfSize, settings, rootNode, hash * 4 + 2, depth + 1,Quadrant.SE);
        children[3] = new QuadTreeNode(center + new Vector3(-qtrSize.x, 0, -qtrSize.z), halfSize, settings, rootNode, hash * 4 + 3, depth + 1,Quadrant.SW);
    }

    public Neighbors NeighborsStruct()
    {
        return new Neighbors(neighbors[Direction.North],neighbors[Direction.South],neighbors[Direction.East],neighbors[Direction.West]);
    }
    
    public void CheckNeighbors() 
    {
        neighbors = new bool[4];
        
        switch (corner) {
            case 0: //nw
                neighbors[Direction.West] = CheckNeighborDepth(Direction.West,hash);
                neighbors[Direction.North] = CheckNeighborDepth(Direction.North,hash);
                break;
            case 1: //ne
                neighbors[Direction.East] = CheckNeighborDepth(Direction.East,hash);
                neighbors[Direction.North] = CheckNeighborDepth(Direction.North,hash);
                break;
            case 2: //se
                neighbors[Direction.East] = CheckNeighborDepth(Direction.East,hash);
                neighbors[Direction.South] = CheckNeighborDepth(Direction.South,hash);
                break;
            case 3: //sw
                neighbors[Direction.West] = CheckNeighborDepth(Direction.West,hash);
                neighbors[Direction.South] = CheckNeighborDepth(Direction.South,hash);
                break;
        }
    }

    private bool CheckNeighborDepth(int direction, uint hash)
    {
        uint bitmask = 0;
        byte count = 0;
        uint localQuadrant;

        while (count < depth) {
            count += 1;
            localQuadrant = (hash & 3);
            bitmask *= 4;

            if (direction == 2 || direction == 3) { bitmask += 3; } 
            else { bitmask += 1; }
            
            if ((direction == Direction.E && (localQuadrant == Quadrant.NW || localQuadrant == Quadrant.SW)) ||
                (direction == Direction.W && (localQuadrant == Quadrant.NE || localQuadrant == Quadrant.SE)) ||
                (direction == Direction.N && (localQuadrant == Quadrant.SW || localQuadrant == Quadrant.SE)) ||
                (direction == Direction.S && (localQuadrant == Quadrant.NW || localQuadrant == Quadrant.NE))) 
            { break; }

            hash >>= 2;
        }

        int nd = rootNode.GetNeighborDepth(this.hash ^ bitmask, depth);
        return  nd < depth || nd == 0;
    }

    public int GetNeighborDepth(uint queryHash, int targetDepth)
    {
        int res = 0;
        if (hash == queryHash) {
            res = depth;
        } else {
            if (children != null) {
                res += children[((queryHash >> ((targetDepth - 1) * 2)) & 3)].GetNeighborDepth(queryHash, (targetDepth - 1));
            } 
        }
        return res;
    }

    public void DrawBounds()
    {
        Gradient gradient = new Gradient();
        
        var colors = new GradientColorKey[2];
        colors[0] = new GradientColorKey(Color.blue, 0.0f);
        colors[1] = new GradientColorKey(Color.red, 1.0f);

        var alphas = new GradientAlphaKey[2];
        alphas[0] = new GradientAlphaKey(1.0f, 0.0f);
        alphas[1] = new GradientAlphaKey(0.0f, 1.0f);

        gradient.SetKeys(colors,alphas);
        float a = (float)depth / maxDepth;
        Color result = gradient.Evaluate(a);
        result.a = 0.3f;
       
        Gizmos.color = result;
        Gizmos.DrawWireCube(center,size);
    }
}

public struct QuadTreeLeaf
{
    private Vector3 center;
    private Vector3 size;
    private Neighbors neighbors;

    public QuadTreeLeaf(Vector3 center, Vector3 size, Neighbors neighbors)
    {
        this.center = center;
        this.size = size;
        this.neighbors = neighbors;
    }
}

public struct Neighbors
{
    public bool North;
    public bool South;
    public bool East;
    public bool West;

    public Neighbors(bool N, bool S, bool E, bool W)
    {
        North = N;
        South = S;
        East = E;
        West = W;
    }
    
}