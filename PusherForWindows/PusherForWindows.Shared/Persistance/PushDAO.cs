using PusherForWindows.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PusherForWindows.Persistance
{
    public interface PushDAO
    {
        void DropTableAsync();

        void InsertPushAsync(Push push);

        void DeletePushAsync(Push push);

        void UpdatePushAsync(Push push);

        Task<bool> PushExistsAsync(Push push);

        Task<IList<Push>> GetAllPushesAsync();
    }
}
