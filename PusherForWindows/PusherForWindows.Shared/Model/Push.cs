using System;
namespace PusherForWindows.Model
{
    public abstract class Push
    {
        public string Iden { get; set; }

        public string Title { get; set; }

        public string Created
        {
            get
            {
                return GetTimeSpanString(this.created);
            }
        }

        public string Modified { 
            get
            {
                return GetTimeSpanString(this.modified);
            }
        }

        private DateTime created;
        private DateTime modified;

        public Push(string iden, string title, long created, long modified)
        {
            this.Iden = iden;
            this.Title = title;

            this.created = (new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc))
                .AddSeconds(created).ToLocalTime();

            this.modified = (new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc))
                .AddSeconds(modified).ToLocalTime();
        }

        private string GetTimeSpanString(DateTime date)
        {
            TimeSpan span = DateTime.Now.Subtract(this.created);

            if(span.Days > 365)
                return date.ToString("more than a year ago");
            if (span.Days > 30)
                return date.ToString("dd MMM");
            else if (span.Days > 0)
                return span.Days + " days ago";
            else if (span.Hours > 0)
                return span.Hours + "h";
            else if (span.Minutes > 0)
                return span.Minutes + "mins";
            else if (span.Seconds > 20)
                return span.Seconds + "s";
            else return "now";
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
