using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using theori.Charting;

namespace theori.InternetRanking
{
    public class InternetRankingService : IDisposable
    {
        #region Singleton

        private static InternetRankingService? m_service = null;
        public static InternetRankingService Service 
            => m_service ??  throw new InvalidOperationException("the internet ranking service has not been initialized");

        public static bool IsInitialized => m_service != null;

        public static void Initialize()
        {
            // TODO(neko) Load providerKind from config
            m_service = new InternetRankingService(InternetRankingProviderKind.OrchestraFm);
        }

        public static InternetRankingSubmissionStatus SubmissionStatus => Service.m_provider.SubmissionStatus;

        #endregion

        private readonly HttpClient m_http;
        private readonly InternetRankingProvider m_provider;
        private readonly InternetRankingProviderKind m_providerKind;

        private Task? m_submissionTask;

        public readonly Dictionary<InternetRankingProviderKind, IInternetRankingScoreAdapter> Adapters;

        private InternetRankingService(InternetRankingProviderKind providerKind)
        {
            // TODO(neko) Recycle this HttpClient
            m_http = new HttpClient();

            m_providerKind = providerKind;

            m_provider = m_providerKind switch
            {
                InternetRankingProviderKind.OrchestraFm => new OrchestraFmIr(m_http),
                _ => m_provider
            };

            Adapters = new Dictionary<InternetRankingProviderKind, IInternetRankingScoreAdapter>();
        }

        public void SubmitScore(ChartInfo chart, object scoreData)
        {
            var scoreObject = Adapters[m_providerKind].AdaptScore(scoreData);
            m_submissionTask = Task.Run(() => m_provider.SubmitScore(chart, scoreObject));
        }
        
        public void Update()
        {
            if (m_submissionTask != null && m_submissionTask.IsCompleted)
                m_submissionTask = null;
        }

        public void Dispose()
        {
            m_http.Dispose(); 
        }

        public struct InternetRankingSubmissionStatus
        {
            public DateTime LastUpdate { get; internal set; }
            public bool InProgress { get; internal set; }
            public ChartInfo? Chart { get; internal set; }
            public dynamic Score { get; internal set; }
            public Exception? Error { get; internal set; }
        }
    }
}
