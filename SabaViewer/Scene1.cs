using ImGuiNET;
using Saba.Helpers;
using SabaViewer.Contracts;
using Silk.NET.OpenGLES;
using System.Numerics;

namespace SabaViewer;

public class Scene1 : Game
{
    private readonly List<MikuMikuDance> _mikuMikuDances = new();

    private Vector3 translate = new(0.0f, 0.0f, -2.6f);
    private Vector3 scale = new(0.2f, 0.2f, 0.2f);
    private bool isFirstFrame = true;

    protected override void Load()
    {
        _mikuMikuDances.Add(new MikuMikuDance(gl,
                                              "Resources/大喜/模型/登门喜鹊泠鸢yousa-ver2.0/泠鸢yousa登门喜鹊153cm-Apose2.1完整版(2).pmx".FormatFilePath(),
                                              "Resources/大喜/动作数据/大喜MMD动作数据-喜鹊泠鸢专用版.vmd".FormatFilePath()));

        _mikuMikuDances.Add(new MikuMikuDance(gl,
                                              "Resources/KizunaAI_ver1.01/kizunaai/kizunaai.pmx".FormatFilePath(),
                                              "Resources/大喜/动作数据/大喜动作数据2.0配布修正滑步身体穿模等问题.vmd".FormatFilePath()));
    }

    protected override void Render(double obj)
    {
        gl.ClearColor(1.0f, 0.8f, 0.75f, 1.0f);
        gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

        foreach (MikuMikuDance mmd in _mikuMikuDances)
        {
            mmd.Draw(camera, Width, Height);
        }
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

        ImGui_Button("Play / Pause", () => _mikuMikuDances.ForEach((mmd) => mmd.IsPlaying = !mmd.IsPlaying));

        ImGui_Button("Enable physical", () => _mikuMikuDances.ForEach((mmd) => mmd.EnablePhysical = !mmd.EnablePhysical));

        ImGui.End();

        ImGui.Begin("Transform");

        ImGui.DragFloat3("Translate", ref translate, 0.01f);
        ImGui.DragFloat3("Scale", ref scale, 0.01f);

        ImGui.End();

        if (isFirstFrame)
        {
            ImGui.LoadIniSettingsFromDisk("layout.ini");

            isFirstFrame = false;
        }
    }

    protected override void Update(double obj)
    {
        float time = (float)Time;

        int index = 0;
        foreach (MikuMikuDance mmd in _mikuMikuDances)
        {
            mmd.Translate = translate + new Vector3(-4.0f * index, 0.0f, -4.0f * index);
            mmd.Scale = scale;

            mmd.Update(time);

            index++;
        }
    }

    private static void ImGui_Button(string label, Action action)
    {
        if (ImGui.Button(label))
        {
            action();
        }
    }
}