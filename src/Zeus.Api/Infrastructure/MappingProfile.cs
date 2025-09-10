using AutoMapper;
using Zeus.Api.Models.Entities;
using Zeus.Api.Models.Resources;

namespace Zeus.Api.Mappers
{
    public class MappingProfile : Profile
    {
        public MappingProfile() {

            // CreateMap<source, destination>()
            //.ForMember(dest => dest.PropertyName, options => options.MapFrom(source => source.AnotherPropertyName));

             CreateMap<SubjectsEntity, SubjectsResponse>()
                .ForMember(dest => dest.Self, static options => options.MapFrom(source =>
                        Link.To(nameof(Controllers.SubjectsController.GetSubjects), default)));
        }
    }
}
