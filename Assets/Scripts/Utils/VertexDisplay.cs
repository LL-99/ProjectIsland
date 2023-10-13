using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    /// <summary>
    /// Debugging class that can be attached to any mesh object to draw all of its vertices without requiring a wireframe shading mode in the scene view
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [ExecuteAlways]
    public class VertexDisplay : MonoBehaviour
    {
        MeshFilter mf;
        [SerializeField] private bool drawWhenNotSelected = false;

        [SerializeField] private bool showIndexNumbers = false;

        [SerializeField] private bool showPositions = false;
        [SerializeField] private bool useWorldSpace = false;
        [SerializeField] private Color textColor = Color.blue;

        [SerializeField] private bool showVertexSpheres = false;
        [SerializeField] private Color vertexSphereColors = Color.red;
        [SerializeField] private float vertexSphereRadius = .1f;

        private void Start()
        {
            mf = GetComponent<MeshFilter>();
        }

        private void OnDrawGizmosSelected()
        {
            if (drawWhenNotSelected)
                return;

            DrawVertices();
        }

        private void OnDrawGizmos()
        {
            if (!drawWhenNotSelected)
                return;

            DrawVertices();
        }

        void DrawVertices()
        {
            if (!showIndexNumbers && !showPositions && !showVertexSpheres) return;

            var m = mf.sharedMesh;
            if (m == null) m = mf.mesh;
            if (m == null) return;

            var verts = m.vertices;

            Gizmos.color = vertexSphereColors;
            for(int i = 0; i < verts.Length; i++)
            {
                //var pos = Vector3.Scale(transform.lossyScale, (transform.rotation * verts[i])) + transform.position;
                var pos = transform.TransformPoint(verts[i]);

                if (showVertexSpheres)
                {
                    Gizmos.DrawSphere(pos, vertexSphereRadius);
                }
                if (showIndexNumbers || showPositions)
                {
                    string str = "";

                    if (showIndexNumbers)
                        str += i;

                    if (showPositions)
                    {
                        if (showIndexNumbers)
                            str += "\n";

                        string posStr = "";

                        if(useWorldSpace)
                            posStr = $"{Mathf.Round(pos.x * 10f) / 10f}; {Mathf.Round(pos.y * 10f) / 10f}; {Mathf.Round(pos.z * 10f) / 10f}";
                        else
                            posStr = $"{Mathf.Round(verts[i].x * 10f) / 10f}; {Mathf.Round(verts[i].y * 10f) / 10f}; {Mathf.Round(verts[i].z * 10f) / 10f}";

                        str += posStr;
                    }

                    DebugUtils.DrawString(str, pos, textColor, Vector2.zero);
                }
            }
        }
    }
}