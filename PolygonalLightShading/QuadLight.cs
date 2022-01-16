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
        public Matrix4 ModelMatrix =>
            Matrix4.CreateScale(Width, Height, 1) *
            Matrix4.CreateRotationZ(Utils.DegToRad(Rotation.X)) * 
            Matrix4.CreateRotationY(Utils.DegToRad(Rotation.Y)) *
            Matrix4.CreateRotationX(Utils.DegToRad(Rotation.Z)) * 
            Matrix4.CreateTranslation(Position);

        public Mesh FrontMesh { get; private set; }
        public Mesh BackMesh { get; private set; }
        public Vector3 Color { get; set; } = new(1, 1, 1);
        public Texture Texture { get; set; }
        public bool UseTexture = false;
        public float Intensity = 1f;
        public bool TwoSided = false;
        public Vector3 Position = new(0, 0, 0);
        public Vector3 Rotation = new(0, 0, 0);
        public float Width = 1;
        public float Height = 1;

        public QuadLight(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            vertices[0] = p1;
            vertices[1] = p2;
            vertices[2] = p3;
            vertices[3] = p4;

            var frontVertexPositions = new List<float>();
            var backVertexPositions = new List<float>();

            foreach(var vertex in vertices)
            {
                frontVertexPositions.Add(vertex.X);
                frontVertexPositions.Add(vertex.Y);
                frontVertexPositions.Add(vertex.Z);
            }

            foreach(var vertex in vertices.Reverse())
            {
                backVertexPositions.Add(vertex.X);
                backVertexPositions.Add(vertex.Y);
                backVertexPositions.Add(vertex.Z);
            }

            //light shader will not use these, but you can use these to pass texture coordinates for textured lights
            var vertexNormals = Enumerable.Repeat(0f, 12).ToArray();
            var vertexColors = Enumerable.Repeat(0f, 16).ToArray();
            FrontMesh = new Mesh(frontVertexPositions.ToArray(), vertexNormals, vertexColors, new float[]{1, 0, 1, 1, 0, 1, 0, 0}, new int[] { 0, 1, 2, 0, 2, 3 }, OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles);
            BackMesh = new Mesh(backVertexPositions.ToArray(), vertexNormals, vertexColors, new float[]{0, 0, 0, 1, 1, 1, 1, 0}, new int[] { 0, 1, 2, 0, 2, 3 }, OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles);

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
