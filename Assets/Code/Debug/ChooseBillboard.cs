using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

[CustomEditor(typeof(ChooseBillboard))]
public class ChooseBillboardEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if(GUILayout.Button("Calculate"))
        {

        }
    }
}

public class ChooseBillboard : MonoBehaviour
{
    public Transform m_UtilityTransform;

    public int m_ImageCount = 8;

    public float m_InstanceRotation = 0;

    public GameObject[] m_Texts;

    private void OnDrawGizmos()
    {
        if(m_Texts == null || m_Texts.Length != m_ImageCount || m_Texts[0] == null)
        {
            if(m_Texts != null)
            {
                for (int i = 0; i < m_Texts.Length; i++)
                    DestroyImmediate(m_Texts[i]);
            }

            m_Texts = new GameObject[m_ImageCount];            

            for (int i = 0; i < m_Texts.Length; i++)
            {
                GameObject text = new GameObject();
                text.transform.parent = transform;

                text.AddComponent<TextMesh>();

                // text.hideFlags = HideFlags.HideAndDontSave;

                m_Texts[i] = text;
            }
        }

        Gizmos.DrawWireSphere(transform.position, 0.3f);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(m_UtilityTransform.position, 0.3f);        

        // Draw the axe
        float angle = 360.0f / m_ImageCount;        

        for(int i = 0; i < m_ImageCount; i++)
        {
            Vector3 pos = m_UtilityTransform.position;

            // Rotate it and draw the gizmos
            m_UtilityTransform.RotateAround(transform.position, Vector3.up, angle * i);
            m_Texts[i].transform.position = m_UtilityTransform.position;            

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(m_UtilityTransform.position, 0.15f);

            // Make calculations to see what we get when we apply the billboard calculations
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(m_UtilityTransform.position, transform.position);

            Vector3 v1 = new Vector3(-1, 0, 0);
            Vector3 v2 = m_UtilityTransform.position - transform.position;

            v1.Normalize();

            float dotA, detA;

            dotA = v1.x * v2.x + v1.z * v2.z;
            detA = v1.x * v2.z - v1.z * v2.x;

            float angle180 = (Mathf.Atan2(detA, dotA) ) * Mathf.Rad2Deg - m_InstanceRotation;
            float angle360 = ((Mathf.Atan2(detA, dotA) + 180 * Mathf.Deg2Rad) * Mathf.Rad2Deg - m_InstanceRotation);

            if (angle360 < 0) angle360 = 360 + angle360;

            m_Texts[i].GetComponent<TextMesh>().text =
                "b idx: " + Mathf.CeilToInt((Vector3.Angle(v1, v2) / angle)) +
                "\nb idx a180: " + (angle180 / angle) % 8 +
                "\nb idx a360: " + (angle360 / angle) % 8 + // Correct one
                "\n" + i + 
                "\na: " + Vector3.Angle(v1, v2) +
                "\na180: " + (angle180) +
                "\na360: " + (angle360);            

            // Last operation
            m_UtilityTransform.rotation = Quaternion.identity;
            m_UtilityTransform.position = pos;
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, new Vector3(1, 0));
        Gizmos.DrawWireCube(transform.position + new Vector3(1, 0), new Vector3(0.3f, 0.3f, 0.3f));
    }    

    
    void Start ()
    {
        
	}
	
	
	void Update ()
    {
		
	}
}


#endif