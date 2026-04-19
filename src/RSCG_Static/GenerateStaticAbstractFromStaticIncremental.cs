using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace RSCG_Static;

[Generator]
public class GenerateStaticAbstractFromStaticIncremental : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var methods = context
            .SyntaxProvider
            .CreateSyntaxProvider(
                (syntaxNode, cancellationToken) => StaticGenerationHelpers.IsTriggerMethod(syntaxNode, cancellationToken, "GenerateInterfaceFrom"),
                StaticGenerationHelpers.GetReturnTypeFromMethod)
            .Where(type => type.Method is not null && type.TypeSymbol is not null)
            .Collect();

        context.RegisterSourceOutput(methods, GenerateText);
    }

    private void GenerateText(SourceProductionContext spc, ImmutableArray<(MethodDeclarationSyntax Method, ITypeSymbol TypeSymbol)> methods)
    {
        foreach (var item in methods.Distinct())
        {
            var method = item.Method;
            var fullNameType = item.TypeSymbol.ToDisplayString();
            var typeName = item.TypeSymbol.Name;
            var strNamespace = StaticGenerationHelpers.GetNamespace(method);
            var members = StaticGenerationHelpers.FromType(item.TypeSymbol);
            var template = GenerateImplementation(members, strNamespace, fullNameType, typeName);
            spc.AddSource($"{method.Identifier.Text}.StaticAbstract", template);
        }
    }

    private string GenerateImplementation(ToGenerate[] members, string strNamespace, string fullNameType, string typeName)
    {
        var rn = "\r\n";
        var template = $"{rn}#nullable enable";
        if (!string.IsNullOrWhiteSpace(strNamespace))
            template += $"{rn} namespace {strNamespace} {{";

        template += $"{rn}      public interface I{typeName} {{";
        foreach (var member in members)
        {
            switch (member.symbolKind)
            {
                case SymbolKind.Property:
                    template += $"{rn}          static abstract {member.TypeName} {member.Name} {{get;}}";
                    template += $"{rn}          public static {member.TypeName} Get{member.Name}<T>() where T : I{typeName}";
                    template += $"{rn}          {{";
                    template += $"{rn}              return T.{member.Name};";
                    template += $"{rn}          }}";
                    break;
                case SymbolKind.Method:
                    template += $"{rn}          static abstract {member.TypeName} {member.Name}{MethodArguments(member.MethodSymbol)};";
                    template += $"{rn}          public static {member.TypeName} {member.Name}<T>{MethodArguments(member.MethodSymbol)} where T : I{typeName}";
                    template += $"{rn}          {{";
                    template += $"{rn}              {ReturnPrefix(member.TypeName)} T.{member.Name}{MethodCallArguments(member.MethodSymbol)};";
                    template += $"{rn}          }}";
                    break;
                default:
                    throw new ArgumentException("do not support " + member.symbolKind);
            }
        }

        template += $"{rn}      }}";
        template += $"{rn}      public class {typeName}ImplDefault : I{typeName}";
        template += $"{rn}      {{";

        foreach (var member in members)
        {
            switch (member.symbolKind)
            {
                case SymbolKind.Property:
                    template += $"{rn}          public static {member.TypeName} {member.Name} => {fullNameType}.{member.Name};";
                    break;
                case SymbolKind.Method:
                    template += $"{rn}          public static {member.TypeName} {member.Name}{MethodArguments(member.MethodSymbol)}";
                    template += $"{rn}          {{";
                    template += $"{rn}              {ReturnPrefix(member.TypeName)} {fullNameType}.{member.Name}{MethodCallArguments(member.MethodSymbol)};";
                    template += $"{rn}          }}";
                    break;
                default:
                    throw new ArgumentException("do not support " + member.symbolKind);
            }
        }

        template += $"{rn}      }}";
        if (!string.IsNullOrWhiteSpace(strNamespace))
            template += $"{rn} }}";
        template += $"{rn}#nullable disable";
        return template;
    }

    private string MethodArguments(IMethodSymbol methodSymbol)
    {
        if (methodSymbol.Parameters.Length == 0)
            return "()";

        var parameters = methodSymbol.Parameters
            .Select(it => it.ToDisplayString())
            .ToArray();

        return $"({string.Join(",", parameters)})";
    }

    private string MethodCallArguments(IMethodSymbol methodSymbol)
    {
        if (methodSymbol.Parameters.Length == 0)
            return "()";

        var parameters = methodSymbol.Parameters
            .Select(StaticGenerationHelpers.NameParamToCall)
            .ToArray();

        return $"({string.Join(",", parameters)})";
    }

    private string ReturnPrefix(string typeName)
    {
        return string.Equals(typeName, "void", StringComparison.OrdinalIgnoreCase) ? string.Empty : "return";
    }
}
