﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using Google.Storage.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace MyVote.Services.AppServer.Controllers
{
    [Route("api/[controller]")]
    public sealed class PollImageController
        : Controller
    {
        private IOptions<BucketStorageName> iconfig { get; set; }

        public PollImageController(IOptions<BucketStorageName> configuration)
        {
            iconfig = configuration;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var imageFile = Request.Form.Files[0];

            var uploadStream = new MemoryStream(new BinaryReader(imageFile.OpenReadStream()).ReadBytes((int)imageFile.Length));
            await StorageClient.Create().UploadObjectAsync(iconfig.Value.Name, imageFile.FileName, imageFile.ContentType, uploadStream);

            return new OkObjectResult(new { imageUrl = "https://storage.googleapis.com/" + iconfig.Value.Name + "/" + imageFile.FileName });
        }

    }
}