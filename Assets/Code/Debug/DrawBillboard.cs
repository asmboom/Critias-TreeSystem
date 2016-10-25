using UnityEngine;
using System.Collections;

#if UNITY_EDITOR

using UnityEditor;

[CustomEditor(typeof(DrawBillboard))]
public class DrawBillboardEditor : Editor
{
    public override void OnInspectorGUI()
    {        
        DrawDefaultInspector();

        DrawBillboard bill = target as DrawBillboard;

        if (GUILayout.Button("Set"))
        {
            bill.Start();
        }
    }
}

#endif

[RequireComponent(typeof(MeshRenderer))]
public class DrawBillboard : MonoBehaviour
{
   public static void SetMaterialBillProps(Material m)
    {
        Vector2[] uv = FindObjectOfType<TreeSystem>().m_ManagedPrototypes[0].m_VertBillboardUVs;

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

        uv = FindObjectOfType<TreeSystem>().m_ManagedPrototypes[0].m_HorzBillboardUVs;

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

        Material bl = FindObjectOfType<TreeSystem>().m_ManagedPrototypes[0].m_BillboardMasterMaterial;

        m.SetVector("_UVHorz_U", UV_U[0]);
        m.SetVector("_UVHorz_V", UV_V[0]);

        m.SetTexture("_MainTex", bl.GetTexture("_MainTex"));
        m.SetTexture("_BumpMap", bl.GetTexture("_BumpMap"));
    }

    public Mesh m_SystemQuad;
    public Material m_Material;    

	public void Start ()
    {        
        MeshRenderer r = GetComponent<MeshRenderer>();

        m_SystemQuad = GetComponent<MeshFilter>().mesh;

        m_Material = r.sharedMaterial;

        // Set the arrays
        Bounds b = m_SystemQuad.bounds;
        b.Expand(30);
        m_SystemQuad.bounds = b;

        // Get them 4 by 4 and set them
        SetMaterialBillProps(m_Material);
    }
	
	void Update ()
    {
        Graphics.DrawMesh(m_SystemQuad, transform.localToWorldMatrix, m_Material, 0);
	}
}
