using System;
using System.Collections.Generic;
using System.Text;

namespace PusherForWindows.Model
{
    class PushLink : Push
    {
        public Uri URL { get; set; }

        public PushLink(string Iden, TYPES Type, string Title, long Created, long Modified, string url)
            : base(Iden, Type, Title, Created, Modified)
        {
            this.URL = new Uri(url);
        }
    }
}
