using System;
using System.Collections.Generic;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SQLite;
using PusherForWindows.Model;
using System.Threading.Tasks;
using PusherForWindows.Pusher;


namespace PusherForWindows
{
    public sealed partial class App : Application
    {

        private static readonly string DB_NAME = "DBPush";

#if WINDOWS_PHONE_APP
        private TransitionCollection transitions;
#endif

        public App()
        {
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;

            CreateDB();

            PushbulletStream stream = new PushbulletStream();
            stream.Connect();
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

        public async void CreateDB()
        {
            SQLiteAsyncConnection conn = new SQLiteAsyncConnection(DB_NAME);
            await conn.CreateTableAsync<PushEntry>();
        }

        public async void DropTable()
        {
            var conn = new SQLiteAsyncConnection(DB_NAME);
            await conn.ExecuteAsync("DROP TABLE PushEntry;");
        }

        public async void InsertPush(Push push)
        {
            var conn = new SQLiteAsyncConnection(DB_NAME);
            await conn.InsertAsync(GetPushEntryFromPush(push));
        }

        public async void DeletePush(Push push)
        {
            var conn = new SQLiteAsyncConnection(DB_NAME);
            await conn.ExecuteAsync("DELETE FROM PushEntry WHERE Iden = '" + push.Iden + "';");
        }

        public async void UpdatePush(Push push)
        {
            if (!await PushEntryExists(push))
                this.InsertPush(push);
        }

        public async Task<bool> PushEntryExists(Push push)
        {
            var conn = new SQLiteAsyncConnection(DB_NAME);
            var queryResult = await conn.Table<PushEntry>().Where(x => x.Iden.Equals(push.Iden)).CountAsync();
            return queryResult > 0;
        }

        public async Task<IList<Push>> GetAllPushes()
        {
            SQLiteAsyncConnection conn = new SQLiteAsyncConnection(DB_NAME);

            var result = await (conn.Table<PushEntry>()).ToListAsync();

            IList<Push> pushes = new List<Push>();
            foreach (PushEntry pushEntry in result)
            {
                switch (pushEntry.Type)
                {
                    case TYPE.FILE:
                        pushes.Add(new PushFile(pushEntry.Iden, pushEntry.Title, pushEntry.Body, pushEntry.Created,
                            pushEntry.Modified, pushEntry.FileName, pushEntry.MimeType, pushEntry.Url));
                        break;
                    case TYPE.LINK:
                        pushes.Add(new PushLink(pushEntry.Iden, pushEntry.Title, pushEntry.Created, pushEntry.Modified,
                            pushEntry.Url));
                        break;
                    case TYPE.NOTE:
                        pushes.Add(new PushNote(pushEntry.Iden, pushEntry.Title, pushEntry.Created, pushEntry.Modified,
                            pushEntry.Body));
                        break;
                }
            }

            return pushes;
        }

        private PushEntry GetPushEntryFromPush(Push push)
        {
            PushEntry entry = new PushEntry()
            {
                Iden = push.Iden,
                Title = push.Title,
                Created = push.Created,
                Modified = push.Modified,
            };

            if (push is PushFile)
            {
                entry.Type = TYPE.FILE;
                entry.FileName = ((PushFile)push).FileName;
                entry.MimeType = ((PushFile)push).MimeType;
                entry.Url = ((PushFile)push).URL.ToString();
            }
            else if (push is PushNote)
            {
                entry.Type = TYPE.NOTE;
                entry.Body = ((PushNote)push).Body;
            }
            else if (push is PushLink)
            {
                entry.Type = TYPE.LINK;
                entry.Url = ((PushLink)push).URL.ToString();
            }

            return entry;
        }

        public enum TYPE { FILE, NOTE, LINK }

        public class PushEntry
        {
            [SQLite.PrimaryKey, SQLite.NotNull]
            public string Iden { get; set; }
            public string Title { get; set; }
            [SQLite.NotNull]
            public long Created { get; set; }
            [SQLite.NotNull]
            public long Modified { get; set; }
            [SQLite.NotNull]
            public TYPE Type { get; set; }
            public string FileName { get; set; }
            public string MimeType { get; set; }
            public string Body { get; set; }
            public string Url { get; set; }
        }
    }
}