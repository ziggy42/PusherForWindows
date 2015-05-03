using PusherForWindows.Model;
using PusherForWindows.Pusher;
using System;
using System.Collections.Generic;
using Windows.Security.Authentication.Web;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace PusherForWindows
{

    public sealed partial class MainPage : Page
    {
        PushDataSource pushDataSource = new PushDataSource();

        public MainPage()
        {
            this.InitializeComponent();
            PushesListView.DataContext = pushDataSource;
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
            pushDataSource.Populate();
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

        private void PushButton_Click(object sender, RoutedEventArgs e)
        {
            var flyout = new NewPushFlyout(this);
            flyout.ShowIndependent();
        }

        private void PushesListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            //TODO do something
        }

        public void OnNewPush(Push push)
        {
            pushDataSource.Add(push);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            pushDataSource.Refresh();
        }
    }
}
