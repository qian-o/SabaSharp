using Saba;

internal class Program
{
    private static void Main(string[] args)
    {
        _ = args;

        PmxParsing pmxParsing = PmxParsing.ParsingByFile("Resources/大喜/模型/登门喜鹊泠鸢yousa-ver2.0/泠鸢yousa登门喜鹊153cm-Apose2.1完整版(2).pmx")!;

        Console.WriteLine($"{pmxParsing.Header.Magic}{pmxParsing.Header.Version}");
        Console.WriteLine($"{pmxParsing.Info.ModelName} {pmxParsing.Info.ModelNameEn}");
        Console.WriteLine($"{pmxParsing.Info.Comment} {pmxParsing.Info.CommentEn}");
        Console.WriteLine($"Vertices: {pmxParsing.Vertices.Length}");
        Console.WriteLine($"Faces: {pmxParsing.Faces.Length}");
        Console.WriteLine($"Textures: {pmxParsing.Textures.Length}");
        Console.WriteLine($"Materials: {pmxParsing.Materials.Length}");
        Console.WriteLine($"Bones: {pmxParsing.Bones.Length}");
        Console.WriteLine($"Morphs: {pmxParsing.Morphs.Length}");
        Console.WriteLine($"DisplayFrames: {pmxParsing.DisplayFrames.Length}");
        Console.WriteLine($"RigidBodies: {pmxParsing.RigidBodies.Length}");
        Console.WriteLine($"Joints: {pmxParsing.Joints.Length}");
        Console.WriteLine($"SoftBodies: {pmxParsing.SoftBodies.Length}");
    }
}