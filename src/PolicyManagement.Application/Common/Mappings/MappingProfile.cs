//Generated using AI. 
using AutoMapper;
using PolicyManagement.Application.DTOs.Claim;
using PolicyManagement.Application.DTOs.Policy;
using PolicyManagement.Domain.Entities.TenantsDb;

namespace PolicyManagement.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Policy, PolicyDto>()
            .ForMember(dest => dest.PolicyTypeName, opt => opt.MapFrom(src => 
                src.PolicyType != null ? src.PolicyType.Name : string.Empty))
            .ForMember(dest => dest.Claims, opt => opt.MapFrom(src => src.Claims))
            .ForMember(dest => dest.ClientPolicies, opt => opt.MapFrom(src => src.ClientPolicies))
            .ForMember(dest => dest.TenantId, opt => opt.MapFrom(src => src.TenantId))
            .ForMember(dest => dest.TenantName, opt => opt.Ignore());

        CreateMap<(Policy Policy, string TenantId, string TenantName), PolicyDto>()
            .IncludeMembers(src => src.Policy)
            .ForMember(dest => dest.TenantName, opt => opt.MapFrom(src => src.TenantName));
            
        CreateMap<CreatePolicyDto, Policy>()
            .ForMember(dest => dest.CreationDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PolicyType, opt => opt.Ignore())
            .ForMember(dest => dest.ClientPolicies, opt => opt.Ignore())
            .ForMember(dest => dest.Claims, opt => opt.Ignore())
            .ForMember(dest => dest.TenantId, opt => opt.MapFrom(src => src.TenantId));
            
        CreateMap<UpdatePolicyDto, Policy>()
            .ForMember(dest => dest.CreationDate, opt => opt.Ignore())
            .ForMember(dest => dest.PolicyType, opt => opt.Ignore())
            .ForMember(dest => dest.ClientPolicies, opt => opt.Ignore())
            .ForMember(dest => dest.Claims, opt => opt.Ignore());
            
        CreateMap<Policy, UpdatePolicyDto>();
        
        CreateMap<Claim, ClaimDto>()
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => 
                src.Status.ToString()))
            .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => 
                src.Client != null ? src.Client.FullName : string.Empty));
                
        CreateMap<Client, ClientDto>();
        
        CreateMap<ClientPolicy, ClientPolicyDto>()
            .ForMember(dest => dest.Client, opt => opt.MapFrom(src => src.Client));
    }
} 