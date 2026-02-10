using Drive.Api.Models;
using Drive.Business.UseCases.DeleteFile;
using Drive.Business.UseCases.ListFiles;
using Drive.Business.UseCases.UploadFile;
using Drive.Business.UseCases.DownloadFile;
using Microsoft.AspNetCore.Mvc;

namespace Drive.Api.Controllers;

[ApiController]
[Route("files")]
public class FilesController : ControllerBase
{
    private readonly UploadFileUseCase _upload;
    private readonly ListFilesUseCase _list;
    private readonly DeleteFileUseCase _delete;
    private readonly DownloadFileUseCase _download;

    public FilesController(UploadFileUseCase upload, ListFilesUseCase list, DeleteFileUseCase delete,DownloadFileUseCase download)
    {
        _upload = upload;
        _list = list;
        _delete = delete;
        _download = download;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(200_000_000)]
    public async Task<IActionResult> Upload([FromForm] UploadFileForm form, CancellationToken ct)
    {
        var file = form.File;

        if (file == null || file.Length == 0)
            return BadRequest("Arquivo inválido.");

        await using var stream = file.OpenReadStream();

        // Por enquanto: usuário fixo (até JWT)
        var ownerUserId = "dev-user";

        var req = new UploadFileRequest(
            OriginalName: file.FileName,
            ContentType: string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
            SizeInBytes: file.Length,
            Content: stream,
            OwnerUserId: ownerUserId
        );

        var result = await _upload.ExecuteAsync(req, ct);

        return Created($"/files/{result.Id}", result);
    }


    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? search, CancellationToken ct)
    {
        var ownerUserId = "dev-user";

        var req = new ListFilesRequest(
            OwnerUserId: ownerUserId,
            Search: search
        );

        var result = await _list.ExecuteAsync(req, ct);
        return Ok (result);

    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken ct)
    {
        var OwnerUserId = "dev-user";

        try
        {
            var result = await _download.ExecuteAsync(new DownloadFileRequest(id, OwnerUserId),ct);

            return File(
                fileStream: result.Content,
                contentType: result.ContentType,
                fileDownloadName: result.FileName
            );
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }





    [HttpDelete("{id:guid}")]
public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
{
    var ownerUserId = "dev-user";

    try
    {
        await _delete.ExecuteAsync(new DeleteFileRequest(id, ownerUserId), ct);
        return NoContent(); // 204
    }
    catch (KeyNotFoundException)
    {
        return NotFound();
    }
    catch (UnauthorizedAccessException)
    {
        return Forbid();
    }
}




}
