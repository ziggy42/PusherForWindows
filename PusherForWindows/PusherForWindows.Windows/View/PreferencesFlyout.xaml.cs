using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace PusherForWindows.View
{
    public sealed partial class PreferencesFlyout : SettingsFlyout
    {
        private static readonly string MIRRORING_ENABLED = "mirroring";

        private IPropertySet localSettings;

        private IPropertySet LocalSettings
        {
            get
            {
                if (localSettings == null)
                    this.localSettings = Windows.Storage.ApplicationData.Current.LocalSettings.Values;
                return this.localSettings;
            }
        }

        public PreferencesFlyout()
        {
            this.InitializeComponent();

            this.MirroringCheckBox.IsChecked = (this.LocalSettings[MIRRORING_ENABLED] == null) || ((bool)this.LocalSettings[MIRRORING_ENABLED]);
        }

        private void MirroringCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            this.LocalSettings[MIRRORING_ENABLED] = true;
            ((App)Application.Current).Stream.Connect();
        }

        private void MirroringCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            this.LocalSettings[MIRRORING_ENABLED] = false;
            ((App)Application.Current).Stream.Close();
        }
    }
}
