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
        public Matrix4 ModelMatrix
        { 
            get { return Matrix4.CreateRotationZ(Utils.DegToRad(Rotation.X)) * Matrix4.CreateRotationY(Utils.DegToRad(Rotation.Y)) *
                    Matrix4.CreateRotationX(Utils.DegToRad(Rotation.Z)) * Matrix4.CreateScale(Scale) * Matrix4.CreateTranslation(Position); }
        }
        public Mesh FrontMesh { get; private set; }
        public Mesh BackMesh { get; private set; }
        public Vector3 Color { get; set; } = new Vector3(1, 1, 1);
        public Texture Texture { get; set; }
        public bool UseTexture = false;
        public float Intensity = 1f;
        public bool TwoSided = false;
        public Vector3 Position = new Vector3(0, 0, 0);
        public Vector3 Rotation = new Vector3(0, 0, 0);
        public float Scale = 1;

        public QuadLight(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            vertices[0] = p1;
            vertices[1] = p2;
            vertices[2] = p3;
            vertices[3] = p4;

            var frontVertexPosiitions = new List<float>();
            var backVertexPosiitions = new List<float>();

            foreach(var vertex in vertices)
            {
                frontVertexPosiitions.Add(vertex.X);
                frontVertexPosiitions.Add(vertex.Y);
                frontVertexPosiitions.Add(vertex.Z);
            }

            foreach(var vertex in vertices.Reverse())
            {
                backVertexPosiitions.Add(vertex.X);
                backVertexPosiitions.Add(vertex.Y);
                backVertexPosiitions.Add(vertex.Z);
            }

            //light shader will not use these, but you can use these to pass texture coordinates for textured lights
            var vertexNormals = Enumerable.Repeat(0f, 12).ToArray();
            var vertexColors = Enumerable.Repeat(0f, 16).ToArray();
            FrontMesh = new Mesh(frontVertexPosiitions.ToArray(), vertexNormals, vertexColors, new float[]{1, 0, 1, 1, 0, 1, 0, 0}, new int[] { 0, 1, 2, 0, 2, 3 }, OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles);
            BackMesh = new Mesh(backVertexPosiitions.ToArray(), vertexNormals, vertexColors, new float[]{0, 0, 0, 1, 1, 1, 1, 0}, new int[] { 0, 1, 2, 0, 2, 3 }, OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles);

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
