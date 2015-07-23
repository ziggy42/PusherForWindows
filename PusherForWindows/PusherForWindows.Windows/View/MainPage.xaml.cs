using System;
using System.Linq;
using PusherForWindows.Model;
using PusherForWindows.Pusher;
using PusherForWindows.View;
using System.Text.RegularExpressions;
using Windows.Security.Authentication.Web;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.System;
using Windows.ApplicationModel.Resources;

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
                if (!Pushbullet.IsUserLoggedIn())
                {
                    this.CreateLoginDialogAsync();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("I'm in");
                    this.pushDataSource.Populate();
                }
            }
        }

        private async void CreateLoginDialogAsync()
        {
            var loader = new ResourceLoader();
            var dialog = new CoreWindowDialog(loader.GetString("Welcome"));
            dialog.Commands.Add(new UICommand(loader.GetString("Login"), this.PerformLoginAsync));
            await dialog.ShowAsync();
        }

        private async void PerformLoginAsync(IUICommand command)
        {
            try
            {
                System.Uri startUri = new Uri(Pushbullet.GetPushbulletLoginURL());
                System.Uri endUri = new Uri(Pushbullet.REDIRECT_URI);

                WebAuthenticationResult webAuthenticationResult = await WebAuthenticationBroker.AuthenticateAsync(
                                                        WebAuthenticationOptions.None, startUri, endUri);
                if (webAuthenticationResult.ResponseStatus == WebAuthenticationStatus.Success)
                {
                    Pushbullet.StoreAccessToken(webAuthenticationResult.ResponseData.ToString());
                    this.pushDataSource.Populate();
                }
                else if (webAuthenticationResult.ResponseStatus == WebAuthenticationStatus.ErrorHttp)
                {
                    System.Diagnostics.Debug.WriteLine("HTTP Error returned by AuthenticateAsync() : " +
                        webAuthenticationResult.ResponseErrorDetail.ToString());
                    this.CreateLoginDialogAsync();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Error returned by AuthenticateAsync() : " +
                        webAuthenticationResult.ResponseStatus.ToString());
                    this.CreateLoginDialogAsync();
                }
            }
            catch (Exception Error)
            {
                System.Diagnostics.Debug.WriteLine(Error.ToString());
                this.CreateLoginDialogAsync();
            }
        }

        private void FastPushButton_Click(object sender, RoutedEventArgs e)
        {
            var flyout = new FastPushFlyout();
            flyout.NewPushSent += OnNewPush;
            flyout.ShowIndependent();
        }

        public void OnNewPush(object sender, Push p)
        {
            this.pushDataSource.Add(p);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            this.pushDataSource.Refresh();
        }

        private void PushButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(PushToPage), this.pushDataSource.Items);
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(SearchTextBox.Text))
            {
                PushesListView.DataContext = this.pushDataSource;
            }
            else
            {
                var result = pushDataSource.Items.Where(item => !string.IsNullOrEmpty(item.Title) && item.Title.Contains(SearchTextBox.Text));
                PushesListView.DataContext = new PushDataSource(result.ToList());
            }
        }

        private void FilterMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            switch (((MenuFlyoutItem)sender).Tag.ToString())
            {
                case "links":
                    PushesListView.DataContext = new PushDataSource(
                        pushDataSource.Items.Where((Push p) => p.GetType() == typeof(PushLink)).ToList());
                    break;
                case "notes":
                    PushesListView.DataContext = new PushDataSource(
                        pushDataSource.Items.Where((Push p) => p.GetType() == typeof(PushNote)).ToList());
                    break;
                case "files":
                    PushesListView.DataContext = new PushDataSource(
                        pushDataSource.Items.Where((Push p) => p.GetType() == typeof(PushFile)).ToList());
                    break;
                case "images":
                    PushesListView.DataContext = new PushDataSource(
                        pushDataSource.Items.Where((Push p) => (p.GetType() == typeof(PushFile)) && 
                            (new Regex(@"(^image\/)(.*)")).Match((string)((PushFile)p).MimeType).Success).ToList());
                    break;
                default:
                    PushesListView.DataContext = this.pushDataSource;
                    break;
            }
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement);
        }

        private async void DeleteMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            var push = senderElement.DataContext as Push;
            if (await Pushbullet.DeletePushAsync(push))
                this.pushDataSource.Remove(push);
        }

        private async void OpenMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            if (senderElement.DataContext is PushLink)
                await Launcher.LaunchUriAsync((senderElement.DataContext as PushLink).URL);
            else
                await Launcher.LaunchUriAsync((senderElement.DataContext as PushFile).URL);
        }
    }
}
