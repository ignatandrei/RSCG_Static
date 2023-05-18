using Microsoft.CodeAnalysis;

namespace RSCG_Static
{
    public class ToGenerate
    {
        public string Name { get; set; }
        public string TypeName { get; set; }

        public SymbolKind symbolKind { get; set; }
    }
}
