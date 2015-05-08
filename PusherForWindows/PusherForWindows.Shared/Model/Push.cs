namespace PusherForWindows.Model
{
    public abstract class Push
    {
        public string Iden { get; set; }

        public string Title { get; set; }

        public long Created { get; set; }
        
        public long Modified { get; set; }

        public Push(string iden, string title, long created, long modified)
        {
            this.Iden = iden;
            this.Title = title;
            this.Created = created;
            this.Modified = modified;
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
