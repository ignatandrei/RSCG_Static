using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace RSCG_Static
{
    internal class ReceivePartialFunctionToStatic : ISyntaxReceiver
    {
        internal List<MethodDeclarationSyntax> candidates = new List<MethodDeclarationSyntax>();
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is MethodDeclarationSyntax met)
            {
                
                var isPartial = met.Modifiers.FirstOrDefault(it=>it.ValueText=="partial");
                if(isPartial != null && isPartial.ValueText=="partial" )
                {

                    candidates.Add(met);
                }

            }
        }
    }
}
 