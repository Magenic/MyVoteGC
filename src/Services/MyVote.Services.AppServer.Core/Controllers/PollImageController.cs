using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Apis.Services;
using Google.Apis.Storage.v1;
using Microsoft.AspNetCore.Http;

namespace MyVote.Services.AppServer.Controllers
{
    [Route("api/[controller]")]
    public sealed class PollImageController
        : Controller
    {
        const string bucketName = "myvote";

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Post(IFormFile file)
        {
            string filename = file.FileName;
            string content = file.ContentType;

            BinaryReader reader = new BinaryReader(file.OpenReadStream());
            byte[] imageBytes = reader.ReadBytes((int)file.Length);
            var uploadStream = new MemoryStream(imageBytes);

            var credentials = await Google.Apis.Auth.OAuth2.GoogleCredential.GetApplicationDefaultAsync();

            if (credentials.IsCreateScopedRequired)
            {
                credentials = credentials.CreateScoped(new[] { StorageService.Scope.DevstorageFullControl });
            }

            var serviceInitializer = new BaseClientService.Initializer()
            {
                ApplicationName = "MyVote",
                HttpClientInitializer = credentials
            };

            StorageService storage = new StorageService(serviceInitializer);

            storage.Objects.Insert(
                bucket: bucketName,
                stream: uploadStream,
                contentType: content,
                body: new Google.Apis.Storage.v1.Data.Object() { Name = filename }
            ).Upload();

            return new OkObjectResult(new { imageUrl = "https://storage.googleapis.com/" + bucketName + "/" + filename});
        }

    }
}