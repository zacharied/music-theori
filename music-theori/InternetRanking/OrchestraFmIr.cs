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
        private string? m_jwtBearer = null;

        public OrchestraFmIr(HttpClient http) : base(http)
        {
            m_serviceUrl = UserConfigManager.GetFromKey("theori.InternetRankingUrl")?.ToString() ??
                           throw new NullReferenceException("InternetRankingUrl not set");
            m_jwtBearer = null;
        }

        public override async Task SubmitScore(ChartInfo chart, Dictionary<string, dynamic> scoreObject)
        {
            try
            {
                if (m_jwtBearer == null)
                {
                    await Login();
                }

                Logger.Log("Sending score");

                if (chart.SourceFileHash == null)
                    throw new InvalidOperationException("missing source file hash");

                m_submissionStatus.InProgress = true;

                Logger.Log($"Chart file hash: {chart.SourceFileHash}");

                var responseText = await Request(HttpMethod.Get, $"board/sha3/{chart.SourceFileHash.ToLower()}").Result
                    .Content.ReadAsStringAsync();
                Logger.Log($"Response: {responseText}");

                // Deserialize response to a list of dictionaries. This is probably kinda stupid.
                var result = JsonConvert.DeserializeObject<JObject>(responseText);
                Logger.Log("Parsed");

                // TODO(neko) Type check object
                scoreObject.Add("track", result["track_id"]);
                scoreObject.Add("board", result["id"]);

                var text = JsonConvert.SerializeObject(scoreObject);
                Logger.Log($"Sending score request: {text}");
                responseText =
                    await Request(HttpMethod.Post, $"score", new ByteArrayContent(Encoding.UTF8.GetBytes(text))).Result.Content.ReadAsStringAsync();
                Logger.Log($"Response: {responseText}");
            }
            catch (Exception e)
            {
                m_submissionStatus.Error = e;
                Logger.Log(e.Message);
                throw e;
            }
            finally
            {
                m_submissionStatus.InProgress = false;
            }
        }

        public async Task Login()
        {
            const string loginReq = "{\"username\":\"test123\",\"password\":\"x\"}";
            Logger.Log($"Request: {loginReq}");
            var responseText =
                await Request(HttpMethod.Post, "authorize/basic", new ByteArrayContent(Encoding.UTF8.GetBytes(loginReq))).Result.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<JObject>(responseText);
            Logger.Log($"Login response: {result}");
            m_jwtBearer = result["bearer"].ToString().Trim();
            Logger.Log($"Got bearer: {m_jwtBearer}");
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
            return res.EnsureSuccessStatusCode();
        }

        public class OrchestraFmApiException : Exception
        {
            public OrchestraFmApiException(string message) : base(message)
            { }
        }
    }
}
