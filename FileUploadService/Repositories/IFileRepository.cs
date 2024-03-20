using FileUploadService.Models;

namespace FileUploadService.Repositories {
    public interface IFileRepository : IDisposable {

        IQueryable<FileEntity> GetFiles(long userId);

        Task<FileEntity?> GetByIdAsync(long fileId, long userId);

        Task InsertAsync(FileEntity file);

        Task SaveAsync();
    }
}
