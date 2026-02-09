namespace Drive.Business.UseCases.UploadFile;

public record UploadFileRequest(
    string OriginalName,
    string ContentType,
    long SizeInBytes,
    Stream Content,
    string OwnerUserId
);