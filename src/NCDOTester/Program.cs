using System;
using NCDO;
using NCDO.Definitions;

namespace NCDOTester
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var pdSession = new CDOSession(new Uri("http://raddev21.reynaers.org:8081/UserMan"));
            pdSession.Login().Wait();

            pdSession.AddCatalog(new Uri("http://raddev21.reynaers.org:8081/UserMan/static/UserManService.json")).Wait();
            #region Print Info from catalog
            Console.WriteLine(pdSession.LoginResult);
            Console.WriteLine(pdSession.Connected);
            Console.WriteLine();

            foreach (var service in pdSession.Services)
            {
                Console.WriteLine($"Service : {service.Name}\n");
                foreach (var resource in service.Resources)
                {
                    Console.WriteLine($"Resource : {resource.Name}");
                    Console.WriteLine("========");
                    foreach (var operation in resource.Operations)
                    {
                        Console.WriteLine($"Operation : {operation.Name} {operation.Type}");

                    }
                    Console.WriteLine();
                }
            }
            #endregion

            var cdo = new CDO("UMUser");

            Console.WriteLine($"CDO {cdo.Name} created");
            Console.ReadKey();
        }
    }
}
