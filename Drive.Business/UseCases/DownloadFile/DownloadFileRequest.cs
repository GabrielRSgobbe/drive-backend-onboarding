namespace Drive.Business.UseCases.DownloadFile;

public record DownloadFileRequest (
    Guid FileId,
    string OwnerUserId
);