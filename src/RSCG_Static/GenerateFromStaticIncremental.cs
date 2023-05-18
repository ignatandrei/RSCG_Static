using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

namespace RSCG_Static;

[Generator]
public class GenerateFromStaticIncremental : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var methods= context
                    .SyntaxProvider
                  .CreateSyntaxProvider(CouldBeMethodPartial, GetPartialMethod)
                  .Where(type => type.Item1 is not null)
                  .Collect();
        context.RegisterSourceOutput(methods, act1);
    }


    private void act1(SourceProductionContext spc, ImmutableArray<(MethodDeclarationSyntax, ITypeSymbol)> arg2)
    {
        var data = arg2.Distinct();
        foreach(var item in data)
        {
            var met = item.Item1;
            var cd = met.Parent as ClassDeclarationSyntax;
            string strNamespace = null;
            var sn = cd.Parent;
            while (sn != null)
            {
                if (sn is BaseNamespaceDeclarationSyntax nsp)
                {
                    strNamespace = (nsp.Name as IdentifierNameSyntax).Identifier.Text;
                    break;
                }
                sn = sn.Parent;
            }
            var ret = (met.ReturnType as IdentifierNameSyntax)?.Identifier.Text;
            string nameType = ret.Replace("_", ".")?.Substring(1);
            var props = fromType(item.Item2);
            var optionalArgPartial = $"{item.Item2.ToDisplayString()} doesNotMatter";

            var template = this.GenerateImplementation(props, strNamespace, ret, cd.Identifier.Text, met.Identifier.Text, nameType, optionalArgPartial);
            spc.AddSource(met.Identifier.Text, template);
        }


    }
    string GenerateImplementation(ToGenerate[] props, string strNamespace, string ret, string className, string funcName, string fullNameType, string optionalArgPartial)
    {
        var rn = "\r\n";
        var template = "";
        template += $"{rn} namespace {strNamespace} {{";
        template += $"{rn} public interface {ret} {{";
        foreach (var prop in props)
        {
            switch (prop.symbolKind)
            {
                case SymbolKind.Property:
                    template += $"{rn} {prop.TypeName} {prop.Name}  {{get;}}";
                    break;
                case SymbolKind.Method:
                    template += $"{rn} {prop.TypeName} {prop.Name}();";
                    break;
                default:
                    throw new ArgumentException("do not support " + prop.symbolKind);
            }

        }
        template += $"{rn} }}// interface";
        template += $"{rn}//now the partial class";
        template += $"{rn} public record rec{ret} ";
        var strDef = props.Where(it => it.symbolKind == SymbolKind.Property).Select(it => $"{it.TypeName} {it.Name}").ToArray();
        template += $"({string.Join(",", strDef)}) : {ret}";
        template += $"{rn} {{ ";
        template += $"{rn}public static rec{ret} MakeNew() {{";
        var strConstrParams = props.Where(it => it.symbolKind == SymbolKind.Property).Select(it => $"{fullNameType}.{it.Name}");
        template += $"{rn}return new rec{ret}({string.Join(",", strConstrParams)});";
        template += $"{rn} }} //end makenew";
        var methods = props.Where(it => it.symbolKind == SymbolKind.Method).ToArray();
        foreach (var prop in methods)
        {
            var isVoid = (prop.TypeName.ToLower() == "void");
            var retData = isVoid ? "" : "return";
            template += $"{rn}public  {prop.TypeName} {prop.Name}()  {{  {retData} {fullNameType}.{prop.Name}();  }}";

        }
        //adding methods

        template += $"{rn} }} //end record";

        template += $"{rn} public class cls{ret} : {ret} ";
        template += $"{rn} {{ ";
        foreach (var prop in props)
        {
            switch (prop.symbolKind)
            {
                case SymbolKind.Property:
                    template += $"{rn}public  {prop.TypeName} {prop.Name}  {{get {{ return {fullNameType}.{prop.Name}; }} }}";
                    break;
                case SymbolKind.Method:
                    var isVoid = (prop.TypeName.ToLower() == "void");
                    var retData = isVoid ? "" : "return";
                    template += $"{rn}public  {prop.TypeName} {prop.Name}()  {{  {retData} {fullNameType}.{prop.Name}();  }}";
                    break;
            }
        }
        template += $"{rn} }} //end record";
        template += $"{rn}partial class {className} {{";

        template += $"{rn}public partial {ret} {funcName}({optionalArgPartial}) {{";

        template += $"{rn}return rec{ret}.MakeNew();";
        template += $"{rn} }} // method";
        template += $"{rn} }} // class";
        template += $"{rn} }} // namespace";
        return template;
    }
    ToGenerate[] fromType(ITypeSymbol t1)
    {
        return 
        t1.GetMembers()
                   .Where(it =>
                   it.IsStatic == true &&
                   it.DeclaredAccessibility == Accessibility.Public
                   &&
                   (it.Kind == SymbolKind.Property
                   ||
                   (it.Kind == SymbolKind.Method && (it as IMethodSymbol).Parameters.Length == 0))
                   )
                   .Select(it =>
                   {
                       var ret = new ToGenerate();
                       ret.Name = it.Name;
                       ret.symbolKind = it.Kind;
                       switch (it.Kind)
                       {
                           case SymbolKind.Method:
                               var typeName = (it as IMethodSymbol).ReturnType;
                               ret.TypeName = typeName.ToDisplayString();
                               break;
                           case SymbolKind.Property:
                               ret.TypeName = (it as IPropertySymbol).Type.ToDisplayString();
                               break;
                           default:
                               throw new ArgumentException("cannot have " + it.Kind);


                       }
                       return ret;
                   })
                   .ToArray();
    }
    private (MethodDeclarationSyntax,ITypeSymbol) GetPartialMethod(GeneratorSyntaxContext gsc, CancellationToken arg2)
    {
        if (gsc.Node is not MethodDeclarationSyntax met) return (null,null);
        if (!met.Identifier.Text.StartsWith("FromStatic")) return (null, null);
        if (met.ParameterList.Parameters.Count != 1) return (null, null);
        var type= gsc.SemanticModel.GetDeclaredSymbol(met.ParameterList.Parameters[0]);   
        if(type == null) return (null, null);
        return (met,type.Type);
    }

    private bool CouldBeMethodPartial(SyntaxNode syntaxNode, CancellationToken arg2)
    {
        if (syntaxNode is not MethodDeclarationSyntax met)
                return false;
        var isPartial = met.Modifiers.Any(it => it.IsKind(SyntaxKind.PartialKeyword));
        if(!isPartial) return false;

        return true;


    }
}
