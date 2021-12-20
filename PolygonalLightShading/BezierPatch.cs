using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace PolygonalLightShading
{
    public class BezierPatch : IDisposable
    {
        public Mesh Patch { get; private set; }
        public Mesh Mesh { get; private set; }

        public static BezierPatch Create(Func<int, int, float> heights = null)
        {
            heights ??= (i, j) => 0;
            
            BezierPatch patch = new BezierPatch();

            Vector3[,] surface = new Vector3[13, 13];
            
            for (int i = 0; i < 13; i++)
            {
                for (int j = 0; j < 13; j++)
                {
                     surface[j, i] = new Vector3(i - 6, heights(j, i), j - 6);
                }
            }
            
            List<int> patchIndices = new List<int>();

            for (int i = 0; i < 12; i += 3)
            {
                for (int j = 0; j < 12; j += 3)
                {
                    patchIndices.AddRange(
                        new[]
                        {
                            (j + 0) * 13 + (i + 0),
                            (j + 1) * 13 + (i + 0),
                            (j + 2) * 13 + (i + 0),
                            (j + 3) * 13 + (i + 0),

                            (j + 0) * 13 + (i + 1),
                            (j + 1) * 13 + (i + 1),
                            (j + 2) * 13 + (i + 1),
                            (j + 3) * 13 + (i + 1),
                            
                            (j + 0) * 13 + (i + 2),
                            (j + 1) * 13 + (i + 2),
                            (j + 2) * 13 + (i + 2),
                            (j + 3) * 13 + (i + 2),
                            
                            (j + 0) * 13 + (i + 3),
                            (j + 1) * 13 + (i + 3),
                            (j + 2) * 13 + (i + 3),
                            (j + 3) * 13 + (i + 3)
                        }
                    );
                }
            }

            List<float> positions = new List<float>();
            List<int> meshIndices = new List<int>();
            for (int i = 0; i < surface.GetLength(0); i++)
            for (int j = 0; j < surface.GetLength(1); j++)
            {
                Vector3 pos = surface[i, j];
                positions.AddRange(new[] {pos.X, pos.Y, pos.Z});
                
                int idx = j + i * surface.GetLength(1);

                if (j + 1 < surface.GetLength(1))
                {
                    meshIndices.AddRange(new[]{idx, idx + 1});
                }

                if (i + 1 < surface.GetLength(0))
                {
                    meshIndices.AddRange(new[]{idx, idx + surface.GetLength(1)});
                }
            }
            
            patch.Mesh = new Mesh(
                positions.ToArray(),
                meshIndices.ToArray(),
                PrimitiveType.Lines
            );
            
            patch.Patch = new Mesh(
                positions.ToArray(), 
                patchIndices.ToArray(), 
                PrimitiveType.Patches
            );

            return patch;
        }

        public void Dispose()
        {
            Patch?.Dispose();
            Mesh?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}