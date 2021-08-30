using System;

namespace RSCG_Static_Console
{
    partial class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Console.WriteLine(new Program().FromStaticDate());
            Console.WriteLine(clsISystem_Environment.MakeNew());
        }

        public partial ISystem_Environment FromStaticEnv();
        public partial ISystem_DateTime FromStaticDate();

    }

}




