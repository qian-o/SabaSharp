using Saba;
using SabaViewer;

internal class Program
{
    private static void Main(string[] args)
    {
        _ = args;

        // 设置是否使用 OpenCL。
        Kernel.UseOpenCL = false;

        Scene1 scene1 = new();

        scene1.Run();
    }
}