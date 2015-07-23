using PusherForWindows.Model;
using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PusherForWindows.Persistance
{
    public class PushDAOImpl : PushDAO
    {
        private static readonly string DB_NAME = "DBPush";
        private SQLiteAsyncConnection conn;

        private static PushDAOImpl pushDAOImpl;

        public static PushDAOImpl GetInstance()
        {
            if (pushDAOImpl == null)
            {
                pushDAOImpl = new PushDAOImpl();
                pushDAOImpl.InitializeAsync();
            }

            return pushDAOImpl;
        }

        private PushDAOImpl()
        {
            this.conn = new SQLiteAsyncConnection(DB_NAME);
        }

        private async void InitializeAsync()
        {
            await this.conn.CreateTableAsync<PushDTO>();
        }

        public async void DropTableAsync()
        {
            await this.conn.ExecuteAsync("DROP TABLE PushDTO;");
        }

        public async void InsertPushAsync(Push push)
        {
            await conn.InsertAsync(PushDTO.GetPushDTOFromPush(push));
        }

        public async void DeletePushAsync(Push push)
        {
            await this.conn.ExecuteAsync(
                "DELETE FROM PushDTO WHERE Iden = '" + push.Iden + "';");
        }

        public async void UpdatePushAsync(Push push)
        {
            if (!await this.PushExistsAsync(push))
                this.InsertPushAsync(push);
        }

        public async Task<bool> PushExistsAsync(Push push)
        {
            var queryResult = await this.conn.Table<PushDTO>().Where(
                x => x.Iden.Equals(push.Iden)).CountAsync();
            return queryResult > 0;
        }

        public async Task<IList<Model.Push>> GetAllPushesAsync()
        {
            var result = await (conn.Table<PushDTO>()).ToListAsync();

            IList<Push> pushes = new List<Push>();
            foreach (PushDTO pushEntry in result)
            {
                switch (pushEntry.Type)
                {
                    case PusherForWindows.Persistance.PushDTO.TYPE.FILE:
                        pushes.Add(new PushFile(pushEntry.Iden, pushEntry.Title, pushEntry.Body, pushEntry.Created,
                            pushEntry.Modified, pushEntry.FileName, pushEntry.MimeType, pushEntry.Url));
                        break;
                    case PusherForWindows.Persistance.PushDTO.TYPE.LINK:
                        pushes.Add(new PushLink(pushEntry.Iden, pushEntry.Title, pushEntry.Created, pushEntry.Modified,
                            pushEntry.Url));
                        break;
                    case PusherForWindows.Persistance.PushDTO.TYPE.NOTE:
                        pushes.Add(new PushNote(pushEntry.Iden, pushEntry.Title, pushEntry.Created, pushEntry.Modified,
                            pushEntry.Body));
                        break;
                }
            }

            return pushes;
        }
    }
}
