using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskbar.AdoAlerters
{
    public class WorkItem
    {
        public int Id { get; set; }
        public DateTime DtCreated { get; set; }
        public string Url => $"https://dev.azure.com/DialogInsight/Openfield/_workitems/edit/{Id}";
        public string Title { get; set; }
    }
}
