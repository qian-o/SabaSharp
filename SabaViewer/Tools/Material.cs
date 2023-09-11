using Saba;

namespace SabaViewer.Tools;

public class Material
{
    public MMDMaterial MMDMaterial { get; }

    public Texture2D? Texture { get; set; }

    public Texture2D? SpTexture { get; set; }

    public Texture2D? ToonTexture { get; set; }

    public Material(MMDMaterial mmdMaterial)
    {
        MMDMaterial = mmdMaterial;
    }
}
