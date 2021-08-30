using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using System.Reflection;

namespace RSCG_Static
{
    [Generator]
    public class GenerateFromStatic : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var rec = context.SyntaxReceiver as ReceivePartialFunctionToStatic;
            foreach (var item in rec.candidates)
            {
                if (!item.Identifier.Text.StartsWith("FromStatic"))
                    continue; 
                var cd = item.Parent as ClassDeclarationSyntax;
                var nameSpace =(cd?.Parent as NamespaceDeclarationSyntax);
                var strNamespace = (nameSpace.Name as IdentifierNameSyntax).Identifier.Text;
                var ret = (item.ReturnType as IdentifierNameSyntax)?.Identifier.Text;
                string nameType= ret?.Replace("_", ".")?.Substring(1);
                var t = Type.GetType(nameType);
                var props= t.GetProperties(BindingFlags.Public | BindingFlags.Static);
                var template = "";
                template += $"{Environment.NewLine} namespace {strNamespace} {{";
                template += $"{Environment.NewLine} public interface {ret} {{";
                foreach (var prop in props)
                {
                    template += $"{Environment.NewLine} {prop.PropertyType.FullName} {prop.Name}  {{get;}}";

                }
                template += $"{Environment.NewLine} }}// interface";
                template += $"{Environment.NewLine}//now the partial class";
                template += $"{Environment.NewLine} public record cls{ret} ";
                var strDef = props.Select(it => $"{it.PropertyType.FullName} {it.Name}").ToArray();
                template += $"({string.Join(",", strDef)}) : {ret}";
                template += $"{Environment.NewLine} {{ ";
                template += $"public static cls{ret} MakeNew() {{";
                var strConstrParams = props.Select(it => $"{t.FullName}.{it.Name}");
                template += $"{Environment.NewLine}return new cls{ret}({string.Join(",", strConstrParams)});";
                template += $"{Environment.NewLine} }} //end makenew";
                template += $"{Environment.NewLine} }} //end record";
                template += $"{Environment.NewLine}//now the class";
                template += $"{Environment.NewLine}partial class {cd.Identifier.Text} {{";

                template += $"{Environment.NewLine}public partial {ret} {item.Identifier.Text}() {{";
                
                template += $"{Environment.NewLine}return cls{ret}.MakeNew();";
                template += $"{Environment.NewLine} }} // method";
                template += $"{Environment.NewLine} }} // class";
                template += $"{Environment.NewLine} }} // namespace";

                context.AddSource(ret, template);
                //var x = 1;
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new ReceivePartialFunctionToStatic());
        }
    }
}
