﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using SportsStoreWebApp.Models.Abstract;

namespace SportsStoreWebApp.Models.Services
{
  public class PhotoService : IPhotoService
  {
    private CloudStorageAccount _storageAccount;
    private readonly ILogger<PhotoService> _logger;

    public PhotoService(IOptions<StorageUtility> storageUtility, ILogger<PhotoService> logger)
    {
      _storageAccount = storageUtility.Value.StorageAccount;
      _logger = logger;
    }

    public async Task<string> UploadPhotoAsync(string category, IFormFile photoToUpload)
    {
            if(photoToUpload == null || photoToUpload.Length == 0) return null;

            string categoryLowerCase = category.ToLower().Trim();
            string fullPath = null;

            try
            {
                CloudBlobClient blobClient = _storageAccount.CreateCloudBlobClient();
                CloudBlobContainer blobContainer = blobClient.GetContainerReference(categoryLowerCase);

                if(await blobContainer.CreateIfNotExistsAsync())
                {
                    await blobContainer.SetPermissionsAsync(new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob
                    });

                    _logger.LogInformation($"Successfully created Blob Storage COntainer '{blobContainer.Name}'" +
                        $"and made it public");
                }

                string imageName = $"productPhoto{Guid.NewGuid().ToString()}{Path.GetExtension(photoToUpload.FileName.Substring(photoToUpload.FileName.LastIndexOf("/")+1))}";

                CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(imageName);
                blockBlob.Properties.ContentType = photoToUpload.ContentType;
                await blockBlob.UploadFromStreamAsync(photoToUpload.OpenReadStream());

                fullPath = blockBlob.Uri.ToString();

                _logger.LogInformation($"Blob Service. PhotoService.UploadPhoto. imagePath = '{fullPath}'");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading the photo blob to storage");
                throw;
            }


            return fullPath;

    }

    public async Task<bool> DeletePhotoAsync(string category, string photoUrl)
    {
            if (string.IsNullOrEmpty(photoUrl)) return true;

            string categoryLowerCase = category.ToLower().Trim();
            bool deletedFlag = false;

            try
            {
                CloudBlobClient blobClient = _storageAccount.CreateCloudBlobClient();
                CloudBlobContainer blobContainer = blobClient.GetContainerReference(categoryLowerCase);

                if (blobContainer.Name == categoryLowerCase)
                {
                    string blobName = photoUrl.Substring(photoUrl.LastIndexOf("/")+1);
                    CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(blobName);
                    deletedFlag = await blockBlob.DeleteIfExistsAsync();

                    _logger.LogInformation($"Blob Service. PhotoService.DeletePhoto. deletedImagePath = '{photoUrl}'");
                }

                //assignment

                return deletedFlag;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in deleting the photo blob from storage");
                throw;
            }
       
    }
  }
}
