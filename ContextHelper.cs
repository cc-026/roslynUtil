using System.Threading;
using Microsoft.CodeAnalysis;

namespace cc_026.RoslynUtil;

public static class ContextHelper
{
    public static void GenInfo(this IncrementalGeneratorInitializationContext context, IIncrementalGenerator generator,  string extraInfo = "")
    {
        // https://stackoverflow.com/questions/75154619/add-non-c-files-in-source-generators
        var projectDirProvider = context.AnalyzerConfigOptionsProvider
            .Select(static (provider, _) =>
            {
                provider.GlobalOptions.TryGetValue("build_property.projectdir", out string? projectDirectory);
                return projectDirectory;
            }).Combine(context.CompilationProvider);
        
        context.RegisterSourceOutput(
            projectDirProvider,
            (productionContext, source) =>
            {
                string? projectDirectory = source.Left;
                var main = source.Right.GetEntryPoint(productionContext.CancellationToken);
                

                productionContext.AddSource($"{generator}-Info.g.cs", @$"
//build_property.projectdir: {projectDirectory?? "null"}
{source.Right.PathInfo()}
//main: {main?.FullName() ?? "null"}

/*
{extraInfo}
*/");
            });
    }
    
    public static T? GetSymbol<T>(this (GeneratorSyntaxContext, CancellationToken) ct) where T : ISymbol
    {
        var (context, token) = ct;
        var symbol = context.SemanticModel.GetDeclaredSymbol(context.Node, token);
        return null == symbol ? default : (T)symbol;
    }
}