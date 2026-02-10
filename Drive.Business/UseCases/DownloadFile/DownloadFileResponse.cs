namespace Drive.Business.UseCases.DownloadFile;

public record DownloadFileResponse(
    Stream Content,
    string ContentType,
    string FileName
);


