using System.Collections.Generic;
using System.Threading.Tasks;

public interface ITimesheetService
{
    Task<List<Timesheet>> GetTimesheetsAsync();
    Task<Timesheet> GetTimesheetByIdAsync(int id);
    Task CreateTimesheetAsync(Timesheet timesheet);
    Task UpdateTimesheetAsync(Timesheet timesheet);
    Task DeleteTimesheetAsync(int id);
}
