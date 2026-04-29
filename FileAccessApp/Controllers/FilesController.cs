using FileAccessApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileAccessApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController: ControllerBase
    {
        FileService _fileService;
        public FilesController(FileService fileService)
        {
            _fileService = fileService;
        }

        [HttpGet("{*path}")]
        public IActionResult Get(string path, 
            [FromQuery] int? maxWidth = null,
            [FromQuery] int? maxHeight = null
            )
        {
            try
            {
                var file = _fileService.GetFile(path, maxWidth, maxHeight);

                return File(file.Content, file.ContentType);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("delete")]
        public IActionResult Delete([FromBody] string path)
        {
            try
            {
                _fileService.DeleteEntry(path);

                return Ok($"File successfully deleted at {path}");
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("create-dir")]
        public IActionResult CreateDir([FromBody] string path)
        {
            try
            {
                string dirPath = Path.GetDirectoryName(path);
                string dirName = Path.GetFileName(path);


                _fileService.CreateDir(dirPath, dirName);

                return Ok($"Directory successfully created at {dirPath}");
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
