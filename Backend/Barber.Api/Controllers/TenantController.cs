using Barber.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Barber.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TenantController : ControllerBase
{
    private readonly TenantContext _db;

    public TenantController(TenantContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var database = _db.Database.GetDbConnection().Database;
        var barbershops = await _db.Barbershops.CountAsync(ct);
        return Ok(new { database, barbershops });
    }
}
