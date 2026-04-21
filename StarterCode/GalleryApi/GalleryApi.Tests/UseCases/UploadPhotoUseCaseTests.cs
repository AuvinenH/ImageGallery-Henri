using GalleryApi.Application.DTOs;
using GalleryApi.Application.UseCases.Photos;
using GalleryApi.Domain.Entities;
using GalleryApi.Domain.Interfaces;
using Moq;
using Xunit;

namespace GalleryApi.Tests.UseCases;

public class UploadPhotoUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_PalauttaaOnnistumisen_KunPyyntoOnValidi()
    {
        var albumId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var mockAlbumRepository = new Mock<IAlbumRepository>();
        mockAlbumRepository
            .Setup(r => r.GetByIdAsync(albumId))
            .ReturnsAsync(new Album
            {
                Id = albumId,
                Name = "Albumi",
                Description = "Testi",
                CreatedAt = now,
                Photos = []
            });

        var expectedUrl = $"/uploads/{albumId}/photo.jpg";
        var mockStorageService = new Mock<IStorageService>();
        mockStorageService
            .Setup(s => s.UploadAsync(It.IsAny<Stream>(), "photo.jpg", "image/jpeg", albumId))
            .ReturnsAsync(expectedUrl);

        var mockPhotoRepository = new Mock<IPhotoRepository>();
        mockPhotoRepository
            .Setup(r => r.CreateAsync(It.IsAny<Photo>()))
            .ReturnsAsync((Photo p) => p);

        var useCase = new UploadPhotoUseCase(
            mockPhotoRepository.Object,
            mockAlbumRepository.Object,
            mockStorageService.Object);

        using var stream = new MemoryStream([1, 2, 3, 4]);
        var request = new UploadPhotoRequest(
            AlbumId: albumId,
            Title: "Auringonlasku",
            FileStream: stream,
            FileName: "photo.jpg",
            ContentType: "image/jpeg",
            FileSize: stream.Length);

        var result = await useCase.ExecuteAsync(request);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(albumId, result.Value!.AlbumId);
        Assert.Equal("Auringonlasku", result.Value.Title);
        Assert.Equal(expectedUrl, result.Value.ImageUrl);

        mockAlbumRepository.Verify(r => r.GetByIdAsync(albumId), Times.Once);
        mockStorageService.Verify(s => s.UploadAsync(It.IsAny<Stream>(), "photo.jpg", "image/jpeg", albumId), Times.Once);
        mockPhotoRepository.Verify(r => r.CreateAsync(It.IsAny<Photo>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_PalauttaaVirheen_KunAlbumiaEiLoydy()
    {
        var albumId = Guid.NewGuid();

        var mockAlbumRepository = new Mock<IAlbumRepository>();
        mockAlbumRepository
            .Setup(r => r.GetByIdAsync(albumId))
            .ReturnsAsync((Album?)null);

        var mockPhotoRepository = new Mock<IPhotoRepository>();
        var mockStorageService = new Mock<IStorageService>();

        var useCase = new UploadPhotoUseCase(
            mockPhotoRepository.Object,
            mockAlbumRepository.Object,
            mockStorageService.Object);

        using var stream = new MemoryStream([1, 2, 3]);
        var request = new UploadPhotoRequest(
            AlbumId: albumId,
            Title: "Kuva",
            FileStream: stream,
            FileName: "photo.jpg",
            ContentType: "image/jpeg",
            FileSize: stream.Length);

        var result = await useCase.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Contains("ei löydy", result.Error ?? string.Empty);

        mockStorageService.Verify(s => s.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Never);
        mockPhotoRepository.Verify(r => r.CreateAsync(It.IsAny<Photo>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_PalauttaaVirheen_KunTiedostotyyppiOnVaara()
    {
        var albumId = Guid.NewGuid();

        var mockAlbumRepository = new Mock<IAlbumRepository>();
        mockAlbumRepository
            .Setup(r => r.GetByIdAsync(albumId))
            .ReturnsAsync(new Album { Id = albumId, Name = "Albumi", Photos = [] });

        var mockPhotoRepository = new Mock<IPhotoRepository>();
        var mockStorageService = new Mock<IStorageService>();

        var useCase = new UploadPhotoUseCase(
            mockPhotoRepository.Object,
            mockAlbumRepository.Object,
            mockStorageService.Object);

        using var stream = new MemoryStream([1, 2, 3]);
        var request = new UploadPhotoRequest(
            AlbumId: albumId,
            Title: "Kuva",
            FileStream: stream,
            FileName: "malicious.exe",
            ContentType: "application/octet-stream",
            FileSize: stream.Length);

        var result = await useCase.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Contains("ei ole sallittu", result.Error ?? string.Empty);

        mockStorageService.Verify(s => s.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Never);
        mockPhotoRepository.Verify(r => r.CreateAsync(It.IsAny<Photo>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_PalauttaaVirheen_KunTiedostoOnLiianSuuri()
    {
        var albumId = Guid.NewGuid();

        var mockAlbumRepository = new Mock<IAlbumRepository>();
        mockAlbumRepository
            .Setup(r => r.GetByIdAsync(albumId))
            .ReturnsAsync(new Album { Id = albumId, Name = "Albumi", Photos = [] });

        var mockPhotoRepository = new Mock<IPhotoRepository>();
        var mockStorageService = new Mock<IStorageService>();

        var useCase = new UploadPhotoUseCase(
            mockPhotoRepository.Object,
            mockAlbumRepository.Object,
            mockStorageService.Object);

        const long elevenMb = 11 * 1024 * 1024;
        using var stream = new MemoryStream([1, 2, 3]);
        var request = new UploadPhotoRequest(
            AlbumId: albumId,
            Title: "Iso kuva",
            FileStream: stream,
            FileName: "big.jpg",
            ContentType: "image/jpeg",
            FileSize: elevenMb);

        var result = await useCase.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Contains("liian suuri", result.Error ?? string.Empty);

        mockStorageService.Verify(s => s.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Never);
        mockPhotoRepository.Verify(r => r.CreateAsync(It.IsAny<Photo>()), Times.Never);
    }
}
