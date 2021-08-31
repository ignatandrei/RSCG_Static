# RSCG_Static

Roslyn Source Code Generator - transform static classes into instances and interfaces 

More, there is a MakeNew static method created to can have DI.

Just put a function like this ( example for System.DateTime)
```csharp
        public partial ISystem_DateTime FromStaticDate();
```
( i.e. I<Full_Name_To_Static_Class)

and the properties of the classes will be generated into interfaces and you can write:

```csharp
var  dateStatic1 = (new Program().FromStaticDate());//static
var dateStatic2 = recISystem_DateTime.MakeNew();//static
var dateVar3 = new clsISystem_DateTime();//variable = real 
await Task.Delay(10*1000);
Console.WriteLine(dateStatic1.Now.Second);
Console.WriteLine(dateStatic2.Now.Second);
Console.WriteLine(dateVar3.Now.Second);
```

