namespace WebApplication1.DTOs;

public record AdmissionDto
(
    int Id,
    DateTime AdmissionDate,
    DateTime? DischargeDate,
    WardDto Ward
);