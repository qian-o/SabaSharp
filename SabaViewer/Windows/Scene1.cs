using Saba.Contracts;
using Saba;
using SabaViewer.Contracts.Windows;
using SabaViewer.Shaders;

namespace SabaViewer.Windows;

public class Scene1 : Game
{
    private MMDShader mmdShader;
    private MMDEdgeShader mmdEdgeShader;
    private MMDGroundShadowShader mmdGroundShadowShader;

    protected override void Load()
    {
        mmdShader = new(gl);
        mmdEdgeShader = new(gl);
        mmdGroundShadowShader = new(gl);

        IModel model = new PmxModel();
        model.Load("Resources/大喜/模型/登门喜鹊泠鸢yousa-ver2.0/泠鸢yousa登门喜鹊153cm-Apose2.1完整版(2).pmx", "Resources/MMD/");
    }
}
