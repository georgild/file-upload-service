namespace FileUploadService.Models {
    public class FileEntity {

        public long Id { get; set; }

        public required string FileName { get; set; }

        public required string ContentType { get; set; }

        public required string FilePath { get; set; }

        public required long UserId { get; set; }
    }
}
