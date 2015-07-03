using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Windows.Storage;
using Newtonsoft.Json;
using PusherForWindows.Model;
using Windows.UI.Xaml;
using Windows.Foundation.Collections;

namespace PusherForWindows.Pusher
{
    public static class Pushbullet
    {
        public static readonly string CLIENT_ID = "IfgaX7cNfg0bdIcXgoLmROL6xFlT9dgq";
        public static readonly string CLIENT_SECRET = "w6QmD8gtBtz9gcT6RcjJ9JtIwgP5KVRx";
        public static readonly string REDIRECT_URI = "http://andreapivetta.altervista.org";
        public static readonly string LOGIN_KEY = "userloggedin";
        public static readonly string ACCESS_TOKEN_KEY = "token";
        public static readonly string USER_NAME_KEY = "name";
        public static readonly string USER_PIC_URL_KEY = "picurl";
        public static readonly string LAST_TIME_CHECKED_KEY = "lasttime";

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

        private static IPropertySet localSettings;

        private static IPropertySet LocalSettings
        {
            get
            {
                if (localSettings == null)
                    localSettings = Windows.Storage.ApplicationData.Current.LocalSettings.Values;
                return localSettings;
            }
        }

        public static bool IsUserLoggedIn()
        {
            if (LocalSettings.ContainsKey(LOGIN_KEY))
                return (bool)LocalSettings[LOGIN_KEY];

            return false;
        }

        public static string GetPushbulletLoginURL()
        {
            return "https://www.pushbullet.com/authorize?client_id=" + Pushbullet.CLIENT_ID +
                "&redirect_uri=" + Uri.EscapeUriString(Pushbullet.REDIRECT_URI) + "&response_type=token";
        }

        public static void StoreAccessToken(string redirect)
        {
            LocalSettings[ACCESS_TOKEN_KEY] = redirect.Substring(redirect.IndexOf('=') + 1);
            LocalSettings[LOGIN_KEY] = true;
        }

        public static void ClearPreferences()
        {
            LocalSettings[ACCESS_TOKEN_KEY] = null;
            LocalSettings[LOGIN_KEY] = false;
            LocalSettings[USER_NAME_KEY] = null;
            LocalSettings[USER_PIC_URL_KEY] = null;
            LocalSettings[LAST_TIME_CHECKED_KEY] = null;
        }

        public static async Task<Push> PushNoteAsync(string message, string title = "", string device = "")
        {
            var values = new Dictionary<string, string>();
            values.Add("title", title);
            values.Add("device_iden", device);
            if (Uri.IsWellFormedUriString(message, UriKind.Absolute))
            {
                values.Add("type", "link");
                values.Add("url", message);
            }
            else
            {
                values.Add("type", "note");
                values.Add("body", message);
            }

            var response = await Client.PostAsync(
                "https://api.pushbullet.com/v2/pushes", new FormUrlEncodedContent(values));

            if (response.IsSuccessStatusCode)
            {
                dynamic push = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
                Push currentPush = null;
                switch ((string)push.type)
                {
                    case "note":
                        currentPush = new PushNote((string)push.iden, (string)push.title, (long)push.created,
                            (long)push.modified, (string)push.body);
                        break;
                    case "link":
                        currentPush = new PushLink((string)push.iden, (string)push.title, (long)push.created,
                            (long)push.modified, (string)push.url);
                        break;
                }

                ((App)Application.Current).Database.InsertPushAsync(currentPush);
                return currentPush;
            }

            return null;
        }

        public static async Task<Push> PushFileAsync(StorageFile file, string body = "", string title = "", string device = "")
        {
            var mime = String.IsNullOrEmpty(file.ContentType) ? Utils.FallbackGetMimeType(file.FileType) : file.ContentType;
            var values = new Dictionary<string, string>
            {
               { "file_name", file.Name }, 
               { "file_type", mime }
            };

            var response = await Client.PostAsync(
                "https://api.pushbullet.com/v2/upload-request", new FormUrlEncodedContent(values));

            dynamic res = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
            var upload_url = (string)res.upload_url;
            var file_url = (string)res.file_url;
            dynamic data = res.data;

            using (HttpClient noAuthHttpClient = new HttpClient())
            {
                var multipartFormDataContent = new MultipartFormDataContent();
                multipartFormDataContent.Add(AddContent("acl", (string)data.acl));
                multipartFormDataContent.Add(AddContent("awsaccesskeyid", (string)data.awsaccesskeyid));
                multipartFormDataContent.Add(AddContent("content-type", mime));
                multipartFormDataContent.Add(AddContent("key", (string)data.key));
                multipartFormDataContent.Add(AddContent("policy", (string)data.policy));
                multipartFormDataContent.Add(AddContent("signature", (string)data.signature));

                var fileContent = new StreamContent((await file.OpenReadAsync()).AsStreamForRead());
                fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "\"file\"",
                    FileName = "\"" + file.Name + "\""
                };
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(mime);

                multipartFormDataContent.Add(fileContent);

                response = await noAuthHttpClient.PostAsync(upload_url, multipartFormDataContent);
            }

            if (response.IsSuccessStatusCode)
            {
                values = new Dictionary<string, string>
                {
                    { "type", "file" }, 
                    { "file_name", file.Name }, 
                    { "file_type", mime }, 
                    { "file_url", file_url }, 
                    { "title", title },
                    { "body", body },
                    { "device_iden", device }
                };

                response = await Client.PostAsync(
                    "https://api.pushbullet.com/v2/pushes", new FormUrlEncodedContent(values));

                if (response.IsSuccessStatusCode)
                {
                    dynamic push = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
                    var currentPush = new PushFile((string)push.iden, (string)push.title, (string)push.body, (long)push.created,
                        (long)push.modified, (string)push.file_name, (string)push.file_type, (string)push.file_url);
                    ((App)Application.Current).Database.InsertPushAsync(currentPush);
                    return currentPush;
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

        public static async Task<Dictionary<string, string>> GetUserInfoAsync()
        {
            var response = await Client.GetAsync("https://api.pushbullet.com/v2/users/me");
            if (response.IsSuccessStatusCode)
            {
                dynamic json = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
                var values = new Dictionary<string, string>()
                {
                    { USER_NAME_KEY, (string)json.name },
                    { USER_PIC_URL_KEY, (string)json.image_url }
                };

                LocalSettings[USER_NAME_KEY] = (string)json.name;
                LocalSettings[USER_PIC_URL_KEY] = (string)json.image_url;
            }

            return null;
        }

        public static async Task<List<Device>> GetDeviceListAsync()
        {
            var response = await Client.GetAsync("https://api.pushbullet.com/v2/devices");
            if (response.IsSuccessStatusCode)
            {
                dynamic json = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);

                List<Device> devicesList = new List<Device>();
                foreach (dynamic device in json["devices"])
                    if ((bool)device.active)
                        devicesList.Add(new Device((string)device.iden, (string)device.nickname));

                return devicesList;
            }

            return null;
        }

        public static async Task<ObservableCollection<Push>> GetPushListAsync()
        {
            var response = await Client.GetAsync("https://api.pushbullet.com/v2/pushes");

            if (response.IsSuccessStatusCode)
            {
                LocalSettings[LAST_TIME_CHECKED_KEY] = Utils.GetUNIXTimeStamp();
                dynamic json = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);

                var pushes = new ObservableCollection<Push>();
                foreach (dynamic push in json["pushes"])
                {
                    if ((bool)push.active)
                    {
                        Push currentPush = null;
                        switch ((string)push.type)
                        {
                            case "note":
                                currentPush = new PushNote((string)push.iden, (string)push.title, (long)push.created,
                                    (long)push.modified, (string)push.body);
                                break;
                            case "link":
                                currentPush = new PushLink((string)push.iden, (string)push.title, (long)push.created,
                                    (long)push.modified, (string)push.url);
                                break;
                            case "file":
                                currentPush = new PushFile((string)push.iden, (string)push.title, (string)push.body,
                                    (long)push.created, (long)push.modified, (string)push.file_name, (string)push.file_type,
                                    (string)push.file_url);
                                break;
                        }
                        pushes.Add(currentPush);
                        ((App)Application.Current).Database.InsertPushAsync(currentPush);
                    }
                }

                return pushes;
            }

            return null;
        }

        public async static Task<ObservableCollection<Push>> UpdatePushListAsync()
        {
            if (!LocalSettings.ContainsKey(LAST_TIME_CHECKED_KEY))
                return await GetPushListAsync();

            var response = await 
                Client.GetAsync("https://api.pushbullet.com/v2/pushes?modified_after=" + LocalSettings[LAST_TIME_CHECKED_KEY]);
            if (response.IsSuccessStatusCode)
            {
                LocalSettings[LAST_TIME_CHECKED_KEY] = Utils.GetUNIXTimeStamp();
                dynamic json = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);

                var pushes = new ObservableCollection<Push>();
                foreach (dynamic push in json["pushes"])
                {
                    Push currentPush = null;
                    if ((bool)push.active)
                    {
                        switch ((string)push.type)
                        {
                            case "note":
                                currentPush = new PushNote((string)push.iden, (string)push.title, (long)push.created,
                                    (long)push.modified, (string)push.body);
                                break;
                            case "link":
                                currentPush = new PushLink((string)push.iden, (string)push.title, (long)push.created,
                                    (long)push.modified, (string)push.url);
                                break;
                            case "file":
                                currentPush = new PushFile((string)push.iden, (string)push.title, (string)push.body,
                                    (long)push.created, (long)push.modified, (string)push.file_name, (string)push.file_type,
                                    (string)push.file_url);
                                break;
                        }
                        ((App)Application.Current).Database.UpdatePushAsync(currentPush);
                    }
                    else
                    {
                        currentPush = new Push((string)push.iden, (long)push.created, (long)push.modified, false);
                        ((App)Application.Current).Database.DeletePushAsync(currentPush);
                    }

                    pushes.Add(currentPush);

                }

                return pushes;
            }

            return null;
        }

        public async static Task<Boolean> DeletePushAsync(Push push)
        {
            var response = await Client.DeleteAsync("https://api.pushbullet.com/v2/pushes/" + push.Iden);

            if (response.IsSuccessStatusCode)
                ((App)Application.Current).Database.DeletePushAsync(push);

            return response.IsSuccessStatusCode;
        }

    }
}
