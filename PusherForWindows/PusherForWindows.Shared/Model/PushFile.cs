using System;

namespace PusherForWindows.Model
{
    class PushFile : Push
    {
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public Uri FileUrl { get; set; }

        public PushFile(string Iden, TYPES Type, string Title, long Created, long Modified,
            string FileName, string MimeType, string FileUrl)
            : base(Iden, Type, Title, Created, Modified)
        {
            this.FileName = FileName;
            this.MimeType = MimeType;
            this.FileUrl = new Uri(FileUrl);
        }
    }
}
