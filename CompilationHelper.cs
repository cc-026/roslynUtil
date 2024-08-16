using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace cc_026.RoslynUtil;

public static class CompilationHelper
{
    public static IEnumerable<INamedTypeSymbol> GetAllTypes(this Compilation compilation) =>
        GetAllTypes(compilation.GlobalNamespace);

    static IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol @namespace)
    {
        foreach (var type in @namespace.GetTypeMembers())
        foreach (var nestedType in GetNestedTypes(type))
            yield return nestedType;

        foreach (var nestedNamespace in @namespace.GetNamespaceMembers())
        foreach (var type in GetAllTypes(nestedNamespace))
            yield return type;
    }

    static IEnumerable<INamedTypeSymbol> GetNestedTypes(INamedTypeSymbol type)
    {
        yield return type;
        foreach (var nestedType in type.GetTypeMembers()
                     .SelectMany(nestedType => GetNestedTypes(nestedType)))
            yield return nestedType;
    }

    public static string PathInfo(this Compilation compilation)
    {
        // https://blog.lindexi.com/post/Roslyn-%E5%88%86%E6%9E%90%E5%99%A8-EnforceExtendedAnalyzerRules-%E5%B1%9E%E6%80%A7%E7%9A%84%E4%BD%9C%E7%94%A8.html
        return 
$@"// Environment.CurrentDirectory: {Environment.CurrentDirectory}
// Directory.GetCurrentDirectory(): {Directory.GetCurrentDirectory()}
// AppContext.BaseDirectory: {AppContext.BaseDirectory}
// Assembly.Locations Path: {Path.GetDirectoryName(compilation.Assembly.Locations.FirstOrDefault()?.ToString())}
";
    }
}