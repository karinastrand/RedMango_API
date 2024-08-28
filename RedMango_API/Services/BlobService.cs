

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Reflection.Metadata.Ecma335;

namespace RedMango_API.Service;

public class BlobService : IBlobService
{
    private readonly BlobServiceClient _blobClient;
    public BlobService(BlobServiceClient blobClient)
    {
        _blobClient = blobClient;
    }
    public async Task<bool> DeleteBlob(string blobName, string containterName)
    {
        BlobContainerClient blobContainerClient = _blobClient.GetBlobContainerClient(containterName);
        BlobClient blobClient=blobContainerClient.GetBlobClient(blobName);
        return await blobClient.DeleteIfExistsAsync();
    }

    public async Task<string> GetBlob(string blobName, string containterName)
    {
        BlobContainerClient blobContainerClient = _blobClient.GetBlobContainerClient(containterName);
        BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);
        return blobClient.Uri.AbsoluteUri;
    }

    public async Task<string> UploadBlob(string blobName, string containterName, IFormFile file)
    {
        BlobContainerClient blobContainerClient = _blobClient.GetBlobContainerClient(containterName);
        BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);
        var httpHeaders = new BlobHttpHeaders()
        {
            ContentType = file.ContentType
        };
        var result = await blobClient.UploadAsync(file.OpenReadStream(),httpHeaders);
        if (result != null) 
        {
            return await GetBlob(blobName, containterName);
        }
        return "";
    }
}
