using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using theori.Database;
using theori.Scoring;

namespace theori.InternetRanking
{
    public abstract class InternetRankingService
    {
        private HttpClient m_client;

        public InternetRankingService(HttpClient client)
        {
            m_client = client;
        }

        public void SubmitScore(string token, Dictionary<string, object> values)
        {
        }
    }
}
