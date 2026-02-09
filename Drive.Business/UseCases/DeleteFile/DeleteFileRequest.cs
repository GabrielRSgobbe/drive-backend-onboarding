namespace Drive.Business.UseCases.DeleteFile;

public record DeleteFileRequest(
    Guid FileId,
    string OwnerUserId
);