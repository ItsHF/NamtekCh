using System.ComponentModel.DataAnnotations;

public class Employee
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [Phone]
    public string PhoneNumber { get; set; }

    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required]
    [StringLength(100)]
    public string JobTitle { get; set; }

    [Required]
    [StringLength(100)]
    public string Department { get; set; }

    [Required]
    [Range(1500, 1000000)]
    public decimal Salary { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; } 

    public string? PhotoPath { get; set; }
    public string? CVPath { get; set; }
    public string?  IdCardPath { get; set; }
}