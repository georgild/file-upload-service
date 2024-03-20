namespace FileUploadService.DTOs {
    public class FileDto {

        public long Id { get; set; }

        public required string FileName { get; set; }

        public required string ContentType { get; set; }
    }
}
