using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public int Count() { return lights.Count; }
    }
}