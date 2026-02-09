namespace Drive.Business.UseCases.ListFiles;

public record ListFilesRequest(
    string OwnerUserId,
    string? Search
);

