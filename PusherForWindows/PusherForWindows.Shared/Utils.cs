using System;
using System.Collections.Generic;
using System.Text;

namespace PusherForWindows
{
    class Utils
    {
        public static String FallbackGetMimeType(String extension)
        {
            switch(extension)
            {
                case ".pdf":
                    return "application/pdf";
                case ".vsd":
                    return "application/x-visio";
                case ".zip":
                    return "application/zip";
                // MORE
                default:
                    return "application/octet-stream";
            }
        }

        public static long GetUNIXTimeStamp()
        {
            return (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

    }
}
