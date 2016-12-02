using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugUtils : MonoBehaviour
{
    public TreeSystem m_TreeSystem;

    public UnityEngine.UI.Text m_Info;
    
    public float m_Distance = 300;
    public bool m_UseInstancing = true;

    void Start()
    {
        m_TreeSystem.m_UseInstancing = m_UseInstancing;
        m_TreeSystem.SetTreeDistance(m_Distance);
    }

	void OnGUI()
    {
        float dist = GUI.HorizontalSlider(new Rect(20, 20, 100, 20), m_Distance, 0f, 2000f);
        GUI.Label(new Rect(20, 40, 200, 20), "Distance: " + m_Distance);

        if(dist != m_Distance)
        {
            m_Distance = Mathf.Floor(dist);
            m_TreeSystem.SetTreeDistance(m_Distance);
        }

        bool inst = GUI.Toggle(new Rect(20, 60, 200, 20), m_UseInstancing, " Use Instancing");

        if(inst != m_UseInstancing)
        {
            m_UseInstancing = inst;
            m_TreeSystem.m_UseInstancing = m_UseInstancing;
        }

        if(m_Info)
            m_Info.text = m_TreeSystem.GetDrawInfo();
    }
}
