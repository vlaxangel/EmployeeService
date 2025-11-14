using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace InterviewConsole
{
    class Program
    {
        private static readonly string ServiceBaseUrl = "http://localhost:64014/EmployeeService.svc";

        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            Console.WriteLine("Employee Service Test Client");
            Console.WriteLine(new string('=', 60));
            Console.WriteLine($"Service URL: {ServiceBaseUrl}");
            Console.WriteLine(new string('=', 60));
            Console.WriteLine();

            try
            {
                Console.WriteLine("Testing GetEmployeeById(1) - Root employee with tree:");
                Console.WriteLine(new string('-', 60));
                await TestGetEmployeeById(1);
                Console.WriteLine();

                Console.WriteLine("Testing GetEmployeeById(2) - Manager with subordinates:");
                Console.WriteLine(new string('-', 60));
                await TestGetEmployeeById(2);
                Console.WriteLine();

                Console.WriteLine("Testing GetEmployeeById(5) - Employee without subordinates:");
                Console.WriteLine(new string('-', 60));
                await TestGetEmployeeById(5);
                Console.WriteLine();

                Console.WriteLine("Testing EnableEmployee(7, false) - Disable Igor:");
                Console.WriteLine(new string('-', 60));
                await TestEnableEmployee(7, false);
                Console.WriteLine();

                Console.WriteLine("Verifying Igor is disabled:");
                Console.WriteLine(new string('-', 60));
                await TestGetEmployeeById(7);
                Console.WriteLine();

                Console.WriteLine("Testing EnableEmployee(7, true) - Enable Igor:");
                Console.WriteLine(new string('-', 60));
                await TestEnableEmployee(7, true);
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("Make sure EmployeeService is running!");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static async Task TestGetEmployeeById(int id)
        {
            using (var client = new HttpClient())
            {
                string url = $"{ServiceBaseUrl}/Employees/{id}";
                var response = await client.GetStringAsync(url);
                
                var employee = JsonConvert.DeserializeObject<JObject>(response);
                
                Console.WriteLine($"ID: {employee["ID"]}");
                Console.WriteLine($"Name: {employee["Name"]}");
                Console.WriteLine($"ManagerID: {employee["ManagerID"]?.ToString() ?? "NULL"}");
                Console.WriteLine($"Enable: {employee["Enable"]}");
                
                if (employee["Employees"] != null && employee["Employees"].HasValues)
                {
                    Console.WriteLine($"Subordinates: {employee["Employees"].Count()}");
                    foreach (var sub in employee["Employees"])
                    {
                        Console.WriteLine($"  - {sub["Name"]} (ID: {sub["ID"]})");
                    }
                }
                else
                {
                    Console.WriteLine("Subordinates: None");
                }
            }
        }

        private static async Task TestEnableEmployee(int id, bool enable)
        {
            using (var client = new HttpClient())
            {
                string url = $"{ServiceBaseUrl}/Employees/{id}/Enable?enable={enable}";
                var request = new HttpRequestMessage(HttpMethod.Put, url);
                
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                
                Console.WriteLine($"Employee {id} Enable flag set to: {enable}");
            }
        }
    }
}
