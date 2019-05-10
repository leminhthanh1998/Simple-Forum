using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SimpleForum.Helper
{
    public class MetricsReader
    {
        private const string AzureAppInsightApiEndpointAddress = "https://api.applicationinsights.io/v1/apps/{0}/{1}/{2}?{3}";
        private const string AzureAppInsightAppId = "";
        private const string AzureAppInsightApiKey = "";

        public async Task<dynamic> GetP1DIntMetrics(string query, string aggregation)
        {
            var response = await GetAppInsightDataAsync(query, $"timespan=P1D&aggregation={aggregation}");
            if (!string.IsNullOrEmpty(response))
            {
                var obj = JsonConvert.DeserializeObject<dynamic>(response);
                return obj;
            }

            return null;
        }

        public async Task<string> GetAppInsightDataAsync
            (string queryPath, string parameterString)
        {
            return await GetAppInsightDataAsync(AzureAppInsightAppId, AzureAppInsightApiKey, "metrics", queryPath, parameterString);
        }

        private async Task<string> GetAppInsightDataAsync
            (string appid, string apikey, string queryType, string queryPath, string parameterString)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("x-api-key", apikey);

            var req = string.Format(AzureAppInsightApiEndpointAddress, appid, queryType, queryPath, parameterString);
            var response = client.GetAsync(req).Result;
            var result = string.Empty;
            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsStringAsync();
                return result;
            }
            return result;
           
        }
    }
}
