using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Commons
{
    public static class HttpHelper
    {
        public static async Task SaveToFileAsync(this HttpResponseMessage repMsg,
            string file, CancellationToken cancellationToken = default)
        {
            if (repMsg.IsSuccessStatusCode==false)
            {
                throw new ArgumentException($"StatusCode of HttpResponseMessage is {repMsg.StatusCode}", nameof(repMsg));
            }
            using (FileStream fs = new FileStream(file,FileMode.Create))
            {
                await repMsg.Content.CopyToAsync(fs,cancellationToken);
            }
        }

        public static async Task<HttpStatusCode> DownloadFileAsync(this HttpClient httpClient,
            Uri url, string localFile,CancellationToken ct=default)
        {
            var resp = await httpClient.GetAsync(url, ct);
            
            if (resp.IsSuccessStatusCode)
            {
                await SaveToFileAsync(resp,localFile, ct);
                return resp.StatusCode;
            }
            else
            {
                return HttpStatusCode.OK;
            }

        }

        public static async Task<T?> GetJsonAsync<T>(this HttpClient httpClient, Uri url, 
            CancellationToken ct=default)
        {
            string json = await httpClient.GetStringAsync(url, ct);
            return json.ParseJson<T>();
        }
    }
}
