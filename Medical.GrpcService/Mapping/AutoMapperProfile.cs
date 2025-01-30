using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Medical.GrpcService.Entities;
using Medical.GrpcService.Entities.DTOs;
using Medical.GrpcService.Entities.Enums;
using Enum = System.Enum;

namespace Medical.GrpcService.Mapping;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Appointment, AppointmentDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));

        CreateMap<Doctor, DoctorDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));

        CreateMap<Patient, PatientDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));

        CreateMap<MedicalRecord, MedicalRecordDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));

        CreateMap<AppointmentDto, AppointmentModel>()
            .ForMember(dest => dest.AppointmentDate, opt =>
                opt.MapFrom(src =>
                    Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(src.AppointmentDate.ToUniversalTime())))
            .ReverseMap()
            .ForMember(dest => dest.AppointmentDate, opt =>
                opt.MapFrom(src => src.AppointmentDate.ToDateTime()));

        CreateMap<UpdateAppointmentRequest, AppointmentDto>();
        CreateMap<CreateAppointmentRequest, AppointmentDto>();

        CreateMap<LabResult, LabResultDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));

        // Proto mappings
        CreateMap<Appointment, AppointmentModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.AppointmentDate, opt =>
                opt.MapFrom(src => Timestamp.FromDateTime(
                    DateTime.SpecifyKind(src.AppointmentDate, DateTimeKind.Utc))))
            .ReverseMap()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.Parse(src.Id)));

        CreateMap<CreateAppointmentRequest, Appointment>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.AppointmentDate, opt =>
                opt.MapFrom(src => src.AppointmentDate.ToDateTime()))
            .ForMember(dest => dest.Status, opt =>
                opt.MapFrom(src => AppointmentStatus.Scheduled))
            .ForMember(dest => dest.IsPaid, opt => opt.MapFrom(src => false));

        CreateMap<PatientDto, PatientModel>()
            .ForMember(dest => dest.DateOfBirth,
                opt => opt.MapFrom(src => Timestamp.FromDateTime(src.DateOfBirth.ToUniversalTime())))
            .ForMember(dest => dest.Gender,
                opt => opt.MapFrom(src => src.Gender.ToString()))
            .ReverseMap()
            .ForMember(dest => dest.DateOfBirth,
                opt => opt.MapFrom(src => src.DateOfBirth.ToDateTime()))
            .ForMember(dest => dest.Gender,
                opt => opt.MapFrom(src => Enum.Parse<Gender>(src.Gender)));

        CreateMap<MedicalRecordDto, MedicalRecordModel>()
            .ForMember(dest => dest.CreatedAt,
                opt => opt.MapFrom(src =>
                    Timestamp.FromDateTime(src.CreatedAt.ToUniversalTime())));

        CreateMap<LabResultDto, LabResultModel>()
            .ForMember(dest => dest.TestDate,
                opt => opt.MapFrom(src =>
                    Timestamp.FromDateTime(src.TestDate.ToUniversalTime())));

        CreateMap<LabResultModel, LabResult>()
            .ForMember(dest => dest.TestDate,
                opt => opt.MapFrom(src => src.TestDate.ToDateTime()));

        CreateMap<Doctor, DoctorDto>()
            .ReverseMap();
        CreateMap<DoctorDto, DoctorModel>()
            .ReverseMap();

        CreateMap<UpdateAppointmentRequest, Appointment>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
            .ForMember(dest => dest.Symptoms, opt => opt.MapFrom(src => src.Symptoms))
            .ForMember(dest => dest.IsPaid, opt => opt.MapFrom(src => src.IsPaid));
        
        CreateMap<MedicalRecord, MedicalRecordModel>()
            .ForMember(d => d.CreatedAt, 
                opt => opt.MapFrom(s => Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(s.CreatedAt)));

        CreateMap<LabResult, LabResultModel>().ReverseMap();
        CreateMap<CreateMedicalRecordRequest, MedicalRecord>()
            .ForMember(d => d.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(d => d.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
        
        
        
        CreateMap<AppointmentDto, AppointmentModel>()
            .ForMember(d => d.AppointmentDate,
                opt => opt.MapFrom(s => Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(
                    DateTime.SpecifyKind(s.AppointmentDate, DateTimeKind.Utc))));
        
        
        CreateMap<TimeSlot, TimeSlotDto>().ReverseMap();
        CreateMap<Schedule, ScheduleDto>().ReverseMap();
        
        CreateMap<TimeSlot, TimeSlotModel>()
            .ForMember(d => d.StartTime, 
                opt => opt.MapFrom(s => Timestamp.FromDateTime(s.StartTime.ToUniversalTime())))
            .ForMember(d => d.EndTime, 
                opt => opt.MapFrom(s => Timestamp.FromDateTime(s.EndTime.ToUniversalTime())));
                
        
        CreateMap<Schedule, ScheduleDto>()
            .ForMember(d => d.TimeSlots, 
                opt => opt.MapFrom(s => s.TimeSlots.OrderBy(ts => ts.StartTime)));

        CreateMap<Schedule, ScheduleModel>()
            .ForMember(d => d.StartTime, 
                opt => opt.MapFrom(s => 
                    Timestamp.FromDateTime(DateTime.Today.Add(s.StartTime).ToUniversalTime())))
            .ForMember(d => d.EndTime, 
                opt => opt.MapFrom(s => 
                    Timestamp.FromDateTime(DateTime.Today.Add(s.EndTime).ToUniversalTime())))
            .ForMember(d => d.TimeSlots, 
                opt => opt.MapFrom(s => s.TimeSlots.OrderBy(ts => ts.StartTime)));
        
        
        CreateMap<TimeSlotDto, TimeSlotModel>()
            .ForMember(d => d.StartTime, 
                opt => opt.MapFrom(s => 
                    Timestamp.FromDateTime(s.StartTime.ToUniversalTime())))
            .ForMember(d => d.EndTime, 
                opt => opt.MapFrom(s => 
                    Timestamp.FromDateTime(s.EndTime.ToUniversalTime())))
            .ForMember(d => d.AppointmentId, 
                opt => opt.MapFrom(s => s.AppointmentId ?? string.Empty))
            .ForMember(d => d.IsBooked, 
                opt => opt.MapFrom(s => s.IsBooked));

        CreateMap<ScheduleDto, ScheduleModel>()
            .ForMember(d => d.StartTime, 
                opt => opt.MapFrom(s => 
                    Timestamp.FromDateTime(DateTime.Today.Add(s.StartTime).ToUniversalTime())))
            .ForMember(d => d.EndTime, 
                opt => opt.MapFrom(s => 
                    Timestamp.FromDateTime(DateTime.Today.Add(s.EndTime).ToUniversalTime())))
            .ForMember(d => d.ValidFrom,
                opt => opt.MapFrom(s => 
                    s.ValidFrom.HasValue ? 
                        Timestamp.FromDateTime(s.ValidFrom.Value.ToUniversalTime()) : 
                        Timestamp.FromDateTime(DateTime.MinValue.ToUniversalTime())))
            .ForMember(d => d.ValidTo,
                opt => opt.MapFrom(s => 
                    s.ValidTo.HasValue ? 
                        Timestamp.FromDateTime(s.ValidTo.Value.ToUniversalTime()) : 
                        Timestamp.FromDateTime(DateTime.MaxValue.ToUniversalTime())));
        
        
        CreateMap<UpdateScheduleRequest, Schedule>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.Parse(src.Id)))
            .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.IsAvailable))
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime.ToDateTime().TimeOfDay))
            .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime.ToDateTime().TimeOfDay))
            .ForMember(dest => dest.SlotDurationMinutes, opt => opt.MapFrom(src => src.SlotDurationMinutes))
            .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
            .ForMember(dest => dest.DayOfWeek, opt => opt.MapFrom(src => (DayOfWeek)src.DayOfWeek));
    }
}