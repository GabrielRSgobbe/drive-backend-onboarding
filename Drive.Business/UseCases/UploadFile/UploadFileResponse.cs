namespace Drive.Business.UseCases.UploadFile;

public record UploadFileResponse(
    Guid Id,
    string OriginalName,
    long SizeInBytes,
    string ContentType,
    DateTime CreatedAtUtc
);