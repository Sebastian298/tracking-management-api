using tracking_management_api.Dtos;
using tracking_management_api.Mappers;
using tracking_management_api.Repositories.Loads;

namespace tracking_management_api.Services.Loads;

public sealed class LoadService : ILoadService
{
    private readonly ILoadRepository _repository;
    private readonly ILoadMapper _mapper;

    public LoadService(ILoadRepository repository, ILoadMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(mapper);

        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<LoadDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var loads = await _repository.GetAllAsync(cancellationToken);
        return _mapper.ToDtoList(loads);
    }
}
