using Saba.Helpers;
using Silk.NET.Maths;

namespace Saba;

#region Enums
public enum ShadowType : byte
{
    Off,
    Mode1,
    Mode2,
}
#endregion

#region Classes
public class VmdHeader
{
    public string Title { get; }

    public string ModelName { get; }

    public VmdHeader(BinaryReader binaryReader)
    {
        Title = binaryReader.ReadString(30);
        ModelName = binaryReader.ReadString(20);
    }
}

public class VmdMotion
{
    public string BoneName { get; }

    public uint Frame { get; }

    public Vector3D<float> Translate { get; }

    public Quaternion<float> Quaternion { get; }

    public byte[] Interpolation { get; }

    public VmdMotion(BinaryReader binaryReader)
    {
        BoneName = binaryReader.ReadString(15, BinaryReaderExtensions.ShiftJIS);
        Frame = binaryReader.ReadUInt32();
        Translate = binaryReader.ReadVector3D();
        Quaternion = binaryReader.ReadQuaternion();
        Interpolation = binaryReader.ReadBytes(64);
    }
}

public class VmdMorph
{
    public string BlendShapeName { get; }

    public uint Frame { get; }

    public float Weight { get; }

    public VmdMorph(BinaryReader binaryReader)
    {
        BlendShapeName = binaryReader.ReadString(15, BinaryReaderExtensions.ShiftJIS);
        Frame = binaryReader.ReadUInt32();
        Weight = binaryReader.ReadSingle();
    }
}

public class VmdCamera
{
    public uint Frame { get; }

    public float Distance { get; }

    public Vector3D<float> Interest { get; }

    public Vector3D<float> Rotate { get; }

    public byte[] Interpolation { get; }

    public uint ViewAngle { get; }

    public bool IsPerspective { get; }

    public VmdCamera(BinaryReader binaryReader)
    {
        Frame = binaryReader.ReadUInt32();
        Distance = binaryReader.ReadSingle();
        Interest = binaryReader.ReadVector3D();
        Rotate = binaryReader.ReadVector3D();
        Interpolation = binaryReader.ReadBytes(24);
        ViewAngle = binaryReader.ReadUInt32();
        IsPerspective = binaryReader.ReadBoolean();
    }
}

public class VmdLight
{
    public uint Frame { get; }

    public Vector3D<float> Color { get; }

    public Vector3D<float> Position { get; }

    public VmdLight(BinaryReader binaryReader)
    {
        Frame = binaryReader.ReadUInt32();
        Color = binaryReader.ReadVector3D();
        Position = binaryReader.ReadVector3D();
    }
}

public class VmdShadow
{
    public uint Frame { get; }

    public ShadowType Mode { get; }

    public float Distance { get; }

    public VmdShadow(BinaryReader binaryReader)
    {
        Frame = binaryReader.ReadUInt32();
        Mode = (ShadowType)binaryReader.ReadByte();
        Distance = binaryReader.ReadSingle();
    }
}

public class VmdIk
{
    public class Info
    {
        public string Name { get; }

        public bool Enable { get; }

        public Info(BinaryReader binaryReader)
        {
            Name = binaryReader.ReadString(20, BinaryReaderExtensions.ShiftJIS);
            Enable = binaryReader.ReadBoolean();
        }
    }

    public uint Frame { get; }

    public bool Show { get; }

    public Info[] Infos { get; }

    public VmdIk(BinaryReader binaryReader)
    {
        Frame = binaryReader.ReadUInt32();
        Show = binaryReader.ReadBoolean();

        Infos = new Info[binaryReader.ReadUInt32()];

        for (int i = 0; i < Infos.Length; i++)
        {
            Infos[i] = new Info(binaryReader);
        }
    }
}
#endregion

public class VmdParsing
{
    public VmdHeader Header { get; }

    public VmdMotion[] Motions { get; }

    public VmdMorph[] Morphs { get; }

    public VmdCamera[] Cameras { get; }

    public VmdLight[] Lights { get; }

    public VmdShadow[] Shadows { get; }

    public VmdIk[] Iks { get; }

    internal VmdParsing(VmdHeader header,
                        VmdMotion[] motions,
                        VmdMorph[] morphs,
                        VmdCamera[] cameras,
                        VmdLight[] lights,
                        VmdShadow[] shadows,
                        VmdIk[] iks)
    {
        Header = header;
        Motions = motions;
        Morphs = morphs;
        Cameras = cameras;
        Lights = lights;
        Shadows = shadows;
        Iks = iks;
    }

    public static VmdParsing? ParsingByFile(string path)
    {
        using BinaryReader binaryReader = new(File.OpenRead(path));

        VmdHeader header = ReadHeader(binaryReader);

        if (header.Title != "Vocaloid Motion Data 0002" && header.Title != "Vocaloid Motion Data")
        {
            return null;
        }

        return new VmdParsing(header,
                              ReadMotions(binaryReader),
                              ReadMorphs(binaryReader),
                              ReadCameras(binaryReader),
                              ReadLights(binaryReader),
                              ReadShadows(binaryReader),
                              ReadIks(binaryReader));
    }

    private static VmdHeader ReadHeader(BinaryReader binaryReader)
    {
        return new VmdHeader(binaryReader);
    }

    private static VmdMotion[] ReadMotions(BinaryReader binaryReader)
    {
        VmdMotion[] motions = new VmdMotion[binaryReader.ReadUInt32()];

        for (int i = 0; i < motions.Length; i++)
        {
            motions[i] = new VmdMotion(binaryReader);
        }

        return motions;
    }

    private static VmdMorph[] ReadMorphs(BinaryReader binaryReader)
    {
        VmdMorph[] morphs = new VmdMorph[binaryReader.ReadUInt32()];

        for (int i = 0; i < morphs.Length; i++)
        {
            morphs[i] = new VmdMorph(binaryReader);
        }

        return morphs;
    }

    private static VmdCamera[] ReadCameras(BinaryReader binaryReader)
    {
        VmdCamera[] cameras = new VmdCamera[binaryReader.ReadUInt32()];

        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i] = new VmdCamera(binaryReader);
        }

        return cameras;
    }

    private static VmdLight[] ReadLights(BinaryReader binaryReader)
    {
        VmdLight[] lights = new VmdLight[binaryReader.ReadUInt32()];

        for (int i = 0; i < lights.Length; i++)
        {
            lights[i] = new VmdLight(binaryReader);
        }

        return lights;
    }

    private static VmdShadow[] ReadShadows(BinaryReader binaryReader)
    {
        VmdShadow[] shadows = new VmdShadow[binaryReader.ReadUInt32()];

        for (int i = 0; i < shadows.Length; i++)
        {
            shadows[i] = new VmdShadow(binaryReader);
        }

        return shadows;
    }

    private static VmdIk[] ReadIks(BinaryReader binaryReader)
    {
        VmdIk[] iks = new VmdIk[binaryReader.ReadUInt32()];

        for (int i = 0; i < iks.Length; i++)
        {
            iks[i] = new VmdIk(binaryReader);
        }

        return iks;
    }
}
