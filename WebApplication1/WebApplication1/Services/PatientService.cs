using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs;
using WebApplication1.Exceptions;
using WebApplication1.Models;

namespace WebApplication1.Services;

public class PatientService(HospitalContext context) : IPatientService
{
    public async Task<List<PatientDto>> GetPatientsAsync(string? search)
    {
        var query = context.Patients.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search}%";
            query = query.Where(p =>
                EF.Functions.Like(p.FirstName, pattern) ||
                EF.Functions.Like(p.LastName, pattern));
        }

        return await query.Select(p => new PatientDto(
            p.Pesel,
            p.FirstName,
            p.LastName,
            p.Age,
            p.Sex ? "Male" : "Female",
            p.Admissions.Select(a => new AdmissionDto(
                a.Id,
                a.AdmissionDate,
                a.DischargeDate,
                new WardDto(
                    a.WardId,
                    a.Ward.Name,
                    a.Ward.Description
                )
            )).ToList(),
            p.BedAssignments.Select(ba => new BedAssignmentDto(
                ba.Id,
                ba.From,
                ba.To,
                new BedDto(
                    ba.Bed.Id,
                    new BedTypeDto(
                        ba.Bed.BedTypeId,
                        ba.Bed.BedType.Name,
                        ba.Bed.BedType.Description
                    ),
                    new RoomDto(
                        ba.Bed.Room.Id,
                        ba.Bed.Room.HasTv,
                        new WardDto(
                            ba.Bed.Room.Ward.Id,
                            ba.Bed.Room.Ward.Name,
                            ba.Bed.Room.Ward.Description
                        )
                    )
                ))
            ).ToList()
        )).ToListAsync();
    }

    public async Task<int> AssignBedAsync(string pesel, CreateBedAssignmentDto dto)
    {
        var patient = await context.Patients.FirstOrDefaultAsync(p => p.Pesel == pesel);
        if (patient is null) throw new NotFoundException($"Patient with PESEL {pesel} not found.");
        var ward = await context.Wards.FirstOrDefaultAsync(w => w.Name == dto.Ward);
        if (ward is null) throw new NotFoundException($"Ward '{dto.Ward}' not found.");
        var bedType = await context.BedTypes.FirstOrDefaultAsync(bt => bt.Name == dto.BedType);
        if (bedType is null) throw new NotFoundException($"Bed type '{dto.BedType}' not found.");
        var availableBed = await context.Beds
            .Where(b => b.BedTypeId == bedType.Id && b.Room.WardId == ward.Id)
            .Where(b => !b.BedAssignments.Any(ba =>
                ba.From < (dto.To ?? DateTime.MaxValue)
                && (ba.To == null || ba.To > dto.From)))
            .FirstOrDefaultAsync();
        if (availableBed is null)
            throw new NotFoundException(
                "No available bed of the specified type in the specified ward for the requested time range.");
        var assignment = new BedAssignment
        {
            PatientPesel = pesel,
            BedId = availableBed.Id,
            From = dto.From,
            To = dto.To
        };
        context.BedAssignments.Add(assignment);
        await context.SaveChangesAsync();

        return assignment.Id;
    }
}