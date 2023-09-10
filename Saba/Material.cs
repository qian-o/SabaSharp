using Silk.NET.Maths;

namespace Saba;

#region Enums
public enum SphereTextureMode
{
    None,
    Mul,
    Add
}
#endregion

public class Material
{
    public string Name { get; set; }

    public Vector3D<float> Diffuse { get; set; }

    public float Alpha { get; set; }

    public Vector3D<float> Specular { get; set; }

    public float SpecularPower { get; set; }

    public Vector3D<float> Ambient { get; set; }

    public byte EdgeFlag { get; set; }

    public float EdgeSize { get; set; }

    public Vector4D<float> EdgeColor { get; set; }

    public string Texture { get; set; }

    public string SpTexture { get; set; }

    public string ToonTexture { get; set; }

    public SphereTextureMode SpTextureMode { get; set; }

    public Vector4D<float> TextureMulFactor { get; set; }

    public Vector4D<float> SpTextureMulFactor { get; set; }

    public Vector4D<float> ToonTextureMulFactor { get; set; }

    public Vector4D<float> TextureAddFactor { get; set; }

    public Vector4D<float> SpTextureAddFactor { get; set; }

    public Vector4D<float> ToonTextureAddFactor { get; set; }

    public bool BothFace { get; set; }

    public bool GroundShadow { get; set; }

    public bool ShadowCaster { get; set; }

    public bool ShadowReceiver { get; set; }

    public Material()
    {
        Name = string.Empty;
        Diffuse = Vector3D<float>.One;
        Alpha = 1.0f;
        Specular = Vector3D<float>.Zero;
        SpecularPower = 1.0f;
        Ambient = new Vector3D<float>(0.2f);
        EdgeFlag = 0;
        EdgeSize = 0.0f;
        EdgeColor = Vector4D<float>.UnitW;
        Texture = string.Empty;
        SpTexture = string.Empty;
        ToonTexture = string.Empty;
        SpTextureMode = SphereTextureMode.None;
        TextureMulFactor = Vector4D<float>.One;
        SpTextureMulFactor = Vector4D<float>.One;
        ToonTextureMulFactor = Vector4D<float>.One;
        TextureAddFactor = Vector4D<float>.Zero;
        SpTextureAddFactor = Vector4D<float>.Zero;
        ToonTextureAddFactor = Vector4D<float>.Zero;
        BothFace = false;
        GroundShadow = true;
        ShadowCaster = true;
        ShadowReceiver = true;
    }
}
