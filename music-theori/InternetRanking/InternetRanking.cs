using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

    /// <summary>
    /// Represents the errors occurring during the IR communication process.
    /// </summary>
    public class InternetRankingException : Exception
    {
        public HttpResponseMessage? HttpResponse;
        public bool IsNetworkError => HttpResponse != null;

        public override string Message
            => base.Message + (HttpResponse != null ? $" (HTTP status code {HttpResponse.StatusCode})" : string.Empty);

        public InternetRankingException(string message)
            : base(message)
        { }

        /// <summary>
        /// Create a new instance of this exception when the error is caused by network communications.
        /// </summary>
        public InternetRankingException(string message, HttpResponseMessage httpResponse)
            : base(message)
        {
            HttpResponse = httpResponse;
        }

        /// <summary>
        /// Helper for rethrowing instances of this exception with a new message.
        /// </summary>
        public InternetRankingException(string message, InternetRankingException exception)
            : base(message, exception)
        {
            HttpResponse = exception.HttpResponse;
        }
    }
}
