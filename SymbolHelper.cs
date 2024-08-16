using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cc_026.RoslynUtil;

public static class SymbolHelper
{
    public static string FullName(this ISymbol? symbol)
    {
        return symbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace("global::", string.Empty) ??
               string.Empty;
    }

    public static string MiniName(this ISymbol? symbol)
    {
        return symbol?.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)
            .Replace("global::", string.Empty) ?? string.Empty;
    }

    public static string FileName(this ISymbol? symbol)
    {
        return symbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace("global::", string.Empty)
            .Replace("-", "_")
            .Replace("<", "_")
            .Replace(">", "_") ?? string.Empty;
    }

    public static bool Extends(this ITypeSymbol? symbol, string className, params Func<ITypeSymbol, bool>[]? typeArgs)
    {
        if (null == symbol)
            return false;

        var baseType = symbol.BaseType;

        if (null == baseType)
            return false;

        var result = false;
        if (baseType.IsGenericType)
        {
            if (typeArgs is { Length: > 0 })
            {
                if (baseType.TypeArguments.Length == typeArgs.Length)
                {
                    var name = Regex.Replace(baseType.FullName(), @"(<)[\s\S]*?(>)",
                        string.Empty);
                    result = name == className && typeArgs
                        .Zip(baseType.TypeArguments, (args, typeSymbol) => (args, typeSymbol))
                        .All(tuple =>
                        {
                            var (arg, typeSymbol) = tuple;
                            return arg?.Invoke(typeSymbol) ?? false;
                        });
                }
            }
            else
            {
                result = baseType.FullName() ==
                         $"{className}<{string.Join(", ", baseType.TypeArguments.Select(typeSymbol => typeSymbol.FullName()))}>";
            }

        }
        else
        {
            result = baseType.FullName() == className;
        }

        return result || baseType.Extends(className, typeArgs);
    }

    public static bool Extends(this ITypeSymbol? symbol, Type? type)
    {
        if (symbol == null || type == null)
            return false;

        while (symbol != null)
        {
            if (symbol.Matches(type))
                return true;

            symbol = symbol.BaseType;
        }

        return false;
    }

    public static bool Matches(this ITypeSymbol? symbol, Type? type)
    {
        if (symbol == null || type == null)
            return false;

        //if (type == typeof(UnityEngine.IEnumeratorOrVoid))
        //{
        //    return symbol.SpecialType is SpecialType.System_Void or SpecialType.System_Collections_IEnumerator;
        //}

        switch (symbol.SpecialType)
        {
            case SpecialType.System_Void:
                return type == typeof(void);
            case SpecialType.System_Boolean:
                return type == typeof(bool);
            case SpecialType.System_Int32:
                return type == typeof(int);
            case SpecialType.System_Single:
                return type == typeof(float);
        }

        if (type.IsArray)
        {
            return symbol is IArrayTypeSymbol array && Matches(array.ElementType, type.GetElementType()!);
        }

        if (symbol is not INamedTypeSymbol named)
            return false;

        if (type.IsConstructedGenericType)
        {
            var args = type.GetTypeInfo().GenericTypeArguments;
            if (args.Length != named.TypeArguments.Length)
                return false;

            for (var i = 0; i < args.Length; i++)
                if (!Matches(named.TypeArguments[i], args[i]))
                    return false;

            return Matches(named.ConstructedFrom, type.GetGenericTypeDefinition());
        }

        return named.Name == type.Name
               && named.ContainingNamespace?.ToDisplayString() == type.Namespace;
    }

    public static bool IsPartial(this ITypeSymbol? symbol)
    {
        if (null == symbol) return false;

        if (symbol.Locations.Length > 1)
            return true;

        if (0 == symbol.DeclaringSyntaxReferences.Length)
            return false;

        return symbol.DeclaringSyntaxReferences[0].GetSyntax() is TypeDeclarationSyntax syntax &&
               syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
        //return symbol?.DeclaringSyntaxReferences
        //    .Select(reference => reference.GetSyntax() as TypeDeclarationSyntax)
        //    .Any(syntax => syntax?.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)) ?? false) ?? false;
    }
}