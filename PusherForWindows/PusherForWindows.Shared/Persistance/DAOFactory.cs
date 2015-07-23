using System;
using System.Collections.Generic;
using System.Text;

namespace PusherForWindows.Persistance
{
    static class DAOFactory
    {
        public static PushDAO GetPushDAO()
        {
            return SQLitePushDAO.GetInstance();
        }
    }
}
