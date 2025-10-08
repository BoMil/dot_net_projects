using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace CityInfo.API.Controllers
{
    [ApiController]
    [Route("api/files")]
    public class FilesController : ControllerBase
    {

        private readonly FileExtensionContentTypeProvider _fileExtensionContentTypeProvider;
        public FilesController(FileExtensionContentTypeProvider fileExtensionContentTypeProvider)
        {
            _fileExtensionContentTypeProvider = fileExtensionContentTypeProvider ?? throw new ArgumentNullException(nameof(fileExtensionContentTypeProvider));
        }

        [HttpGet("{fileId}")]
        public IActionResult GetFile(string fileId)
        {
            var filePath = "dnevni-Detox.pdf";
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            if (!_fileExtensionContentTypeProvider.TryGetContentType(filePath, out var contentType))
            {
                contentType = "application/octet-stream";
            }
            var bytes = System.IO.File.ReadAllBytes(filePath);
            return File(bytes, contentType, Path.GetFileName(filePath));
        }

        [HttpPost]
        public async Task<ActionResult> UploadFile(IFormFile file)
        {
            // Validate the input. Put a limit on the file size to prevent abuse.
            // Accept only PDF files.
            if (file == null || file.Length > 20971520 || file.ContentType != "application/pdf")
            {
                return BadRequest("No file or invalid one has been uploaded.");
            }

            // Create a file path. Avoid using file.FileName because an attacker could provide a malicius one, including full paths or relative paths.
            var path = Path.Combine(
                Directory.GetCurrentDirectory(), // This will save the file in the current app directory.
                $"uploaded_file_{Guid.NewGuid()}.pdf"
            );

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok("Your file has been uploaded successfully.");
        }
        
    }
}