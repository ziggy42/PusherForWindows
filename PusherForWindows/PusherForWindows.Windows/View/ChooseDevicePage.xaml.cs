using PusherForWindows.Model;
using PusherForWindows.Pusher;
using System;
using System.Collections.ObjectModel;
using Windows.Data.Xml.Dom;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace PusherForWindows.View
{

    public sealed partial class ChooseDevicePage : Page
    {
        private ObservableCollection<Device> devices = new ObservableCollection<Device>();
        public ObservableCollection<Device> Devices { get { return devices; } }

        private StorageFile file;

        public ChooseDevicePage()
        {
            this.InitializeComponent();
            this.DevicesListView.DataContext = this;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            foreach (Device device in await PusherUtils.GetDeviceListAsync())
            {
                devices.Add(device);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

        private async void FilePickerButton_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add("*");

            file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                FileImage.Visibility = Visibility.Visible;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.SetSource(await file.GetThumbnailAsync(ThumbnailMode.SingleItem));
                FileImage.Source = bitmapImage;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Operazione cancellata");
            }
        }

        private async void SendPushButton_Click(object sender, RoutedEventArgs e)
        {
            string title = TitleTextBox.Text;
            string body = BodyTextBox.Text;

            if (title.Length > 0 || body.Length > 0 || file != null)
            {
                if (NetworkInformation.GetInternetConnectionProfile().GetNetworkConnectivityLevel()
                    == NetworkConnectivityLevel.InternetAccess)
                {
                    SendingProgressRing.IsActive = true;
                    SendPushButton.IsEnabled = false;
                    FilePickerButton.IsEnabled = false;

                    Push newPush;
                    if (DevicesListView.SelectedItems.Count > 0)
                    {
                        newPush = (file != null) ? await PusherUtils.PushFileAsync(file, body, title, ((Device)DevicesListView.SelectedItems[0]).Iden) :
                            await PusherUtils.PushNoteAsync(body, title, ((Device)DevicesListView.SelectedItems[0]).Iden);
                    }
                    else
                    {
                        newPush = (file != null) ? await PusherUtils.PushFileAsync(file, body, title) :
                            await PusherUtils.PushNoteAsync(body, title);
                    }

                    if (newPush != null)
                    {
                        TitleTextBox.Text = "";
                        BodyTextBox.Text = "";
                        FileImage.Source = null;

                        ToastTemplateType toastTemplate = ToastTemplateType.ToastText02;
                        XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);

                        XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
                        toastTextElements[0].AppendChild(toastXml.CreateTextNode("Push Sent!"));
                        toastTextElements[1].AppendChild(toastXml.CreateTextNode(body));

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
            SendingProgressRing.IsActive = false;
            SendPushButton.IsEnabled = true;
            FilePickerButton.IsEnabled = true;
        }
    }
}
