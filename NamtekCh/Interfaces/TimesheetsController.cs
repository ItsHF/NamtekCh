using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class TimesheetController : ControllerBase
{
    private readonly ITimesheetService _timesheetService;

    public TimesheetController(ITimesheetService timesheetService)
    {
        _timesheetService = timesheetService;
    }

    // GET: api/Timesheet
    [HttpGet]
    public async Task<IActionResult> GetTimesheets()
    {
        var timesheets = await _timesheetService.GetTimesheetsAsync();
        return Ok(timesheets);
    }

    // GET: api/Timesheet/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTimesheet(int id)
    {
        var timesheet = await _timesheetService.GetTimesheetByIdAsync(id);
        if (timesheet == null)
        {
            return NotFound();
        }
        return Ok(timesheet);
    }

    // POST: api/Timesheet
    [HttpPost]
    public async Task<IActionResult> CreateTimesheet([FromBody] Timesheet timesheet)
    {
        // Ensure start time is before end time
        if (timesheet.StartTime >= timesheet.EndTime)
        {
            return BadRequest("Start time must be before end time.");
        }

        await _timesheetService.CreateTimesheetAsync(timesheet);
        return CreatedAtAction(nameof(GetTimesheet), new { id = timesheet.Id }, timesheet);
    }

    // PUT: api/Timesheet/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTimesheet(int id, [FromBody] Timesheet timesheet)
    {
        if (id != timesheet.Id)
        {
            return BadRequest();
        }

        // Ensure start time is before end time
        if (timesheet.StartTime >= timesheet.EndTime)
        {
            return BadRequest("Start time must be before end time.");
        }

        await _timesheetService.UpdateTimesheetAsync(timesheet);
        return NoContent();
    }

    // DELETE: api/Timesheet/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTimesheet(int id)
    {
        var timesheet = await _timesheetService.GetTimesheetByIdAsync(id);
        if (timesheet == null)
        {
            return NotFound();
        }

        await _timesheetService.DeleteTimesheetAsync(id);
        return NoContent();
    }
}
