using System;

namespace PusherForWindows.Model
{
    class Push
    {
        public string Iden { get; set; }
        public TYPES Type { get; set; }
        public string Title { get; set; }
        public long Created { get; set; }
        public long Modified { get; set; }

        public enum TYPES
        {
            LINK, FILE, NOTE, IMAGE
        }

        public Push(string Iden, TYPES Type, string Title, long Created, long Modified)
        {
            this.Iden = Iden;
            this.Title = Title;
            this.Created = Created;
            this.Modified = Modified;
            this.Type = Type;
        }
    }
}
