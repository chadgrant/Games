using AutoMapper;

namespace GameStudio.Games.WebApi.v1
{
	public class DomainProfile : Profile
	{
		public DomainProfile()
		{
			CreateMap<AddGameRequest, Game>();
			CreateMap<UpdateGameRequest, Game>();
			CreateMap<AddTagRequest, ReferenceItem>();
			CreateMap<UpdateTagsRequest, Reference>();
			CreateMap<AddBonusGameTypeRequest, ReferenceItem>();
			CreateMap<UpdateBonusGameTypesRequest, Reference>();
			CreateMap<AddSymbolRequest, ReferenceItem>();
			CreateMap<UpdateSymbolsRequest, Reference>();
		}
	}
}
