namespace FileUploadService.Services {
    public interface IStorageService {
        /// <summary>
        /// Writes file to storage and returns full file path on the storage that will be used to read
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        Task<string> WriteFileAsync(IFormFile file);

        /// <summary>
        /// Read file from storage
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        Task<byte[]> ReadBytesAsync(string filePath);
    }
}
