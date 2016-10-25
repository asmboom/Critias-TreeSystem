// Copyright Ioan-Bogdan Lazu. All Rights Reserved.

using System;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

[System.Serializable]
public struct TreeSystemStoredInstance
{
    public int m_TreeHash;
    public Matrix4x4 m_PositionMtx;
    public Vector3 m_WorldPosition;
    public Vector3 m_WorldScale;
    public float m_WorldRotation;
    public Bounds m_WorldBounds;
}

[System.Serializable]
public class TreeSystemStructuredTrees
{
    public Bounds m_Bounds;
    public RowCol m_Position;

    public TreeSystemStoredInstance[] m_Instances;

    // TODO: hold extra data to see if they're visibile or not
    public TreeSystemLODInstance[] m_InstanceData;
}

[System.Serializable]
public class TreeSystemLODData
{
    public bool m_IsBillboard;

    public float m_StartDistance;
    public float m_EndDistance;

    public Material[] m_Materials;
    public Mesh m_Mesh;

    // BEGIN RUNTIME UPDATED DATA
    public Renderer m_TreeRenderer;
    public MaterialPropertyBlock m_Block;
    // END RUNTIME UPDATED DATA

    public void CopyBlock()
    {
        // If we are a 3D model, copy the wind data
        if (m_IsBillboard == false)
            m_TreeRenderer.GetPropertyBlock(m_Block);
    }

    public bool IsInRange(float distance)
    {
        if (distance < m_StartDistance || distance > m_EndDistance)
            return false;

        return true;
    }
}

[System.Serializable]
public class TreeSystemPrototypeData
{
    // BEGIN DATA MANUALLY SET
    public TextAsset m_TreeBillboardData;
    // END DATA MANUALLY SET

    // BEGIN DATA GENERATED
    public GameObject m_TreePrototype;
    // Not using index any more, but hash so that we are sure that it is not lost upon any reorder
    public int m_TreePrototypeHash;
    public Vector3 m_Size; // Width, height, bottom. To be used by the tree system for each instance
    public Material m_BillboardBatchMaterial; // Billboard material to be used for tree billboards
    public Material m_BillboardMasterMaterial;
    public Vector2[] m_VertBillboardUVs;
    public Vector2[] m_HorzBillboardUVs;

    // Maximum LOD level including billboard as index
    public int m_MaxLodIndex;
    // Maximum LOD level excluding billboard as index
    public int m_MaxLod3DIndex;
    // END DATA GENERATED

    // BEGIN UPDATED AT RUNTIME
    public TreeSystemLODData[] m_LODData;
    // END UPDATED AT RUNTIME
}

/**
 * This guy is going to be stored at the cell's tree index, so that we don't need to hold that data internally
 */
[System.Serializable]
public struct TreeSystemLODInstance
{
    public int m_LODLevel;
    public float m_LODTransition;
    public float m_LODFullFade;
}

// public class TreeSystem
[System.Serializable]
public class TreeSystemTerrain
{
    public Terrain m_ManagedTerrain;    

    public int m_CellSize;
    public int m_CellCount;
}

[System.Serializable]
public class TreeSystemSettings
{
    public float m_MaxTreeDistance = 300;
    public float m_MaxTreeDistanceThres = 295;

    // 5 meters when we're fading between LOD levels
    public float m_LODTranzitionThreshold = 5.0f;
    public float m_LODFadeSpeed = 5.0f;
}

public class TreeSystem : MonoBehaviour
{
    private static readonly int MAX_BATCH = 1000;

    // Data to add
    public TreeSystemSettings m_Settings;

    public Shader m_ShaderTreeMaster;
    public Shader m_ShaderBillboardMaster;

    public int m_ShaderIDFadeLOD;

    // TOOD: we also need scale I think
    public int m_ShaderIDBillboardScaleRotation;

    public static void SetMaterialBillProps(TreeSystemPrototypeData data, Material m)
    {
        Vector2[] uv = data.m_VertBillboardUVs;

        Vector4[] UV_U = new Vector4[8];
        Vector4[] UV_V = new Vector4[8];

        for (int i = 0; i < 8; i++)
        {
            // 4 by 4 elements
            UV_U[i].x = uv[4 * i + 0].x;
            UV_U[i].y = uv[4 * i + 1].x;
            UV_U[i].z = uv[4 * i + 2].x;
            UV_U[i].w = uv[4 * i + 3].x;

            UV_V[i].x = uv[4 * i + 0].y;
            UV_V[i].y = uv[4 * i + 1].y;
            UV_V[i].z = uv[4 * i + 2].y;
            UV_V[i].w = uv[4 * i + 3].y;
        }

        m.SetVectorArray("_UVVert_U", UV_U);
        m.SetVectorArray("_UVVert_V", UV_V);

        uv = data.m_HorzBillboardUVs;

        for (int i = 0; i < 1; i++)
        {
            // 4 by 4 elements
            UV_U[i].x = uv[4 * i + 0].x;
            UV_U[i].y = uv[4 * i + 1].x;
            UV_U[i].z = uv[4 * i + 2].x;
            UV_U[i].w = uv[4 * i + 3].x;

            UV_V[i].x = uv[4 * i + 0].y;
            UV_V[i].y = uv[4 * i + 1].y;
            UV_V[i].z = uv[4 * i + 2].y;
            UV_V[i].w = uv[4 * i + 3].y;
        }

        m.SetVector("_UVHorz_U", UV_U[0]);
        m.SetVector("_UVHorz_V", UV_V[0]);        
    }

    private void UpdateLODDataDistances(TreeSystemPrototypeData data)
    {
        LOD[] lods = data.m_TreePrototype.GetComponent<LODGroup>().GetLODs();
        TreeSystemLODData[] lodData = data.m_LODData;

        for (int i = 0; i < lodData.Length; i++)
        {
            // If it's the billboard move on
            if (i == lods.Length - 1)
                continue;

            TreeSystemLODData d = lodData[i];

            if (i == 0)
            {
                // If it's first 3D LOD
                d.m_StartDistance = 0;
                d.m_EndDistance = ((1.0f - lods[i].screenRelativeTransitionHeight) * m_Settings.m_MaxTreeDistance);
            }
            else if (i == lodData.Length - 2)
            {
                // If it's last 3D LOD
                d.m_StartDistance = ((1.0f - lods[i - 1].screenRelativeTransitionHeight) * m_Settings.m_MaxTreeDistance);
                d.m_EndDistance = m_Settings.m_MaxTreeDistance;
            }
            else
            {
                // If it's a LOD in between
                d.m_StartDistance = ((1.0f - lods[i - 1].screenRelativeTransitionHeight) * m_Settings.m_MaxTreeDistance);
                d.m_EndDistance = ((1.0f - lods[i].screenRelativeTransitionHeight) * m_Settings.m_MaxTreeDistance);
            }
        }
    }

    private void GenerateRuntimePrototypeData(TreeSystemPrototypeData data)
    {
        // All stuff that we draw must have billboard renderers, that's why it's trees
        LOD[] lods = data.m_TreePrototype.GetComponent<LODGroup>().GetLODs();
        TreeSystemLODData[] lodData = data.m_LODData;        

        // Set material's UV at runtime
        SetMaterialBillProps(data, data.m_BillboardBatchMaterial);
        SetMaterialBillProps(data, data.m_BillboardMasterMaterial);

        // Get the same tree stuff as the grass
        for (int i = 0; i < lodData.Length; i++)
        {
            if (lods[i].renderers.Length != 1)
                Debug.LogError("Renderer length != 1! Problem!");

            // Assume only one renderer            
            TreeSystemLODData d = lodData[i];
            
            // If it's last element
            if (i == lods.Length - 1)
            {
                d.m_Block = new MaterialPropertyBlock();                
            }
            else
            {
                d.m_Block = new MaterialPropertyBlock();

                GameObject weirdTree = Instantiate(lods[i].renderers[0].gameObject);
                weirdTree.hideFlags = HideFlags.HideAndDontSave;
                
                // Get the tree renderer
                d.m_TreeRenderer = weirdTree.GetComponent<MeshRenderer>();
                d.m_TreeRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                d.m_TreeRenderer.transform.SetParent(Camera.main.transform);
                d.m_TreeRenderer.transform.localPosition = new Vector3(0, 0, -2);
                d.m_TreeRenderer.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

                MeshFilter mf = d.m_TreeRenderer.GetComponent<MeshFilter>();

                // Get an instance mesh from the weird tree
                Mesh m = mf.mesh;

                // Set the new bounds so that it's always drawn
                Bounds b = m.bounds;
                b.Expand(50.0f);
                m.bounds = b;
                mf.mesh = m;
            }
        }
        
        // Update the LOD distances
        UpdateLODDataDistances(data);
    }
    
    public static TreeSystem Instance;
    
    public TreeSystemPrototypeData[] m_ManagedPrototypes;        
    private Dictionary<int, TreeSystemPrototypeData> m_ManagedPrototypesIndexed;
    
    public Mesh m_SystemQuad;

    // Managed terrain systems
    public TreeSystemTerrain[] m_ManagedTerrains;       
    
    void Awake()
    {        
        Instance = this;

        m_ShaderIDFadeLOD = Shader.PropertyToID("master_LODFade");
        m_ShaderIDBillboardScaleRotation = Shader.PropertyToID("_InstanceScaleRotation");

        // Generate runtime data
        for (int i = 0; i < m_ManagedPrototypes.Length; i++)
            GenerateRuntimePrototypeData(m_ManagedPrototypes[i]);
                
        // Build the dictionary based on the index
        m_ManagedPrototypesIndexed = new Dictionary<int, TreeSystemPrototypeData>();
        for (int i = 0; i < m_ManagedPrototypes.Length; i++)
            m_ManagedPrototypesIndexed.Add(m_ManagedPrototypes[i].m_TreePrototypeHash, m_ManagedPrototypes[i]);                        
    }

    public Terrain m_Terrain;
    
    private int[] m_IdxTemp = new int[MAX_BATCH];
    private float[] m_DstTemp = new float[MAX_BATCH];

    private Bounds m_Bounds;    

    private Action<Plane[], Matrix4x4> ExtractPlanes;
    private Plane[] planes = new Plane[6];

    void Start()
    {        
        m_Terrain.drawTreesAndFoliage = false;

        TreePrototype[] proto = m_Terrain.terrainData.treePrototypes;
        GameObject prefab = proto[0].prefab;

        for (int i = 0; i < m_Cells.Length; i++)
        {
            // TODO: maybe allocate dinamically at runtime to save memory
            m_Cells[i].m_InstanceData = new TreeSystemLODInstance[m_Cells[i].m_Instances.Length];
        }

        MethodInfo info = typeof(GeometryUtility).GetMethod("Internal_ExtractPlanes", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { typeof(Plane[]), typeof(Matrix4x4) }, null);
        ExtractPlanes = Delegate.CreateDelegate(typeof(Action<Plane[], Matrix4x4>), info) as Action<Plane[], Matrix4x4>;
    }


    public TreeSystemStructuredTrees[] m_Cells;

    private Vector3[] m_TempCorners = new Vector3[8];
    int issuedTrees = 0;
    
    void Update()
    {
        // if (useSpeedTree)
           // return;

        issuedTrees = 0;

        // Draw all trees instanced in MAX_BATCH chunks
        int tempIndex = 0;

        // TODO: calculate it without allocating
        // Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        Camera camera = Camera.main;
        ExtractPlanes(planes, camera.projectionMatrix * camera.worldToCameraMatrix);

        Vector3 pos = Camera.main.transform.position;

        // 0.7 is the aprox ratio between the diagonal and sides
        float boundsExtra = (m_Cells[0].m_Bounds.size.x * 1.414213f) / 2.0f;
        float cellDist = m_Settings.m_MaxTreeDistance + boundsExtra;

        for (int proto = 0; proto < m_ManagedPrototypes.Length; proto++)
        {
            TreeSystemLODData[] lod = m_ManagedPrototypes[proto].m_LODData;
            for (int i = 0; i < lod.Length; i++)
                lod[i].CopyBlock();
        }
        
        // Go bounds by bounds
        for (int cell = 0; cell < m_Cells.Length; cell++)
        {
            TreeSystemStructuredTrees str = m_Cells[cell];
            if (str.m_Instances.Length <= 0) continue;

            if (Vector3.Distance(pos, str.m_Bounds.center) < cellDist && GeometryUtility.TestPlanesAABB(planes, str.m_Bounds))
            {                 
                // If we are completely inside frustum we don't need to test each tree
                bool insideFrustum = TUtils.IsCompletelyInsideFrustum(planes, TUtils.BoundsCorners(ref m_Bounds, ref m_TempCorners));

                TreeSystemStoredInstance[] treeInstances = str.m_Instances;

                // Tree hashes that switch

                // Hm... if we take that hash it doesn't mean that it's the first visible one...
                int treeHash = treeInstances[0].m_TreeHash;
                int currentTreeHash = treeHash;

                float sqrtMaxDistance = m_Settings.m_MaxTreeDistance * m_Settings.m_MaxTreeDistance;
                float x, y, z;

                for (int treeIndex = 0; treeIndex < treeInstances.Length; treeIndex++)
                {
                    // 1.33 ms for 110k trees
                    // This is an order of magnitude faster than (treeInstances[treeIndex].m_WorldPosition - pos).sqrMagnitude
                    // since it does not initiate with a ctor an extra vector during the computation
                    x = treeInstances[treeIndex].m_WorldPosition.x - pos.x;
                    y = treeInstances[treeIndex].m_WorldPosition.y - pos.y;
                    z = treeInstances[treeIndex].m_WorldPosition.z - pos.z;

                    float distToTree = x * x + y * y + z * z;

                    // 17 ms for 110k trees
                    // float distToTree = (treeInstances[treeIndex].m_WorldPosition - pos).sqrMagnitude;

                    if (insideFrustum)
                    {
                        // If we are completely inside the frustum we don't need to check each individual tree's bounds
                        if (distToTree <= sqrtMaxDistance)
                        {
                            currentTreeHash = treeInstances[treeIndex].m_TreeHash;

                            if (tempIndex >= MAX_BATCH || treeHash != currentTreeHash)
                            {
                                IssueDrawTrees(m_ManagedPrototypesIndexed[treeHash], str, m_IdxTemp, m_DstTemp, tempIndex);
                                tempIndex = 0;

                                // Update the hash
                                treeHash = currentTreeHash;
                            }

                            m_IdxTemp[tempIndex] = treeIndex;
                            m_DstTemp[tempIndex] = Mathf.Sqrt(distToTree);
                            tempIndex++;

                            issuedTrees++;
                        }
                    }
                    else
                    {
                        // If we are not completely inside the frustum we need to check the bounds of each individual tree
                        if (distToTree <= sqrtMaxDistance && GeometryUtility.TestPlanesAABB(planes, treeInstances[treeIndex].m_WorldBounds))
                        {
                            currentTreeHash = treeInstances[treeIndex].m_TreeHash;

                            if (tempIndex >= MAX_BATCH || treeHash != currentTreeHash)
                            {
                                IssueDrawTrees(m_ManagedPrototypesIndexed[treeHash], str, m_IdxTemp, m_DstTemp, tempIndex);
                                tempIndex = 0;

                                // Update the hash
                                treeHash = currentTreeHash;
                            }

                            m_IdxTemp[tempIndex] = treeIndex;
                            m_DstTemp[tempIndex] = Mathf.Sqrt(distToTree);
                            tempIndex++;

                            issuedTrees++;
                        }
                    }
                } // End cell tree iteration
                                
                if (tempIndex > 0)
                {
                    // Get a tree hash from the first element of the array so that we know for sure that we use the correc prototype data
                    IssueDrawTrees(m_ManagedPrototypesIndexed[treeInstances[m_IdxTemp[0]].m_TreeHash], str,
                        m_IdxTemp, m_DstTemp, tempIndex);

                    tempIndex = 0;
                }
                // */
            } // End cell visibility determination
        } // End cell iteration        
    }

    // 5 is the maximum possible count of LOD levels
    private int[,] m_IdxLODTemp = new int[MAX_BATCH, 5];
    private int m_IdxLODTemp_0;
    private int m_IdxLODTemp_1;
    private int m_IdxLODTemp_2;
    private int m_IdxLODTemp_3;
    private int m_IdxLODTemp_4;

    private void IssueDrawTrees(TreeSystemPrototypeData data, TreeSystemStructuredTrees trees, int[] indices, float[] dist, int count)
    {
        TreeSystemLODData[] lodData = data.m_LODData;

        TreeSystemStoredInstance[] treeInstances = trees.m_Instances;
        TreeSystemLODInstance[] lodInstanceData = trees.m_InstanceData;

        int maxLod3D = data.m_MaxLod3DIndex;

        // Process LOD so that we know what to batch
        for(int i = 0; i < count; i++)
        {
            ProcessLOD(ref lodData, ref maxLod3D, ref lodInstanceData[indices[i]], ref dist[i]);
        }

        // Draw the processed lod
        for(int i = 0; i < count; i++)
        {
            int idx = indices[i];
            DrawProcessedLOD(ref lodData, ref maxLod3D, ref lodInstanceData[idx], ref treeInstances[idx]);
        }

        // TODO: Build the batched stuff if we want to LOD batch them using instancing
    }
    
    // Draw and process routines
    private void DrawProcessedLOD(ref TreeSystemLODData[] data, ref int maxLod3D, ref TreeSystemLODInstance lodInst, ref TreeSystemStoredInstance inst)
    {
        int lod = lodInst.m_LODLevel;

        // Draw the stuff with the material specific for each LOD
        Draw3DLOD(ref data[lod], ref lodInst, ref inst);

        if (lod == maxLod3D && lodInst.m_LODFullFade < 1)
        {
            // Since we only need for the last 3D lod the calculations...
            DrawBillboardLOD(ref data[lod + 1], ref lodInst, ref inst);
        }
    }

    /**
     * Must be called only if the camera distance is smaller than the maximum tree view distance.
     */
    private void ProcessLOD(ref TreeSystemLODData[] data, ref int max3DLOD, ref TreeSystemLODInstance inst, ref float cameraDistance)
    {
        if (cameraDistance < m_Settings.m_MaxTreeDistance - m_Settings.m_LODTranzitionThreshold)
        {
            for (int i = 0; i < data.Length - 1; i++)
            {
                if (data[i].IsInRange(cameraDistance))
                {
                    // Calculate lod tranzition value
                    if (cameraDistance > data[i].m_EndDistance - m_Settings.m_LODTranzitionThreshold)
                        inst.m_LODTransition = 1.0f - (data[i].m_EndDistance - cameraDistance) / m_Settings.m_LODTranzitionThreshold;
                    else
                        inst.m_LODTransition = 0.0f;

                    inst.m_LODLevel = i;
                    break;
                }
            }
            
            if (inst.m_LODLevel == max3DLOD && inst.m_LODFullFade < 1)
            {
                // If we are the last 3D lod then we animate nicely
                inst.m_LODFullFade += Time.deltaTime * m_Settings.m_LODFadeSpeed;
            }
            else if(inst.m_LODFullFade < 1)
            {
                // If we are not the lost LOD level simply set the value to 1
                inst.m_LODFullFade = 1f;
            }
        }
        else
        {
            inst.m_LODLevel = max3DLOD;
            if (inst.m_LODFullFade > 0) inst.m_LODFullFade -= Time.deltaTime * m_Settings.m_LODFadeSpeed;
        }
    }
    
    public void Draw3DLOD(ref TreeSystemLODData data, ref TreeSystemLODInstance lodInst, ref TreeSystemStoredInstance inst)
    {
        data.m_Block.SetVector(m_ShaderIDFadeLOD, new Vector4(lodInst.m_LODTransition, lodInst.m_LODFullFade, 0, 0));
        
        for (int mat = 0; mat < data.m_Materials.Length; mat++)
        {
            Graphics.DrawMesh(data.m_Mesh, inst.m_PositionMtx, data.m_Materials[mat], 0, null, mat, data.m_Block, true, true);
        }
    }

    public void DrawBillboardLOD(ref TreeSystemLODData data, ref TreeSystemLODInstance lodInst, ref TreeSystemStoredInstance inst)
    {
        data.m_Block.SetVector(m_ShaderIDFadeLOD, new Vector4(lodInst.m_LODTransition, 1.0f - lodInst.m_LODFullFade, 0, 0));

        // Set extra scale and stuff
        Vector4 extra = inst.m_WorldScale;
        extra.w = inst.m_WorldRotation;
        data.m_Block.SetVector(m_ShaderIDBillboardScaleRotation, extra);
        
        for (int mat = 0; mat < data.m_Materials.Length; mat++)
        {            
            Graphics.DrawMesh(data.m_Mesh, inst.m_PositionMtx, data.m_Materials[mat], 0, null, mat, data.m_Block, true, true);
        }
    }       
}
