using ImGuiNET;
using Saba.Helpers;
using SabaViewer.Contracts.Windows;
using Silk.NET.OpenGLES;
using System.Numerics;

namespace SabaViewer.Windows;

public class Scene1 : Game
{
    private MikuMikuDance mmd = null!;
    private Vector3 translate = new(0.0f, 0.0f, -2.6f);
    private Vector3 scale = new(0.2f, 0.2f, 0.2f);

    protected override void Load()
    {
        mmd = new MikuMikuDance(gl);

        mmd.LoadModel("Resources/大喜/模型/登门喜鹊泠鸢yousa-ver2.0/泠鸢yousa登门喜鹊153cm-Apose2.1完整版(2).pmx".FormatFilePath(),
                      "Resources/大喜/动作数据/大喜MMD动作数据-喜鹊泠鸢专用版.vmd".FormatFilePath());

        mmd.Setup();
    }

    protected override void Render(double obj)
    {
        gl.ClearColor(1.0f, 0.8f, 0.75f, 1.0f);
        gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

        mmd.Draw(camera, Width, Height);
    }

    protected override void RenderImGui(double obj)
    {
        ImGui.Begin("MMD");

        Vector3 lightColor = MikuMikuDance.LightColor;
        ImGui.ColorEdit3(nameof(MikuMikuDance.LightColor), ref lightColor);
        MikuMikuDance.LightColor = lightColor;

        Vector4 shadowColor = MikuMikuDance.ShadowColor;
        ImGui.ColorEdit4(nameof(MikuMikuDance.ShadowColor), ref shadowColor);
        MikuMikuDance.ShadowColor = shadowColor;

        Vector3 lightDir = MikuMikuDance.LightDir;
        ImGui.DragFloat3(nameof(MikuMikuDance.LightDir), ref lightDir, 0.05f);
        MikuMikuDance.LightDir = lightDir;

        ImGui_Button("Play / Pause", () => mmd.IsPlaying = !mmd.IsPlaying);

        ImGui_Button("Enable physical", () => mmd.EnablePhysical = !mmd.EnablePhysical);

        ImGui.End();

        ImGui.Begin("Transform");

        ImGui.DragFloat3("Translate", ref translate, 0.01f);
        ImGui.DragFloat3("Scale", ref scale, 0.01f);

        ImGui.End();
    }

    protected override void Update(double obj)
    {
        mmd.Transform = Matrix4x4.CreateScale(scale) * Matrix4x4.CreateTranslation(translate);

        mmd.Update((float)Time);
    }

    private static void ImGui_Button(string label, Action action)
    {
        if (ImGui.Button(label))
        {
            action();
        }
    }
}