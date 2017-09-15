using System;
using NCDO;
using NCDO.Definitions;
using System.Json;

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

            var cdo = new CDO("UMUser", null, true);
            Console.WriteLine($"CDO {cdo.Name} created");
            Console.WriteLine($"CDO {cdo.Name} invoke GetLegacyUser");

            var paramObj = new JsonObject
            {
                { "pUstaCode", new JsonPrimitive("roh") },
            };

            var resp = cdo.Invoke("GetLegacyUser", paramObj).Result;

            var paramObj2 = new JsonObject
            {
                { "pUstaCode", new JsonPrimitive("roh") },
                { "pUstaPass", new JsonPrimitive("123") }
            };

            Console.WriteLine($"Success {resp.Success}");
            Console.WriteLine($"Status {resp.ResponseMessage.StatusCode}");

            Console.WriteLine(resp.Response.ToString());

            Console.WriteLine($"CDO {cdo.Name} invoke ValidateLegacyUser");
            var resp2 = cdo.Invoke("ValidateLegacyUser", paramObj2).Result;

            Console.WriteLine($"Success {resp2.Success}");
            Console.WriteLine($"Status {resp2.ResponseMessage.StatusCode}");

            Console.WriteLine(resp2.Response.ToString());


            var resp3 = cdo.Fill().Result;
            Console.WriteLine(resp3.Response.ToString());
            Console.ReadKey();
        }
    }
}
