using System;
using System.Diagnostics;

namespace RSCG_Static_Console;

public partial class StaticToInterface
{
    public Type GenerateInterfaceFromEnvironment()=> typeof(Environment);
    public Type GenerateInterfaceFromDate()=>typeof(DateTime);
    public Type GenerateInterfaceFromProcess => typeof(Process);

}


