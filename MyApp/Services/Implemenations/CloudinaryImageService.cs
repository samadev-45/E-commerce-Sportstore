using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MyApp.Helpers;
using System;
using System.Threading.Tasks;

public class CloudinaryImageService : IImageService
{
    private readonly Cloudinary _cloudinary;
    private readonly string _defaultFolder;

    public CloudinaryImageService(Cloudinary cloudinary, IOptions<CloudinarySettings> settings)
    {
        _cloudinary = cloudinary ?? throw new ArgumentNullException(nameof(cloudinary));
        _defaultFolder = settings.Value.DefaultFolder ?? "ecommerce";
        //  debug log
        Console.WriteLine($"Cloudinary INIT: Cloud={_cloudinary.Api.Account.Cloud}, ApiKey={_cloudinary.Api.Account.ApiKey}, ApiSecret={_cloudinary.Api.Account.ApiSecret}");
    }

    public async Task<ImageUploadResult> UploadFromUrlAsync(string url, string folder = null, string publicId = null, bool convertToPng = false)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(url),
            Folder = $"{_defaultFolder}/{(string.IsNullOrWhiteSpace(folder) ? "products" : folder)}",
            PublicId = publicId,
            Overwrite = true,
            Format = convertToPng ? "png" : null,
            Transformation = new Transformation().Quality("auto").FetchFormat("auto")
        };

        // debug log 
        Console.WriteLine($"UploadFromUrl Params: Url={url}, Folder={uploadParams.Folder}, PublicId={uploadParams.PublicId}");
        var result = await _cloudinary.UploadAsync(uploadParams);
        if (result.Error != null) throw new ApplicationException(result.Error.Message);
        return result;
    }

    public async Task<ImageUploadResult> UploadFromFileAsync(IFormFile file, string folder = null, string publicId = null, bool convertToPng = false)
    {
        if (file == null || file.Length == 0) throw new ArgumentException("File is empty.");
        if (!file.ContentType.StartsWith("image/")) throw new ArgumentException("File is not an image.");

        await using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = $"{_defaultFolder}/{(string.IsNullOrWhiteSpace(folder) ? "products" : folder)}",
            PublicId = publicId,
            Overwrite = true,
            Format = convertToPng ? "png" : null,
            Transformation = new Transformation().Quality("auto").FetchFormat("auto")
        };
        // debug log 
        Console.WriteLine($"UploadFromFile Params: FileName={file.FileName}, Folder={uploadParams.Folder}, PublicId={uploadParams.PublicId}, CloudName={_cloudinary.Api.Account.Cloud}");
        Console.WriteLine($"Uploading with CloudName: {_cloudinary.Api.Account.Cloud}, ApiKey: {_cloudinary.Api.Account.ApiKey}");

        var result = await _cloudinary.UploadAsync(uploadParams);
        if (result.Error != null) throw new ApplicationException(result.Error.Message);
        return result;
    }

    public async Task<DeletionResult> DeleteAsync(string publicId)
    {
        if (string.IsNullOrWhiteSpace(publicId)) throw new ArgumentException("publicId required");
        var result = await _cloudinary.DestroyAsync(new DeletionParams(publicId) { ResourceType = ResourceType.Image });
        if (result.Error != null) throw new ApplicationException(result.Error.Message);
        return result;
    }
}