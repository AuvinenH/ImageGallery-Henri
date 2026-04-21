using GalleryApi.Application.DTOs;
using GalleryApi.Application.UseCases.Photos;
using Microsoft.AspNetCore.Mvc;
using GalleryApi.Application.Common;
using GalleryApi.Domain.Entities;

namespace GalleryApi.WebApi.Controllers;

[ApiController]
[Route("api/albums/{albumId:guid}/photos")]
public class PhotosController : ControllerBase
{
    private readonly GetPhotosByAlbumUseCase _getPhotosByAlbum;
    private readonly UploadPhotoUseCase _uploadPhoto;
    private readonly DeletePhotoUseCase _deletePhoto;

    public PhotosController(
        GetPhotosByAlbumUseCase getPhotosByAlbum,
        UploadPhotoUseCase uploadPhoto,
        DeletePhotoUseCase deletePhoto)
    {
        _getPhotosByAlbum = getPhotosByAlbum;
        _uploadPhoto = uploadPhoto;
        _deletePhoto = deletePhoto;
    }

    /// <summary>Palauttaa albumin kuvat</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PhotoDto>>> GetByAlbum(Guid albumId)
    {
        var photos = await _getPhotosByAlbum.ExecuteAsync(albumId);
        return Ok(photos);
    }

    /// <summary>
    /// Lataa kuvan albumiin.
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<PhotoDto>> Upload(
        Guid albumId,
        [FromForm] string title,
        IFormFile file)
    {
        var request = new UploadPhotoRequest(
            AlbumId: albumId,
            Title: title,
            FileStream: file.OpenReadStream(),
            FileName: file.FileName,
            ContentType: file.ContentType,
            FileSize: file.Length);

        var result = await _uploadPhoto.ExecuteAsync(request);
        if (!result.IsSuccess)
            return BadRequest(new { Error = result.Error });

        return CreatedAtAction(nameof(GetByAlbum), new { albumId }, result.Value);
    }

    /// <summary>
    /// Poistaa kuvan.
    /// Palauttaa 501 kunnes DeletePhotoUseCase on toteutettu (Vaihe 8).
    /// </summary>
    [HttpDelete("{photoId:guid}")]
    public async Task<IActionResult> Delete(Guid albumId, Guid photoId)
    {
        try
        {
            var result = await _deletePhoto.ExecuteAsync(photoId);
            if (!result.IsSuccess)
                return NotFound(new { Error = result.Error });
            return NoContent();
        }
        catch (NotImplementedException ex)
        {
            return StatusCode(501, new { Error = ex.Message });
        }
    }
}
