using tracking_management_api.Dtos;
using tracking_management_api.Mappers;
using tracking_management_api.Repositories.SafetyIncidents;

namespace tracking_management_api.Services.SafetyIncidents;

public sealed class SafetyIncidentService : ISafetyIncidentService
{
    private readonly ISafetyIncidentRepository _repository;
    private readonly ISafetyIncidentMapper _mapper;

    public SafetyIncidentService(ISafetyIncidentRepository repository, ISafetyIncidentMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(mapper);

        _repository = repository;
        _mapper = mapper;
    }

    public async Task<SafetyIncidentDto> RegisterAsync(
        CreateSafetyIncidentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var incident = _mapper.ToModel(request);
        var stored = await _repository.AddAsync(incident, cancellationToken);
        return _mapper.ToDto(stored);
    }
}
