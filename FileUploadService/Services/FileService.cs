
using Microsoft.EntityFrameworkCore;
using System.Security.Authentication;
using FileUploadService.Controllers;
using FileUploadService.DTOs;
using FileUploadService.Exceptions;
using FileUploadService.Models;
using FileUploadService.Repositories;

namespace FileUploadService.Services {

    public class FileService : IFileService {

        private readonly IFileRepository fileRepository;

        private readonly int maxFileSizeBytes;

        private readonly string storagePath;

        private readonly string[] allowedFileTypes;

        public FileService(IFileRepository fileRepository, IConfiguration config) {
            this.fileRepository = fileRepository;

            maxFileSizeBytes = config.GetValue("MaxFileSize", 10000);

            storagePath = getDefaultStoragePath();

            string? allowedFileTypesConfig;
            allowedFileTypesConfig = config.GetValue("AllowedFileTypes", "pdf,images");

            if (!string.IsNullOrWhiteSpace(allowedFileTypesConfig)) {
                allowedFileTypes = allowedFileTypesConfig.Split(",");
            } else {
                throw new ApplicationException("AllowedFileTypes in config is null or empty");
            }
            
            if (allowedFileTypes.Length <= 0) {
                throw new ApplicationException("Invalid value provided for AllowedFileTypes in config. Please provide valid comma-separated values");
            }
        }

        // Construct passing values instead of config
        public FileService(IFileRepository fileRepository, int maxFileSizeBytes, string[] allowedFileTypes, string storagePath) {
            this.fileRepository = fileRepository;
            this.maxFileSizeBytes = maxFileSizeBytes;
            this.allowedFileTypes = allowedFileTypes;
            this.storagePath = storagePath;
        }

        public async Task<List<FileDto>> GetUserFilesAsync(long userId) {
            if (userId <= 0) {
                throw new ArgumentException("Invalid parameters");
            }

            return await fileRepository.GetFiles(userId).Select(f => new FileDto {
                Id = f.Id,
                FileName = f.FileName,
                ContentType = f.ContentType
            }).ToListAsync();
        }

        public async Task<byte[]> ReadFileAsync(long fileId, long userId) {
            if (userId <= 0 || userId <= 0) {
                throw new ArgumentException("Invalid parameters");
            }

            FileEntity? fileEntity = await fileRepository.GetByIdAsync(fileId, userId);

            if (fileEntity == null) {
                throw new FileNotFoundException();
            }

            return await File.ReadAllBytesAsync(fileEntity.FilePath);
        }

        public async Task<FileDto> UploadFileAsync(long userId, IFormFile file) {

            if (userId <= 0 || file == null) {
                throw new ArgumentException("Invalid parameters");
            }

            validateFile(file);
 
            string filePath = Path.Combine(storagePath, file.FileName + "_" + Guid.NewGuid().ToString());

            using (FileStream output = File.Create(filePath)) {
                await file.CopyToAsync(output);
            }

            FileEntity fileToSave = new FileEntity {
                FilePath = filePath,
                UserId = userId,
                FileName = file.FileName,
                ContentType = file.ContentType
            };

            await fileRepository.InsertAsync(fileToSave);
            await fileRepository.SaveAsync();

            return new FileDto {
                Id = fileToSave.Id,
                FileName = fileToSave.FileName,
                ContentType = fileToSave.ContentType
            };
        }

        private void validateFile(IFormFile file) {
            if (file == null) {
                throw new ArgumentException("File is invalid");
            }

            if (string.IsNullOrWhiteSpace(file.FileName)) {
                throw new ArgumentException("File name must not be blank");
            }

            if (file == null || file.Length <= 0 || file.Length > maxFileSizeBytes) {
                throw new InvalidFileSizeException("File size is invalid");
            }

            string contentType = file.ContentType;
            if (!allowedFileTypes.Any(ft => contentType.Contains(ft))) {
                throw new InvalidFileTypeException("File type is invalid");
            }
        }

        private string getDefaultStoragePath() {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            string path = Path.Join(Environment.GetFolderPath(folder), "Storage");
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
            return path;
        }
    }
}
