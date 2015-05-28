using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace PusherForWindows.View
{
    public sealed partial class PreferencesFlyout : SettingsFlyout
    {
        private static readonly string MIRRORING_ENABLED = "mirroring";

        private IPropertySet localSettings;

        public PreferencesFlyout()
        {
            this.localSettings = Windows.Storage.ApplicationData.Current.LocalSettings.Values;

            this.InitializeComponent();

            this.MirroringCheckBox.IsChecked = (localSettings[MIRRORING_ENABLED] == null) || ((bool)localSettings[MIRRORING_ENABLED]);
        }

        private void MirroringCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            localSettings[MIRRORING_ENABLED] = true;
            ((App)Application.Current).StartMirroringNotificationStream();
        }

        private void MirroringCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            localSettings[MIRRORING_ENABLED] = false;
            ((App)Application.Current).StopMirroringNotificationStream();
        }
    }
}
