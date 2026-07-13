using System.Collections.Generic;
using UnityEngine;

public class PolygonCreator : MonoBehaviour
{
    public List<Vector3> unorderedPoints = new List<Vector3>();
    public LineRenderer lineRenderer;
    public bool drawGizmos = true;

    void Start()
    {
        FormClosedPolygon();
    }

    public void FormClosedPolygon()
    {
        if (unorderedPoints.Count < 3)
        {
            Debug.LogError("Need at least 3 points to form a polygon");
            return;
        }

        List<Vector3> orderedPoints = OrderPoints(unorderedPoints);
        orderedPoints.Add(orderedPoints[0]); // Close the polygon

        // Set up LineRenderer
        lineRenderer.positionCount = orderedPoints.Count;
        lineRenderer.SetPositions(orderedPoints.ToArray());
        lineRenderer.loop = true;
    }

    private List<Vector3> OrderPoints(List<Vector3> points)
    {
        // Calculate centroid
        Vector3 centroid = Vector3.zero;
        foreach (Vector3 point in points)
        {
            centroid += point;
        }
        centroid /= points.Count;

        // Sort by polar angle
        points.Sort((a, b) =>
        {
            Vector3 da = a - centroid;
            Vector3 db = b - centroid;
            return Mathf.Atan2(da.z, da.x).CompareTo(Mathf.Atan2(db.z, db.x));
        });

        return points;
    }

    // Visualize points in Scene view
    void OnDrawGizmos()
    {
        if (!drawGizmos || unorderedPoints == null) return;

        Gizmos.color = Color.red;
        foreach (Vector3 point in unorderedPoints)
        {
            Gizmos.DrawSphere(point, 0.1f);
        }
    }
}