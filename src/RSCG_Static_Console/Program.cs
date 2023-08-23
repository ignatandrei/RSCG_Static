using RSCG_Static_Console;
using System;
using System.Threading.Tasks;
//for DI, register
//ISystem_DateTime  with transient for new clsSystem_DateTime()
Console.WriteLine("Hello World!");
ISystem_DateTime dateStatic = recSystem_DateTime.MakeNew();//static
ISystem_DateTime dateVar = new clsSystem_DateTime(); //variable = real 

Console.WriteLine(dateStatic.Now.Second);
Console.WriteLine(dateVar.Now.Second);
await Task.Delay(10 * 1000);
Console.WriteLine(dateStatic.Now.Second);
Console.WriteLine(dateVar.Now.Second);

var docs=Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
var env = new clsSystem_Environment();
var docs1 = env.UserName;
//Console.ReadLine();