using System;
using System.Diagnostics;

namespace RSCG_Static_Console;

public partial class StaticToInterface
{
    public Type GenerateInterfaceFromEnvironment()=>typeof(Environment);
    //public partial ISystem_Environment FromStaticEnv(Environment doesNotMatter);
    public partial ISystem_DateTime FromStaticDate(DateTime doesNotMatter);
    public partial ISystem_Diagnostics_Process FromStaticProcess(Process p);

}


