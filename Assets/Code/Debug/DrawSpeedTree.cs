using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DrawSpeedTree : MonoBehaviour
{
    public GameObject m_SpeedTree;

    private Mesh m_Mesh;
    private Material[] m_Materials;

    public Vector4 unity_LODFade;
    
    void Start ()
    {
        m_Mesh = m_SpeedTree.GetComponent<MeshFilter>().sharedMesh;
        m_Materials = m_SpeedTree.GetComponent<MeshRenderer>().sharedMaterials;        
	}
		
	void Update ()
    {        
        for (int i = 0; i < m_Materials.Length; i++)
        {
            Graphics.DrawMesh(m_Mesh, transform.localToWorldMatrix, m_Materials[i], 0, null, i);
        }
    }
}
