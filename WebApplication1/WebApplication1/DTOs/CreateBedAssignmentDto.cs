namespace WebApplication1.DTOs;

public record CreateBedAssignmentDto(
    DateTime From,
    DateTime? To,
    string BedType,
    string Ward);