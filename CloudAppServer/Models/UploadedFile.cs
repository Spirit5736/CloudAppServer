namespace CloudAppServer.Models
{
    public class UploadedFile
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public DateTimeOffset DateOfUpload { get; set; }
    }
}
