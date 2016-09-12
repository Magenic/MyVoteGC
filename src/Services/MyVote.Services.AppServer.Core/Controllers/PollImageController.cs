using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using Google.Storage.V1;
using Microsoft.AspNetCore.Http;

namespace MyVote.Services.AppServer.Controllers
{
    [Route("api/[controller]")]
    public sealed class PollImageController
        : Controller
    {
        const string bucketName = "";

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Post(IFormFile file)
        {
            string filename = file.FileName;
            string contentType = file.ContentType;

            BinaryReader reader = new BinaryReader(file.OpenReadStream());
            byte[] imageBytes = reader.ReadBytes((int)file.Length);
            var uploadStream = new MemoryStream(imageBytes);

            var client = StorageClient.Create();
            var uploaded = client.UploadObjectAsync(bucketName, filename, contentType, uploadStream);

            return new OkObjectResult(new { imageUrl = "https://storage.googleapis.com/" + bucketName + "/" + filename });
        }

    }
}