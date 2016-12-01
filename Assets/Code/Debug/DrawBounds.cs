using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DrawBounds : MonoBehaviour
{
    public MeshFilter m_Bounds;
    public MeshRenderer m_Renderer;
    public Collider m_Collider;
    
    void Start()
    {
        m_Bounds = GetComponent<MeshFilter>();
        m_Renderer = GetComponent<MeshRenderer>();
        m_Collider = GetComponent<Collider>();
    }

    void OnDrawGizmosSelected()
    {
        if (m_Bounds)
        {
            Bounds b = m_Bounds.sharedMesh.bounds;

            b = LocalToWorld(b, transform.localToWorldMatrix);

            Vector3 center = b.center;
            float radius = b.extents.magnitude;

            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(center, radius);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(b.center, b.size);
        }

        if (m_Renderer)
        {
            Gizmos.color = Color.red;
            float offset = 0.1f;
            Gizmos.DrawWireCube(m_Renderer.bounds.center, m_Renderer.bounds.size + new Vector3(offset, offset, offset));
        }

        if(m_Collider)
        {
            Gizmos.color = Color.cyan;
            float offset = 0.05f;
            Gizmos.DrawWireCube(m_Collider.bounds.center, m_Collider.bounds.size + new Vector3(offset, offset, offset));
        }
    }

    public static Bounds LocalToWorld(Bounds box, Matrix4x4 m)
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
}
