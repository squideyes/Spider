using System.IO;

namespace Spider
{
    public static class StringExtenders
    {
        public static void EnsurePathExists(this string fileName)
        {
            var path = Path.GetDirectoryName(fileName);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}
