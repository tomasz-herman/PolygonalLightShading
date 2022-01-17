using System;
using System.Collections.Generic;
using System.Reflection;
using Dear_ImGui_Sample;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ShaderType = OpenTK.Graphics.OpenGL4.ShaderType;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace PolygonalLightShading
{
    public class Program : GameWindow
    {
        private ImGuiController imGuiController;
        private Shader ltcShader, lightShader;
        private Camera camera;

        private int ltc_mat;
        private int ltc_mag;

        private float roughness = 0.25f;
        private System.Numerics.Vector3 dcolor = new (1);
        private System.Numerics.Vector3 scolor = new (1);

        private bool loaded = false;

        private List<Mesh> walls = new List<Mesh>();
        private Mesh duck;

        private System.Numerics.Vector3 ambientColor = new(0.1f);
        private Vector3 lightOffColor = new Vector3(0, 0, 0);
        private Lighting lighting;
        private float defaultLightIntensity = 4f;
        private Texture shrek, shrekLod;
        private Texture paints, paintsLod;
        private bool drawDuck = true;

        public static void Main(string[] args)
        {
            using Program program = new Program(GameWindowSettings.Default, NativeWindowSettings.Default);
            program.Title = "Tesselation Demo";
            program.Size = new Vector2i(768, 768);
            program.Run();
        }

        public Program(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings) { }

        protected override void OnLoad()
        {
            base.OnLoad();

            ltcShader = new Shader(("pls.vert", ShaderType.VertexShader), ("pls.frag", ShaderType.FragmentShader));
            lightShader = new Shader(("light.vert", ShaderType.VertexShader), ("light.frag", ShaderType.FragmentShader));
            camera = new PerspectiveCamera();
            camera.UpdateVectors();
            imGuiController = new ImGuiController(Size.X, Size.Y);
            shrek = new Texture("shrek.png");
            paints = new Texture("paints.png");
            shrekLod = new Texture("shrek0.png", "shrek1.png", "shrek2.png", "shrek3.png", "shrek4.png", "shrek5.png", "shrek6.png", "shrek7.png", "shrek8.png", "shrek9.png", "shrek10.png", "shrek11.png");
            paintsLod = new Texture("paints0.png", "paints1.png", "paints2.png", "paints3.png", "paints4.png", "paints5.png", "paints6.png", "paints7.png", "paints8.png", "paints9.png", "paints10.png", "paints11.png");

            loaded = true;

            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);

            // load quad
            float[] wallVertices = new float[] {
                -0.5f, 0, -0.5f,
                 -0.5f, 0, 0.5f,
                0.5f, 0, -0.5f,
                 0.5f, 0, 0.5f };
            float[] wallNormals = new float[]{
                     0, 1, 0,
                     0, 1, 0,
                     0, 1, 0,
                     0, 1, 0,};
            float planeBrightness = 0.5f;
            float[] wallColors = new float[] {
                 planeBrightness, planeBrightness, planeBrightness, 1,
                 planeBrightness, planeBrightness, planeBrightness, 1,
                 planeBrightness, planeBrightness, planeBrightness, 1,
                 planeBrightness, planeBrightness, planeBrightness, 1,};
            int[] wallIndices = new int[] { 0, 1, 2, 2, 1, 3 };

            float cubeSize = 40;
            float halfpi = (float)Math.PI / 2f;
            Mesh bottomWall = new Mesh(wallVertices, wallNormals, wallColors, null, wallIndices, PrimitiveType.Triangles);
            bottomWall.ModelMatrix = Matrix4.CreateScale(cubeSize, cubeSize, cubeSize) * Matrix4.CreateTranslation(0, 0, cubeSize / 2);
            walls.Add(bottomWall);

            Mesh leftWall = new Mesh(wallVertices, wallNormals, wallColors, null, wallIndices, PrimitiveType.Triangles);
            leftWall.ModelMatrix = Matrix4.CreateRotationZ(halfpi) * Matrix4.CreateScale(cubeSize) * Matrix4.CreateTranslation(cubeSize / 2, cubeSize / 2, cubeSize / 2);
            walls.Add(leftWall);

            Mesh rightWall = new Mesh(wallVertices, wallNormals, wallColors, null, wallIndices, PrimitiveType.Triangles);
            rightWall.ModelMatrix = Matrix4.CreateRotationZ(-halfpi) * Matrix4.CreateScale(cubeSize) * Matrix4.CreateTranslation(-cubeSize / 2, cubeSize / 2, cubeSize / 2);
            walls.Add(rightWall);

            Mesh topWall = new Mesh(wallVertices, wallNormals, wallColors, null, wallIndices, PrimitiveType.Triangles);
            topWall.ModelMatrix = Matrix4.CreateRotationX(2 *halfpi) * Matrix4.CreateScale(cubeSize) * Matrix4.CreateTranslation(0, cubeSize, cubeSize / 2);
            walls.Add(topWall);

            Mesh backWall = new Mesh(wallVertices, wallNormals, wallColors, null, wallIndices, PrimitiveType.Triangles);
            backWall.ModelMatrix = Matrix4.CreateRotationX(-halfpi) * Matrix4.CreateScale(cubeSize) * Matrix4.CreateTranslation(0, cubeSize / 2, cubeSize);
            walls.Add(backWall);

            float duckScale = 0.05f;
            Vector3 duckPosition = new Vector3(0, 0, cubeSize / 2);
            string duckFile = "duck.txt";
            Vector4 duckColor = new Vector4(1f, 1f, 1f, 1f);
            duck = DuckLoader.Load(Utils.LoadResourceStream(duckFile), duckColor);
            duck.ModelMatrix = Matrix4.CreateScale(duckScale) * Matrix4.CreateTranslation(duckPosition);

            lighting = new Lighting();

            var light1 = new QuadLight(
                new Vector3(-0.5f, -0.5f, 0),
                new Vector3(-0.5f, 0.5f, 0),
                new Vector3(0.5f, 0.5f, 0),
                new Vector3(0.5f, -0.5f, 0)
            );
            light1.Width = light1.Height = 10;
            light1.Rotation.Y = 45;
            light1.Position = new Vector3(10, 6, 25);
            light1.Color = new Vector3(1, 0, 0);
            light1.Texture = paints;
            light1.TextureLod = paintsLod;
            lighting.Add(light1);

            var light2 = new QuadLight(
                new Vector3(-0.5f, -0.5f, 0),
                new Vector3(-0.5f, 0.5f, 0),
                new Vector3(0.5f, 0.5f, 0),
                new Vector3(0.5f, -0.5f, 0)
            );
            light2.Width = light2.Height = 10;
            light2.Position = new Vector3(0, 6, 30);
            light2.Color = new Vector3(0, 1, 0);
            light2.Texture = shrek;
            light2.TextureLod = shrekLod;
            lighting.Add(light2);

            var light3 = new QuadLight(
                new Vector3(-0.5f, -0.5f, 0),
                new Vector3(-0.5f, 0.5f, 0),
                new Vector3(0.5f, 0.5f, 0),
                new Vector3(0.5f, -0.5f, 0)
            );
            light3.Width = light3.Height = 10;
            light3.Rotation.Y = 315;
            light3.Position = new Vector3(-10, 6, 25);
            light3.Color = new Vector3(0, 0, 1);
            light3.Texture = paints;
            light3.TextureLod = paintsLod;
            lighting.Add(light3);

            foreach (var light in lighting)
                light.Intensity = defaultLightIntensity;

            // load textures
            ltc_mat = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, ltc_mat);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, 64, 64, 0, PixelFormat.Rgba, PixelType.Float, Utils.Read("ltc_mat.txt"));
            
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.ClampToEdge);
            
            ltc_mag = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, ltc_mag);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R32f, 64, 64, 0, PixelFormat.Alpha, PixelType.Float, Utils.Read("ltc_mag.txt"));

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.ClampToEdge);
        }

        protected override void OnUnload()
        {
            base.OnUnload();

            ltcShader.Dispose();
            lightShader.Dispose();
            imGuiController.Dispose();
            
            foreach(var mesh in walls)
                mesh.Dispose();
            
            duck.Dispose();
            
            shrek.Dispose();
            shrekLod.Dispose();
            
            GL.DeleteTexture(ltc_mat);
            GL.DeleteTexture(ltc_mag);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            if (!loaded) return;
            camera.Aspect = (float) Size.X / Size.Y;
            GL.Viewport(0, 0, Size.X, Size.Y);
            imGuiController.WindowResized(Size.X, Size.Y);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            
            imGuiController.Update(this, (float) args.Time);

            if(ImGui.GetIO().WantCaptureMouse) return;

            KeyboardState keyboard = KeyboardState.GetSnapshot();
            MouseState mouse = MouseState.GetSnapshot();
            
            camera.HandleInput(keyboard, mouse, (float)args.Time);

            if (keyboard.IsKeyDown(Keys.Escape)) Close();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            ltcShader.Use();
            ltcShader.LoadFloat("roughness", roughness);
            ltcShader.LoadFloat3("dcolor", new Vector3(dcolor.X, dcolor.Y, dcolor.Z));
            ltcShader.LoadFloat3("scolor", new Vector3(scolor.X, scolor.Y, scolor.Z));
            ltcShader.LoadMatrix4("view", camera.GetViewMatrix());
            ltcShader.LoadMatrix4("proj", camera.GetProjectionMatrix());
            ltcShader.LoadFloat3("cameraPosition", camera.Position);
            ltcShader.LoadFloat3("ambient", new Vector3(ambientColor.X, ambientColor.Y, ambientColor.Z));

            ltcShader.LoadFloat3("lightVertices", lighting.GetVertexData());
            ltcShader.LoadFloat3("lightColors", lighting.GetColorData());
            ltcShader.LoadFloat("lightIntensity", lighting.GetIntensityData());
            ltcShader.LoadInt("lightTwoSided", lighting.GetTwoSidedData());
            ltcShader.LoadInt("lightTexture", lighting.GetTexturesLodData());
            ltcShader.LoadInt("useTexture", lighting.GetTexturesUsageData());
            ltcShader.LoadInteger("activeLightCount", lighting.Count());
            
            GL.ActiveTexture(TextureUnit.Texture31);
            GL.BindTexture(TextureTarget.Texture2D, ltc_mat);
            ltcShader.LoadInteger("ltc_mat", 31);
            
            GL.ActiveTexture(TextureUnit.Texture30);
            GL.BindTexture(TextureTarget.Texture2D, ltc_mag);
            ltcShader.LoadInteger("ltc_mag", 30);
            
            foreach(var mesh in walls)
            {
                ltcShader.LoadMatrix4("model", mesh.ModelMatrix);
                mesh.Render();
            }

            if (drawDuck)
            {
                ltcShader.LoadMatrix4("model", duck.ModelMatrix);
                duck.Render();
            }

            lightShader.Use();
            lightShader.LoadMatrix4("view", camera.GetViewMatrix());
            lightShader.LoadMatrix4("proj", camera.GetProjectionMatrix());
            lighting.GetTexturesData();

            for (int i = 0; i < lighting.Count(); i++)
            {
                var light = lighting[i];
                lightShader.LoadFloat("intensity", light.Intensity);
                lightShader.LoadMatrix4("model", light.ModelMatrix);
                lightShader.LoadFloat3("lightColor", light.Color);
                lightShader.LoadInt("useTexture", new[] {light.UseTexture ? 1 : 0});
                light.Texture.Use(TextureUnit.Texture0 + i);
                lightShader.LoadInt("tex", new[] {i});

                light.FrontMesh.Render();

                if (!light.TwoSided)
                {
                    lightShader.LoadInt("useTexture", new[] {0});
                    lightShader.LoadFloat3("lightColor", lightOffColor);
                }

                light.BackMesh.Render();
            }

            RenderGui();

            Context.SwapBuffers();
        }

        private void RenderGui()
        {
            ImGui.Begin("Options");
            if(ImGui.CollapsingHeader("Material"))
            {
                ImGui.SliderFloat("Roughness", ref roughness, 0.00f, 1f);
                ImGui.ColorPicker3("Diffuse Color", ref dcolor);
                ImGui.ColorPicker3("Specular Color", ref scolor);
                ImGui.ColorPicker3("Ambient Color", ref ambientColor);
            }
            for(int i = 0; i < lighting.Count(); i++)
            {
                if(ImGui.CollapsingHeader($"Light {i+1}"))
                {
                    var light = lighting[i];

                    ImGui.SliderFloat($"Position X {i+1}", ref light.Position.X, -20f, 20f);
                    ImGui.SliderFloat($"Position Y {i+1}", ref light.Position.Y, 0f, 40f);
                    ImGui.SliderFloat($"Position Z {i+1}", ref light.Position.Z, 0f, 40f);
                    ImGui.SliderFloat($"Rotation X {i+1}", ref light.Rotation.X, 0f, 360f);
                    ImGui.SliderFloat($"Rotation Y {i+1}", ref light.Rotation.Y, 0f, 360f);
                    ImGui.SliderFloat($"Rotation Z {i+1}", ref light.Rotation.Z, 0f, 360f);
                    ImGui.SliderFloat($"Width {i+1}", ref light.Width, 0f, 100f);
                    ImGui.SliderFloat($"Height {i+1}", ref light.Height, 0f, 100f);
                    ImGui.SliderFloat($"Intensity {i+1}", ref light.Intensity, 0f, 10f);
                    ImGui.Checkbox($"Two-sided {i+1}", ref light.TwoSided);
                    ImGui.Checkbox($"Use Texture {i+1}", ref light.UseTexture);

                    var color = new System.Numerics.Vector3(light.Color.X, light.Color.Y, light.Color.Z);
                    ImGui.ColorPicker3($"Color {i+1}", ref color);
                    light.Color = new Vector3(color.X, color.Y, color.Z);
                }
            }
            ImGui.Checkbox("Draw duck", ref drawDuck);
            ImGui.End();
            
            imGuiController.Render();
        }

        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);
            
            imGuiController.PressChar((char)e.Unicode);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            
            imGuiController.MouseScroll(e.Offset);
        }
    }
}