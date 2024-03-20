
namespace FileUploadService.Services {
    public class StorageService : IStorageService {

        private readonly string storagePath;

        public StorageService() {
            storagePath = getDefaultStoragePath();
        }

        public async Task<byte[]> ReadBytesAsync(string filePath) {

            if (string.IsNullOrWhiteSpace(filePath)) {
                throw new ArgumentException("Invalid parameters");
            }

            return await File.ReadAllBytesAsync(filePath);
        }

        public async Task<string> WriteFileAsync(IFormFile file) {
            if (file == null) {
                throw new ArgumentException("Invalid parameters");
            }

            string filePath = Path.Combine(storagePath, file.FileName + "_" + Guid.NewGuid().ToString());

            using (FileStream output = File.Create(filePath)) {
                await file.CopyToAsync(output);
            }

            return filePath;
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
