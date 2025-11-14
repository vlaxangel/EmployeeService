using EmployeeService.Models;

namespace EmployeeService.Repositories
{
    public interface IEmployeeRepository
    {
        /// <summary>
        /// Get employee by ID with all subordinates in tree structure
        /// </summary>
        Employee GetEmployeeById(int id);

        /// <summary>
        /// Enable or disable employee
        /// </summary>
        void EnableEmployee(int id, bool enable);
    }
}

