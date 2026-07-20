using Perigon.AspNetCore.Toolkit.Helpers;
using SixLabors.ImageSharp;
using TUnit.Assertions;

namespace ApiTest;

public sealed class ImageHelperTests
{
    [Test]
    [Category("Unit")]
    public async Task GenerateImageCaptcha_WhenGivenDimensions_ReturnsDecodablePng()
    {
        var imageBytes = ImageHelper.GenerateImageCaptcha("A1B2", width: 120, height: 60);

        using var image = Image.Load(imageBytes);

        await Assert.That(image.Width).IsEqualTo(120);
        await Assert.That(image.Height).IsEqualTo(60);
        await Assert.That(image.Metadata.DecodedImageFormat!.Name).IsEqualTo("PNG");
    }
}
