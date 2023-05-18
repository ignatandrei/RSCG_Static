using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RSCG_Static_Console
{
    partial class Program
    {
        static async Task Main(string[] args)
        {
            //Console.WriteLine("Hello World!");
            //var  dateStatic1 = (new Program().FromStaticDate());//static
            //var dateStatic2 = recISystem_DateTime.MakeNew();//static
            //var dateVar3 = new clsISystem_DateTime(); //variable = real 
            //await Task.Delay(10*1000);
            //Console.WriteLine(dateStatic1.Now.Second);
            //Console.WriteLine(dateStatic2.Now.Second);
            //Console.WriteLine(dateVar3.Now.Second);
            //await Task.Delay(10 * 1000);
            //Console.WriteLine(dateStatic1.Now.Second);
            //Console.WriteLine(dateStatic2.Now.Second);
            //Console.WriteLine(dateVar3.Now.Second);
            await Task.Delay(1000);
        }

        //public partial ISystem_Environment FromStaticEnv();
        public partial ISystem_DateTime FromStaticDate(DateTime doesNotMatter);
        //public partial ISystem_Diagnostics_Process FromStaticProcess(Process p);

    }

}




