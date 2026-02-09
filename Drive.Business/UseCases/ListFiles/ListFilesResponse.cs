namespace Drive.Business.UseCases.ListFiles;

public record ListFilesItemResponse(
    Guid Id,
    string OriginalName,
    long SizeInBytes,
    string ContentType,
    DateTime CreatedAtUtc
);

public record ListFilesResponse(
    IReadOnlyList<ListFilesItemResponse> Items
);