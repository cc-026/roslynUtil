using Microsoft.CodeAnalysis;

namespace cc_026.RoslynUtil;

public static class AccessibilityHelper
{
    public static string CodeString(this Accessibility accessibility)
    {
        switch (accessibility)
        {
            case Accessibility.Private:
                return "private";
            case Accessibility.ProtectedAndInternal:
                return "private protected";
            case Accessibility.Protected:
                return "protected";
            case Accessibility.Internal:
                return "internal";
            case Accessibility.ProtectedOrInternal:
                return "protected internal";
            case Accessibility.Public:
                return "public";
            case Accessibility.NotApplicable:
            default:
                return "";
        }
    } 
}