using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FilterBuilderApi.Controllers;

[ApiController]
[Route("")]
[Route("api")]
public class SearchController : ControllerBase
{
    private readonly AppDbContext _db;

    public SearchController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search(
        [FromBody] JsonArray filter)
    {
        FilterGroup group = DevExtremeFilterParser.Parse(filter);

        var predicate =
            FilterExpressionBuilder.Build<Employee>(group);

        var result = await _db.Employees
            .Where(predicate)
            .ToListAsync();

        return Ok(result);
    }
}
