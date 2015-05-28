using PusherForWindows.Model;

namespace PusherForWindows.Persistance
{
    class PushDTO
    {
        public enum TYPE { FILE, NOTE, LINK }

        [SQLite.PrimaryKey, SQLite.NotNull]
        public string Iden { get; set; }

        public string Title { get; set; }

        [SQLite.NotNull]
        public long Created { get; set; }

        [SQLite.NotNull]
        public long Modified { get; set; }

        [SQLite.NotNull]
        public TYPE Type { get; set; }

        public string FileName { get; set; }

        public string MimeType { get; set; }

        public string Body { get; set; }

        public string Url { get; set; }

        public static PushDTO GetPushDTOFromPush(Push push)
        {
            PushDTO entry = new PushDTO()
            {
                Iden = push.Iden,
                Title = push.Title,
                Created = push.Created,
                Modified = push.Modified,
            };

            if (push is PushFile)
            {
                entry.Type = TYPE.FILE;
                entry.FileName = ((PushFile)push).FileName;
                entry.MimeType = ((PushFile)push).MimeType;
                entry.Url = ((PushFile)push).URL.ToString();
            }
            else if (push is PushNote)
            {
                entry.Type = TYPE.NOTE;
                entry.Body = ((PushNote)push).Body;
            }
            else if (push is PushLink)
            {
                entry.Type = TYPE.LINK;
                entry.Url = ((PushLink)push).URL.ToString();
            }

            return entry;
        }
    }
}
