using Newtonsoft.Json;
using PusherForWindows.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Windows.Storage;
using System.IO;

namespace PusherForWindows.Pusher
{
    static class PusherUtils
    {
        public static readonly string CLIENT_ID = "IfgaX7cNfg0bdIcXgoLmROL6xFlT9dgq";
        public static readonly string CLIENT_SECRET = "w6QmD8gtBtz9gcT6RcjJ9JtIwgP5KVRx";
        public static readonly string REDIRECT_URI = "http://andreapivetta.altervista.org";
        public static readonly string LOGIN_KEY = "isuserloggedin";
        public static readonly string ACCESS_TOKEN_KEY = "token";
        public static readonly string USER_NAME_KEY = "name";
        public static readonly string USER_PIC_URL_KEY = "picurl";

        private static HttpClient client;
        private static HttpClient Client
        {
            get
            {
                if (client == null)
                {
                    client = new HttpClient();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                        (string)Windows.Storage.ApplicationData.Current.LocalSettings.Values[ACCESS_TOKEN_KEY]);
                }

                return client;
            }
        }

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

        public async static Task<Push> PushNoteAsync(string message, string title = "", string device = "")
        {
            var values = new Dictionary<string, string>
            {
               { "type", "note" },
               { "title", title },
               { "body", message },
               { "device_iden", device}
            };

            var response = await Client.PostAsync(
                "https://api.pushbullet.com/v2/pushes", new FormUrlEncodedContent(values));

            if (response.IsSuccessStatusCode)
            {
                dynamic push = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
                switch ((string)push.type)
                {
                    case "note":
                        return new PushNote((string)push.iden, (string)push.title, (long)push.created, (long)push.modified,
                                (string)push.body);
                    case "file":
                        return new PushFile((string)push.iden, (string)push.title, (long)push.created, (long)push.modified,
                            (string)push.file_name, (string)push.file_type, (string)push.file_url);
                    case "link":
                        return new PushLink((string)push.iden, (string)push.title, (long)push.created, (long)push.modified,
                            (string)push.url);
                }

            }

            return null;
        }

        public async static Task<Push> PushFileAsync(StorageFile file, string body="", string title = "", string device = "")
        {
            var values = new Dictionary<string, string>
            {
               { "file_name", file.Name }, 
               { "file_type", file.ContentType }
            };

            var response = await Client.PostAsync(
                "https://api.pushbullet.com/v2/upload-request", new FormUrlEncodedContent(values));

            dynamic res = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
            var upload_url = (string)res.upload_url;
            var file_url = (string)res.file_url;
            dynamic data = res.data;

            HttpClient noAouthClient = new HttpClient();
            var multipartFormDataContent = new MultipartFormDataContent();
            multipartFormDataContent.Add(AddContent("acl", (string)data.acl));
            multipartFormDataContent.Add(AddContent("awsaccesskeyid", (string)data.awsaccesskeyid));
            multipartFormDataContent.Add(AddContent("content-type", file.ContentType));
            multipartFormDataContent.Add(AddContent("key", (string)data.key));
            multipartFormDataContent.Add(AddContent("policy", (string)data.policy));
            multipartFormDataContent.Add(AddContent("signature", (string)data.signature));

            var fileContent = new StreamContent((await file.OpenReadAsync()).AsStreamForRead());
            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "\"file\"",
                FileName = "\"" + file.Name + "\""
            };
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            multipartFormDataContent.Add(fileContent);

            response = await noAouthClient.PostAsync(upload_url, multipartFormDataContent);
            if (response.IsSuccessStatusCode)
            {
                values = new Dictionary<string, string>
                {
                    { "type", "file" }, 
                    { "file_name", file.Name}, 
                    { "file_type", file.ContentType }, 
                    { "file_url", file_url}, 
                    { "title", title},
                    { "body", body}
                };

                response = await Client.PostAsync(
                    "https://api.pushbullet.com/v2/pushes", new FormUrlEncodedContent(values));

                if (response.IsSuccessStatusCode)
                {
                    dynamic push = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
                    return new PushFile((string)push.iden, (string)push.title, (long)push.created, (long)push.modified,
                                (string)push.file_name, (string)push.file_type, (string)push.file_url);
                }
            }

            return null;
        }

        private static StringContent AddContent(string name, string content)
        {
            var fileContent = new StringContent(content);
            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "\"" + name + "\""
            };
            return fileContent;
        }

        public async static Task<Dictionary<string, string>> GetUserInfoAsync()
        {
            var response = await Client.GetAsync("https://api.pushbullet.com/v2/users/me");
            if(response.IsSuccessStatusCode)
            {
                dynamic json = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
                var values = new Dictionary<string, string>()
                {
                    { USER_NAME_KEY, (string)json.name},
                    {USER_PIC_URL_KEY, (string)json.image_url}
                };

                Windows.Storage.ApplicationData.Current.LocalSettings.Values[USER_NAME_KEY] = (string)json.name;
                Windows.Storage.ApplicationData.Current.LocalSettings.Values[USER_PIC_URL_KEY] = (string)json.image_url;
            }

            return null;
        }

        public async static Task<List<string[]>> GetDeviceListAsync()
        {
            var response = await Client.GetAsync("https://api.pushbullet.com/v2/devices");
            if (response.IsSuccessStatusCode)
            {
                dynamic json = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);

                List<string[]> devicesList = new List<string[]>();
                foreach (dynamic device in json["devices"])
                    devicesList.Add(new string[] { (string)device.iden, (string)device.nickname });

                return devicesList;
            }

            return null;
        }

        public async static Task<ObservableCollection<Push>> GetNotesListAsync()
        {
            var response = await Client.GetAsync("https://api.pushbullet.com/v2/pushes");
            if (response.IsSuccessStatusCode)
            {
                dynamic json = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);

                var pushes = new ObservableCollection<Push>();
                foreach (dynamic push in json["pushes"])
                {
                    if ((bool)push.active)
                    {
                        switch ((string)push.type)
                        {
                            case "note":
                                pushes.Add(new PushNote(
                                    (string)push.iden, (string)push.title, (long)push.created, (long)push.modified,
                                    (string)push.body));
                                break;
                            case "link":
                                pushes.Add(new PushLink(
                                    (string)push.iden, (string)push.title, (long)push.created, (long)push.modified,
                                    (string)push.url));
                                break;
                            case "file":
                                pushes.Add(new PushFile(
                                     (string)push.iden, (string)push.title, (long)push.created, (long)push.modified,
                                     (string)push.file_name, (string)push.file_type, (string)push.file_url));
                                break;
                        }
                    }
                }

                return pushes;
            }

            return null;
        }
    }
}
