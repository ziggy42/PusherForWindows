using System;

namespace PusherForWindows.Model
{
    class PushLink : Push
    {
        public Uri URL { get; set; }

        public PushLink(string Iden, string Title, long Created, long Modified, string url)
            : base(Iden, Title, Created, Modified)
        {
            this.URL = new Uri(url);
        }
    }
}
