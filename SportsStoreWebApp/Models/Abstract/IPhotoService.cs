using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace SportsStoreWebApp.Models.Abstract
{
  public interface IPhotoService
  {
    Task<string> UploadPhotoAsync(string category, IFormFile photoToUpload);
    Task<bool> DeletePhotoAsync(string category, string photoUrl);
  }
}
