using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace PusherForWindows
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

        }
    }
}
