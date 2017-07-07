// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RestSharpUtils.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Extensions of Net Core to return RestSharp functions
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Tests
{
#if CORECLR
    using System;
    using System.Threading.Tasks;

    using RestSharp;
#endif

    /// <summary>
    /// Extensions of Net Core to return RestSharp functions
    /// </summary>
    public static class RestSharpUtils
    {
#if CORECLR

        /// <summary>
        /// Provides the async version for <see cref="RestClient.ExecuteAsync"/>
        /// </summary>
        /// <typeparam name="T">The expected type of the result</typeparam>
        /// <param name="client">The configured client</param>
        /// <param name="request">The request</param>
        /// <returns>The response</returns>
        public static async Task<IRestResponse<T>> ExecuteTaskAsync<T>(this RestClient client, RestRequest request) where T : new()
        {
            var completionSource = new TaskCompletionSource<IRestResponse<T>>();
            Action<IRestResponse<T>> callback = response => completionSource.SetResult(response);
            client.ExecuteAsync(request, callback);
            return await completionSource.Task;
        }

        /// <summary>
        /// Provides the async version for <see cref="RestClient.ExecuteAsync"/>
        /// </summary>
        /// <param name="client">The configured client</param>
        /// <param name="request">The request</param>
        /// <returns>The response</returns>
        public static async Task<IRestResponse> ExecuteTaskAsync(this RestClient client, RestRequest request)
        {
            var completionSource = new TaskCompletionSource<IRestResponse>();
            Action<IRestResponse> callback = response => completionSource.SetResult(response);
            client.ExecuteAsync(request, callback);
            return await completionSource.Task;
        }
#endif
    }
}
