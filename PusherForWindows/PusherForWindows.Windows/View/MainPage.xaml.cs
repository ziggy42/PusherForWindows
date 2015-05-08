using PusherForWindows.Model;
using PusherForWindows.Pusher;
using PusherForWindows.View;
using System;
using System.Linq;
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
        PushDataSource pushDataSource = new PushDataSource();

        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;

            PushesListView.DataContext = pushDataSource;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode != NavigationMode.Back)
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
            if (NetworkInformation.GetInternetConnectionProfile().GetNetworkConnectivityLevel()
                    == NetworkConnectivityLevel.InternetAccess)
                pushDataSource.Populate();

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
            pushDataSource.Add(e.NewPush);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            pushDataSource.Refresh();
        }

        private void ChooseDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ChooseDevicePage));
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
                    PushesListView.DataContext = pushDataSource;
                    break;
            }

            PushesListView.DataContext = new PushDataSource(pushQuery.ToList());
        }
    }
}
