namespace tracking_management_api.Dtos;

public sealed record LoadReadResult(IReadOnlyList<LoadDto> Loads, bool FromCache);
