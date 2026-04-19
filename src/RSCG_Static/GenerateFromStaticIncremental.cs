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
        //GenerateInterfaceFrom(context);
        FromStatic(context);

    }

    //private void GenerateInterfaceFrom(IncrementalGeneratorInitializationContext context)
    //{
    //    var methods = context
    //                .SyntaxProvider
    //              .CreateSyntaxProvider(GenerateInterfaceFrom, GetReturnTypeFromMethod)
    //              .Where(type => type.Item1 is not null)
    //              .Collect();
    //    context.RegisterSourceOutput(methods, GenerateText);

    //}

    private (MethodDeclarationSyntax, ITypeSymbol) GetReturnTypeFromMethod(GeneratorSyntaxContext gsc, CancellationToken arg2)
    {
        return StaticGenerationHelpers.GetReturnTypeFromMethod(gsc, arg2);
    }

    private bool GenerateInterfaceFrom(SyntaxNode syntaxNode, CancellationToken arg2)
    {
        return StaticGenerationHelpers.IsTriggerMethod(syntaxNode, arg2, "GenerateInterfaceFrom");
    }

    private void FromStatic(IncrementalGeneratorInitializationContext context)
    {
        var methods = context
                    .SyntaxProvider
                  .CreateSyntaxProvider(GenerateInterfaceFrom, GetReturnTypeFromMethod)
                  .Where(type =>  type.Item1 is not null)
                  .Collect();
        context.RegisterSourceOutput(methods, GenerateText);
    }

    private void GenerateText(SourceProductionContext spc, ImmutableArray<(MethodDeclarationSyntax, ITypeSymbol)> arg2)
    {
        var data = arg2.Distinct();
        foreach(var item in data)
        {
            var met = item.Item1;
            var strNamespace = StaticGenerationHelpers.GetNamespace(met);
            var ret = item.Item2.ToDisplayString();
            string nameInterface = ret.Replace(".", "_");
            var props = StaticGenerationHelpers.FromType(item.Item2);
            var template = this.GenerateImplementation(props, strNamespace, ret, nameInterface);
            spc.AddSource(met.Identifier.Text, template);
        }


    }
    private string nameParamToCall(IParameterSymbol it)
    {
        return StaticGenerationHelpers.NameParamToCall(it);
    }
    string GenerateImplementation(ToGenerate[] props, string strNamespace, string fullNameType, string nameInterface)
    {
        var rn = "\r\n";
        var template = $"{rn}#nullable enable";
        if (!string.IsNullOrWhiteSpace(strNamespace))
            template += $"{rn} namespace {strNamespace} {{";
        template += $"{rn}      public interface I{nameInterface} {{";
        foreach (var prop in props)
        {
            switch (prop.symbolKind)
            {
                case SymbolKind.Property:
                    template += $"{rn}          public {prop.TypeName} {prop.Name}  {{get;}}";
                    break;
                case SymbolKind.Method:
                    template += $"{rn}          public {prop.TypeName} {prop.Name}";
                    var parameters = prop.MethodSymbol.Parameters;
                    if(parameters.Length == 0)
                    {
                        template += "();";
                    }
                    else
                    {
                        
                        template += "(";
                        var arrParams=parameters
                            .Select(it => it.ToDisplayString() )
                            .ToArray();
                        template += string.Join(",", arrParams);
                        template += ");";

                    }
                    break;
                default:
                    throw new ArgumentException("do not support " + prop.symbolKind);
            }

        }
        template += $"{rn}      }}// interface";
        template += $"{rn}//now the partial class";
        template += $"{rn}      public record rec{nameInterface} ";
        var strDef = props.Where(it => it.symbolKind == SymbolKind.Property).Select(it => $"{it.TypeName} {it.Name}").ToArray();
        template += $"({string.Join(",", strDef)}) : I{nameInterface}";
        template += $"{rn}      {{ ";
        template += $"{rn}            public static rec{nameInterface} MakeNew() {{";
        var strConstrParams = props.Where(it => it.symbolKind == SymbolKind.Property).Select(it => $"{fullNameType}.{it.Name}");
        template += $"{rn}            return new rec{nameInterface}({string.Join(",", strConstrParams)});";
        template += $"{rn}            }} //end makenew";
        var methods = props.Where(it => it.symbolKind == SymbolKind.Method).ToArray();
        foreach (var prop in methods)
        {
            var isVoid = (prop.TypeName.ToLower() == "void");
            var retData = isVoid ? "" : "return";
            var parameters = prop.MethodSymbol.Parameters;
            var argsMethod = "";
            var callMethodArgs = "";
            if (parameters.Length == 0)
            {
                argsMethod += "()";
                callMethodArgs += "()";

            }
            else
            {

                argsMethod += "(";
                callMethodArgs += "(";

                var arrParams = parameters
                    .Select(it => it.ToDisplayString())
                    .ToArray();
                var nameParams= parameters
                    .Select(it => nameParamToCall( it))
                    .ToArray();
                argsMethod += string.Join(",", arrParams);
                callMethodArgs += string.Join(",", nameParams);

                argsMethod += ")";
                callMethodArgs += ")";


            }

            template += $"{rn}            public  {prop.TypeName} {prop.Name} {argsMethod}  {{  {retData} {fullNameType}.{prop.Name}{callMethodArgs};  }}";

        }
        //adding methods

        template += $"{rn}      }} //end record";

        template += $"{rn}      public class cls{nameInterface} : I{nameInterface} ";
        template += $"{rn}      {{ ";
        foreach (var prop in props)
        {
            switch (prop.symbolKind)
            {
                case SymbolKind.Property:
                    template += $"{rn}            public {prop.TypeName} {prop.Name}  {{get {{ return {fullNameType}.{prop.Name}; }} }}";
                    break;
                case SymbolKind.Method:
                    var isVoid = (prop.TypeName.ToLower() == "void");
                    var retData = isVoid ? "" : "return";
                    var parameters = prop.MethodSymbol.Parameters;
                    var argsMethod = "";
                    var callMethodArgs = "";
                    if (parameters.Length == 0)
                    {
                        argsMethod += "()";
                        callMethodArgs += "()";

                    }
                    else
                    {

                        argsMethod += "(";
                        callMethodArgs += "(";

                        var arrParams = parameters
                            .Select(it => it.ToDisplayString())
                            .ToArray();
                        var nameParams = parameters
                            .Select(it => nameParamToCall(it))
                            .ToArray();
                        argsMethod += string.Join(",", arrParams);
                        callMethodArgs += string.Join(",", nameParams);

                        argsMethod += ")";
                        callMethodArgs += ")";


                    }



                    template += $"{rn}            public  {prop.TypeName} {prop.Name}{argsMethod}  {{  {retData} {fullNameType}.{prop.Name}{callMethodArgs};  }}";
                    break;
            }
        }
        template += $"{rn}       }} //end class";
        //template += $"{rn}partial class {className} {{";

        //template += $"{rn}public partial {fullNameType} {funcName}({optionalArgPartial}) {{";

        //template += $"{rn}return rec{fullNameType}.MakeNew();";
        //template += $"{rn} }} // method";
        //template += $"{rn} }} // class";
        if (!string.IsNullOrWhiteSpace(strNamespace))
            template += $"{rn} }} // namespace";
        template += $"{rn}#nullable disable";

        return template;
    }

    ToGenerate[] fromType(ITypeSymbol t1)
    {
        return StaticGenerationHelpers.FromType(t1);
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
