public interface IEmployeeService
{
    Task<IEnumerable<Employee>> GetAllEmployees();
    Task<Employee> GetEmployeeById(int id);
    Task<Employee> CreateEmployee(Employee employee);
    Task<Employee> UpdateEmployee(Employee employee);
    Task<bool> DeleteEmployee(int id);
    Task<bool> EmployeeExists(int id);
    Task<bool> IsEmailUnique(string email, int? excludeEmployeeId = null);
}
