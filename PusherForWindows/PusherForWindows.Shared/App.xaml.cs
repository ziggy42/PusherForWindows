using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using PusherForWindows.Pusher;
using Windows.UI.ApplicationSettings;
using PusherForWindows.View;
using Windows.UI.Popups;
using PusherForWindows.Persistance;


namespace PusherForWindows
{
    public sealed partial class App : Application
    {

        public PushbulletStream Stream { get; private set; }

#if WINDOWS_PHONE_APP
        private TransitionCollection transitions;
#endif

        public App()
        {
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;

            this.Stream = new PushbulletStream();

            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings.Values;
            if ((localSettings["mirroring"] == null) || ((bool)localSettings["mirroring"]))
                this.Stream.Connect();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                rootFrame = new Frame();

                rootFrame.CacheSize = 1;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO: Caricare lo stato dall'applicazione sospesa in precedenza
                }

                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
#if WINDOWS_PHONE_APP
                // Rimuove l'avvio della navigazione turnstile.
                if (rootFrame.ContentTransitions != null)
                {
                    this.transitions = new TransitionCollection();
                    foreach (var c in rootFrame.ContentTransitions)
                    {
                        this.transitions.Add(c);
                    }
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += this.RootFrame_FirstNavigated;
#endif

                if (!rootFrame.Navigate(typeof(MainPage), e.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            Window.Current.Activate();
        }

        protected override void OnWindowCreated(WindowCreatedEventArgs args)
        {
            SettingsPane.GetForCurrentView().CommandsRequested += OnCommandsRequested;
        }

        private void OnCommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {

            args.Request.ApplicationCommands.Add(new SettingsCommand(
                "Mirroring Notifications", "Mirroring Notifications", (handler) =>
                {
                    PreferencesFlyout preferencesFlyout = new PreferencesFlyout();
                    preferencesFlyout.Show();
                }));

            args.Request.ApplicationCommands.Add(new SettingsCommand(
                "Logout", "Logout", async (handler) =>
                {
                    var dialog = new MessageDialog("Are you sure you want to logout?");
                    dialog.Commands.Add(new UICommand("Logout", (command) =>
                    {
                        Pushbullet.ClearPreferences();
                        PushDAOImpl.GetInstance().DropTableAsync();
                        App.Current.Exit();
                    }));
                    dialog.Commands.Add(new UICommand("Cancel", null));
                    var asyncOperation = await dialog.ShowAsync();
                }));
        }

#if WINDOWS_PHONE_APP
        /// <summary>
        /// Ripristina le transizioni del contenuto dopo l'avvio dell'applicazione.
        /// </summary>
        /// <param name="sender">Oggetto a cui è associato il gestore.</param>
        /// <param name="e">Dettagli sull'evento di navigazione.</param>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }
#endif

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            deferral.Complete();
        }

    }
}