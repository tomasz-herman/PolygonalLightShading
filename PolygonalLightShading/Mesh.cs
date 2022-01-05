using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace PolygonalLightShading
{
    public class Mesh : IDisposable
    {
        public int Vao { get; private set; }
        private List<int> Vbos { get; } = new ();
        public PrimitiveType Type { get; }
        public int Count { get; }
        public Matrix4 ModelMatrix { get; set; } = Matrix4.Identity;

        public Mesh(float[] positions, float[] normals, float[] colors, float[] textures, int[] indices, PrimitiveType type)
        {
            Type = type;
            Count = indices.Length;
            Load(positions, normals, colors, textures, indices);
        }

        public void Load(float[] positions, float[] normals, float[] colors, float[] textures, int[] indices) {
            Vao = GL.GenVertexArray();
            GL.BindVertexArray(Vao);
            if(positions != null) LoadData(positions, 0, 3);
            if(normals != null) LoadData(normals, 1, 3);
            if(colors != null) LoadData(colors, 2, 4);
            if(textures != null) LoadData(textures, 3, 2);

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