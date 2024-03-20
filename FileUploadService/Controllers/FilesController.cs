using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Security.Claims;
using FileUploadService.Exceptions;
using FileUploadService.Models;
using FileUploadService.Repositories;
using FileUploadService.Services;

namespace FileUploadService.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class FilesController : ControllerBase
    {
        private readonly IFileService fileService;

        private readonly ILogger<FilesController> logger;

        public FilesController(ILogger<FilesController> logger, IFileService fileService) {
            this.logger = logger;
            this.fileService = fileService;
        }

        [HttpGet]
        public async Task<IActionResult> Get() {

            try {
                long userId = getUserId(HttpContext.User);
                return Ok(await fileService.GetUserFilesAsync(userId));
            } catch (AuthenticationException) {
                return Unauthorized();
            } catch (Exception exc) {
                logger.LogError("Generic exception occurred in GET user files endpoint: {}", exc);
                return BadRequest("Failed to get user files");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id) {

            try {
                long userId = getUserId(HttpContext.User);
                return Ok(await fileService.ReadFileAsync(id, userId));
            } catch (AuthenticationException) {
                return Unauthorized();
            } catch (FileNotFoundException) {
                return NotFound("Requested file not found");
            } catch (Exception exc) {
                logger.LogError("Generic exception occurred in GET user file by ID endpoint: {}", exc);
                return BadRequest("Failed to get file");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file) {

            FileEntity uploadedFile;
            try {
                long userId = getUserId(HttpContext.User);
                uploadedFile = await fileService.UploadFileAsync(userId, file);
            } catch (AuthenticationException) {
                return Unauthorized("Invalid user");
            } catch (InvalidFileSizeException) {
                return BadRequest("File size is out of range");
            } catch (InvalidFileTypeException) {
                return BadRequest("File type is invalid");
            } catch (Exception exc) {
                logger.LogError("Generic exception occurred in Upload file endpoint: {}", exc);
                return BadRequest("Failed to upload file");
            }

            return CreatedAtAction(nameof(Get), new { id = uploadedFile.Id });
        }

        private long getUserId(ClaimsPrincipal currentUser) {
            if (currentUser != null && currentUser.HasClaim(c => c.Type.Equals("uid"))) {
                Claim? userIdClaim = currentUser.Claims.FirstOrDefault(c => c.Type.Equals("uid"));
                if (userIdClaim != null) {
                    return long.Parse(userIdClaim.Value);
                } else {
                    throw new AuthenticationException("Invalid JWT");
                }
            } else {
                throw new AuthenticationException("Invalid JWT");
            }
               
        }
    }
}
