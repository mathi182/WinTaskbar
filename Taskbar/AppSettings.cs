using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Taskbar
{
    public static class AppSettings
    {
        public static string PersonalAccessToken => ConfigurationManager.AppSettings["PAT"];
        public static string IdQueryHighPriority => ConfigurationManager.AppSettings["idQueryHighPriority"];
        public static string HighPriorityBugsConnectorUrl => ConfigurationManager.AppSettings["HighPriorityBugsConnectorUrl"];
    }
}
