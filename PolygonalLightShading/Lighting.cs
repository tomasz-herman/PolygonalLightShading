﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace PolygonalLightShading
{
    public class Lighting : IEnumerable<QuadLight>
    {
        private List<QuadLight> lights = new List<QuadLight>();

        public void Add(QuadLight light)
        {
            lights.Add(light);
        }

        public IEnumerator<QuadLight> GetEnumerator()
        {
            foreach (var light in lights)
                yield return light;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public QuadLight this[int index]
        {
            get { return lights[index]; }
        }


        public float[] GetVertexData()
        {
            var data = new List<float>();
            foreach (var light in lights)
            {
                foreach (var vertex in light.GetVertices())
                {
                    data.Add(vertex.X);
                    data.Add(vertex.Y);
                    data.Add(vertex.Z);
                }
            }

            return data.ToArray();
        }

        public float[] GetColorData()
        {
            var data = new List<float>();
            foreach (var light in lights)
            {
                var color = light.Color;
                data.Add(color.X);
                data.Add(color.Y);
                data.Add(color.Z);
            }

            return data.ToArray();
        }

        public int[] GetTexturesData()
        {
            var data = new List<int>();
            for (var index = 0; index < lights.Count; index++)
            {
                lights[index].Texture.Use(TextureUnit.Texture0 + index);
                data.Add(index);
            }

            return data.ToArray();
        }
        
        public int[] GetTexturesLodData()
        {
            var data = new List<int>();
            for (var index = 0; index < lights.Count; index++)
            {
                lights[index].TextureLod.Use(TextureUnit.Texture0 + index);
                data.Add(index);
            }

            return data.ToArray();
        }
        
        public int[] GetTexturesUsageData()
        {
            var data = new List<int>();
            for (var index = 0; index < lights.Count; index++)
            {
                data.Add(lights[index].UseTexture ? 1 : 0);
            }

            return data.ToArray();
        }
        
        public float[] GetIntensityData()
        {
            return lights.Select(l => l.Intensity).ToArray();
        }

        public int[] GetTwoSidedData()
        {
            return lights.Select(l => l.TwoSided ? 1 : 0).ToArray();
        }

        public int Count() { return lights.Count; }
    }
}