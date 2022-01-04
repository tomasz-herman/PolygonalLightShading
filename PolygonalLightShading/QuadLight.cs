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
        private Matrix4 modelMatrix = Matrix4.Identity;
        public Matrix4 ModelMatrix { get { return modelMatrix; } set { modelMatrix = value; Mesh.ModelMatrix = value; } }
        public Mesh Mesh { get; private set; }
        public Vector3 Color { get; set; } = new Vector3(1, 1, 1);

        public QuadLight(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            vertices[0] = p1;
            vertices[1] = p2;
            vertices[2] = p3;
            vertices[3] = p4;

            var vertexPositions = new List<float>();
            foreach(var vertex in vertices)
            {
                vertexPositions.Add(vertex.X);
                vertexPositions.Add(vertex.Y);
                vertexPositions.Add(vertex.Z);
            }

            //light shader will not use these, but you can use these to pass texture coordinates for textured lights
            var vertexNormals = Enumerable.Repeat(0f, 12).ToArray();
            var vertexColors = Enumerable.Repeat(0f, 16).ToArray();
            Mesh = new Mesh(vertexPositions.ToArray(), vertexNormals, vertexColors, new int[] { 0, 1, 2, 0, 2, 3 }, OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles);
        }

        public Vector3 GetVertex(int i)
        {
            return (new Vector4(vertices[i], 1) * ModelMatrix).Xyz;
        }

        public IEnumerable<Vector3> GetVertices()
        {
            for (int i = 0; i < 4; i++)
                yield return GetVertex(i);
        }
    }
}
