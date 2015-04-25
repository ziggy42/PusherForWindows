using System;

namespace PusherForWindows.Model
{
    class PushFile : Push
    {
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public Uri URL { get; set; }

        public PushFile(string Iden, string Title, long Created, long Modified,
            string FileName, string MimeType, string FileUrl)
            : base(Iden, Title, Created, Modified)
        {
            this.FileName = FileName;
            this.MimeType = MimeType;
            this.URL = new Uri(FileUrl);
        }
    }
}
