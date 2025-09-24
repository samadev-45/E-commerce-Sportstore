using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

public interface IImageService
{
    Task<ImageUploadResult> UploadFromUrlAsync(string url, string folder = null, string publicId = null, bool convertToPng = false);
    Task<ImageUploadResult> UploadFromFileAsync(IFormFile file, string folder = null, string publicId = null, bool convertToPng = false);
    Task<DeletionResult> DeleteAsync(string publicId);
}
