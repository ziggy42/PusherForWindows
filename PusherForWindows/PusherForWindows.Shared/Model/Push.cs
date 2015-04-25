
namespace PusherForWindows.Model
{
    class Push
    {
        public string Iden { get; set; }
        public string Title { get; set; }
        public long Created { get; set; }
        public long Modified { get; set; }

        public Push(string Iden, string Title, long Created, long Modified)
        {
            this.Iden = Iden;
            this.Title = Title;
            this.Created = Created;
            this.Modified = Modified;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Push))
                return false;
            return this.Iden.Equals(((Push)obj).Iden);
        }

        public override int GetHashCode()
        {
            return this.Iden.GetHashCode();
        }
    }
}
