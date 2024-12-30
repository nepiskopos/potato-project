namespace PotatoProject.Endpoints
{
    public sealed record PostCreatePotatoRequest
    {
        [FastEndpoints.BindFrom("description")]
        public required string Description { get; set; }
    }

    public sealed record PostCreatePotatoResponse
    {
        public required int Id { get; set; }
        public required string? Description { get; set; }
    }

    public sealed class PostCreatePotatoFastEndpoint : FastEndpoints.Endpoint<PostCreatePotatoRequest, PostCreatePotatoResponse>
    {
        private readonly AutoMapper.IMapper _mapper;

        public PostCreatePotatoFastEndpoint(AutoMapper.IMapper mapper) => _mapper = mapper;

        public override void Configure()
        {
            Post("/api/potato");
            AllowAnonymous();
        }

        public override async Task HandleAsync(PostCreatePotatoRequest r, CancellationToken c)
        {
            if (string.IsNullOrEmpty(r.Description))
            {
                await SendErrorsAsync((int)System.Net.HttpStatusCode.UnprocessableEntity, cancellation: c);
                return;
            }

            using (var dbContext = new DBContexts.PotatoDbContext())
            {
                // Map from Request DTO to Domain Model
                var potato = _mapper.Map<Models.Potato>(r);

                Serilog.Log.Information(potato.Description);

                var result = await dbContext.createPotatoAsync(potato);

                if (result is null)
                {
                    Serilog.Log.Information("Potato creation failed");
                    await SendErrorsAsync(cancellation: c);
                    return;
                }
                else
                {
                    // Map from Domain Model (with generated ID) to Response DTO
                    var response = _mapper.Map<PostCreatePotatoResponse>(result);

                    Serilog.Log.Information(potato.Id.ToString());
                    Serilog.Log.Information(potato.Description);
                    await SendOkAsync(response, cancellation: c);
                    return;
                }
            }
        }
    }
}