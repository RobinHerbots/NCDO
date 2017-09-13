using System;
using System.Collections.Generic;
using System.Text;

namespace NCDO.Definitions
{
   public struct OfflineReason
   {
       public const string DEVICE_OFFLINE = "Device is offline";
       public const string SERVER_OFFLINE = "Cannot contact server";
       public const string WEB_APPLICATION_OFFLINE = "Mobile Web Application is not available";
       public const string APPSERVER_OFFLINE = "Appserver is not abailable";
    }
}
