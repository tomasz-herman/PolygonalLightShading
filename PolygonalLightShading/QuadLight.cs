using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolygonalLightShading
{
    public class QuadLight
    {
        private readonly Vector3[] vertices  = new Vector3[4];
        public Matrix4 modelMatrix { get; set; } = Matrix4.Identity;

        public QuadLight(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            vertices[0] = p1;
            vertices[1] = p2;
            vertices[2] = p3;
            vertices[3] = p4;
        }

        public Vector3 GetVertex(int i)
        {
            return (new Vector4(vertices[i], 1) * modelMatrix).Xyz;
        }

        public IEnumerable<Vector3> GetVertices()
        {
            for (int i = 0; i < 4; i++)
                yield return GetVertex(i);
        }
    }
}
