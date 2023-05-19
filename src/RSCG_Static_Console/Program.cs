using RSCG_Static_Console;
using System;
using System.Threading.Tasks;

Console.WriteLine("Hello World!");
ISystem_DateTime dateStatic1 = (new StaticToInterface()).FromStaticDate(DateTime.Now);//static
ISystem_DateTime dateStatic2 = recISystem_DateTime.MakeNew();//static
ISystem_DateTime dateVar3 = new clsISystem_DateTime(); //variable = real 
Console.WriteLine(dateStatic1.Now.Second);
Console.WriteLine(dateStatic2.Now.Second);
Console.WriteLine(dateVar3.Now.Second);
await Task.Delay(10 * 1000);
Console.WriteLine(dateStatic1.Now.Second);
Console.WriteLine(dateStatic2.Now.Second);
Console.WriteLine(dateVar3.Now.Second);
await Task.Delay(10 * 1000);

