using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[DisallowMultipleComponent]
sealed public class Path : MonoBehaviour
{
    #region Backup data for editor reverting.
    [HideInInspector]
    public List<Path.Node> NodesBackup = new List<Path.Node>();
    [HideInInspector]
    public Vector3 PositionBackup;
    [HideInInspector]
    public Quaternion RotationBackup;
    [HideInInspector]
    public Vector3 ScaleBackup;
    #endregion

    /// <summary>
    /// Determines if the path neighbors the first and last bezier points.
    /// </summary>
    public bool Closed = true;
    /// <summary>
    /// Determines if the path's points and tangents are allowed to use the Z axis.
    /// </summary>
    public bool Flat;
    /// <summary>
    /// Determines the quantity of points along the curve.
    /// </summary>
    [Range(2, 120)]
    public int Resolution = 30;
    /// <summary>
    /// Contains the bezier points of the path.
    /// </summary>
    public List<Node> Nodes = new List<Path.Node>() 
        {
            new Path.Node(new Vector3(0, 1, 0), new Vector3(0.56f, 0, 0), new Vector3(0.56f, 0, 0)),
            new Path.Node(new Vector3(1, 0, 0), new Vector3(0, -0.56f, 0), new Vector3(0, -0.56f, 0)), 
            new Path.Node(new Vector3(0, -1, 0),new Vector3(-0.56f, 0, 0), new Vector3(-0.56f, 0, 0)), 
            new Path.Node(new Vector3(-1, 0, 0), new Vector3(0, 0.56f, 0), new Vector3(0, 0.56f, 0))
        };


    #region Core Functions
    /// <summary>
    /// Adds a new BezierPoint to the BezierPath.
    /// </summary>
    /// <param name="index">The position within the point list.</param>
    /// <param name="curveTime">The time along curve.</param>
    public void Add(int index, float curveTime)
    {
        Vector3 newPoint = Vector3.zero;
        Vector3 newTangentPoint = Vector3.zero;

        if (index != Nodes.Count - 1)
        {
            // Define new  mid-point
            newPoint = Path.CurvePoint(Nodes[index], Nodes[index + 1], curveTime, Vector3.one);
            newTangentPoint = Path.CurvePoint(Nodes[index], Nodes[index + 1], curveTime, Vector3.one);
            Nodes.Insert(index + 1, new Path.Node(newPoint, newTangentPoint - newPoint, newTangentPoint - newPoint));
        }
        else
        {
            // Define new end-point
            newPoint = Path.CurvePoint(Nodes[Nodes.Count - 1], Nodes[0], curveTime, Vector3.one);
            Nodes.Insert(0, new Path.Node(newPoint, newTangentPoint - newPoint, Vector3.zero));
        }
    }
    /// <summary>
    /// Remove an existing point from the path.
    /// </summary>
    /// <param name="point">The point to remove.</param>
    public void Remove(Node point)
    {
        // Remove point for point list
        if (Nodes.Count > 2)
            Nodes.Remove(point);
    }

    public Vector3[] NodePoints(bool worldRelative)
    {
        List<Vector3> nodePoints = new List<Vector3>();
        foreach (Node node in Nodes)
        {
            if (worldRelative)

                nodePoints.Add(Vector3.Scale(node.Position, transform.localScale).RotateAround(Vector3.zero, transform.rotation) + transform.position);
            else
                nodePoints.Add(node.Position);
        }
        return nodePoints.ToArray();
    }
    public Vector3 NodePoint(int index, bool worldRelative)
    {
        if (worldRelative)
            return Vector3.Scale(Nodes[index].Position, transform.localScale).RotateAround(Vector3.zero, transform.rotation) + transform.position;
        else
            return Nodes[index].Position;
    }



    // Returns a point along a curve between two points.
    public static Vector3 CurvePoint(Node point, Node neighborPoint, float fraction, Vector3 scale)
    {
        float Cx = 3f * ((point.Position.x + point.TangentA.x) - point.Position.x);
        float Bx = 3f * ((neighborPoint.Position.x + -neighborPoint.TangentB.x) - (point.Position.x + point.TangentA.x)) - Cx;
        float Ax = neighborPoint.Position.x - point.Position.x - Cx - Bx;

        float Cy = 3f * ((point.Position.y + point.TangentA.y) - point.Position.y);
        float By = 3f * ((neighborPoint.Position.y + -neighborPoint.TangentB.y) - (point.Position.y + point.TangentA.y)) - Cy;
        float Ay = neighborPoint.Position.y - point.Position.y - Cy - By;

        float Cz = 3f * ((point.Position.z + point.TangentA.z) - point.Position.z);
        float Bz = 3f * ((neighborPoint.Position.z + -neighborPoint.TangentB.z) - (point.Position.z + point.TangentA.z)) - Cz;
        float Az = neighborPoint.Position.z - point.Position.z - Cz - Bz;

        float t2 = fraction * fraction;
        float t3 = fraction * fraction * fraction;
        float x = Ax * t3 + Bx * t2 + Cx * fraction + point.Position.x;
        float y = Ay * t3 + By * t2 + Cy * fraction + point.Position.y;
        float z = Az * t3 + Bz * t2 + Cz * fraction + point.Position.z;
        return Vector3.Scale(new Vector3(x, y, z), scale);
    }
    // Returns an array of all the points along the path
    public static Vector3[] CurvePoints(Path path, Vector3 position, Quaternion rotation, Vector3 scale, bool overstepVectorDensity, bool display)
    {
        // Define path points
        List<Vector3> pathPoints = new List<Vector3>();

        // Add default curves
        for (int i = 0; i < path.Nodes.Count; i++)
        {
            if (i < path.Nodes.Count - 1)
            {
                Node pointA = path.Nodes[i];
                Node pointB = path.Nodes[i + 1];
                if (pointA.TangentType == Node.Type.Vector && pointB.TangentType == Node.Type.Vector && overstepVectorDensity)
                {
                    // Create a two point line if there isnt a curve between
                    Vector3 newPointA = (Vector3.Scale(pointA.Position, scale) + position).RotateAround(position, rotation);
                    Vector3 newPointB = (Vector3.Scale(pointB.Position, scale) + position).RotateAround(position, rotation);
                    if (!pathPoints.Contains(newPointA))
                        pathPoints.Add(newPointA);
                    if (!pathPoints.Contains(newPointB))
                        pathPoints.Add(newPointB);
                    continue;
                }
                for (int j = 0; j < path.Resolution + 1; j++)
                {
                    if ((j / (float)path.Resolution) == 0 && (i != 0))
                        continue;
                    pathPoints.Add((CurvePoint(pointA, pointB, (j / (float)path.Resolution), scale) + position).RotateAround(position, rotation));
                }
            }
        }

        // Add closing curve
        if (path.Closed)
        {
            Node pointA = path.Nodes[path.Nodes.Count - 1];
            Node pointB = path.Nodes[0];
            if (pointA.TangentType == Node.Type.Vector && pointB.TangentType == Node.Type.Vector && overstepVectorDensity)
            {
                Vector3 newPointB = (Vector3.Scale(pointB.Position, scale) + position).RotateAround(position, rotation);
                pathPoints.Add(newPointB);
            }
            else
                for (int i = 1; i < path.Resolution + 1; i++)
                {
                    if ((i / (float)path.Resolution) == 0)
                        continue;
                    pathPoints.Add((CurvePoint(pointA, pointB, (i / (float)path.Resolution), scale) + position).RotateAround(position, rotation));
                }
        }

        if (!display && path.Closed)
        pathPoints.RemoveAt(pathPoints.Count - 1);

        return pathPoints.ToArray();
    }
    public static Vector2[] CurvePoints2D(Path path, Vector3 position, Quaternion rotation, Vector3 scale, bool overstepVectorDensity, bool display)
    {
        // Define path points
        List<Vector2> pathPoints = new List<Vector2>();

        // Add default curves
        for (int i = 0; i < path.Nodes.Count; i++)
        {
            if (i < path.Nodes.Count - 1)
            {
                Node pointA = path.Nodes[i];
                Node pointB = path.Nodes[i + 1];
                if (pointA.TangentType == Node.Type.Vector && pointB.TangentType == Node.Type.Vector && overstepVectorDensity)
                {
                    // Create a two point line if there isnt a curve between
                    Vector3 newPointA = (Vector3.Scale(pointA.Position, scale) + position).RotateAround(position, rotation);
                    Vector3 newPointB = (Vector3.Scale(pointB.Position, scale) + position).RotateAround(position, rotation);
                    if (!pathPoints.Contains(newPointA))
                        pathPoints.Add(newPointA);
                    if (!pathPoints.Contains(newPointB))
                        pathPoints.Add(newPointB);
                    continue;
                }
                for (int j = 0; j < path.Resolution + 1; j++)
                {
                    if ((j / (float)path.Resolution) == 0 && (i != 0))
                        continue;
                    pathPoints.Add((CurvePoint(pointA, pointB, (j / (float)path.Resolution), scale) + position).RotateAround(position, rotation));
                }
            }
        }

        // Add closing curve
        if (path.Closed)
        {
            Node pointA = path.Nodes[path.Nodes.Count - 1];
            Node pointB = path.Nodes[0];
            if (pointA.TangentType == Node.Type.Vector && pointB.TangentType == Node.Type.Vector && overstepVectorDensity)
            {
                Vector3 newPointB = (Vector3.Scale(pointB.Position, scale) + position).RotateAround(position, rotation);
                pathPoints.Add(newPointB);
            }
            else
                for (int i = 1; i < path.Resolution + 1; i++)
                {
                    if ((i / (float)path.Resolution) == 0)
                        continue;
                    pathPoints.Add((CurvePoint(pointA, pointB, (i / (float)path.Resolution), scale) + position).RotateAround(position, rotation));
                }
        }

        if (!display && path.Closed)
            pathPoints.RemoveAt(pathPoints.Count - 1);

        return pathPoints.ToArray();
    }

    public Vector3[] CurvePoints(bool worldRelative, bool overstepVectorLineDensity, bool display)
    {
        if (worldRelative)
            return CurvePoints(this, transform.position, transform.rotation, transform.localScale, overstepVectorLineDensity, display);
        else
            return CurvePoints(this, Vector3.zero, Quaternion.identity, Vector3.one, overstepVectorLineDensity, display);
    }
    public Vector2[] CurvePoints2D(bool worldRelative, bool overstepVectorLineDensity, bool display)
    {
        if (worldRelative)
            return CurvePoints2D(this, transform.position, transform.rotation, transform.localScale, overstepVectorLineDensity, display);
        else
            return CurvePoints2D(this, Vector3.zero, Quaternion.identity, Vector3.one, overstepVectorLineDensity, display);
    }
    #endregion

    #region Classes
    [Serializable]
    public class Node
    {
        public Vector3 Position;
        public enum Type
        {
            Auto,
            Aligned,
            Free,
            Vector
        }
        /// <summary>
        /// Determines the tangents modification type.
        /// </summary>
        [Space(10)]
        public Type TangentType;
        /// <summary>
        /// Defines the primary tangent of the curve.
        /// </summary>
        public Vector3 TangentA;
        /// <summary>
        /// Defines the secondary tangent of the curve.
        /// </summary>
        public Vector3 TangentB;

        public Node(Vector3 position, Vector3 tangentA, Vector3 tangentB)
        {
            Position = position;
            TangentA = tangentA;
            TangentB = tangentB;
        }
    }
    #endregion
}