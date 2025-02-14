public class Timesheet
{
    public int Id { get; set; }
    
    public int EmployeeId { get; set; } //fk
    public Employee Employee { get; set; }

    // Start and end time fields
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    //  text input specifying the work done during the timesheet period
    public string Summary { get; set; }
}
