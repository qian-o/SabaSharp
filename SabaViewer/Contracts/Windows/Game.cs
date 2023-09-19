using ImGuiNET;
using SabaViewer.Tools;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using Silk.NET.OpenGLES.Extensions.ImGui;
using Silk.NET.Windowing;
using System.Numerics;
using System.Runtime.InteropServices;

namespace SabaViewer.Contracts.Windows;

public abstract unsafe class Game
{
    private readonly IWindow _window;

    protected GL gl = null!;
    protected IInputContext inputContext = null!;
    protected ImGuiController imGuiController = null!;
    protected Camera camera = null!;
    protected string renderer = string.Empty;

    #region Input
    protected IMouse mouse = null!;
    protected IKeyboard keyboard = null!;
    protected bool firstMove = true;
    protected Vector2D<float> lastPos;
    #endregion

    #region Speeds
    protected float cameraSpeed = 4.0f;
    protected float cameraSensitivity = 0.2f;
    #endregion

    public int Width { get; private set; }

    public int Height { get; private set; }

    public double Time => _window.Time;

    public bool IsWindowHovered { get; private set; }

    public Frame? Frame { get; private set; }

    public Game()
    {
        WindowOptions windowOptions = WindowOptions.Default;
        windowOptions.API = new GraphicsAPI(ContextAPI.OpenGLES, new APIVersion(3, 2));
        windowOptions.Samples = 8;
        windowOptions.VSync = false;
        windowOptions.PreferredDepthBufferBits = 32;
        windowOptions.PreferredStencilBufferBits = 32;
        windowOptions.PreferredBitDepth = new Vector4D<int>(8);

        _window = Window.Create(windowOptions);
        _window.Load += () =>
        {
            imGuiController = new ImGuiController(gl = _window.CreateOpenGLES(), _window, inputContext = _window.CreateInput());
            camera = new Camera
            {
                Position = new Vector3(0.0f, 2.0f, 3.0f),
                Fov = 45.0f
            };
            renderer = Marshal.PtrToStringAnsi((nint)gl.GetString(GLEnum.Renderer))!;

            mouse = inputContext.Mice[0];
            keyboard = inputContext.Keyboards[0];

            Load();
        };
        _window.Resize += (obj) => { gl.Viewport(obj); WindowResize(obj); };
        _window.Update += (obj) =>
        {
            if (IsWindowHovered)
            {
                if (mouse.IsButtonPressed(MouseButton.Middle))
                {
                    Vector2D<float> vector = new(mouse.Position.X, mouse.Position.Y);

                    if (firstMove)
                    {
                        lastPos = vector;

                        firstMove = false;
                    }
                    else
                    {
                        float deltaX = vector.X - lastPos.X;
                        float deltaY = vector.Y - lastPos.Y;

                        camera.Yaw += deltaX * cameraSensitivity;
                        camera.Pitch += -deltaY * cameraSensitivity;

                        lastPos = vector;
                    }
                }
                else
                {
                    firstMove = true;
                }

                if (keyboard.IsKeyPressed(Key.W))
                {
                    camera.Position += camera.Front * cameraSpeed * (float)obj;
                }

                if (keyboard.IsKeyPressed(Key.A))
                {
                    camera.Position -= camera.Right * cameraSpeed * (float)obj;
                }

                if (keyboard.IsKeyPressed(Key.S))
                {
                    camera.Position -= camera.Front * cameraSpeed * (float)obj;
                }

                if (keyboard.IsKeyPressed(Key.D))
                {
                    camera.Position += camera.Right * cameraSpeed * (float)obj;
                }

                if (keyboard.IsKeyPressed(Key.Q))
                {
                    camera.Position -= camera.Up * cameraSpeed * (float)obj;
                }

                if (keyboard.IsKeyPressed(Key.E))
                {
                    camera.Position += camera.Up * cameraSpeed * (float)obj;
                }
            }

            camera.Width = Width;
            camera.Height = Height;

            Update(obj);
        };
        _window.Render += (obj) =>
        {
            gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            Frame ??= new Frame(gl);
            Frame.Create(Width, Height);

            gl.BindFramebuffer(FramebufferTarget.Framebuffer, Frame.Id);
            gl.Viewport(0, 0, (uint)Width, (uint)Height);

            Render(obj);

            gl.Viewport(0, 0, (uint)_window.Size.X, (uint)_window.Size.Y);
            gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            imGuiController!.Update((float)obj);

            ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;

            ImGui.DockSpaceOverViewport();

            DrawHost();

            ImGui.Begin(renderer);
            ImGui.Value("FPS", ImGui.GetIO().Framerate);
            ImGui.End();

            ImGui.Begin("Camera Settings");
            ImGui.DragFloat("Camera Speed", ref cameraSpeed, 0.5f, 0.5f, 20.0f);
            ImGui.DragFloat("Camera Sensitivity", ref cameraSensitivity, 0.2f, 0.2f, 10.0f);
            ImGui.End();

            RenderImGui(obj);

            imGuiController.Render();
        };
        _window.Closing += Closing;
    }

    public void Run() => _window.Run();

    public void Close() => _window.Close();

    protected virtual void Load() { }

    protected virtual void WindowResize(Vector2D<int> obj) { }

    protected virtual void FramebufferResize(Vector2D<int> obj) { }

    protected virtual void Update(double obj) { }

    protected virtual void Render(double obj) { }

    protected virtual void RenderImGui(double obj) { }

    protected virtual void Closing() { }

    private void DrawHost()
    {
        ImGui.Begin("Main");

        IsWindowHovered = ImGui.IsWindowHovered();

        Vector2 size = ImGui.GetContentRegionAvail();

        int newWidth = Convert.ToInt32(size.X);
        int newHeight = Convert.ToInt32(size.Y);

        if (newWidth - Width != 0 || newHeight - Height != 0)
        {
            Width = newWidth;
            Height = newHeight;

            FramebufferResize(new Vector2D<int>(newWidth, newHeight));
        }

        ImGui.Image((nint)Frame!.Framebuffer, size, new Vector2(0.0f, 1.0f), new Vector2(1.0f, 0.0f));

        ImGui.End();
    }
}
