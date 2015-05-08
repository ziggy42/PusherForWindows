﻿using PusherForWindows.Model;
using PusherForWindows.Pusher;
using System;
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
    public sealed partial class NewPushFlyout : SettingsFlyout
    {
        private StorageFile file;

        public event EventHandler<PushEventArgs> NewPushSent;

        public NewPushFlyout()
        {
            this.InitializeComponent();
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

                    var newPush = (file != null) ? await PusherUtils.PushFileAsync(file, body, title) : 
                        await PusherUtils.PushNoteAsync(body, title);
                    
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

            file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                FileImage.Visibility = Visibility.Visible;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.SetSource(await file.GetThumbnailAsync(ThumbnailMode.SingleItem));
                FileImage.Source = bitmapImage;
                this.ShowIndependent();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Operazione cancellata");
            }
        }

        private void OnNewPushSent(Push newPush)
        {
            if(NewPushSent != null)
                NewPushSent(this, new PushEventArgs(newPush));
        }
    }

    public class PushEventArgs : EventArgs
    {
        public Push NewPush
        {
            get
            {
                return newPush;
            }
        }
        private Push newPush;

        public PushEventArgs(Push newPush)
        {
            this.newPush = newPush;
        }
    }
}