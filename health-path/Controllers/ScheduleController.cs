using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using health_path.Model;

namespace health_path.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScheduleController : ControllerBase
{
    private readonly ILogger<ScheduleController> _logger;
    private readonly IDbConnection _connection;

    public ScheduleController(ILogger<ScheduleController> logger, IDbConnection connection)
    {
        _logger = logger;
        _connection = connection;
    }

    [HttpGet]
    public ActionResult<IEnumerable<ScheduleEvent>> Fetch()
    {
        var dbResults = ReadData();

        return Ok(dbResults);
    }

    private IEnumerable<ScheduleEvent> ReadData() {
        var sql = @"
            SELECT e.*, r.*
            FROM Event e
            JOIN EventRecurrence r ON e.Id = r.EventId
            ORDER BY e.Id, r.DayOfWeek, r.StartTime, r.EndTime
        ";
        var lookup = new Dictionary<Guid, ScheduleEvent>();
        var result = _connection.Query<ScheduleEvent, ScheduleEventRecurrence, ScheduleEvent>(sql, (e, r) => {
            ScheduleEvent? res;
            if (!lookup.TryGetValue(e.Id, out res)) 
            {
                res = e;
                lookup.Add(e.Id, e);
            }
            if (res.Recurrences == null) 
            {
                res.Recurrences = new List<ScheduleEventRecurrence>();
            }
            res.Recurrences.Add(r);
            return res;
        });
        return result.Distinct();
    }
}
