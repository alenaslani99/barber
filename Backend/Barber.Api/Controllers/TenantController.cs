using Barber.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Barber.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class TenantController : ControllerBase
{
    private readonly TenantContext _db;

    public TenantController(TenantContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        string database = _db.Database.GetDbConnection().Database;
        int barbershops = await _db.Barbershops.CountAsync(ct);
        return Ok(new { database, barbershops });
    }
}
