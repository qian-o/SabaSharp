using System.Numerics;

namespace Saba;

#region Enums
public enum SphereTextureMode
{
    None,
    Mul,
    Add
}
#endregion

public class MMDMaterial
{
    public string Name { get; set; }

    public Vector3 Diffuse { get; set; }

    public float Alpha { get; set; }

    public Vector3 Specular { get; set; }

    public float SpecularPower { get; set; }

    public Vector3 Ambient { get; set; }

    public byte EdgeFlag { get; set; }

    public float EdgeSize { get; set; }

    public Vector4 EdgeColor { get; set; }

    public string Texture { get; set; }

    public string SpTexture { get; set; }

    public string ToonTexture { get; set; }

    public SphereTextureMode SpTextureMode { get; set; }

    public Vector4 TextureMulFactor { get; set; }

    public Vector4 SpTextureMulFactor { get; set; }

    public Vector4 ToonTextureMulFactor { get; set; }

    public Vector4 TextureAddFactor { get; set; }

    public Vector4 SpTextureAddFactor { get; set; }

    public Vector4 ToonTextureAddFactor { get; set; }

    public bool BothFace { get; set; }

    public bool GroundShadow { get; set; }

    public bool ShadowCaster { get; set; }

    public bool ShadowReceiver { get; set; }

    public MMDMaterial()
    {
        Name = string.Empty;
        Diffuse = Vector3.One;
        Alpha = 1.0f;
        Specular = Vector3.Zero;
        SpecularPower = 1.0f;
        Ambient = new Vector3(0.2f);
        EdgeFlag = 0;
        EdgeSize = 0.0f;
        EdgeColor = Vector4.UnitW;
        Texture = string.Empty;
        SpTexture = string.Empty;
        ToonTexture = string.Empty;
        SpTextureMode = SphereTextureMode.None;
        TextureMulFactor = Vector4.One;
        SpTextureMulFactor = Vector4.One;
        ToonTextureMulFactor = Vector4.One;
        TextureAddFactor = Vector4.Zero;
        SpTextureAddFactor = Vector4.Zero;
        ToonTextureAddFactor = Vector4.Zero;
        BothFace = false;
        GroundShadow = true;
        ShadowCaster = true;
        ShadowReceiver = true;
    }
}
