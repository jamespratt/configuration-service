using System.IO;

namespace ConfigurationService.Hosting.Extensions;

public static class StringExtensions
{

    public static string NormalizePathSeparators(this string path)
    {
        return path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
    }

}