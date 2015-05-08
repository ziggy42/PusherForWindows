using System;

namespace PusherForWindows.Model
{
    public class PushFile : Push
    {
        public string FileName { get; set; }

        public string MimeType { get; set; }
        
        public Uri URL { get; set; }

        public PushFile(string iden, string title, long created, long modified,
            string fileName, string mimeType, string fileUrl)
            : base(iden, title, created, modified)
        {
            this.FileName = fileName;
            this.MimeType = mimeType;
            this.URL = new Uri(fileUrl);
        }
    }
}
