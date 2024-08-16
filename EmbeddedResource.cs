using System;
using System.IO;
using System.Reflection;

namespace cc_026.RoslynUtil;

/// <remarks>
///     https://www.cazzulino.com/source-generators.html
/// </remarks>>
static class EmbeddedResource
{
    public static string GetContent(string relativePath)
    {
        var baseName = Assembly.GetExecutingAssembly().GetName().Name;
        var resourceName = relativePath
            .TrimStart('.')
            .Replace(Path.DirectorySeparatorChar, '.')
            .Replace(Path.AltDirectorySeparatorChar, '.');

        var resPath = baseName + "." + resourceName;
        
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream(resPath);

        if (stream == null)
            throw new NotSupportedException();

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}