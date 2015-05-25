using System;
using PusherForWindows.Model;
using PusherForWindows.Pusher;
using Windows.Data.Xml.Dom;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace PusherForWindows
{
    public sealed partial class FastPushFlyout : SettingsFlyout
    {
        private StorageFile file;

        public event EventHandler<Push> NewPushSent;

        public FastPushFlyout()
        {
            this.InitializeComponent();
        }

        private async void SendPushButton_Click(object sender, RoutedEventArgs e)
        {
            string title = TitleTextBox.Text;
            string body = BodyTextBox.Text;

            if (title.Length > 0 || body.Length > 0 || this.file != null)
            {
                if (NetworkInformation.GetInternetConnectionProfile().GetNetworkConnectivityLevel()
                    == NetworkConnectivityLevel.InternetAccess)
                {
                    SendingProgressRing.IsActive = true;
                    SendPushButton.IsEnabled = false;
                    FilePickerButton.IsEnabled = false;

                    var newPush = (this.file != null) ? await Pushbullet.PushFileAsync(this.file, body, title) :
                        await Pushbullet.PushNoteAsync(body, title);

                    if (newPush != null)
                    {
                        TitleTextBox.Text = string.Empty;
                        BodyTextBox.Text = string.Empty;
                        FileImage.Source = null;

                        ToastTemplateType toastTemplate = ToastTemplateType.ToastText02;
                        XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);

                        XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
                        toastTextElements[0].AppendChild(toastXml.CreateTextNode("Push Sent!"));
                        toastTextElements[1].AppendChild(toastXml.CreateTextNode(body));

                        ToastNotification toast = new ToastNotification(toastXml);
                        toast.ExpirationTime = DateTimeOffset.UtcNow.AddSeconds(5);

                        ToastNotificationManager.CreateToastNotifier().Show(toast);

                        OnNewPushSent(newPush);
                        this.Hide();
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

            SendingProgressRing.IsActive = false;
            SendPushButton.IsEnabled = true;
            FilePickerButton.IsEnabled = true;
        }

        private async void FilePickerButton_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add("*");

            this.file = await openPicker.PickSingleFileAsync();
            if (this.file != null)
            {
                FileImage.Visibility = Visibility.Visible;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.SetSource(await this.file.GetThumbnailAsync(ThumbnailMode.SingleItem));
                FileImage.Source = bitmapImage;
                this.ShowIndependent();
            }
        }

        private void OnNewPushSent(Push newPush)
        {
            if (this.NewPushSent != null)
                this.NewPushSent(this, newPush);
        }
    }
}
