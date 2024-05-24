using System.Collections.Generic;
using UnityEngine;

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
    }

    public void GenerateTree()
    {
        leaves.Clear();
        center = position + (size / 2);
        rootNode = new QuadTreeNode(center, size, settings);
        rootNode.GenerateNode(leaves);
    }
}

public class QuadTreeNode
{
    public Bounds bounds;
    public Vector3 center;
    public Vector3 size;
    
    private uint hash;
    private int depth;
    private int corner;
    private int maxDepth;
    private float minSize;

    private QuadTreeNode rootNode;
    public byte[] neighbors;
    private QuadTreeNode[] children; // 0:NW, 1:NE, 2:SW, 3:SE

    private QuadTreeSettings settings;
    
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
        bounds = new Bounds(this.center,size);
        this.settings = settings;
    }

    public void GenerateNode(List<QuadTreeNode> leaves)
    {
        if (NodeCheck() && DistanceCheck()) {
            Split();
            foreach (QuadTreeNode node in children) {
                node.GenerateNode(leaves);
            }
        } else {
            CheckNeighbors();
            leaves.Add(this);
        }
    }

    public bool DistanceCheck()
    {
        float dist = Vector2.Distance(settings.viewerPosition, new Vector2(center.x,center.z));
        if (dist < size.x * settings.distanceModifier)
        {
            return true;
        }
        return false;
    }
    
    public bool NodeCheck()
    {
        if (size.x / 2f >= minSize && depth < maxDepth)
        {
            return true;
        }
        return false;
    }
    
    public void Split()
    {
        Vector3 halfSize = new Vector3(size.x * 0.5f, size.y, size.z * 0.5f);
        Vector3 qtrSize = new Vector3(halfSize.x * 0.5f, halfSize.y, halfSize.z * 0.5f);
        children = new QuadTreeNode[4];

        children[0] = new QuadTreeNode(center + new Vector3(-qtrSize.x, qtrSize.y, qtrSize.z), halfSize, settings, rootNode, hash * 4, depth + 1);
        children[1] = new QuadTreeNode(center + new Vector3(qtrSize.x, qtrSize.y, qtrSize.z), halfSize, settings, rootNode, hash * 4 + 1, depth + 1,1);
        children[2] = new QuadTreeNode(center + new Vector3(qtrSize.x, qtrSize.y, -qtrSize.z), halfSize, settings, rootNode, hash * 4 + 2, depth + 1,2);
        children[3] = new QuadTreeNode(center + new Vector3(-qtrSize.x, qtrSize.y, -qtrSize.z), halfSize, settings, rootNode, hash * 4 + 3, depth + 1,4);
    } 
    
    public void CheckNeighbors() // east, west, north, south
    {
        neighbors = new byte[4];
        switch (corner) {
            case 0:
                neighbors[1] = CheckNeighborDepth(1,hash);
                neighbors[2] = CheckNeighborDepth(2,hash);
                break;
            case 1:
                neighbors[0] = CheckNeighborDepth(0,hash);
                neighbors[2] = CheckNeighborDepth(2,hash);
                break;
            case 2:
                neighbors[0] = CheckNeighborDepth(0,hash);
                neighbors[3] = CheckNeighborDepth(3,hash);
                break;
            case 3:
                neighbors[1] = CheckNeighborDepth(1,hash);
                neighbors[3] = CheckNeighborDepth(3,hash);
                break;
        }
    }

    private byte CheckNeighborDepth(int side, uint hash)
    {
        uint bitmask = 0;
        byte count = 0;
        uint twoLast;

        while (count < depth * 2) {
            count += 2;
            twoLast = (hash & 3);

            bitmask = bitmask * 4;

            if (side == 2 || side == 4) {
                bitmask += 3;
            } else {
                bitmask += 1;
            }
            
            if((side == 0 && (twoLast == 0 || twoLast == 3)) ||
               (side == 1 && (twoLast == 1 || twoLast == 2)) || 
               (side == 2 && (twoLast == 3 || twoLast == 2)) ||
               (side == 3 && (twoLast == 0 || twoLast == 1))){ break; }

            hash = hash >> 2;
        }

        if (rootNode.GetNeighborDepth(this.hash ^ bitmask, depth) < depth) {
            return 1;
        }
        return 0;
    }

    public int GetNeighborDepth(uint queryHash, int targetDepth)
    {
        if (hash == queryHash) {
            return depth;
        } else {
            if (children!=null) {
                return children[((queryHash >> ((targetDepth - 1) * 2)) & 3)].GetNeighborDepth(queryHash, depth - 1);
            }
        }
        return 0;
    }
}
