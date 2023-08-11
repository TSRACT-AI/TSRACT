using Azure.Storage.Blobs;

namespace TSRACT.Services
{
    public class CloudStorageService
    {
        public string AzureBlobSasUrl { get; set; } = "";

        public async Task<bool> UploadFileToBlob(string filePath, string blobName)
        {
            Console.WriteLine($"Beginning upload of {filePath} to {blobName}.");
            BlobContainerClient containerClient = new BlobContainerClient(new Uri(AzureBlobSasUrl));
            BlobClient blobClient = containerClient.GetBlobClient(blobName);
            blobClient.UploadAsync(filePath, true).GetAwaiter().GetResult();
            Console.WriteLine($"Upload of {filePath} to {blobName} completed!");
            return true;
        }
    }
}
