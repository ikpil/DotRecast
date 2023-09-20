using System.IO;

namespace DotRecast.Core
{
    public static class RcResources
    {
        public static byte[] Load(string filename)
        {
            var filepath = RcDirectory.SearchFile($"resources/{filename}");
            using var fs = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);
            byte[] buffer = new byte[fs.Length];
            fs.Read(buffer, 0, buffer.Length);

            return buffer;
        }
    }
}