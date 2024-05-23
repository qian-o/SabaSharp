using System.Numerics;
using Saba.Helpers;

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
public class VmdHeader(BinaryReader binaryReader)
{
    public string Title { get; } = binaryReader.ReadString(30);

    public string ModelName { get; } = binaryReader.ReadString(20);
}

public class VmdMotion(BinaryReader binaryReader)
{
    public string BoneName { get; } = binaryReader.ReadString(15, BinaryReaderExtensions.ShiftJIS);

    public uint Frame { get; } = binaryReader.ReadUInt32();

    public Vector3 Translate { get; } = binaryReader.ReadVector3();

    public Quaternion Quaternion { get; } = binaryReader.ReadQuaternion();

    public byte[] Interpolation { get; } = binaryReader.ReadBytes(64);
}

public class VmdMorph(BinaryReader binaryReader)
{
    public string BlendShapeName { get; } = binaryReader.ReadString(15, BinaryReaderExtensions.ShiftJIS);

    public uint Frame { get; } = binaryReader.ReadUInt32();

    public float Weight { get; } = binaryReader.ReadSingle();
}

public class VmdCamera(BinaryReader binaryReader)
{
    public uint Frame { get; } = binaryReader.ReadUInt32();

    public float Distance { get; } = binaryReader.ReadSingle();

    public Vector3 Interest { get; } = binaryReader.ReadVector3();

    public Vector3 Rotate { get; } = binaryReader.ReadVector3();

    public byte[] Interpolation { get; } = binaryReader.ReadBytes(24);

    public uint ViewAngle { get; } = binaryReader.ReadUInt32();

    public bool IsPerspective { get; } = binaryReader.ReadBoolean();
}

public class VmdLight(BinaryReader binaryReader)
{
    public uint Frame { get; } = binaryReader.ReadUInt32();

    public Vector3 Color { get; } = binaryReader.ReadVector3();

    public Vector3 Position { get; } = binaryReader.ReadVector3();
}

public class VmdShadow(BinaryReader binaryReader)
{
    public uint Frame { get; } = binaryReader.ReadUInt32();

    public ShadowType Mode { get; } = (ShadowType)binaryReader.ReadByte();

    public float Distance { get; } = binaryReader.ReadSingle();
}

public class VmdIk
{
    public class Info(BinaryReader binaryReader)
    {
        public string Name { get; } = binaryReader.ReadString(20, BinaryReaderExtensions.ShiftJIS);

        public bool Enable { get; } = binaryReader.ReadBoolean();
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
