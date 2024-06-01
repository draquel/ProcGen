using UnityEngine;

public static class FrustrumUtility
{
    public static Plane[] ScalePlanes(Plane[] planes, float scale)
    {
        Plane[] scaledPlanes = new Plane[planes.Length];
    
        for (int i = 0; i < planes.Length; i++) {
            scaledPlanes[i] = ScalePlane(planes[i], scale);
        } 
        
        return scaledPlanes;
    }

    public static Plane ScalePlane(Plane plane, float scale)
    {
        float scaledDistance = plane.distance * scale;
        return new Plane(plane.normal, scaledDistance);
    }
   
    public static Plane[] MovePlanes(Plane[] planes, float panDistance, Vector3 direction)
    {
        Plane[] transformedPlanes = new Plane[planes.Length];
        Vector3 panVector = direction * panDistance;
        
        for (int i = 0; i < planes.Length; i++) {
            transformedPlanes[i] = MovePlane(planes[i], panVector);
        }
        
        return transformedPlanes;
    }
    
    public static Plane MovePlane(Plane plane, Vector3 panVector)
    {
        float adjustedDistance = plane.distance - Vector3.Dot(panVector, plane.normal);
        return new Plane(plane.normal, adjustedDistance);
    }
    
    public static bool IsPartiallyInFrustum(Plane[] planes, Bounds bounds)
    {
        Vector3[] corners = GetBoundingBoxCorners(bounds);

        foreach (Vector3 corner in corners) {
            bool insideAllPlanes = true;
            foreach (Plane plane in planes) {
                if (plane.GetDistanceToPoint(corner) < 0) {
                    insideAllPlanes = false;
                    break;
                }
            }
            if (insideAllPlanes) return true;
        }

        return false;
    }

    public static Vector3[] GetBoundingBoxCorners(Bounds bounds)
    {
        Vector3[] corners = new Vector3[8];
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;

        corners[0] = new Vector3(min.x, min.y, min.z);
        corners[1] = new Vector3(max.x, min.y, min.z);
        corners[2] = new Vector3(min.x, max.y, min.z);
        corners[3] = new Vector3(max.x, max.y, min.z);
        corners[4] = new Vector3(min.x, min.y, max.z);
        corners[5] = new Vector3(max.x, min.y, max.z);
        corners[6] = new Vector3(min.x, max.y, max.z);
        corners[7] = new Vector3(max.x, max.y, max.z);

        return corners;
    }
}
