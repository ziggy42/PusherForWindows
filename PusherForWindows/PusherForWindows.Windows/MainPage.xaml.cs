using PusherForWindows.Pusher;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Connectivity;
using Windows.Security.Authentication.Web;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace PusherForWindows
{

    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void CreateLoginDialogAsync()
        {
            var dialog = new CoreWindowDialog("Welcome to Pusher");
            dialog.Commands.Add(new UICommand("Login with Pushbullet", PerformLoginAsync));
            await dialog.ShowAsync();
        }

        private async void PerformLoginAsync(IUICommand command)
        {
            try
            {
                System.Uri StartUri = new Uri(PusherUtils.GetPushbulletLoginURL());
                System.Uri EndUri = new Uri(PusherUtils.REDIRECT_URI);

                WebAuthenticationResult WebAuthenticationResult = await WebAuthenticationBroker.AuthenticateAsync(
                                                        WebAuthenticationOptions.None, StartUri, EndUri);
                if (WebAuthenticationResult.ResponseStatus == WebAuthenticationStatus.Success)
                {
                    PusherUtils.StoreAccessToken(WebAuthenticationResult.ResponseData.ToString());
                }
                else if (WebAuthenticationResult.ResponseStatus == WebAuthenticationStatus.ErrorHttp)
                {
                    System.Diagnostics.Debug.WriteLine("HTTP Error returned by AuthenticateAsync() : " +
                        WebAuthenticationResult.ResponseErrorDetail.ToString());
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Error returned by AuthenticateAsync() : " +
                        WebAuthenticationResult.ResponseStatus.ToString());
                }
            }
            catch (Exception Error)
            {
                System.Diagnostics.Debug.WriteLine(Error.ToString());
            }

            SetupAsync();
        }

        private async void SetupAsync()
        {
            if (Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey(PusherUtils.USER_NAME_KEY))
            {
                UserNameTextBlock.Text = (string)
                    Windows.Storage.ApplicationData.Current.LocalSettings.Values[PusherUtils.USER_NAME_KEY];
                UserProfileImage.Source = new BitmapImage(new Uri(
                    (string)Windows.Storage.ApplicationData.Current.LocalSettings.Values[PusherUtils.USER_PIC_URL_KEY]));
            }
            else
            {
                Dictionary<string, string> info = await PusherUtils.GetUserInfoAsync();
                UserNameTextBlock.Text = info[PusherUtils.USER_NAME_KEY];
                UserProfileImage.Source = new BitmapImage(new Uri(info[PusherUtils.USER_PIC_URL_KEY]));
            }
        }

        private async void SendSimplePushButton_Click(object sender, RoutedEventArgs e)
        {
            string message = SimplePushTextBox.Text;

            if (message.Length > 0)
            {
                if (NetworkInformation.GetInternetConnectionProfile().GetNetworkConnectivityLevel()
                    == NetworkConnectivityLevel.InternetAccess)
                {
                    if (await PusherUtils.PushNoteAsync(message))
                    {
                        SimplePushTextBox.Text = "";

                        ToastTemplateType toastTemplate = ToastTemplateType.ToastText02;
                        XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);

                        XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
                        toastTextElements[0].AppendChild(toastXml.CreateTextNode("Push Sent!"));
                        toastTextElements[1].AppendChild(toastXml.CreateTextNode(message));

                        ToastNotification toast = new ToastNotification(toastXml);
                        toast.ExpirationTime = DateTimeOffset.UtcNow.AddSeconds(5);

                        ToastNotificationManager.CreateToastNotifier().Show(toast);
                    }
                    else
                    {
                        var messageDialog = new Windows.UI.Popups.MessageDialog(
                            "Something wrong happened here but I don't know anything more...");
                        messageDialog.DefaultCommandIndex = 1;
                        await messageDialog.ShowAsync();
                    }
                }
                else
                {
                    var messageDialog = new Windows.UI.Popups.MessageDialog(
                        "Seems that internet is not available. Check your connection!");
                    messageDialog.DefaultCommandIndex = 1;
                    await messageDialog.ShowAsync();
                }
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!PusherUtils.IsUserLoggedIn())
            {
                CreateLoginDialogAsync();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("I'm in");
                SetupAsync();
            }
        }
    }
}
