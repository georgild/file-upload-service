using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Reflection.Metadata;
using FileUploadService.Models;

namespace FileUploadService.Configuration {
    public class AppDbContext : DbContext {
        public DbSet<FileEntity> Files { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) {
        }
    }
}
