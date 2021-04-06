using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Taskbar.AdoAlerters
{
    /// <summary>
    /// Références
    /// https://docs.microsoft.com/en-us/microsoftteams/platform/webhooks-and-connectors/how-to/connectors-using
    /// https://docs.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/cards-format?tabs=adaptive-md%2Cconnector-html
    /// https://docs.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/cards-reference#office-365-connector-card
    /// </summary>
    public class HighPriorityBugsAlerter
    {
        private bool isRunning = false;
        private Task task;

        public HighPriorityBugsAlerter()
        {
            
        }

        private void Run()
        {
            isRunning = true;
            task = Task.Run(RunInternal);
        }

        private void RunInternal()
        {
            DateTime dtLastCheck = DateTime.MinValue;
            while (isRunning)
            {
                DateTime now = DateTime.Now;
                if (now < dtLastCheck || now.Hour < 8 || now.Hour > 16 || now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday)
                {
                    Thread.Sleep(1000);
                    continue;
                }

                dtLastCheck = now.AddMinutes(10);
                string url = $"https://dev.azure.com/DialogInsight/Openfield/_apis/wit/queries/{AppSettings.IdQueryHighPriority}?api-version=6.0&$expand=clauses";
                dynamic result = GetResultFromGetApi(url);
                string wiql = result?.wiql?.ToString();

                if (wiql == null)
                {
                    Thread.Sleep(1000);
                    continue;
                }

                url = $"https://dev.azure.com/DialogInsight/Openfield/_apis/wit/wiql?api-version=6.0";
                var args = new Dictionary<string, string>
                {
                    { "query", wiql }
                };
                string body = JsonConvert.SerializeObject(args);
                result = GetResultFromPostApi(url, body);
                List<int> ids = new List<int>();
                foreach (var workitem in result.workItems)
                    ids.Add((int)workitem.id);

                if (ids.Any())
                {
                    var workItems = GetWorkItems(ids.ToArray()).Where(w => w.DtCreated <= now.AddMinutes(-5));
                    if (workItems.Any())
                    {
                        body = BuildMessageCard(workItems);
                        GetResultFromPostApi(AppSettings.HighPriorityBugsConnectorUrl, body);
                    }                    
                }
            }
        }

        private IEnumerable<WorkItem> GetWorkItems(params int[] id)
        {
            var workItems = new List<WorkItem>();
            string url = $"https://dev.azure.com/DialogInsight/Openfield/_apis/wit/workitems?ids={string.Join(",", id)}&api-version=6.0";
            dynamic result = GetResultFromGetApi(url);
            foreach (var wi in result.value)
            {
                WorkItem workItem = new WorkItem();
                workItem.Id = (int)wi.id;
                workItem.DtCreated = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Parse(wi.fields["System.CreatedDate"].ToString()), TimeZoneInfo.Local);
                workItem.Title = wi.fields["System.Title"].ToString();

                workItems.Add(workItem);
            }

            return workItems;
        }

        private string BuildMessageCard(IEnumerable<WorkItem> workitems)
        {
            var paragraphs = new List<Dictionary<string, object>>();
            paragraphs.Add(new Dictionary<string, object> 
            { 
                { "type", "TextBlock" },
                { "text", "Bugs urgents en attente <at>Bugs</at>" }
            });
            paragraphs.Add(new Dictionary<string, object>
            {
                { "type", "TextBlock" },
                { "text", string.Join("\r", workitems.Select(u => $"- {u.Title} [{u.Url}]({u.Url})").ToArray()) },
                { "wrap", true }
            });
            var dict = new Dictionary<string, object>
            {
                { "type", "message" },
                { "attachments", new[] { new Dictionary<string, object>
                {
                    { "contentType", "application/vnd.microsoft.card.adaptive" },
                    { "contentUrl", null },
                    { "content", new Dictionary<string, object> {
                        { "$schema", "https://adaptivecards.io/schemas/adaptive-card.json" },
                        { "type", "AdaptiveCard" },
                        { "version", "1.2" },
                        { "body", paragraphs },
                        { "msteams", new Dictionary<string, object> 
                        {
                            { "width", "Full" },
                            { "entities", new[] { new Dictionary<string, object> {
                                { "type", "mention" },
                                { "text", "<at>Bugs</at>" },
                                { "mentioned", new Dictionary<string, object> {
                                    { "id", "19:a14ab1e0d16264377adc477d1b663c096" },
                                    { "name", "Bugs" }
                                } }
                            } } }
                        } }
                    }
                    }
                }
                }
                } 
            };

            return JsonConvert.SerializeObject(dict);
        }

        public void Start()
        {
            if (task == null || task.IsCompleted)
                Run();
        }

        public void Stop()
        {
            isRunning = false;
        }

        private dynamic GetResultFromGetApi(string url)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($":{AppSettings.PersonalAccessToken}")));

            HttpResponseMessage response = client.GetAsync("").Result;
            return JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
        }

        private dynamic GetResultFromPostApi(string url, string body)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($":{AppSettings.PersonalAccessToken}")));
            StringContent sc = new StringContent(body, Encoding.UTF8, "application/json");

            HttpResponseMessage response = client.PostAsync(new Uri(url), sc).Result;
            return JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
        }
    }
}
