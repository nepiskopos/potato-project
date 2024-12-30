namespace PotatoProject.Mapper
{
    public class PotatoAutoMapper : AutoMapper.Profile
    {
        public PotatoAutoMapper()
        {
            // Define GET query potato mappings
            CreateMap<Endpoints.GetQueryPotatoRequest, Models.Potato>();
            CreateMap<Models.Potato, Endpoints.GetQueryPotatoResponse>();

            // Define POST create potato mappings
            CreateMap<Endpoints.PostCreatePotatoRequest, Models.Potato>();
            CreateMap<Models.Potato, Endpoints.PostCreatePotatoResponse>();
        }
    }
}