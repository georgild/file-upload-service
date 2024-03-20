
using Microsoft.EntityFrameworkCore;
using FileUploadService.DTOs;
using FileUploadService.Exceptions;
using FileUploadService.Models;
using FileUploadService.Repositories;

namespace FileUploadService.Services {

    public class FileService : IFileService {

        private readonly IFileRepository fileRepository;

        private readonly IStorageService storageService;

        private readonly int maxFileSizeBytes;

        private readonly string[] allowedFileTypes;

        public FileService(IFileRepository fileRepository, IConfiguration config, IStorageService storageService) {
            this.fileRepository = fileRepository;
            this.storageService = storageService;

            maxFileSizeBytes = config.GetValue("MaxFileSize", 10000);

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
        public FileService(IFileRepository fileRepository, IStorageService storageService, int maxFileSizeBytes, string[] allowedFileTypes) {
            this.fileRepository = fileRepository;
            this.storageService= storageService;
            this.maxFileSizeBytes = maxFileSizeBytes;
            this.allowedFileTypes = allowedFileTypes;
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

            return await storageService.ReadBytesAsync(fileEntity.FilePath);
        }

        public async Task<FileDto> UploadFileAsync(long userId, IFormFile file) {

            if (userId <= 0 || file == null) {
                throw new ArgumentException("Invalid parameters");
            }

            validateFile(file);

            string filePath = await storageService.WriteFileAsync(file);

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
    }
}
