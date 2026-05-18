using WebApplication1.DTOs;

namespace WebApplication1.Services;

public interface IPatientService
{
    Task<List<PatientDto>> GetPatientsAsync(string? search);
    Task<int> AssignBedAsync(string pesel, CreateBedAssignmentDto dto);
}