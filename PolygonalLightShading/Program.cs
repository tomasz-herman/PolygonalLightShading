﻿using System;
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

        private Mesh quad;

        private Vector3 ambientColor;
        private Vector3[] lightVertices;

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
            float planeY = 0.0f;
            quad = new Mesh(new[] { 
                -20.0f, planeY, 0.0f, 
                 20.0f, planeY, 0.0f, 
                -20.0f, planeY, 50.0f, 
                 20.0f, planeY, 50.0f }, 
                new[] { 0, 1, 2, 2, 1, 3 }, 
                PrimitiveType.Triangles);

            ambientColor = new Vector3(0.1f, 0.1f, 0.1f);

            lightVertices = new Vector3[]
            {
                new Vector3(-5, 1, 32),
                new Vector3(5, 1, 32),
                new Vector3(5, 11, 32),
                new Vector3(-5, 11, 32),
            };

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
            
            quad.Dispose();
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
            ltcShader.LoadMatrix4("model", Matrix4.Identity);
            ltcShader.LoadFloat3("cameraPosition", camera.Position);
            ltcShader.LoadFloat3("ambient", ambientColor);

            //TODO light vertices unchangeable, add light model matrices
            var lightPos = new List<float>();
            foreach(var pos in lightVertices)
            {
                lightPos.Add(pos.X);
                lightPos.Add(pos.Y);
                lightPos.Add(pos.Z);
            }
            ltcShader.LoadFloat3("lightVertices", lightPos.ToArray());
            
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, ltc_mat);
            ltcShader.LoadInteger("ltc_mat", 0);
            
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, ltc_mag);
            ltcShader.LoadInteger("ltc_mag", 1);
            
            quad.Render();

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