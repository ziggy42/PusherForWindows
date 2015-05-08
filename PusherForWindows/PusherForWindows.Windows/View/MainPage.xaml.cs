using System;
using System.Linq;
using PusherForWindows.Model;
using PusherForWindows.Pusher;
using PusherForWindows.View;
using System.Text.RegularExpressions;
using Windows.Networking.Connectivity;
using Windows.Security.Authentication.Web;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace PusherForWindows
{
    public sealed partial class MainPage : Page
    {
        private PushDataSource pushDataSource = new PushDataSource();

        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;

            PushesListView.DataContext = this.pushDataSource;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode != NavigationMode.Back)
            {
                if (!PusherUtils.IsUserLoggedIn())
                {
                    this.CreateLoginDialogAsync();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("I'm in");
                    this.SetupAsync();
                }
            }
        }

        private async void CreateLoginDialogAsync()
        {
            var dialog = new CoreWindowDialog("Welcome to Pusher");
            dialog.Commands.Add(new UICommand("Login with Pushbullet", this.PerformLoginAsync));
            await dialog.ShowAsync();
        }

        private async void PerformLoginAsync(IUICommand command)
        {
            try
            {
                System.Uri startUri = new Uri(PusherUtils.GetPushbulletLoginURL());
                System.Uri endUri = new Uri(PusherUtils.REDIRECT_URI);

                WebAuthenticationResult webAuthenticationResult = await WebAuthenticationBroker.AuthenticateAsync(
                                                        WebAuthenticationOptions.None, startUri, endUri);
                if (webAuthenticationResult.ResponseStatus == WebAuthenticationStatus.Success)
                {
                    PusherUtils.StoreAccessToken(webAuthenticationResult.ResponseData.ToString());
                }
                else if (webAuthenticationResult.ResponseStatus == WebAuthenticationStatus.ErrorHttp)
                {
                    System.Diagnostics.Debug.WriteLine("HTTP Error returned by AuthenticateAsync() : " +
                        webAuthenticationResult.ResponseErrorDetail.ToString());
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Error returned by AuthenticateAsync() : " +
                        webAuthenticationResult.ResponseStatus.ToString());
                }
            }
            catch (Exception Error)
            {
                System.Diagnostics.Debug.WriteLine(Error.ToString());
            }

            this.SetupAsync();
        }

        private async void SetupAsync()
        {
            if (NetworkInformation.GetInternetConnectionProfile().GetNetworkConnectivityLevel()
                    == NetworkConnectivityLevel.InternetAccess)
                this.pushDataSource.Populate();

            /*if (Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey(PusherUtils.USER_NAME_KEY))
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
            }*/
        }

        private void PushButton_Click(object sender, RoutedEventArgs e)
        {
            var flyout = new NewPushFlyout();
            flyout.NewPushSent += OnNewPush;
            flyout.ShowIndependent();
        }

        private void PushesListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            //TODO do something
        }

        public void OnNewPush(object sender, PushEventArgs e)
        {
            this.pushDataSource.Add(e.NewPush);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            this.pushDataSource.Refresh();
        }

        private void ChooseDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ChooseDevicePage), this.pushDataSource.Items);
        }

        private void FilterMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var pushQuery = from push in pushDataSource.Items select push;

            switch (((MenuFlyoutItem)sender).Tag.ToString())
            {
                case "links":
                    pushQuery = pushQuery.Where((Push p) => p.GetType() == typeof(PushLink));
                    break;
                case "notes":
                    pushQuery = pushQuery.Where((Push p) => p.GetType() == typeof(PushNote));
                    break;
                case "files":
                    pushQuery = pushQuery.Where((Push p) => p.GetType() == typeof(PushFile));
                    break;
                case "images":
                    pushQuery = pushQuery.Where((Push p) => (p.GetType() == typeof(PushFile)) &&
                        (new Regex(@"(^image\/)(.*)")).Match((string)((PushFile)p).MimeType).Success);
                    break;
                default:
                    PushesListView.DataContext = this.pushDataSource;
                    break;
            }

            PushesListView.DataContext = new PushDataSource(pushQuery.ToList());
        }
    }
}
