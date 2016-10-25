// Copyright Ioan-Bogdan Lazu. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class TUtils
{
    /**
     * Extracts the corners from the bounds and returns the array that was passed in.
     * 
     * NOTE: Make sure that the array has at lest 8 elements
     */
    public static Vector3[] BoundsCorners(ref Bounds bounds, ref Vector3[] tempBounds)
    {
        Vector3 boundPoint1 = bounds.min;
        Vector3 boundPoint2 = bounds.max;

        tempBounds[0] = bounds.min;
        tempBounds[1] = bounds.max;
        tempBounds[2] = new Vector3(boundPoint1.x, boundPoint1.y, boundPoint2.z);
        tempBounds[3] = new Vector3(boundPoint1.x, boundPoint2.y, boundPoint1.z);
        tempBounds[4] = new Vector3(boundPoint2.x, boundPoint1.y, boundPoint1.z);
        tempBounds[5] = new Vector3(boundPoint1.x, boundPoint2.y, boundPoint2.z);
        tempBounds[6] = new Vector3(boundPoint2.x, boundPoint1.y, boundPoint2.z);
        tempBounds[7] = new Vector3(boundPoint2.x, boundPoint2.y, boundPoint1.z);

        return tempBounds;
    }

    /**
     * Test if the world space vertices are completely inside the planes
     */
    public static bool IsCompletelyInsideFrustum(Plane[] planes, Vector3[] vertices)
    {
        for (int i = 0; i < 6; i++)
        {
            Plane p = planes[i];

            for (int j = 0; j < vertices.Length; j++)
            {
                if (p.GetSide(vertices[j]) == false)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public static Bounds LocalToWorld(ref Bounds box, ref Matrix4x4 m)
    {
        float av, bv;
        int i, j;

        Bounds newBox = new Bounds(Vector3.zero, Vector3.zero);

        newBox.min = new Vector3(m[12], m[13], m[14]);
        newBox.max = new Vector3(m[12], m[13], m[14]);

        Vector3 min, max;

        for (i = 0; i < 3; i++)
        {
            for (j = 0; j < 3; j++)
            {

                av = m[i, j] * box.min[j];
                bv = m[i, j] * box.max[j];

                if (av < bv)
                {
                    min = newBox.min;
                    max = newBox.max;

                    min[i] += av;
                    max[i] += bv;

                    newBox.min = min;
                    newBox.max = max;
                }
                else
                {
                    min = newBox.min;
                    max = newBox.max;

                    min[i] += bv;
                    max[i] += av;

                    newBox.min = min;
                    newBox.max = max;
                }
            }
        }

        return newBox;
    }

    public static void Shuffle<T>(IList<T> collection)
    {        
        for (int i = 0; i < collection.Count; i++)
        {
            T temp = collection[i];

            int randomIndex = Random.Range(0, collection.Count);

            collection[i] = collection[randomIndex];
            collection[randomIndex] = temp;
        }
    }

    public static float InterpolateRange(float min, float max, float val)
    {
        if (val <= min) return 0;
        if (val >= max) return 1;

        return (val - min) / (max - min);
    }

    public static void GetNeighbors<T>(List<T> outNeighbors, T[,] array, int rowCount, int colCount, int row, int col)
    {        
        if (IsValidIndex(row, col, rowCount, colCount))
            outNeighbors.Add(array[row, col]);

        if (IsValidIndex(row - 1, col, rowCount, colCount))
            outNeighbors.Add(array[row - 1, col]);

        if (IsValidIndex(row + 1, col, rowCount, colCount))
            outNeighbors.Add(array[row + 1, col]);

        if (IsValidIndex(row, col - 1, rowCount, colCount))
            outNeighbors.Add(array[row, col - 1]);

        if (IsValidIndex(row, col + 1, rowCount, colCount))
            outNeighbors.Add(array[row, col + 1]);

        if (IsValidIndex(row - 1, col - 1, rowCount, colCount))
            outNeighbors.Add(array[row - 1, col - 1]);

        if (IsValidIndex(row + 1, col + 1, rowCount, colCount))
            outNeighbors.Add(array[row + 1, col + 1]);

        if (IsValidIndex(row - 1, col + 1, rowCount, colCount))
            outNeighbors.Add(array[row - 1, col + 1]);

        if (IsValidIndex(row + 1, col - 1, rowCount, colCount))
            outNeighbors.Add(array[row + 1, col - 1]);
    }

    public static bool IsValidIndex(int row, int col, int rowCount, int colCount)
    {
        return row >= 0 && row < rowCount && col >= 0 && col < colCount;
    }

    public static float Angle360(Vector2 v1, Vector2 v2)
    {
        float dot = v1.x * v2.x + v1.y * v2.y;
        float det = v1.x * v2.y - v1.y * v2.x;

        return Mathf.Atan2(det, dot) * Mathf.Rad2Deg;
    }

    public static float Angle360FromVector3(Vector3 v1, Vector3 v2)
    {
        float dot = v1.x * v2.x + v1.z * v2.z;
        float det = v1.x * v2.z - v1.z * v2.x;

        return Mathf.Atan2(det, dot) * Mathf.Rad2Deg;
    }

    public static float OrientateTowards(Vector3 from, Vector3 to)
    {
        Vector3 campos = Camera.main.transform.position;
        Vector3 v2 = Vector3.back;
        Vector3 v1;

        float dot, det;

        v1 = from - to;

        dot = v1.x * v2.x + v1.z * v2.z;
        det = v1.x * v2.z - v1.z * v2.x;

        return Mathf.Atan2(det, dot) * Mathf.Rad2Deg;
    }

    public static string ToString(IEnumerable objects)
    {
        string str = "";

        foreach (object obj in objects)
        {
            str += obj.ToString();
        }

        return str;
    }

    public static string ToString(object[] objects)
    {
        string str = "";

        for(int i = 0; i < objects.Length; i++)
        {
            str += objects[i].ToString() + " ";
        }

        return str;
    }

    public static int GetStableHashCode(string str)
    {
        int hash1 = (5381 << 16) + 5381;
        int hash2 = hash1;

        for (int i = 0; i < str.Length; i += 2)
        {
            hash1 = ((hash1 << 5) + hash1) ^ str[i];
            if (i == str.Length - 1)
                break;
            hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
        }

        return hash1 + (hash2 * 1566083941);
    }

    public static int GetStableHashCode(ref string str)
    {
        int hash1 = (5381 << 16) + 5381;
        int hash2 = hash1;

        for (int i = 0; i < str.Length; i += 2)
        {
            hash1 = ((hash1 << 5) + hash1) ^ str[i];
            if (i == str.Length - 1)
                break;
            hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
        }

        return hash1 + (hash2 * 1566083941);
    }
}
