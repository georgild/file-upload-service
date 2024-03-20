using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using FileUploadService.Exceptions;
using FileUploadService.Models;
using FileUploadService.Repositories;
using FileUploadService.Services;
using System;
using FileUploadService.DTOs;

namespace FileUploadServiceTests.Services
{
    public class FileServiceTests {

        private readonly Mock<IFileRepository> mockFileRepo;

        private readonly Mock<IStorageService> mockStorageService;

        private readonly FileService fileService;

        private readonly int maxFileSizeBytes = 1000;

        string[] allowedFileTypes = ["image", "pdf"];


        public FileServiceTests() {
            mockFileRepo = new Mock<IFileRepository>();
            mockStorageService = new Mock<IStorageService>();
            fileService = new FileService(mockFileRepo.Object, mockStorageService.Object, maxFileSizeBytes, allowedFileTypes);

        }
        [Fact]
        public async void Upload_InvalidParams_ThrowsError() {

            Func<Task> result = () => fileService.UploadFileAsync(0, null);

            await Assert.ThrowsAsync<ArgumentException>(result);
        }

        [Fact]
        public async void Upload_SizeOutOfRange_ThrowsError() {
            string fileName = "test";
            int fileSize = 10000000;

            Mock<IFormFile> formFileMock = new Mock<IFormFile>();
            formFileMock.Setup(n => n.FileName).Returns(fileName);
            formFileMock.Setup(n => n.Length).Returns(fileSize);

            Func<Task> result = () => fileService.UploadFileAsync(1, formFileMock.Object);

            await Assert.ThrowsAsync<InvalidFileSizeException>(result);
        }

        [Fact]
        public async void Upload_InvalidFileType_ThrowsError() {
            string fileName = "test";
            int fileSize = 1000;
            string contentType = "document/docx";

            Mock<IFormFile> formFileMock = new Mock<IFormFile>();
            formFileMock.Setup(n => n.FileName).Returns(fileName);
            formFileMock.Setup(n => n.Length).Returns(fileSize);
            formFileMock.Setup(n => n.ContentType).Returns(contentType);

            Func<Task> result = () => fileService.UploadFileAsync(1, formFileMock.Object);

            await Assert.ThrowsAsync<InvalidFileTypeException>(result);
        }

        [Fact]
        public async void Upload_Success_ReturnsUploadedFile() {
            string fileName = "test";
            string contentType = "image/png";

            Mock<IFormFile> formFileMock = new Mock<IFormFile>();
            formFileMock.Setup(n => n.FileName).Returns(fileName);
            formFileMock.Setup(n => n.Length).Returns(1000);
            formFileMock.Setup(n => n.ContentType).Returns(contentType);

            mockFileRepo.Setup(s => s.InsertAsync(It.IsAny<FileEntity>())).Returns(Task.CompletedTask);
            mockFileRepo.Setup(s => s.SaveAsync()).Returns(Task.CompletedTask);

            mockStorageService.Setup(s => s.WriteFileAsync(formFileMock.Object)).Returns(Task.FromResult("storage_path"));

            FileDto result = await fileService.UploadFileAsync(1, formFileMock.Object);

            Assert.NotNull(result);
            Assert.Equal(result.FileName, fileName);
            Assert.Equal(result.ContentType, contentType);
        }
    }
}