using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PolygonalLightShading
{
    public static class Utils
    {
        public static float[] Read(string file)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream($"PolygonalLightShading.Resources.{file}");
            StreamReader reader = new StreamReader(stream ?? throw new IOException($"File {file} not found in Resources."));
            return reader
                .ReadToEnd()
                .Split('\n')
                .Select(line => float.Parse(line, CultureInfo.InvariantCulture))
                .ToArray();
        }

        public static float ParseFloat(string text)
        {
            return float.Parse(text, CultureInfo.InvariantCulture);
        }

        public static Stream LoadResourceStream(string file)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream($"PolygonalLightShading.Resources.{file}");
        }
    }
}