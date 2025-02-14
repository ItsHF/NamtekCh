using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

public class TimesheetService : ITimesheetService
{
    private readonly ApplicationDbContext _context;

    public TimesheetService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Timesheet>> GetTimesheetsAsync()
    {
        return await _context.Timesheets.ToListAsync();
    }

    public async Task<Timesheet> GetTimesheetByIdAsync(int id)
    {
        return await _context.Timesheets.FindAsync(id);
    }

    public async Task CreateTimesheetAsync(Timesheet timesheet)
    {
        _context.Timesheets.Add(timesheet);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateTimesheetAsync(Timesheet timesheet)
    {
        _context.Entry(timesheet).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteTimesheetAsync(int id)
    {
        var timesheet = await _context.Timesheets.FindAsync(id);
        if (timesheet != null)
        {
            _context.Timesheets.Remove(timesheet);
            await _context.SaveChangesAsync();
        }
    }
}
