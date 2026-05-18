using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using WebApplication1.Data;
using WebApplication1.Exceptions;
using WebApplication1.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<HospitalContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var invalidFields = context.ModelState
            .Where(kvp => kvp.Value?.Errors.Count > 0)
            .Select(kvp =>
            {
                var key = kvp.Key.StartsWith("$.") ? kvp.Key[2..] : kvp.Key;
                if (string.IsNullOrEmpty(key)) key = "body";

                var hasParseError = kvp.Value!.Errors
                    .Any(e => e.ErrorMessage.Contains("could not be converted"));

                return new
                {
                    field = key,
                    error = hasParseError
                        ? "Invalid value for this field."
                        : "This field is required or invalid."
                };
            })
            .ToArray();

        return new BadRequestObjectResult(new
        {
            message = "The request body is invalid.",
            errors = invalidFields
        });
    };
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();