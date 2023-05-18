using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace RSCG_Static
{
    //[Generator]
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
                string strNamespace = null;
                var nameSpace = (cd?.Parent as NamespaceDeclarationSyntax);
                if (nameSpace != null) {
                    strNamespace = (nameSpace.Name as IdentifierNameSyntax).Identifier.Text;
                }
                else
                {
                    var parent = cd.Parent;
                    var propName = parent.GetType().GetProperty("Name");
                    var res= propName.GetGetMethod().Invoke(parent, null);
                    strNamespace = (res as IdentifierNameSyntax).Identifier.Text;
                }    
                var ret = (item.ReturnType as IdentifierNameSyntax)?.Identifier.Text;
                string nameType = ret?.Replace("_", ".")?.Substring(1);
                var t = Type.GetType(nameType);
                ToGenerate[] props = null;
                //string fullName = null;
                string optionalArgPartial = "";
                if (t == null)
                {
                    //let's see if there is a parameter to show
                    var pl = item.ParameterList.Parameters.FirstOrDefault();
                    if(pl == null)
                    {
                        // must have a report diagnostic here
                        continue;
                    }
                    var tl = pl.Type;
                    var model = context.Compilation.GetSemanticModel(tl.SyntaxTree);
                    var objectSymbol1 = model.GetDeclaredSymbol(pl) as IParameterSymbol;
                    //var objectSymbol2 = model.GetDeclaredSymbol(tl);
                    var t1 = objectSymbol1.Type;
                    optionalArgPartial = $"{t1.ToDisplayString()} doesNotMatter";
                    props = t1.GetMembers()
                        .Where(it =>
                        it.IsStatic == true &&
                        it.DeclaredAccessibility == Accessibility.Public
                        &&
                        (it.Kind == SymbolKind.Property
                        ||
                        (it.Kind == SymbolKind.Method && (it as IMethodSymbol).Parameters.Length==0))
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
                                    ret.TypeName = (it as IPropertySymbol).Type.Name;
                                    break;
                                default:
                                    throw new ArgumentException("cannot have " + it.Kind);


                            }
                            return ret;
                        })
                        .ToArray();

                    //fullName = tl.ToFullString();

                    //IPropertySymbol
                    //var p = m[4] as IMethodSymbol;
                    //var s = p.ReturnType;
                    //var m2 = t1.GetTypeMembers().Where(it => it.IsStatic == true).ToArray();
                    //var q = "";
                }
                else
                {
                    var propsProperties = t.GetProperties(BindingFlags.Public | BindingFlags.Static)
                        .Select(prop => new ToGenerate()
                        {
                            Name = prop.Name,
                            TypeName = prop.PropertyType.FullName,
                            symbolKind = SymbolKind.Property
                        }).ToArray()
                        ;
                    var propertiesMethodGet= propsProperties.Select(it =>"get_"+ it.Name).ToArray();

                    var propsFunctions = t.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(it=>it.GetParameters().Length == 0)
                    .Where(it=>it.CallingConvention == CallingConventions.Standard)
                    .Where(it=>!propertiesMethodGet.Contains(it.Name))                            
                        .Select(prop => new ToGenerate()
                    {
                        Name = prop.Name,
                        TypeName = prop.ReturnType.FullName,
                        symbolKind = SymbolKind.Method
                    }).ToArray();

                    props = propsProperties.Union(propsFunctions).ToArray();
                }
                var template = this.GenerateImplementation(props,strNamespace,ret, cd.Identifier.Text, item.Identifier.Text, nameType, optionalArgPartial);
                
                context.AddSource(ret, template);
                //var x = 1;
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            //Debugger.Launch();
            context.RegisterForSyntaxNotifications(() => new ReceivePartialFunctionToStatic());
        }

        string GenerateImplementation(ToGenerate[] props, string strNamespace, string ret, string className, string funcName, string fullNameType,string optionalArgPartial)
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
                        throw new ArgumentException("do not support "+prop.symbolKind);
                }

            }
            template += $"{rn} }}// interface";
            template += $"{rn}//now the partial class";
            template += $"{rn} public record rec{ret} ";
            var strDef = props.Where(it=>it.symbolKind == SymbolKind.Property) .Select(it => $"{it.TypeName} {it.Name}").ToArray();
            template += $"({string.Join(",", strDef)}) : {ret}";
            template += $"{rn} {{ ";
            template += $"{rn}public static rec{ret} MakeNew() {{";
            var strConstrParams = props.Where(it=>it.symbolKind== SymbolKind.Property).Select(it => $"{fullNameType}.{it.Name}");
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
    }
}
