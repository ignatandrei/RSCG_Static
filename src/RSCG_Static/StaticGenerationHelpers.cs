using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using System.Threading;

namespace RSCG_Static;

internal static class StaticGenerationHelpers
{
    internal static bool IsTriggerMethod(SyntaxNode syntaxNode, CancellationToken cancellationToken, string methodPrefix)
    {
        if (syntaxNode is not MethodDeclarationSyntax met)
            return false;

        var returnType = met.ReturnType.ToString();
        if (!string.Equals(returnType, "Type", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(returnType, "System.Type", StringComparison.OrdinalIgnoreCase))
            return false;

        return met.Identifier.Text.StartsWith(methodPrefix, StringComparison.OrdinalIgnoreCase);
    }

    internal static (MethodDeclarationSyntax Method, ITypeSymbol TypeSymbol) GetReturnTypeFromMethod(GeneratorSyntaxContext gsc, CancellationToken cancellationToken)
    {
        if (gsc.Node is not MethodDeclarationSyntax met)
            return default;

        TypeSyntax typeSyntax = null;

        if (met.ExpressionBody?.Expression is TypeOfExpressionSyntax expressionBodyTypeOf)
        {
            typeSyntax = expressionBodyTypeOf.Type;
        }
        else if (met.Body?.Statements.Count == 1 &&
                 met.Body.Statements[0] is ReturnStatementSyntax { Expression: TypeOfExpressionSyntax returnTypeOf })
        {
            typeSyntax = returnTypeOf.Type;
        }

        if (typeSyntax is null)
            return default;

        var typeSymbol = gsc.SemanticModel.GetTypeInfo(typeSyntax, cancellationToken).Type;
        return typeSymbol is null ? default : (met, typeSymbol);
    }

    internal static string GetNamespace(SyntaxNode syntaxNode)
    {
        var namespaces = syntaxNode
            .Ancestors()
            .OfType<BaseNamespaceDeclarationSyntax>()
            .Select(it => it.Name.ToString())
            .Reverse()
            .ToArray();

        return string.Join(".", namespaces);
    }

    internal static string NameParamToCall(IParameterSymbol parameterSymbol)
    {
        var name = parameterSymbol.Name;
        var before = "";
        switch (parameterSymbol.RefKind)
        {
            case RefKind.Ref:
                before = "ref ";
                break;
            case RefKind.Out:
                before = "out ";
                break;
            case RefKind.In:
                before = "in ";
                break;
            default:
                break;
        }

        return before + name;
    }

    internal static ToGenerate[] FromType(ITypeSymbol t1)
    {
        return t1
            .GetMembers()
            .Where(it =>
                it.IsStatic &&
                it.DeclaredAccessibility == Accessibility.Public &&
                (it.Kind == SymbolKind.Property ||
                 (it.Kind == SymbolKind.Method &&
                  it is IMethodSymbol method &&
                  method.MethodKind == MethodKind.Ordinary &&
                  !method.IsGenericMethod &&
                  method.Parameters.All(parameter => !parameter.ToDisplayString().Contains("`1")))))
            .Select(it =>
            {
                var ret = new ToGenerate
                {
                    Name = it.Name,
                    symbolKind = it.Kind
                };

                switch (it.Kind)
                {
                    case SymbolKind.Method:
                        var method = (IMethodSymbol)it;
                        ret.TypeName = method.ReturnType.ToDisplayString();
                        ret.MethodSymbol = method;
                        break;
                    case SymbolKind.Property:
                        ret.TypeName = ((IPropertySymbol)it).Type.ToDisplayString();
                        break;
                    default:
                        throw new ArgumentException("cannot have " + it.Kind);
                }

                return ret;
            })
            .ToArray();
    }
}
