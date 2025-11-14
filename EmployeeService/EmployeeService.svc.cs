using System;
using EmployeeService.Data;
using EmployeeService.Models;
using EmployeeService.Repositories;

namespace EmployeeService
{
    /// <summary>
    /// WCF REST Service for Employee operations using ADO.NET
    /// </summary>
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repository;

        public EmployeeService()
        {
            // Initialize database (creates schema and test data if needed)
            DatabaseInitializer.Initialize();
            
            // Using ADO.NET with SQLite repository
            _repository = new EmployeeRepository();
        }

        public Employee GetEmployeeById(string id)
        {
            try
            {
                if (!int.TryParse(id, out int employeeId))
                {
                    throw new Exception($"Invalid employee ID: {id}");
                }

                var employee = _repository.GetEmployeeById(employeeId);
                
                if (employee == null)
                {
                    throw new Exception($"Employee with ID {employeeId} not found.");
                }
                
                return employee;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving employee: {ex.Message}");
            }
        }

        public void EnableEmployee(string id, bool enable)
        {
            try
            {
                if (!int.TryParse(id, out int employeeId))
                {
                    throw new Exception($"Invalid employee ID: {id}");
                }

                _repository.EnableEmployee(employeeId, enable);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating employee: {ex.Message}");
            }
        }
    }
}