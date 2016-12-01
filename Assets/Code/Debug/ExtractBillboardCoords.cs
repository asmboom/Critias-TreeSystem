using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(ExtractBillboardCoords))]
public class ExtractBillboardCoordsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ExtractBillboardCoords e = target as ExtractBillboardCoords;

        if(GUILayout.Button("Extract"))
        {
            e.Extract();
        }

        if(GUILayout.Button("Generate Billboard Mesh"))
        {
            e.Generate();
        }

        DrawDefaultInspector();
    }
}

[ExecuteInEditMode]
public class ExtractBillboardCoords : MonoBehaviour {

    public GameObject m_BillboardContainer;

    public MeshRenderer r;
    public MeshFilter f;
    
	void Start ()
    {
        r = GetComponent<MeshRenderer>();
        f = GetComponent<MeshFilter>();
	}
	
    public void Generate()
    {
        if (!m_BillboardContainer)
            return;

        BillboardRenderer b = m_BillboardContainer.GetComponent<BillboardRenderer>();

        if (b)
        {
            Mesh m = new Mesh();
            m.name = "Billboard";

            Vector2[] verts = b.billboard.GetVertices();
            Vector3[] newverts = new Vector3[verts.Length];

            for(int i = 0; i < verts.Length; i++)
                newverts[i] = verts[i];

            // Set the indices
            ushort[] indices = b.billboard.GetIndices();
            int[] newindices = new int[indices.Length];

            for (int i = 0; i < indices.Length; i++)
            {
                newindices[i] = indices[i];
            }

            m.vertices = newverts;
            m.triangles = newindices;

            // Set the new mesh
            f.sharedMesh = m;
        }        
    }

	public void Extract()
    {
        if (!m_BillboardContainer)
            return;

        BillboardRenderer b = m_BillboardContainer.GetComponent<BillboardRenderer>();

        if(b)
        {
            List<Vector4> coords = new List<Vector4>();

            b.billboard.GetImageTexCoords(coords);

            Debug.Log("Coord count: " + coords.Count);

            for (int i = 0; i < coords.Count; i++)
            {
                Vector4 c = coords[i];
                Debug.Log("Coord at: " + i + " is: (" + c.x + " ; " + c.y + " ; " + c.z + " ; " + c.w + ")");
            }
            
            Vector2[] verts = b.billboard.GetVertices();

            Debug.Log("Verts: " + verts.Length);

            for (int i = 0; i < verts.Length; i++)
            {
                Log.i("Index: " + i + " vert: " + verts[i]);
            }

            ushort[] indices = b.billboard.GetIndices();

            Debug.Log("Index count: " + indices.Length);
            for (int i = 0; i < indices.Length; i++)
            {
                Log.i("Index: " + i + " trindex: " + indices[i]);
            }
        }
        else
        {
            Debug.Log("Nothing attached!");
        }
    }
}

#endif