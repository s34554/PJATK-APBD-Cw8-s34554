using WebApplication1.DTOs;

namespace WebApplication1.Services;

public interface IPatientService
{
    Task<List<PatientDto>> GetPatientsAsync(string? search, CancellationToken cancellationToken);
    Task<int> AssignBedAsync(string pesel, CreateBedAssignmentDto dto, CancellationToken cancellationToken);
}