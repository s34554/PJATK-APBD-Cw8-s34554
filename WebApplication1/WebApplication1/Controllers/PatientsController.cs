using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs;
using WebApplication1.Exceptions;
using WebApplication1.Services;

namespace WebApplication1.Controllers;


[ApiController]
[Route("api/[controller]")]
public class PatientsController(IPatientService service): ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPatients([FromQuery] string? search)
    {
        var patients = await service.GetPatientsAsync(search);
        return Ok(patients);
    }
    [HttpPost("{pesel}/bedassignments")]
    public async Task<IActionResult> AssignBed(string pesel, [FromBody] CreateBedAssignmentDto dto)
    {
        try
        {
            var id = await service.AssignBedAsync(pesel, dto);
            return Created($"/api/patients/{pesel}/bedassignments/{id}", new { id });
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}