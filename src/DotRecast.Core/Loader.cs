using System.IO;

namespace DotRecast.Core;

public static class Loader
{
    public static byte[] ToBytes(string filename)
    {
        var filepath = ToRPath(filename);
        using var fs = new FileStream(filepath, FileMode.Open);
        byte[] buffer = new byte[fs.Length];
        fs.Read(buffer, 0, buffer.Length);
        
        return buffer;
    }

    public static string ToRPath(string filename)
    {
        string filePath = Path.Combine("resources", filename);
        for (int i = 0; i < 10; ++i)
        {
            if (File.Exists(filePath))
            {
                return Path.GetFullPath(filePath);
            }

            filePath = Path.Combine("..", filePath);
        }

        return filename;
    }
}