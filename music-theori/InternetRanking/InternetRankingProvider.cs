using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using theori.Charting;
using theori.Database;
using theori.Judgement;

namespace theori.InternetRanking
{
    internal abstract class InternetRankingProvider
    {
        protected HttpClient m_http;
        protected InternetRankingService.InternetRankingSubmissionStatus m_submissionStatus;

        public InternetRankingService.InternetRankingSubmissionStatus SubmissionStatus => m_submissionStatus;

        protected InternetRankingProvider(HttpClient http)
        {
            m_http = http;
            m_submissionStatus = new InternetRankingService.InternetRankingSubmissionStatus();

            m_http.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue {NoCache = true};
        }

        public abstract Task SubmitScore(ChartInfo chart, Dictionary<string, dynamic> scoreData);
    }
}
