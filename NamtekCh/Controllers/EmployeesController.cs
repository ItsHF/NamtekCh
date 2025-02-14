using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Annotations;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class EmployeeController : ControllerBase
{
    private readonly ILogger<EmployeeController> _logger;
    private readonly IEmployeeService _employeeService;
    private readonly string _mediaBasePath;

    public EmployeeController(
        ILogger<EmployeeController> logger,
        IEmployeeService employeeService)
    {
        _logger = logger;
        _employeeService = employeeService;
        _mediaBasePath = Path.Combine(Directory.GetCurrentDirectory(), "Media");
    }

    // GET: api/Employee
    [HttpGet]
    public async Task<IActionResult> GetEmployees()
    {
        var employees = await _employeeService.GetAllEmployees();
        return Ok(employees);
    }

    // GET: api/Employee/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEmployee(int id)
    {
        var employee = await _employeeService.GetEmployeeById(id);
        return employee != null ? Ok(employee) : NotFound();
    }

    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<Employee>> CreateEmployee([FromBody] EmployeeCreateRequest request)
    {
        try
        {
            if (request?.Employee == null)
            {
                return BadRequest("Employee data is required.");
            }

            var validationResult = ValidateEmployee(request.Employee);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            request.Employee.PhotoPath = !string.IsNullOrEmpty(request.Photo)
                ? await ProcessBase64File(request.Photo, "Photos", "photo", new[] { ".jpg", ".png", ".jpeg" })
                : null;

            request.Employee.CVPath = !string.IsNullOrEmpty(request.CV)
                ? await ProcessBase64File(request.CV, "CV", "cv", new[] { ".pdf", ".doc", ".docx" })
                : null;

            request.Employee.IdCardPath = !string.IsNullOrEmpty(request.IdCard)
                ? await ProcessBase64File(request.IdCard, "ID", "idcard", new[] { ".jpg", ".png", ".jpeg", ".pdf" })
                : null;

            var createdEmployee = await _employeeService.CreateEmployee(request.Employee);

            if (createdEmployee == null)
            {
                await CleanupFiles(request.Employee);
                return StatusCode(500, "Failed to create employee record");
            }

            return Created($"api/Employee/{createdEmployee.Id}", createdEmployee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employee");
            return StatusCode(500, "An error occurred while creating the employee.");
        }
    }

    private (bool IsValid, List<string> Errors) ValidateEmployee(Employee employee)
    {
        var errors = new List<string>();

        if (employee.DateOfBirth.AddYears(18) > DateTime.Now)
        {
            errors.Add("Employee must be at least 18 years old.");
        }

        if (employee.Salary < 1500)
        {
            errors.Add("Salary must be at least 1500.");
        }

        if (string.IsNullOrEmpty(employee.Email) || !IsValidEmail(employee.Email))
        {
            errors.Add("Valid email address is required.");
        }

        return (errors.Count == 0, errors);
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private async Task<string> ProcessBase64File(string base64String, string folder, string prefix, string[] allowedExtensions)
    {
        if (string.IsNullOrEmpty(base64String))
        {
            return null;
        }

        // Extract file type and data
        var match = Regex.Match(base64String, @"data:(.+);base64,(.+)");
        if (!match.Success)
        {
            return BadRequest("Invalid base64 string format").ToString();
        }

        var contentType = match.Groups[1].Value;
        var base64Data = match.Groups[2].Value;
        var extension = GetFileExtension(contentType);

        if (!allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest($"File type {extension} is not allowed. Allowed types: {string.Join(", ", allowedExtensions)}").ToString();
        }

        var fileName = $"{prefix}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid()}{extension}";
        var relativePath = Path.Combine(folder, fileName);
        var fullPath = Path.Combine(_mediaBasePath, relativePath);

        // Ensure directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

        // Save file
        await System.IO.File.WriteAllBytesAsync(
            fullPath,
            Convert.FromBase64String(base64Data));

        return relativePath;
    }

    private async Task CleanupFiles(Employee employee)
    {
        try
        {
            if (!string.IsNullOrEmpty(employee.PhotoPath))
                await DeleteFileAsync(employee.PhotoPath);
            if (!string.IsNullOrEmpty(employee.CVPath))
                await DeleteFileAsync(employee.CVPath);
            if (!string.IsNullOrEmpty(employee.IdCardPath))
                await DeleteFileAsync(employee.IdCardPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up files after failed employee creation");
        }
    }

    private async Task DeleteFileAsync(string relativePath)
    {
        var fullPath = Path.Combine(_mediaBasePath, relativePath);
        if (await Task.Run(() => System.IO.File.Exists(fullPath)))
        {
            await Task.Run(() => System.IO.File.Delete(fullPath));
        }
    }

    private string GetFileExtension(string contentType)
    {
        return contentType.ToLower() switch
        {
            "image/jpeg" => ".jpg",
            "image/jpg" => ".jpg",
            "image/png" => ".png",
            "application/pdf" => ".pdf",
            "application/msword" => ".doc",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => ".docx",
            _ => string.Empty
        };
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEmployee(int id, [FromForm] Employee updatedEmployee, string photoBase64, string cvBase64, string idCardBase64)
    {
        var existingEmployee = await _employeeService.GetEmployeeById(id);
        if (existingEmployee == null)
        {
            return NotFound();
        }

        // Update provided fields, leave others as they are
        existingEmployee.Name = updatedEmployee.Name ?? existingEmployee.Name;
        existingEmployee.Email = updatedEmployee.Email ?? existingEmployee.Email;
        existingEmployee.PhoneNumber = updatedEmployee.PhoneNumber ?? existingEmployee.PhoneNumber;
        existingEmployee.DateOfBirth = updatedEmployee.DateOfBirth != default ? updatedEmployee.DateOfBirth : existingEmployee.DateOfBirth;
        existingEmployee.JobTitle = updatedEmployee.JobTitle ?? existingEmployee.JobTitle;
        existingEmployee.Department = updatedEmployee.Department ?? existingEmployee.Department;
        existingEmployee.Salary = updatedEmployee.Salary != default ? updatedEmployee.Salary : existingEmployee.Salary;
        existingEmployee.StartDate = updatedEmployee.StartDate != default ? updatedEmployee.StartDate : existingEmployee.StartDate;
        existingEmployee.EndDate = updatedEmployee.EndDate ?? existingEmployee.EndDate; // Keep this

        if (!string.IsNullOrEmpty(photoBase64))
        {
            existingEmployee.PhotoPath = await SaveFileFromBase64(photoBase64, "Photos", "photo");
        }
        if (!string.IsNullOrEmpty(cvBase64))
        {
            existingEmployee.CVPath = await SaveFileFromBase64(cvBase64, "CV", "cv");
        }
        if (!string.IsNullOrEmpty(idCardBase64))
        {
            existingEmployee.IdCardPath = await SaveFileFromBase64(idCardBase64, "ID", "idCard");
        }

        await _employeeService.UpdateEmployee(existingEmployee);
        return NoContent();
    }

    // DELETE: api/Employee/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        var employee = await _employeeService.GetEmployeeById(id);
        if (employee == null)
        {
            return NotFound();
        }

        // Delete associated files only if they exist
        DeleteFile(employee.PhotoPath);
        DeleteFile(employee.CVPath);
        DeleteFile(employee.IdCardPath);

        await _employeeService.DeleteEmployee(id);
        return NoContent();
    }

    private async Task<string> SaveFileFromBase64(string base64String, string folderName, string fileNamePrefix)
    {
        try
        {
            var fileBytes = Convert.FromBase64String(base64String);
            var fileExtension = GetFileExtensionFromBase64(base64String); // Corrected
            var uniqueFileName = $"{fileNamePrefix}_{Guid.NewGuid()}{fileExtension}";

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Media", folderName);
            Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, uniqueFileName);
            await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);

            return Path.Combine(folderName, uniqueFileName);
        }
        catch (IOException ioEx)
        {
            _logger.LogError(ioEx, "Error saving file to disk.");
            return BadRequest("Error saving file to disk.").ToString();
        }
        catch (UnauthorizedAccessException uaEx)
        {
            _logger.LogError(uaEx, "Permission error while saving file.");
            return BadRequest("Permission error while saving file.").ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving file.");
            return BadRequest("Error saving file.").ToString();
        }
    }

    private string GetFileExtensionFromBase64(string base64String)
    {
        var match = Regex.Match(base64String, @"data:(.+);base64,(.+)");
        if (!match.Success)
        {
            return string.Empty;
        }

        var contentType = match.Groups[1].Value;
        return GetFileExtension(contentType);
    }

    private void DeleteFile(string filePath)
    {
        if (!string.IsNullOrEmpty(filePath))
        {
            string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), "Media", filePath);
            if (System.IO.File.Exists(absolutePath))
            {
                System.IO.File.Delete(absolutePath);
            }
        }
    }
}
