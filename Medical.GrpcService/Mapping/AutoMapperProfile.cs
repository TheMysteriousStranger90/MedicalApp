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
                opt.MapFrom(src =>
                    Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(src.AppointmentDate.ToUniversalTime())));
        CreateMap<Appointment, AppointmentModel>()
            .ForMember(dest => dest.Fee, opt => opt.MapFrom(src => src.Fee))
            .ReverseMap();

        CreateMap<CreateAppointmentRequest, Appointment>()
            .ForMember(dest => dest.AppointmentDate, opt =>
                opt.MapFrom(src => src.AppointmentDate.ToDateTime()));

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
    }
}