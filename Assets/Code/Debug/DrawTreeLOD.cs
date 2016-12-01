using UnityEngine;
using System.Collections;




public class DrawTreeLOD : MonoBehaviour
{
    public TreeSystem system;
    public TreeSystemPrototypeData protoData;

    // Instance
    public TreeSystemLODInstance instance;

    float THRESHOLD;
    float procDistance;
    
    void Start()
    {
        system = FindObjectOfType<TreeSystem>();
        protoData = system.m_ManagedPrototypes[0];

        THRESHOLD = system.GetTreeTanzitionThreshold();
        procDistance = system.GetTreeDistance() - THRESHOLD;
    }
    
    void Update()
    {
        // TODO: only update trees that are visible
        for (int i = 0; i < protoData.m_LODData.Length; i++)
            protoData.m_LODData[i].CopyBlock();

        float distance = Vector3.Distance(Camera.main.transform.position, transform.position);
        
        ProcessLOD(protoData.m_LODData, ref instance, ref distance);
        DrawProcessedLOD(protoData.m_LODData, ref instance);        
    }

    public void DrawProcessedLOD(TreeSystemLODData[] data, ref TreeSystemLODInstance inst)
    {
        int lod = inst.m_LODLevel;

        // Draw the stuff with the material specific for each LOD
        Draw3DLOD(data[lod], ref inst);

        if (lod == protoData.m_MaxLod3DIndex && inst.m_LODFullFade < 1)
        {
            // Since we only need for the last 3D lod the calculations...
            DrawBillboardLOD(data[lod + 1], ref inst);
        }
    }

    public float m_FadeSpeed = 5.0f;

    /**
     * Must be called only if the camera distance is smaller than the maximum tree view distance.
     */
    public void ProcessLOD(TreeSystemLODData[] data, ref TreeSystemLODInstance inst, ref float cameraDistance)
    {
        if(cameraDistance < procDistance)
        {
            for (int i = 0; i < data.Length - 1; i++)
            {
                if (data[i].IsInRange(cameraDistance))
                {
                    // Calculate lod tranzition value
                    if (cameraDistance > data[i].m_EndDistance - THRESHOLD)
                        inst.m_LODTransition = 1.0f - (data[i].m_EndDistance - cameraDistance) / THRESHOLD;
                    else
                        inst.m_LODTransition = 0.0f;

                    inst.m_LODLevel = i;
                    break;
                }
            }

            if (inst.m_LODFullFade < 1) inst.m_LODFullFade += Time.deltaTime * m_FadeSpeed;
        }
        else
        {
            inst.m_LODLevel = 2;
            if(inst.m_LODFullFade >= 0) inst.m_LODFullFade -= Time.deltaTime * m_FadeSpeed;
        }
    }

    public void Draw3DLOD(TreeSystemLODData data, ref TreeSystemLODInstance inst)
    {
        // data.m_Block.SetVector(system.m_ShaderIDFadeLOD, new Vector4(inst.m_LODTransition, inst.m_LODFullFade, 0, 0));

        for (int mat = 0; mat < data.m_Materials.Length; mat++)
        {
            Graphics.DrawMesh(data.m_Mesh, transform.localToWorldMatrix, data.m_Materials[mat], mat, null, mat, data.m_Block, true, true);
        }
    }

    public void DrawBillboardLOD(TreeSystemLODData data, ref TreeSystemLODInstance inst)
    {
        // data.m_Block.SetVector(system.m_ShaderIDFadeLOD, new Vector4(inst.m_LODTransition, 1.0f - inst.m_LODFullFade, 0, 0));

        for (int mat = 0; mat < data.m_Materials.Length; mat++)
        {            
            Graphics.DrawMesh(data.m_Mesh, transform.localToWorldMatrix, data.m_Materials[mat], mat, null, mat, data.m_Block, true, true);
        }
    }
}
