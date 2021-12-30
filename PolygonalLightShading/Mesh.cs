using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics.OpenGL4;

namespace PolygonalLightShading
{
    public class Mesh : IDisposable
    {
        public int Vao { get; private set; }
        private List<int> Vbos { get; } = new ();
        public PrimitiveType Type { get; }
        public int Count { get; }

        public Mesh(float[] positions, int[] indices, PrimitiveType type)
        {
            Type = type;
            Count = indices.Length;
            Load(positions, indices);
        }

        public void Load(float[] positions, int[] indices) {
            Vao = GL.GenVertexArray();
            GL.BindVertexArray(Vao);
            LoadData(positions, 0, 3);
            //TODO load normals and colors properly
            Random rnd = new Random(3334);
            var normals = new List<float>();
            var colors = new List<float>();
            int vCount = positions.Length / 3;
            for(int i = 0; i < vCount; i++)
            {
                normals.Add(0);
                normals.Add(1);
                normals.Add(0);

                colors.Add(0.2f + 0.8f * (float)rnd.NextDouble());
                colors.Add(0.2f + 0.8f * (float)rnd.NextDouble());
                colors.Add(0.2f + 0.8f * (float)rnd.NextDouble());
                colors.Add(1);
            }
            LoadData(normals.ToArray(), 1, 3);
            LoadData(colors.ToArray(), 2, 4);

            LoadIndices(indices);
            GL.BindVertexArray(0);
        }

        public void LoadData(float[] data, int index, int size) {
            var vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(float), data, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(index);
            GL.VertexAttribPointer(index, size, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            Vbos.Add(vbo);
        }

        public void LoadIndices(int[] data) {
            var vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, data.Length * sizeof(int), data, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            Vbos.Add(vbo);
        }

        public void Render()
        {
            GL.BindVertexArray(Vao);
            GL.DrawElements(Type, Count, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
        }

        public void Dispose()
        {
            GL.DeleteVertexArray(Vao);
            foreach (var vbo in Vbos)
            {
                GL.DeleteBuffer(vbo);
            }
            Vbos.Clear();
            GC.SuppressFinalize(this);
        }
    }
}