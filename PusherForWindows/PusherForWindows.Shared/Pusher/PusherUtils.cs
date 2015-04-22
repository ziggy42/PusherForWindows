using Newtonsoft.Json;
using PusherForWindows.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PusherForWindows.Pusher
{
    class PusherUtils
    {
        public static readonly string CLIENT_ID = "IfgaX7cNfg0bdIcXgoLmROL6xFlT9dgq";
        public static readonly string CLIENT_SECRET = "w6QmD8gtBtz9gcT6RcjJ9JtIwgP5KVRx";
        public static readonly string REDIRECT_URI = "http://andreapivetta.altervista.org";
        public static readonly string LOGIN_KEY = "isuserloggedin";
        public static readonly string ACCESS_TOKEN_KEY = "token";
        public static readonly string USER_NAME_KEY = "name";
        public static readonly string USER_PIC_URL_KEY = "picurl";

        public static bool IsUserLoggedIn()
        {
            if (Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey(LOGIN_KEY))
                return (bool)Windows.Storage.ApplicationData.Current.LocalSettings.Values[LOGIN_KEY];

            return false;
        }

        public static string GetPushbulletLoginURL()
        {
            return "https://www.pushbullet.com/authorize?client_id=" + PusherUtils.CLIENT_ID +
                "&redirect_uri=" + Uri.EscapeUriString(PusherUtils.REDIRECT_URI) + "&response_type=token";
        }

        public static void StoreAccessToken(string redirect)
        {
            Windows.Storage.ApplicationData.Current.LocalSettings.Values[ACCESS_TOKEN_KEY] =
                redirect.Substring(redirect.IndexOf('=') + 1);
            Windows.Storage.ApplicationData.Current.LocalSettings.Values[LOGIN_KEY] = true;
        }

        public async static Task<bool> PushNoteAsync(string message, string title = "", string device = "")
        {
            var values = new Dictionary<string, string>
            {
               { "type", "note" },
               { "title", title },
               { "body", message },
               { "device_iden", device}
            };

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                (string)Windows.Storage.ApplicationData.Current.LocalSettings.Values[ACCESS_TOKEN_KEY]);
            var response = await client.PostAsync(
                "https://api.pushbullet.com/v2/pushes", new FormUrlEncodedContent(values));

            return response.IsSuccessStatusCode;
        }

        public async static Task<Dictionary<string, string>> GetUserInfoAsync()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                (string)Windows.Storage.ApplicationData.Current.LocalSettings.Values[ACCESS_TOKEN_KEY]);
            var response = await client.GetAsync("https://api.pushbullet.com/v2/users/me");
            var responseString = await response.Content.ReadAsStringAsync();

            dynamic json = JsonConvert.DeserializeObject(responseString);
            Dictionary<string, string> mDict = new Dictionary<string, string>();
            mDict.Add(USER_NAME_KEY, (string)json.name);
            mDict.Add(USER_PIC_URL_KEY, (string)json.image_url);

            Windows.Storage.ApplicationData.Current.LocalSettings.Values[USER_NAME_KEY] = (string)json.name;
            Windows.Storage.ApplicationData.Current.LocalSettings.Values[USER_PIC_URL_KEY] = (string)json.image_url;

            return mDict;
        }

        public async static Task<List<string[]>> GetDeviceListAsync()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                (string)Windows.Storage.ApplicationData.Current.LocalSettings.Values[ACCESS_TOKEN_KEY]);
            var response = await client.GetAsync("https://api.pushbullet.com/v2/devices");
            var responseString = await response.Content.ReadAsStringAsync();
            dynamic json = JsonConvert.DeserializeObject(responseString);

            List<string[]> devicesList = new List<string[]>();
            foreach (dynamic device in json["devices"])
                devicesList.Add(new string[] { (string)device.iden, (string)device.nickname });

            return devicesList;
        }

        public async static void GetNotesListAsync()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                (string)Windows.Storage.ApplicationData.Current.LocalSettings.Values[ACCESS_TOKEN_KEY]);
            var response = await client.GetAsync("https://api.pushbullet.com/v2/pushes");
            var responseString = await response.Content.ReadAsStringAsync();
            dynamic json = JsonConvert.DeserializeObject(responseString);

            List<Push> pushes = new List<Push>();
            foreach (dynamic push in json["pushes"])
            {
                if ((bool)push.active)
                {
                    switch ((string)push.type)
                    {
                        case "note":
                            pushes.Add(new PushNote(
                                (string)push.iden, Push.TYPES.NOTE, (string)push.title, (long)push.created, (long)push.modified,
                                (string)push.body));
                            break;
                        case "link":
                            pushes.Add(new PushLink(
                                (string)push.iden, Push.TYPES.LINK, (string)push.title, (long)push.created, (long)push.modified,
                                (string)push.url));
                            break;
                        case "file":
                            pushes.Add(new PushFile(
                                 (string)push.iden, Push.TYPES.LINK, (string)push.title, (long)push.created, (long)push.modified,
                                 (string)push.file_name, (string)push.file_type, (string)push.file_url));

                            break;
                    }
                }
            }

        }
    }
}
