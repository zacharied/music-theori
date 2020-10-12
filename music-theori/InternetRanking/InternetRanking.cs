using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace theori.InternetRanking
{
    public enum InternetRankingProviderKind
    {
        OrchestraFm
    }

    public interface IInternetRankingScoreAdapter
    {
        public Dictionary<string, dynamic> AdaptScore(object scoreObject);
    }
}
