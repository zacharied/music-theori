using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using theori.Charting;
using theori.Configuration;

namespace theori.InternetRanking
{
    internal class OrchestraFmIr : InternetRankingProvider
    {
        private readonly string m_serviceUrl;
        private readonly string m_username;
        private readonly string m_password;

        private string? m_jwtBearer = null;

        public OrchestraFmIr(HttpClient http) : base(http)
        {
            m_serviceUrl = UserConfigManager.GetFromKey("theori.InternetRankingUrl")?.ToString() ??
                           throw new NullReferenceException("InternetRankingUrl not set");
            m_username = UserConfigManager.GetFromKey("theori.InternetRankingUsername")?.ToString() ??
                           throw new NullReferenceException("InternetRankingUsername not set");
            m_password = UserConfigManager.GetFromKey("theori.InternetRankingPassword")?.ToString() ??
                           throw new NullReferenceException("InternetRankingPassword not set");
            m_jwtBearer = null;
        }

        public override async Task SubmitScore(ChartInfo chart, Dictionary<string, dynamic> scoreObject)
        {
            string responseText;

            try
            {
                m_submissionStatus.InProgress = true;

                if (m_jwtBearer == null)
                    await Login();

                if (chart.SourceFileHash == null)
                    throw new InvalidOperationException("missing source file hash");

                try
                {
                    var res = await Request(HttpMethod.Get, $"board/sha3/{chart.SourceFileHash.ToLower()}");
                    responseText = await res.Content.ReadAsStringAsync();
                }
                catch (InternetRankingException e) when (e.IsNetworkError)
                {
                    throw new InternetRankingException("Failed getting board object for chart hash", e);
                }

                // Deserialize response to a list of dictionaries. This is probably kinda stupid.
                var result = JsonConvert.DeserializeObject<JObject>(responseText);

                // TODO(neko) Type check object
                scoreObject.Add("track", result["track_id"]);
                scoreObject.Add("board", result["id"]);

                var text = JsonConvert.SerializeObject(scoreObject);

                try
                {
                    var res = await Request(HttpMethod.Post, $"score", new ByteArrayContent(Encoding.UTF8.GetBytes(text)));
                    responseText = await res.Content.ReadAsStringAsync();
                }
                catch (InternetRankingException e) when (e.IsNetworkError)
                {
                    throw new InternetRankingException("Failed submitting score object", e);
                }
            }
            catch (InternetRankingException e)
            {
                m_submissionStatus.Error = e;
            }
            finally
            {
                m_submissionStatus.InProgress = false;
            }
        }

        public async Task Login()
        {
            string loginReq = $"{{\"username\":\"{m_username}\",\"password\":\"{m_password}\"}}";

            string responseText;
            try
            {
                var res = await Request(HttpMethod.Post, "authorize/basic", new ByteArrayContent(Encoding.UTF8.GetBytes(loginReq)));
                responseText = await res.Content.ReadAsStringAsync();
            }
            catch (InternetRankingException e) when (e.IsNetworkError)
            {
                throw new InternetRankingException("Failed to login", e);
            }

            var result = JsonConvert.DeserializeObject<JObject>(responseText);
            m_jwtBearer = result["bearer"].ToString().Trim();
        }

        private async Task<HttpResponseMessage> Request(HttpMethod method, string path, HttpContent? body = null)
        {
            var request = new HttpRequestMessage
            {
                Method = method,
                RequestUri = new Uri($"{m_serviceUrl}/{path}"),
                Headers =
                {
                    { HttpRequestHeader.Accept.ToString(), System.Net.Mime.MediaTypeNames.Application.Json },
                    { HttpRequestHeader.CacheControl.ToString(), new CacheControlHeaderValue {NoCache = true}.ToString() },
                    { "Cache-Control", new CacheControlHeaderValue {NoCache = true}.ToString() }
                }
            };

            if (m_jwtBearer != null)
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", m_jwtBearer);

            Logger.Log($"Headers: {request.Headers}");

            if (body != null)
            {
                body.Headers.ContentType = new MediaTypeHeaderValue(System.Net.Mime.MediaTypeNames.Application.Json);
                request.Content = body;
            }

            var res = await m_http.SendAsync(request);
            if (!res.IsSuccessStatusCode)
                throw new InternetRankingException($"HTTP request failure {res.StatusCode}", res);
            return res;
        }
    }
}
