using GalleryApi.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace GalleryApi.Infrastructure.Moderation;

public class ModerationServiceClient
{
    private readonly ModerationServiceOptions _options;

    // Muutettu: ei enää kovakoodattua merkkijonoa
    public ModerationServiceClient(IOptions<ModerationServiceOptions> options)
    {
        _options = options.Value;
    }

    public Task<bool> IsContentSafeAsync(Stream imageStream, string contentType)
    {
        // Simuloitu tarkistus — käyttäisi _options.ApiKey:ta oikeassa toteutuksessa
        return Task.FromResult(true);
    }
}