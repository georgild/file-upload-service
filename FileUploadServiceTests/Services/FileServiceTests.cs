using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using FileUploadService.Exceptions;
using FileUploadService.Models;
using FileUploadService.Repositories;
using FileUploadService.Services;

namespace FileUploadServiceTests.Services
{
    public class FileServiceTests {

        private readonly Mock<IFileRepository> mockFileRepo;

        private readonly FileService fileService;

        private readonly int maxFileSizeBytes = 1000;

        string[] allowedFileTypes = ["image", "pdf"];


        public FileServiceTests() {
            mockFileRepo = new Mock<IFileRepository>();
            fileService = new FileService(mockFileRepo.Object, maxFileSizeBytes, allowedFileTypes, "");

        }
        [Fact]
        public async void Upload_InvalidParams_ThrowsError() {

            Func<Task> result = () => fileService.UploadFileAsync(0, null);

            await Assert.ThrowsAsync<ArgumentException>(result);
        }

        [Fact]
        public async void Upload_SizeOutOfRange_ThrowsError() {
            Mock<IFormFile> formFileMock = new Mock<IFormFile>();
            formFileMock.Setup(n => n.FileName).Returns("test");
            formFileMock.Setup(n => n.Length).Returns(10000000);

            Func<Task> result = () => fileService.UploadFileAsync(1, formFileMock.Object);

            await Assert.ThrowsAsync<InvalidFileSizeException>(result);
        }

        [Fact]
        public async void Upload_InvalidFileType_ThrowsError() {
            Mock<IFormFile> formFileMock = new Mock<IFormFile>();
            formFileMock.Setup(n => n.FileName).Returns("test");
            formFileMock.Setup(n => n.Length).Returns(1000);
            formFileMock.Setup(n => n.ContentType).Returns("docx");

            Func<Task> result = () => fileService.UploadFileAsync(1, formFileMock.Object);

            await Assert.ThrowsAsync<InvalidFileTypeException>(result);
        }
    }
}