using FileUploadService.DTOs;
using FileUploadService.Models;

namespace FileUploadService.Services
{
    public interface IFileService {

        /// <summary>
        /// Get user files
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<FileDto>> GetUserFilesAsync(long userId);

        /// <summary>
        /// Read file stream as bytes array
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<byte[]> ReadFileAsync(long fileId, long userId);

        /// <summary>
        /// Upload file for user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="fileName"></param>
        /// <param name="file"></param>

        Task<FileDto> UploadFileAsync(long userId, IFormFile file);
    }
}
