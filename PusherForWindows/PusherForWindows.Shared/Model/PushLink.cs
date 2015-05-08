using System;

namespace PusherForWindows.Model
{
    public class PushLink : Push
    {
        public Uri URL { get; set; }

        public PushLink(string iden, string title, long created, long modified, string url)
            : base(iden, title, created, modified)
        {
            this.URL = new Uri(url);
        }
    }
}
