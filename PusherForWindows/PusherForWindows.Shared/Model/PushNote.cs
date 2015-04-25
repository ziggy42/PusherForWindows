namespace PusherForWindows.Model
{
    class PushNote : Push
    {
        public string Body { get; set; }

        public PushNote(string Iden, string Title, long Created, long Modified, string Body)
            : base(Iden, Title, Created, Modified)
        {
            this.Body = Body;
        }
    }
}
