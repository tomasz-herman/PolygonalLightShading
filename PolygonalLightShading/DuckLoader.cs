using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolygonalLightShading
{
    public static class DuckLoader
    {
        public static Mesh Load(Stream stream, Vector4 color)
        {
            using (StreamReader sr = new StreamReader(stream))
            {
				string line;

				line = sr.ReadLine();
				int vertexCount = int.Parse(line);
				var vertexPositions = new List<float>(3* vertexCount);
				var vertexNormals = new List<float>(3* vertexCount);
				var vertexColors = new List<float>(4 * vertexCount);
				for (int i = 0; i < vertexCount; i++)
				{
					line = sr.ReadLine();
					string[] str = line.Split(' ');

					vertexPositions.Add(Utils.ParseFloat(str[0]));
					vertexPositions.Add(Utils.ParseFloat(str[1]));
					vertexPositions.Add(Utils.ParseFloat(str[2]));
					vertexNormals.Add(Utils.ParseFloat(str[3]));
					vertexNormals.Add(Utils.ParseFloat(str[4]));
					vertexNormals.Add(Utils.ParseFloat(str[5]));

					vertexColors.Add(color.X);
					vertexColors.Add(color.Y);
					vertexColors.Add(color.Z);
					vertexColors.Add(color.W);
				}

				line = sr.ReadLine();
				int triangleCount = int.Parse(line);
				var indices = new List<int>(3 * triangleCount);

				for (int i = 0; i < triangleCount; i++)
				{
					string[] str = sr.ReadLine().Split(' ');

					indices.Add(int.Parse(str[0]));
					indices.Add(int.Parse(str[1]));
					indices.Add(int.Parse(str[2]));
				}

				return new Mesh(vertexPositions.ToArray(), vertexNormals.ToArray(), vertexColors.ToArray(), null, indices.ToArray(), OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles);
			}
        }
    }
}
