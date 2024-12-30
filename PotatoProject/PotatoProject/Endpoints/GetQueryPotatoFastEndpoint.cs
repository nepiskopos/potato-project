namespace PotatoProject.Endpoints
{
    public sealed record GetQueryPotatoRequest
    {
        [FastEndpoints.QueryParam]
        [FastEndpoints.BindFrom("id")]
        public required int Id { get; set; } = -1;
    }

    public sealed record GetQueryPotatoResponse
    {
        public required int Id { get; set; } = -1;
        public required string? Description { get; set; }
    }

    public sealed class GetQueryPotatoFastEndpoint : FastEndpoints.Endpoint<GetQueryPotatoRequest, GetQueryPotatoResponse>
    {
        private readonly AutoMapper.IMapper _mapper;

        public GetQueryPotatoFastEndpoint(AutoMapper.IMapper mapper) => _mapper = mapper;

        public override void Configure()
        {
            Get("/api/potato");
            AllowAnonymous();
        }

        public override async Task HandleAsync(GetQueryPotatoRequest r, CancellationToken c)
        {
            if (r.Id < 1)
            {
                await SendErrorsAsync(statusCode: (int)System.Net.HttpStatusCode.UnprocessableEntity, cancellation: c);
                return;
            }

            using (var dbContext = new DBContexts.PotatoDbContext())
            {
                var potato = await dbContext.queryPotatoByIdAsync(r.Id);

                Serilog.Log.Information(r.Id.ToString());

                // Map entity to response
                var potatoResponse = _mapper.Map<GetQueryPotatoResponse>(potato);

                if (potato is null)
                {
                    Serilog.Log.Information("Potato not found");
                    await SendOkAsync(response: potatoResponse, cancellation: c);
                    return;
                }
                else
                {
                    Serilog.Log.Information(potato.Id.ToString());
                    Serilog.Log.Information(potato.Description);
                    await SendOkAsync(response: potatoResponse, cancellation: c);
                    return;
                }
            }
        }
    }
}