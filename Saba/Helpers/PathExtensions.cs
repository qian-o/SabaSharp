namespace Saba.Helpers;
public static class PathExtensions
{
    public static string FormatFilePath(this string filePath)
    {
        // 在Windows平台下，将斜杠替换为反斜杠
        if (Path.DirectorySeparatorChar == '\\')
        {
            filePath = filePath.Replace('/', '\\');
        }
        // 在其他平台下，将反斜杠替换为斜杠
        else
        {
            filePath = filePath.Replace('\\', '/');
        }

        return filePath;
    }
}
