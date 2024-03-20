using Microsoft.EntityFrameworkCore;
using FileUploadService.Configuration;
using FileUploadService.Models;

namespace FileUploadService.Repositories {
    public class FileRepository : IFileRepository, IDisposable {

        private AppDbContext context;

        public FileRepository(AppDbContext context) {
            this.context = context;
        }

        public IQueryable<FileEntity> GetFiles(long userId) {
            return context.Files.Where(f => f.UserId.Equals(userId));
        }

        public Task<FileEntity?> GetByIdAsync(long fileId, long userId) {
            return context.Files.Where(f => f.Id.Equals(fileId) && f.UserId.Equals(userId)).FirstOrDefaultAsync();
        }

        public async Task InsertAsync(FileEntity file) {
            await context.Files.AddAsync(file);
        }

        public async Task SaveAsync() {
            await context.SaveChangesAsync();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                if (disposing) {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
