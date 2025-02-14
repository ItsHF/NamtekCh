using Microsoft.EntityFrameworkCore;

public class EmployeeService : IEmployeeService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EmployeeService> _logger;
 
    public EmployeeService(
        ApplicationDbContext context,
        ILogger<EmployeeService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Employee>> GetAllEmployees()
    {
        try
        {
            return await _context.Employees
                .AsNoTracking()
                .OrderByDescending(e => e.StartDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all employees");
            throw new EmployeeServiceException("Failed to retrieve employees", ex);
        }
    }

    public async Task<Employee> GetEmployeeById(int id)
    {
        try
        {
            var employee = await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
            {
                _logger.LogWarning("Employee with ID {EmployeeId} not found", id);
            }

            return employee;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employee with ID {EmployeeId}", id);
            throw new EmployeeServiceException($"Failed to retrieve employee with ID {id}", ex);
        }
    }

    public async Task<Employee> CreateEmployee(Employee employee)
    {
        if (employee == null)
        {
            throw new ArgumentNullException(nameof(employee));
        }

        try
        {
            // Check for duplicate email
            if (await IsEmailInUse(employee.Email))
            {
                throw new EmployeeServiceException($"Email {employee.Email} is already in use");
            }

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            // Refresh the employee object to get any database-generated values
            await _context.Entry(employee).ReloadAsync();

            return employee;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error occurred while creating employee");
            throw new EmployeeServiceException("Failed to create employee record", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employee");
            throw new EmployeeServiceException("Failed to create employee", ex);
        }
    }

    public async Task<Employee> UpdateEmployee(Employee employee)
    {
        if (employee == null)
        {
            throw new ArgumentNullException(nameof(employee));
        }

        try
        {
            // Check if employee exists
            if (!await EmployeeExists(employee.Id))
            {
                throw new EmployeeNotFoundException(employee.Id);
            }

            // Check for duplicate email, excluding current employee
            if (await IsEmailInUse(employee.Email, employee.Id))
            {
                throw new EmployeeServiceException($"Email {employee.Email} is already in use");
            }

            _context.Entry(employee).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            // Refresh the employee object
            await _context.Entry(employee).ReloadAsync();

            return employee;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency error updating employee {EmployeeId}", employee.Id);
            throw new EmployeeServiceException("Employee was modified by another user", ex);
        }
        catch (Exception ex) when (ex is not EmployeeServiceException)
        {
            _logger.LogError(ex, "Error updating employee {EmployeeId}", employee.Id);
            throw new EmployeeServiceException($"Failed to update employee {employee.Id}", ex);
        }
    }

    public async Task<bool> DeleteEmployee(int id)
    {
        try
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return false;
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting employee {EmployeeId}", id);
            throw new EmployeeServiceException($"Failed to delete employee {id}", ex);
        }
    }

    public async Task<bool> EmployeeExists(int id)
    {
        return await _context.Employees.AnyAsync(e => e.Id == id);
    }

    public async Task<bool> IsEmailUnique(string email, int? excludeEmployeeId = null)
    {
        return !await IsEmailInUse(email, excludeEmployeeId);
    }

    private async Task<bool> IsEmailInUse(string email, int? excludeEmployeeId = null)
    {
        var query = _context.Employees.AsNoTracking();

        if (excludeEmployeeId.HasValue)
        {
            query = query.Where(e => e.Id != excludeEmployeeId.Value);
        }

        return await query.AnyAsync(e => e.Email == email);
    }
}

public class EmployeeServiceException : Exception
{
    public EmployeeServiceException(string message) : base(message) { }
    public EmployeeServiceException(string message, Exception innerException)
        : base(message, innerException) { }
}

public class EmployeeNotFoundException : EmployeeServiceException
{
    public EmployeeNotFoundException(int employeeId)
        : base($"Employee with ID {employeeId} not found") { }
}