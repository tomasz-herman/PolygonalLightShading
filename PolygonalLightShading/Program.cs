using System;
using System.Collections.Generic;
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
        private Shader ltcShader;
        private Camera camera;

        private int ltc_mat;
        private int ltc_mag;

        private float roughness = 0.25f;
        private System.Numerics.Vector3 dcolor = new (1);
        private System.Numerics.Vector3 scolor = new (1);
        private float intensity = 4;
        private float width = 8;
        private float height = 8;
        private float roty = 0;
        private float rotz = 0;
        private bool twoSided = false;

        private bool loaded = false;

        private List<Mesh> sceneObjects = new List<Mesh>();

        private Vector3 ambientColor;

        private Lighting lighting;

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
            camera = new PerspectiveCamera();
            camera.UpdateVectors();
            imGuiController = new ImGuiController(Size.X, Size.Y);

            loaded = true;

            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            GL.Disable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);

            // load quad
            float[] wallVertices = new float[] {
                -0.5f, 0, -0.5f,
                 0.5f, 0, -0.5f,
                -0.5f, 0, 0.5f,
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
            Mesh bottomWall = new Mesh(wallVertices, wallNormals, wallColors, wallIndices, PrimitiveType.Triangles);
            bottomWall.ModelMatrix = Matrix4.CreateScale(cubeSize, cubeSize, cubeSize) * Matrix4.CreateTranslation(0, 0, cubeSize / 2);
            sceneObjects.Add(bottomWall);

            Mesh leftWall = new Mesh(wallVertices, wallNormals, wallColors, wallIndices, PrimitiveType.Triangles);
            leftWall.ModelMatrix = Matrix4.CreateRotationZ(halfpi) * Matrix4.CreateScale(cubeSize, cubeSize, cubeSize) * Matrix4.CreateTranslation(cubeSize / 2, cubeSize / 2, cubeSize / 2);
            sceneObjects.Add(leftWall);

            Mesh rightWall = new Mesh(wallVertices, wallNormals, wallColors, wallIndices, PrimitiveType.Triangles);
            rightWall.ModelMatrix = Matrix4.CreateRotationZ(-halfpi) * Matrix4.CreateScale(cubeSize, cubeSize, cubeSize) * Matrix4.CreateTranslation(-cubeSize / 2, cubeSize / 2, cubeSize / 2);
            sceneObjects.Add(rightWall);

            Mesh topWall = new Mesh(wallVertices, wallNormals, wallColors, wallIndices, PrimitiveType.Triangles);
            topWall.ModelMatrix = Matrix4.CreateRotationX(2 *halfpi) * Matrix4.CreateScale(cubeSize, cubeSize, cubeSize) * Matrix4.CreateTranslation(0, cubeSize, cubeSize / 2);
            sceneObjects.Add(topWall);

            Mesh backWall = new Mesh(wallVertices, wallNormals, wallColors, wallIndices, PrimitiveType.Triangles);
            backWall.ModelMatrix = Matrix4.CreateRotationX(-halfpi) * Matrix4.CreateScale(cubeSize, cubeSize, cubeSize) * Matrix4.CreateTranslation(0, cubeSize / 2, cubeSize);
            sceneObjects.Add(backWall);

            ambientColor = new Vector3(0.1f, 0.1f, 0.1f);

            lighting = new Lighting();

            var light1 = new QuadLight(
                new Vector3(-0.5f, -0.5f, 0),
                new Vector3(0.5f, -0.5f, 0),
                new Vector3(0.5f, 0.5f, 0),
                new Vector3(-0.5f, 0.5f, 0)
            );
            light1.modelMatrix = Matrix4.CreateScale(10, 10, 10) * Matrix4.CreateTranslation(0, 6, 32);
            lighting.Add(light1);

            var light2 = new QuadLight(
                new Vector3(-0.5f, -0.5f, 0),
                new Vector3(0.5f, -0.5f, 0),
                new Vector3(0.5f, 0.5f, 0),
                new Vector3(-0.5f, 0.5f, 0)
            );
            light2.modelMatrix = Matrix4.CreateRotationY(1) * Matrix4.CreateScale(10, 10, 10) * Matrix4.CreateTranslation(10, 6, 25);
            lighting.Add(light2);

            var light3 = new QuadLight(
                new Vector3(-0.5f, -0.5f, 0),
                new Vector3(0.5f, -0.5f, 0),
                new Vector3(0.5f, 0.5f, 0),
                new Vector3(-0.5f, 0.5f, 0)
            );
            light3.modelMatrix = Matrix4.CreateRotationY(-1) * Matrix4.CreateScale(10, 10, 10) * Matrix4.CreateTranslation(-10, 6, 25);
            lighting.Add(light3);

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
            imGuiController.Dispose();
            
            foreach(var mesh in sceneObjects)
                mesh.Dispose();
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
            
            GL.Disable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            ltcShader.Use();
            ltcShader.LoadFloat("roughness", roughness);
            ltcShader.LoadFloat3("dcolor", new Vector3(dcolor.X, dcolor.Y, dcolor.Z));
            ltcShader.LoadFloat3("scolor", new Vector3(scolor.X, scolor.Y, scolor.Z));
            ltcShader.LoadFloat("intensity", intensity);
            ltcShader.LoadInteger("twoSided", twoSided ? 1 : 0);
            ltcShader.LoadMatrix4("view", camera.GetViewMatrix());
            ltcShader.LoadMatrix4("proj", camera.GetProjectionMatrix());
            ltcShader.LoadFloat3("cameraPosition", camera.Position);
            ltcShader.LoadFloat3("ambient", ambientColor);

            ltcShader.LoadFloat3("lightVertices", lighting.GetVertexData());
            ltcShader.LoadInteger("activeLightCount", lighting.Count());
            
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, ltc_mat);
            ltcShader.LoadInteger("ltc_mat", 0);
            
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, ltc_mag);
            ltcShader.LoadInteger("ltc_mag", 1);
            
            foreach(var mesh in sceneObjects)
            {
                ltcShader.LoadMatrix4("model", mesh.ModelMatrix);
                mesh.Render();
            }
                

            RenderGui();

            Context.SwapBuffers();
        }

        private void RenderGui()
        {
            ImGui.Begin("Options");
            ImGui.SliderFloat("Roughness", ref roughness, 0.00f, 1f);
            ImGui.ColorPicker3("Diffuse Color", ref dcolor);
            ImGui.ColorPicker3("Specular Color", ref scolor);
            ImGui.SliderFloat("Light Intensity", ref intensity, 0.01f, 10f);
            ImGui.SliderFloat("Width", ref width, 0.1f, 15f);
            ImGui.SliderFloat("Height", ref height, 0.1f, 15f);
            ImGui.SliderFloat("Rotation Y", ref roty, 0f, 1f);
            ImGui.SliderFloat("Rotation Z", ref rotz, 0f, 1f);
            ImGui.Checkbox("Two-sided", ref twoSided);
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