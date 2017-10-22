using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class ExtensionMethods
{
    #region Vector Math
    public static float Towards(this float f, float target, float stepAmount)
    {
        // Make sure we have our step in the appropriate direction.
        stepAmount = Mathf.Abs(stepAmount);
        float difference = Mathf.Abs(f - target);
        if (difference < stepAmount)
            stepAmount = difference;
        if (target < f)
            stepAmount = -stepAmount;

        return f + stepAmount;
    }
    /// <summary>
    /// Takes the given value and randomly returns it either as it was, or negative.
    /// </summary>
    /// <param name="x">The value to randomly negate.</param>
    /// <returns>Randomly will return the original value or a negative counterpart to it.</returns>
    public static float RandomDirection(this float x)
    {
        int direction = UnityEngine.Random.Range(0, 2);
        return (direction == 0) ? -x : x;
    }
    /// <summary>
    /// Takes the given value and randomly returns it either as it was, or negative.
    /// </summary>
    /// <param name="x">The value to randomly negate.</param>
    /// <returns>Randomly will return the original value or a negative counterpart to it.</returns>
    public static int RandomDirection(this int x)
    {
        int direction = UnityEngine.Random.Range(0, 2);
        return (direction == 0) ? -x : x;
    }
    /// <summary>
    /// Takes the given value and randomly returns it either as it was, or negative.
    /// </summary>
    /// <param name="x">The value to randomly negate.</param>
    /// <returns>Randomly will return the original value or a negative counterpart to it.</returns>
    public static short RandomDirection(this short x)
    {
        int direction = UnityEngine.Random.Range(0, 2);
        return (short)((direction == 0) ? -x : x);
    }
    /// <summary>
    /// Returns the given vector with specified components randomly flipped negative.
    /// </summary>
    /// <param name="v">The vector to go in random directions.</param>
    /// <param name="x">Describes if we should consider negating the x component.</param>
    /// <param name="y">Describes if we should consider negating the y component.</param>
    /// <param name="z">Describes if we should consider negating the z component.</param>
    /// <returns>Returns the vector with certain components randomly original, or negative.</returns>
    public static Vector3 RandomDirections(this Vector3 v, bool x = true, bool y = true, bool z = true)
    {
        float vX = x ? v.x.RandomDirection() : v.x;
        float vY = y ? v.y.RandomDirection() : v.y;
        float vZ = z ? v.z.RandomDirection() : v.z;
        return new Vector3(vX, vY, vZ);
    }

    public static Vector3 Abs(this Vector3 vector)
    {
        return new Vector3(Math.Abs(vector.x), Math.Abs(vector.y), Math.Abs(vector.z));
    }
    public static Vector3 AtSlope(this Vector3 direction, Vector3 slopeNormal)
    {
        return direction - Vector3.Dot(slopeNormal, direction) * slopeNormal;
    }

    /// <summary>
    /// Calculates the centroid of an array of Vector3's.
    /// </summary>
    /// <param name="points">An array of Vector3 points.</param>
    /// <returns>A Vector3 at the center of the array arguement.</returns>
    public static Vector3 Centroid(this Vector3[] points)
    {
        Vector3 centroid = Vector3.zero;
        foreach (Vector3 point in points)
            centroid += point;
        return centroid /= points.Length;
    }
    /// <summary>
    /// Calculates the centroid of a list of Vector3's.
    /// </summary>
    /// <param name="points">An array of Vector3 points.</param>
    /// <returns>A Vector3 at the center of the array arguement.</returns>
    public static Vector3 Centroid(this List<Vector3> points)
    {
        Vector3 centroid = Vector3.zero;
        foreach (Vector3 point in points)
            centroid += point;
        return centroid /= points.Count;
    }

    public static void ResetLocal(this Transform transform)
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(Vector3.zero);
        transform.localScale = Vector3.one;
    }

    
    public static void Reset(this Transform transform)
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.Euler(Vector3.zero);
        transform.localScale = Vector3.one;
    }

    public static Vector3 Divide(this Vector3 point, Vector3 d)
    {
        point.x /= d.x;
        point.y /= d.y;
        point.z /= d.z;
        return point;
    }

    public static Vector3 Rotate(this Vector3 vector, float angle, Vector3 axis, Space space = Space.Self)
    {
        return Quaternion.Euler(vector).Rotate(angle, axis, space).eulerAngles;
    }
    public static Quaternion Rotate(this Quaternion quaternion, float angle, Vector3 axis, Space space = Space.Self)
    {
        if (space == Space.Self)
            return (quaternion * Quaternion.AngleAxis(angle, axis));
        else
            return (Quaternion.AngleAxis(angle, axis) * quaternion);
    }

    public static Vector3 RotateAround(this Vector3 point, Vector3 pivot, Quaternion angle)
    {
        Vector3 direction = point - pivot;
        direction = angle * direction;
        point = direction + pivot;
        return point;
    }
    public static Vector3[] RotateAround(this Vector3[] points, Vector3 pivot, Quaternion angle)
    {
        Vector3[] transformedPoints = (Vector3[])points.Clone();

        for (int i = 0; i < transformedPoints.Length; i++)
        {
            Vector3 direction = transformedPoints[i] - pivot;
            direction = angle * direction;
            transformedPoints[i] = direction + pivot;
        }
        return transformedPoints;
    }
    public static Vector2 RotateClockwise(this Vector2 aPoint, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float s = Mathf.Sin(rad);
        float c = Mathf.Cos(rad);
        return new Vector2(
            aPoint.x * c + aPoint.y * s,
            aPoint.y * c - aPoint.x * s
     );
    }
    public static Vector2 RotateCounterClockwise(this Vector2 aPoint, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float s = Mathf.Sin(rad);
        float c = Mathf.Cos(rad);
        return new Vector2(
            aPoint.x * c - aPoint.y * s,
            aPoint.y * c + aPoint.x * s
     );
    }
    public static Direction RotateClockwise(this Direction direction, QuarterlyRotation rotation)
    {
        switch(rotation)
        {
            case QuarterlyRotation.Rotation90:
                if (direction == Direction.Down)
                    return Direction.Left;
                else if (direction == Direction.Left)
                    return Direction.Up;
                else if (direction == Direction.Up)
                    return Direction.Right;
                else
                    return Direction.Down;

            case QuarterlyRotation.Rotation180:
                if (direction == Direction.Down)
                    return Direction.Up;
                else if (direction == Direction.Left)
                    return Direction.Right;
                else if (direction == Direction.Up)
                    return Direction.Down;
                else
                    return Direction.Left;

            case QuarterlyRotation.Rotation270:
                if (direction == Direction.Down)
                    return Direction.Right;
                else if (direction == Direction.Left)
                    return Direction.Down;
                else if (direction == Direction.Up)
                    return Direction.Left;
                else
                    return Direction.Up;

            default:
                return direction;
        }
    }
    public static QuarterlyRotation GetClockwiseRotation(this Direction direction, Direction desiredDirection)
    {
        if(direction == Direction.Down)
        {
            if (desiredDirection == Direction.Down)
                return QuarterlyRotation.RotationNone;
            else if (desiredDirection == Direction.Left)
                return QuarterlyRotation.Rotation90;
            else if (desiredDirection == Direction.Right)
                return QuarterlyRotation.Rotation270;
            else
                return QuarterlyRotation.Rotation180;
        }
        else if(direction == Direction.Left)
        {
            if (desiredDirection == Direction.Down)
                return QuarterlyRotation.Rotation270;
            else if (desiredDirection == Direction.Left)
                return QuarterlyRotation.RotationNone;
            else if (desiredDirection == Direction.Right)
                return QuarterlyRotation.Rotation180;
            else
                return QuarterlyRotation.Rotation90;
        }
        else if (direction == Direction.Right)
        {
            if (desiredDirection == Direction.Down)
                return QuarterlyRotation.Rotation90;
            else if (desiredDirection == Direction.Left)
                return QuarterlyRotation.Rotation180;
            else if (desiredDirection == Direction.Right)
                return QuarterlyRotation.RotationNone;
            else
                return QuarterlyRotation.Rotation270;
        }
        else 
        {
            if (desiredDirection == Direction.Down)
                return QuarterlyRotation.Rotation180;
            else if (desiredDirection == Direction.Left)
                return QuarterlyRotation.Rotation270;
            else if (desiredDirection == Direction.Right)
                return QuarterlyRotation.Rotation90;
            else
                return QuarterlyRotation.RotationNone;
        }
    }
    public static List<Vector3> RotateAround(this List<Vector3> points, Vector3 pivot, Quaternion angle)
    {
        // This is likely dumb as fuck
        Vector3[] transformedPointsArray = new Vector3[] { };
        points.CopyTo(transformedPointsArray);
        List<Vector3> transformedPoints = transformedPointsArray.ToList<Vector3>();

        for (int i = 0; i < transformedPoints.Count; i++)
        {
            Vector3 direction = transformedPoints[i] - pivot;
            direction = angle * direction;
            transformedPoints[i] = direction + pivot;
        }
        return transformedPoints;
    }

    public static T Random<T>(this T[] array) 
    {
        int randomIndex = UnityEngine.Random.Range(0, array.Length);
        return array[randomIndex];
    }
    public static T Random<T>(this T[] array, RandomGen randomGen)
    {
        int randomIndex = randomGen.Range(0, array.Length);
        return array[randomIndex];
    }
    #endregion

    #region Game Objects
    public static void SetVisibility(this MonoBehaviour gameObject, bool visible, bool thisMeshRenderers = true, bool childMeshRenderers = false, bool parentMeshRenderers = false)
    {
        // List of mesh renderers.
        MeshRenderer[] meshRenderers;

        // Set visibility for the game objects mesh components
        if (thisMeshRenderers)
        {
            meshRenderers = gameObject.GetComponents<MeshRenderer>();
            foreach (MeshRenderer meshRenderer in meshRenderers)
                meshRenderer.enabled = visible;
        }

        // Set visibility for child mesh renderers
        if (childMeshRenderers)
        {
            meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer meshRenderer in meshRenderers)
                meshRenderer.enabled = visible;
        }

        // Set visibility for parent mesh renderers.
        if (parentMeshRenderers)
        {
            meshRenderers = gameObject.GetComponentsInParent<MeshRenderer>();
            foreach (MeshRenderer meshRenderer in meshRenderers)
                meshRenderer.enabled = visible;
        }
    }

    public static bool IsVisibleFrom(this Renderer renderer, Camera camera)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
    }
    #endregion

    #region Primitives/Simple Types
    public static string ToHexString(this byte[] array)
    {
        string result = "";
        foreach (byte b in array)
            result += b.ToString("X2");
        return result;
    }
    public static byte[] ToArrayFromHexString(this string hexstring)
    {
        byte[] data = new byte[hexstring.Length / 2];
        for (int i = 0; i < hexstring.Length; i += 2)
            data[i / 2] = Convert.ToByte(hexstring.Substring(i, 2), 16);
        return data;
    }
    public static bool IsType(this Type type, Type c)
    {
        return c == type || type.IsSubclassOf(c);
    }
    public static string ToArgbString(this Color color)
    {
        return ((byte)(color.r * 255)).ToString("X2") + ((byte)(color.g * 255)).ToString("X2") + ((byte)(color.b * 255)).ToString("X2") + ((byte)(color.a * 255)).ToString("X2");
    }

    public static int Append(this int n, int i)
    {
        int c = 1;
        while (c <= i) c *= 10;
        return n * c + i;
    }

    public static Direction Inverse(this Direction d)
    {
        switch (d)
        {
            case Direction.Down:
                return Direction.Up;
            case Direction.Up:
                return Direction.Down;
            case Direction.Right:
                return Direction.Left;
            default:
            case Direction.Left:
                return Direction.Right;
        }
    }
    public static bool SameAxis(this Direction d, Direction d2)
    {
        return d == d2 || d == d2.Inverse();
    }
    #endregion

    #region Networking
    public static bool IsLocalClient(this NetworkConnection networkConnection)
    {
        return networkConnection.address == "localClient";
    }
    #endregion

    #region Member/Property/Field Info
    public static System.Object GetValue(this MemberInfo mi, System.Object parentObj)
    {
        if (mi.MemberType == MemberTypes.Property)
            return ((PropertyInfo)mi).GetValue(parentObj, null);
        else if (mi.MemberType == MemberTypes.Field)
            return ((FieldInfo)mi).GetValue(parentObj);
        else
            throw new ArgumentException(string.Format("MemberInfo.GetValue() does not support this type: {0}", mi.MemberType.ToString()));
    }
    public static void SetValue(this MemberInfo mi, System.Object parentObj, System.Object value)
    {
        if (mi.MemberType == MemberTypes.Property)
            ((PropertyInfo)mi).SetValue(parentObj, value, null);
        else if (mi.MemberType == MemberTypes.Field)
            ((FieldInfo)mi).SetValue(parentObj, value);
        else
            throw new ArgumentException(string.Format("MemberInfo.SetValue() does not support this type: {0}", mi.MemberType.ToString()));
    }
    public static Type GetUnderlyingValueType(this MemberInfo mi)
    {
        if (mi.MemberType == MemberTypes.Property)
            return ((PropertyInfo)mi).PropertyType;
        else if (mi.MemberType == MemberTypes.Field)
            return ((FieldInfo)mi).FieldType;
        else
            throw new ArgumentException(string.Format("MemberInfo.GetUnderlyingValueType() does not support this type: {0}", mi.MemberType.ToString()));
    }
    #endregion
}