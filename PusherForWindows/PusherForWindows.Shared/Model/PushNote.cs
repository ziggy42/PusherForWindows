using System;
using System.Collections.Generic;
using System.Text;

namespace PusherForWindows.Model
{
    class PushNote : Push
    {
        public string Body { get; set; }

        public PushNote(string Iden, TYPES Type, string Title, long Created, long Modified, string Body)
            : base(Iden, Type, Title, Created, Modified)
        {
            this.Body = Body;
        }
    }
}
