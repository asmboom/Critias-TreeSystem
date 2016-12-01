// Copyright Ioan-Bogdan Lazu. All Rights Reserved.

using UnityEngine;
using System.Collections.Generic;


#if UNITY_EDITOR

public enum ETerrainUtilsToSet
{
    INDEX_0 = 0,
    INDEX_1 = 1,
    INDEX_2 = 3,
    INDEX_3 = 4
}


public enum ETreeOperationMode
{
    TREES_ONLY,
    GRASS_ONLY,
    BOTH,
}

public class TerrainUtils : MonoBehaviour
{
    public Terrain m_MainTerrain;

    public Terrain[] m_ToSetData;
    public ETerrainUtilsToSet m_ToSetIndex;
    
    public void FullCopy()
    {
        Terrain terrain = GetToSetTerrain();

        terrain.terrainData.alphamapResolution = m_MainTerrain.terrainData.alphamapResolution;
        terrain.terrainData.baseMapResolution = m_MainTerrain.terrainData.baseMapResolution;
        terrain.terrainData.heightmapResolution = m_MainTerrain.terrainData.heightmapResolution;
        terrain.terrainData.size = m_MainTerrain.terrainData.size;
        terrain.heightmapPixelError = m_MainTerrain.heightmapPixelError;        

        CopyTerrainTreePrototypes();
        CopyTerrainDetailPrototypes();
        CopyTerrainHeightmap();
        CopyTerrainTextures();

        UnityEditor.EditorUtility.SetDirty(terrain);
    }

    public void CopyTerrainTextures()
    {
        Terrain terrain = GetToSetTerrain();

        SplatPrototype[] splat = m_MainTerrain.terrainData.splatPrototypes;
        terrain.terrainData.splatPrototypes = splat;

        UnityEditor.EditorUtility.SetDirty(terrain);
    }

    public void CopyTerrainDetailPrototypes()
    {
        Terrain terrain = GetToSetTerrain();

        DetailPrototype[] proto = m_MainTerrain.terrainData.detailPrototypes;
        terrain.terrainData.detailPrototypes = proto;

        UnityEditor.EditorUtility.SetDirty(terrain);
    }

	public void CopyTerrainTreePrototypes()
    {
        Terrain terrain = GetToSetTerrain();

        TreePrototype[] proto = m_MainTerrain.terrainData.treePrototypes;
        terrain.terrainData.treePrototypes = proto;
        
        UnityEditor.EditorUtility.SetDirty(terrain);
    }
	
	public void CopyTerrainHeightmap()
	{
        Terrain terrain = GetToSetTerrain();

		float[,] heightmapData = m_MainTerrain.terrainData.GetHeights(0, 0, m_MainTerrain.terrainData.heightmapWidth, m_MainTerrain.terrainData.heightmapWidth);
        terrain.terrainData.SetHeights(0, 0, heightmapData);
	
		UnityEditor.EditorUtility.SetDirty(terrain);
	}

    public void CopyTerrainTrees()
    {
        Terrain terrain = GetToSetTerrain();

        CopyTerrainTreePrototypes();
        terrain.terrainData.treeInstances = m_MainTerrain.terrainData.treeInstances;
        
        UnityEditor.EditorUtility.SetDirty(terrain);
    }

    public void ClearMainTerrainTrees()
    {
        m_MainTerrain.terrainData.treeInstances = new TreeInstance[0];
        
        UnityEditor.EditorUtility.SetDirty(m_MainTerrain);
    }

    public void ClearTrees()
    {
        Terrain t = GetToSetTerrain();
        t.terrainData.treeInstances = new TreeInstance[0];

        UnityEditor.EditorUtility.SetDirty(t);
    }

    private Terrain GetToSetTerrain()
    {
        return m_ToSetData[(int)m_ToSetIndex];
    }
    
    public void CellInfo()
    {
        CellInfo(m_MainTerrain);
    }

    public static void CellInfo(Terrain terrain)
    {
        string data = "";

        Debug.Log("Terrain name: " + terrain.name);
        Debug.Log("Terrain sizes: " + terrain.terrainData.size);

        Debug.Log("Possible perfect fit sizes for width:");        
        foreach (Vector2 cell in DivisorsOfSize((int)terrain.terrainData.size.x))
        {
            data += "{S: [" + cell.x + "] C: [" + cell.y + "]}";
        }
        Debug.Log(data);

        data = "";

        Debug.Log("Possible perfect fit sizes for height:");
        foreach (Vector2 cell in DivisorsOfSize((int)terrain.terrainData.size.z))
        {
            data += "{S: [" + cell.x + "] C: [" + cell.y + "]}";
        }
        Debug.Log(data);
    }

    public static Vector2[] DivisorsOfSize(int size)
    {
        List<Vector2> cells = new List<Vector2>();

        for(int i = 2; i < size / 2; i++)
        {
            if(size % i == 0)
            {
                cells.Add(new Vector2(i, size / i));
            }
        }

        return cells.ToArray();
    }

    private void RemoveTrees(Terrain terrain, Terrain operated, ETreeOperationMode mode)
    {
        TreeInstance[] instances = terrain.terrainData.treeInstances;
        TreePrototype[] prototypes = terrain.terrainData.treePrototypes;

        List<TreeInstance> toKeep = new List<TreeInstance>();

        for (int i = 0; i < instances.Length; i++)
        {
            if (mode == ETreeOperationMode.TREES_ONLY)
            {
                // If it is a grass i'll keep it
                if (IsGrassPrefab(prototypes[instances[i].prototypeIndex].prefab))
                {
                    toKeep.Add(instances[i]);
                }
            }
            else if (mode == ETreeOperationMode.GRASS_ONLY)
            {
                // If it is a tree i'll keep it
                if (IsGrassPrefab(prototypes[instances[i].prototypeIndex].prefab) == false)
                {
                    toKeep.Add(instances[i]);
                }
            }
        }

        operated.terrainData.treeInstances = toKeep.ToArray();
        Debug.Log("Removed tree with operation mode: " + mode + ", kept: " + toKeep.Count);
    }
    
    private bool IsGrassPrefab(GameObject possibleGrass)
    {
        return false;

        /*
        bool isGrassPrefab = false;
        
        string name = possibleGrass.name;

        for (int i = 0; i < m_GrassPrefabs.Length; i++)
        {
            if (name == m_GrassPrefabs[i].name)
            {
                isGrassPrefab = true;
                break;
            }
        }

        return isGrassPrefab;
        */
    }

    public static int GetCellCount(Terrain terrain, int cellSize)
    {
        Vector3 tSize = terrain.terrainData.size;

        if ((int)tSize.x % cellSize != 0 || (int)tSize.z % cellSize != 0 || tSize.x != tSize.z)
        {
            Debug.LogError("Non square cells or terrain not allowed!");
            return -1;
        }

        return (int)(tSize.x) / cellSize;
    }

    public static bool CanGridify(Terrain terrain, int cellSize)
    {
        Vector3 tSize = terrain.terrainData.size;

        if ((int)tSize.x % cellSize != 0 || (int)tSize.z % cellSize != 0 || tSize.x != tSize.z)
            return false;

        return true;
    }

    public delegate void OnCellCreate(BoxCollider boxCell, SphereCollider sphereCell, int row, int column);    

    public static void Gridify(Terrain terrain, int cellSize, 
        out int cellCount,  out BoxCollider[,] boxColliders, out SphereCollider[,] sphereColliders,
        GameObject cellHolder, OnCellCreate OnCellCreate, bool followTerrain = true, float minOffset = 5, float maxOffset = 50)
    {
        Vector3 tSize = terrain.terrainData.size;

        if ((int)tSize.x % cellSize != 0 || (int)tSize.z % cellSize != 0 || tSize.x != tSize.z)
        {
            Debug.LogError("Non square cells or terrain not allowed!");

            // Set all to null and zero
            cellCount = 0;
            boxColliders = null;
            sphereColliders = null;

            return;
        }

        cellCount = (int)(tSize.x) / cellSize;
        float halfSize = cellSize / 2.0f;

        BoxCollider[,] cellsBox = new BoxCollider[cellCount, cellCount];
        SphereCollider[,] cellsSphere = new SphereCollider[cellCount, cellCount];

        Vector3 gridSize = new Vector3(cellSize, cellSize, cellSize);

        for (int column = 0; column < cellCount; column++)
        {
            for (int row = 0; row < cellCount; row++)
            {
                GameObject cell = new GameObject("Cell[R:" + row + " C:" + column + "]");
                cell.isStatic = true;

                cell.transform.SetParent(cellHolder.transform, true);

                BoxCollider box = cell.AddComponent<BoxCollider>();
                SphereCollider sphere = cell.AddComponent<SphereCollider>();

                sphere.isTrigger = true;
                box.isTrigger = true;

                // Set data
                box.center = new Vector3();
                box.size = gridSize;

                // Calculate in local space the position
                cell.transform.position = terrain.GetPosition() + new Vector3(cellSize * row + cellSize / 2.0f, 0.0f, cellSize * column + cellSize / 2.0f);

                if (followTerrain)
                {
                    // Calculate terrain min and max there. Sample at those locations.
                    // min is (min - 5) and max is (max + 50). 50m for largest possible tree.
                    Vector3 minPos = box.transform.position - box.size / 2.0f;
                    Vector3 delPos = (box.transform.position + box.size / 2.0f - minPos);

                    float min = float.MaxValue, max = float.MinValue;

                    const int iters = 30;

                    for (int i = 0; i < iters; i++)
                    {
                        for (int j = 0; j < iters; j++)
                        {
                            Vector3 origin = minPos + new Vector3(i / (float)iters * delPos.x, 0, j / (float)iters * delPos.z);

                            float height = terrain.SampleHeight(origin);

                            if (min > height)
                                min = height;
                            if (max < height)
                                max = height;
                        }
                    }

                    box.size = new Vector3(box.size.x, (max - min) + minOffset + maxOffset, box.size.z);
                    sphere.radius = box.size.magnitude / 2f;

                    cell.transform.position = new Vector3(box.transform.position.x,
                        (min - minOffset) + ((max + maxOffset) - (min - minOffset)) / 2.0f,
                        box.transform.position.z);
                }

                // Set the data
                cellsBox[row, column] = box;
                cellsSphere[row, column] = sphere;

                // Invoke delegate
                if (OnCellCreate != null)
                    OnCellCreate(box, sphere, row, column);
            }
        }

        // Set the out values
        boxColliders = cellsBox;
        sphereColliders = cellsSphere;
    }

    /**
     * Checks whether there is a collision within the tree's hashes that has to be remedied
     * 
     * @return False in case there is no collision and true if there is a collision
     */
    public static bool TreeHashCheck(Terrain mainTerrain)
    {
        HashSet<int> set = new HashSet<int>();
        
        TreePrototype[] p = mainTerrain.terrainData.treePrototypes;

        for(int i = 0; i < p.Length; i++)
        {
            if(set.Contains(GetStableHashCode(p[i].prefab.name)))
            {
                return true;
            }

            set.Add(GetStableHashCode(p[i].prefab.name));
        }

        return false;
    }

    /**
     * @return True is all the terrain contain the same tree data, false otherwise
     */
    public static bool IntegrityCheckTrees(Terrain mainTerrain, Terrain[] terrains)
    {
        bool same = true;

        TreePrototype[] proto = mainTerrain.terrainData.treePrototypes;

        for(int i = 0; i < terrains.Length; i++)
        {
            TreePrototype[] p = terrains[i].terrainData.treePrototypes;

            if (p.Length != proto.Length)
            {
                same = false;
                goto NotSame;
            }
            else
            {
                // Compare prototype by prototype
                for(int j = 0; j < proto.Length; j++)
                {
                    if(p[j].prefab != proto[j].prefab || p[j].prefab.name != proto[j].prefab.name)
                    {
                        same = false;
                        goto NotSame;
                    }
                }
            }
        }
        NotSame:

        return same;
    }

    public static bool IntegrityCheckTextures(Terrain mainTerrain, Terrain[] terrains)
    {
        bool same = true;

        SplatPrototype[] splat = mainTerrain.terrainData.splatPrototypes;

        for(int i = 0; i < terrains.Length; i++)
        {
            SplatPrototype[] s = terrains[i].terrainData.splatPrototypes;

            if(s.Length != splat.Length)
            {
                same = false;
                goto NotSame;
            }
            else
            {
                for (int j = 0; j < splat.Length; j++)
                {                    
                    if (s[j].metallic != splat[j].metallic 
                        || s[j].normalMap != splat[j].normalMap
                        || s[j].smoothness != splat[j].smoothness
                        || s[j].specular != splat[j].specular
                        || s[j].texture != splat[j].texture
                        || s[j].tileOffset != splat[j].tileOffset
                        || s[j].tileSize!= splat[j].tileSize)
                    {
                        same = false;
                        goto NotSame;
                    }
                }
            }
        }
        NotSame:

        return same;
    }

    public static bool IntegrityCheckDetails(Terrain mainTerrain, Terrain[] terrains)
    {
        bool same = true;

        DetailPrototype[] proto = mainTerrain.terrainData.detailPrototypes;

        for (int i = 0; i < terrains.Length; i++)
        {
            DetailPrototype[] p = terrains[i].terrainData.detailPrototypes;

            if (p.Length != proto.Length)
            {
                same = false;
                goto NotSame;
            }
            else
            {
                // Compare prototype by prototype
                for (int j = 0; j < proto.Length; j++)
                {
                    if (p[j].prototype != proto[j].prototype 
                        || p[j].renderMode != p[j].renderMode
                        || p[j].noiseSpread != p[j].noiseSpread
                        || p[j].maxHeight != p[j].maxHeight
                        || p[j].maxWidth != p[j].maxWidth
                        || p[j].healthyColor != p[j].healthyColor
                        || p[j].minHeight != p[j].minHeight
                        || p[j].minWidth != p[j].minWidth
                        || p[j].prototypeTexture != p[j].prototypeTexture)
                    {
                        same = false;
                        goto NotSame;
                    }
                }
            }
        }
        NotSame:

        return same;
    }

    /**
     * Converts from the 0..1 range to the terrain's local position. For example, on a 
     * 1000x1000 terrain 0 is 0, 0.5 is 500, 1 is 1000.
     */
    public static Vector3 TerrainToTerrainPos(Vector3 terrainNormalizedLocalPos, Terrain terrain)
    {
        Vector3 size = terrain.terrainData.size;
        Vector3 worldPos = new Vector3(Mathf.Lerp(0.0f, size.x, terrainNormalizedLocalPos.x),
                                       Mathf.Lerp(0.0f, size.y, terrainNormalizedLocalPos.y),
                                       Mathf.Lerp(0.0f, size.z, terrainNormalizedLocalPos.z));
        
        return worldPos;
    }

    /**
     * Converts from the 0..1 range to the world possition. As a difference between this and
     * 'TerrainToTerrainPos' this also applies the terrain's world position.
     */
    public static Vector3 TerrainToWorldPos(Vector3 terrainNormalizedLocalPos, Terrain terrain)
    {
        Vector3 size = terrain.terrainData.size;
        Vector3 worldPos = new Vector3(Mathf.Lerp(0.0f, size.x, terrainNormalizedLocalPos.x),
                                       Mathf.Lerp(0.0f, size.y, terrainNormalizedLocalPos.y),
                                       Mathf.Lerp(0.0f, size.z, terrainNormalizedLocalPos.z));

        worldPos += terrain.transform.position;

        return worldPos;
    }

    public static Vector3 WorldPosToTerrain(Vector3 worldPos, Terrain terrain)
    {
        Vector3 terrainLocalPos = terrain.transform.InverseTransformPoint(worldPos);

        Vector3 normalizedPos = new Vector3(Mathf.InverseLerp(0.0f, terrain.terrainData.size.x, terrainLocalPos.x),
                                            Mathf.InverseLerp(0.0f, terrain.terrainData.size.y, terrainLocalPos.y),
                                            Mathf.InverseLerp(0.0f, terrain.terrainData.size.z, terrainLocalPos.z));

        return normalizedPos;
    }

    public static Vector3 TerrainNormal(Vector3 terrainLocalPos, Terrain terrain)
    {
        return terrain.terrainData.GetInterpolatedNormal(terrainLocalPos.x, terrainLocalPos.z);
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

#endif