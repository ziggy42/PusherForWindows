namespace PusherForWindows.Model
{
    public class PushNote : Push
    {
        public string Body { get; set; }

        public PushNote(string iden, string title, long created, long modified, string body)
            : base(iden, title, created, modified)
        {
            this.Body = body;
        }
    }
}
