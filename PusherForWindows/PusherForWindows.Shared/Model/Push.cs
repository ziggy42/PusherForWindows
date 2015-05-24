using System;

namespace PusherForWindows.Model
{
    public class Push : EventArgs
    {
        public string Iden { get; set; }

        public string Title { get; set; }

        public bool IsActive { get; set; }

        public long Created { get; set; }

        public long Modified { get; set; }

        public string CreatedString { get; set; }

        public string ModifiedString { get; set; }

        public Push(string iden, string title, long created, long modified)
        {
            this.Iden = iden;
            this.Title = title;
            this.IsActive = true;
            this.Created = created;
            this.Modified = modified;

            this.CreatedString = GetTimeSpanString((new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc))
                .AddSeconds(created).ToLocalTime());

            this.ModifiedString = GetTimeSpanString((new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc))
                .AddSeconds(modified).ToLocalTime());
        }

        public Push(string iden, long created, long modified, bool isActive)
        {
            this.Iden = iden;
            this.IsActive = isActive;
            this.Created = created;
            this.Modified = modified;

            this.CreatedString = GetTimeSpanString((new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc))
                .AddSeconds(created).ToLocalTime());

            this.ModifiedString = GetTimeSpanString((new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc))
                .AddSeconds(modified).ToLocalTime());
        }

        private string GetTimeSpanString(DateTime date)
        {
            TimeSpan span = DateTime.Now.Subtract(date);

            if (span.Days > 365)
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
