using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DrawSpeedTree : MonoBehaviour
{
    public GameObject m_SpeedTree;
    public bool m_DrawInstanced;
    public bool m_UseMatSubMeshIfInstanced;

    public Mesh m_Mesh;
    public Material[] m_Materials;

    public Vector4 unity_LODFade;

    public List<Mesh> m_SubMeshes;

    private void ExtractSubMesh(Mesh m, int index)
    {        
        Mesh subMesh = Instantiate(m);
        subMesh.subMeshCount = 1;
        subMesh.triangles = m.GetTriangles(index);

        m_SubMeshes.Add(subMesh);
    }

    void Start ()
    {
        m_Mesh = m_SpeedTree.GetComponent<MeshFilter>().sharedMesh;
        m_Materials = m_SpeedTree.GetComponent<MeshRenderer>().sharedMaterials;

        for(int i = 0; i < m_Mesh.subMeshCount; i++)
            ExtractSubMesh(m_Mesh, i);
	}
		
	void Update ()
    {
        if (m_DrawInstanced)
        {
            for (int i = 0; i < m_Materials.Length; i++)
            {
                if(m_UseMatSubMeshIfInstanced)
                {
                    Graphics.DrawMeshInstanced(m_SubMeshes[i], 0, m_Materials[i], new Matrix4x4[] { transform.localToWorldMatrix }, 1);
                }
                else
                {
                    Graphics.DrawMeshInstanced(m_Mesh, i, m_Materials[i], new Matrix4x4[] { transform.localToWorldMatrix }, 1);
                }                
            }
        }
        else
        {
            for (int i = 0; i < m_Materials.Length; i++)
            {
                Graphics.DrawMesh(m_Mesh, transform.localToWorldMatrix, m_Materials[i], 0, null, i);
            }
        }
    }
}
