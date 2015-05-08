namespace PusherForWindows.Model
{
    public class Device
    {
        public string Iden { get; private set; }

        public string Nickname { get; private set; }

        public Device(string iden, string nickname)
        {
            this.Iden = iden;
            this.Nickname = nickname;
        }
    }
}
