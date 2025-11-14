using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Web.Hosting;
using EmployeeService.Models;

namespace EmployeeService.Repositories
{
    /// <summary>
    /// Employee repository implementation using ADO.NET with SQLite
    /// </summary>
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly string _connectionString;

        public EmployeeRepository()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["EmployeeDB"].ConnectionString;
            
            string dataDir = HostingEnvironment.IsHosted 
                ? HostingEnvironment.MapPath("~/App_Data") 
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
            
            string dbPath = Path.Combine(dataDir, "EmployeeDatabase.db");
            _connectionString = $"Data Source={dbPath};Version=3;";
        }

        public Employee GetEmployeeById(int id)
        {
            using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                var allEmployees = LoadAllEmployees(connection);
                
                if (!allEmployees.ContainsKey(id))
                {
                    return null;
                }
                
                var rootEmployee = allEmployees[id];
                BuildEmployeeTree(rootEmployee, allEmployees);
                
                return rootEmployee;
            }
        }

        private Dictionary<int, Employee> LoadAllEmployees(SQLiteConnection connection)
        {
            var employees = new Dictionary<int, Employee>();
            
            string query = "SELECT ID, Name, ManagerID, Enable FROM Employee";
            
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var employee = new Employee
                        {
                            ID = Convert.ToInt32(reader["ID"]),
                            Name = reader["Name"].ToString(),
                            ManagerID = reader["ManagerID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["ManagerID"]),
                            Enable = Convert.ToBoolean(reader["Enable"]),
                            Employees = new List<Employee>()
                        };
                        
                        employees[employee.ID] = employee;
                    }
                }
            }
            
            return employees;
        }

        private void BuildEmployeeTree(Employee employee, Dictionary<int, Employee> allEmployees)
        {
            foreach (var emp in allEmployees.Values)
            {
                if (emp.ManagerID == employee.ID)
                {
                    employee.Employees.Add(emp);
                    BuildEmployeeTree(emp, allEmployees);
                }
            }
        }

        public void EnableEmployee(int id, bool enable)
        {
            using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                string query = "UPDATE Employee SET Enable = @Enable WHERE ID = @ID";
                
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);
                    command.Parameters.AddWithValue("@Enable", enable ? 1 : 0);
                    
                    int rowsAffected = command.ExecuteNonQuery();
                    
                    if (rowsAffected == 0)
                    {
                        throw new Exception($"Employee with ID {id} not found.");
                    }
                }
            }
        }
    }
}

