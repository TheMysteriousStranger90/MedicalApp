using Medical.GrpcService.Context;
using Medical.GrpcService.Mapping;
using Medical.GrpcService.Repositories;
using Medical.GrpcService.Repositories.Interfaces;
using Medical.GrpcService.Token;
using Microsoft.EntityFrameworkCore;

namespace Medical.GrpcService.Extensions;

public static class ApplicationServicesExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration config)
    {
        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DefaultConnection"))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

        
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IMedicalRecordRepository, MedicalRecordRepository>();
        services.AddScoped<IDoctorRepository, DoctorRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        services.AddAutoMapper(typeof(AutoMapperProfile));
        
        services.AddScoped<ITokenService, TokenService>();
        
        return services;
    }
}