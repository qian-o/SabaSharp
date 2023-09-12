using ImGuiNET;
using Saba.Helpers;
using SabaViewer.Contracts.Windows;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using System.Numerics;

namespace SabaViewer.Windows;

public class Scene1 : Game
{
    private MikuMikuDance mmd = null!;

    protected override void Load()
    {
        mmd = new MikuMikuDance(gl)
        {
            Transform = Matrix4X4.CreateScale(0.2f, 0.2f, 0.2f)
        };

        mmd.LoadModel("Resources/大喜/模型/登门喜鹊泠鸢yousa-ver2.0/泠鸢yousa登门喜鹊153cm-Apose2.1完整版(2).pmx");

        mmd.Setup();
        mmd.Update();
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

        Vector3 lightColor = MikuMikuDance.LightColor.ToSystem();
        ImGui.ColorEdit3(nameof(MikuMikuDance.LightColor), ref lightColor);
        MikuMikuDance.LightColor = lightColor.ToGeneric();

        Vector4 shadowColor = MikuMikuDance.ShadowColor.ToSystem();
        ImGui.ColorEdit4(nameof(MikuMikuDance.ShadowColor), ref shadowColor);
        MikuMikuDance.ShadowColor = shadowColor.ToGeneric();

        Vector3 lightDir = MikuMikuDance.LightDir.ToSystem();
        ImGui.DragFloat3(nameof(MikuMikuDance.LightDir), ref lightDir, 0.05f);
        MikuMikuDance.LightDir = lightDir.ToGeneric();

        ImGui.End();
    }
}
