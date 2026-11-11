using Accounting.SchedulerJobs.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace Accounting.SchedulerJobs.Utilities
{
    /// <summary>
    /// Represents a RestExtension.
    /// </summary>
    public static class RestExtension
    {
        private const string EncodingType = "application/json";

        /// <summary>
        /// Sends a GET request asynchronously.
        /// </summary>
        /// <param name="httpClient">httpClient.</param>
        /// <param name="endpoint">endpoint.</param>
        /// <param name="apiKey">apiKey.</param>
        /// <param name="tokenType">tokenType.</param>
        /// <param name="enableSsl">enableSsl.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public static async Task<ApiResponse> GetAsync(this HttpClient httpClient, string endpoint, string apiKey, string tokenType, bool enableSsl = true)
        {
            if (httpClient == null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }
            try
            {
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, endpoint);

                if (!string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(tokenType))
                {
                    httpRequestMessage.Headers.Add(tokenType, apiKey);
                }

                var cancellationToken = new CancellationTokenSource(TimeSpan.FromMinutes(5)).Token;
                HttpResponseMessage response;

                if (!enableSsl)
                {
                    using var handler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };

                    using var sslHttpClient = new HttpClient(handler);
                    response = await sslHttpClient.SendAsync(httpRequestMessage, cancellationToken);
                }
                else
                {
                    response = await httpClient.SendAsync(httpRequestMessage, cancellationToken);
                }

                var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                var responseData = JsonConvert.DeserializeObject<JObject>(responseString);
                return new ApiResponse(response.ReasonPhrase, responseData, (int)response.StatusCode);
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, ex.Message);
            }
        }

        /// <summary>
        /// Sends a GET request asynchronously for PDF content.
        /// </summary>
        /// <param name="httpClient">The HttpClient instance.</param>
        /// <param name="endpoint">The endpoint URL.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="tokenType">tokenType.</param>
        /// <returns>A <see cref="Task{dynamic}"/> representing the result of the asynchronous operation.</returns>
        public static async Task<dynamic> GetAsyncPDF(this HttpClient httpClient, string endpoint, string apiKey, string tokenType)
        {
            if (httpClient == null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }
            try
            {
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"{endpoint}");

                if (!string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(tokenType))
                {
                    httpRequestMessage.Headers.Add(tokenType, apiKey);
                }

                var cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken.None);
                cancellationToken.CancelAfter(TimeSpan.FromMinutes(5));

                HttpResponseMessage response = await httpClient.SendAsync(httpRequestMessage, cancellationToken.Token);
                var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                return response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, ex.Message);
            }
        }

        /// <summary>
        /// Sends a GET request asynchronously and expects a list in the response.
        /// </summary>
        /// <param name="httpClient">The HttpClient instance.</param>
        /// <param name="endpoint">The endpoint URL.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="tokenType">tokenType.</param>
        /// <returns>A <see cref="Task{ApiResponse}"/> representing the result of the asynchronous operation.</returns>
        public static async Task<ApiResponse> GetListAsync(this HttpClient httpClient, string endpoint, string apiKey, string tokenType)
        {
            if (httpClient == null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }
            try
            {
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"{endpoint}");

                if (!string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(tokenType))
                {
                    httpRequestMessage.Headers.Add(tokenType, apiKey);
                }

                var cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken.None);
                cancellationToken.CancelAfter(TimeSpan.FromMinutes(5));

                HttpResponseMessage response = await httpClient.SendAsync(httpRequestMessage, cancellationToken.Token);
                var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                List<JObject> responseData = JsonConvert.DeserializeObject<List<JObject>>(responseString);

                return new ApiResponse(response.ReasonPhrase, responseData, (int)response.StatusCode);
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, ex.Message);
            }
        }
        /// <summary>
        /// Sends a POST request asynchronously.
        /// </summary>
        /// <param name="httpClient">The HttpClient instance.</param>
        /// <param name="endpoint">The endpoint URL.</param>
        /// <param name="data">The data to include in the request.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="tokenType">tokenType.</param>
        /// <returns>A <see cref="Task{ApiResponse}"/> representing the result of the asynchronous operation.</returns>
        public static async Task<ApiResponse> PostAsync(this HttpClient httpClient, string endpoint, object data, string apiKey, string tokenType, bool enableSSL = true)
        {
            if (httpClient == null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }
            try
            {
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, $"{endpoint}");

                if (!string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(tokenType))
                {
                    httpRequestMessage.Headers.Add(tokenType, apiKey);
                }

                if (data != null)
                {
                    string jsonData = JsonConvert.SerializeObject(data);
                    httpRequestMessage.Content = new StringContent(jsonData, Encoding.UTF8, EncodingType);
                }

                var cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken.None);
                cancellationToken.CancelAfter(TimeSpan.FromMinutes(5));

                HttpResponseMessage response;
                if (!enableSSL)
                {
                    using var handler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };

                    using var sslHttpClient = new HttpClient(handler);
                    response = await sslHttpClient.SendAsync(httpRequestMessage, cancellationToken.Token);
                }
                else
                {
                    response = await httpClient.SendAsync(httpRequestMessage, cancellationToken.Token);
                }

                var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = JsonConvert.DeserializeObject<JObject>(responseString);
                    return new ApiResponse(response.ReasonPhrase, responseData, (int)response.StatusCode);
                }
                else
                {
                    var responseData = JsonConvert.DeserializeObject<JObject>(responseString);
                    return new ApiResponse(response.ReasonPhrase, responseData, (int)response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, ex.Message);
            }
        }

        /// <summary>
        /// Sends a PATCH request asynchronously.
        /// </summary>
        /// <param name="httpClient">The HttpClient instance.</param>
        /// <param name="endpoint">The endpoint URL.</param>
        /// <param name="data">The data to include in the request.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="tokenType">tokenType.</param>
        /// <returns>A <see cref="Task{ApiResponse}"/> representing the result of the asynchronous operation.</returns>
        public static async Task<ApiResponse> PatchAsync(this HttpClient httpClient, string endpoint, object data, string apiKey, string tokenType)
        {
            if (httpClient == null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }
            try
            {
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, $"{endpoint}");

                if (!string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(tokenType))
                {
                    httpRequestMessage.Headers.Add(tokenType, apiKey);
                }

                if (data != null)
                {
                    string jsonData = JsonConvert.SerializeObject(data);
                    httpRequestMessage.Content = new StringContent(jsonData, Encoding.UTF8, EncodingType);
                }

                var cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken.None);
                cancellationToken.CancelAfter(TimeSpan.FromMinutes(5));

                HttpResponseMessage response = await httpClient.SendAsync(httpRequestMessage, cancellationToken.Token);
                var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (responseString == "true" || responseString == "false")
                {
                    var responseData = JsonConvert.DeserializeObject<JObject>(responseString);
                    return new ApiResponse(response.ReasonPhrase, responseData, (int)response.StatusCode);
                }
                else
                {
                    var responseData = JsonConvert.DeserializeObject<JObject>(responseString);
                    return new ApiResponse(response.ReasonPhrase, responseData, (int)response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, ex.Message);
            }
        }
        /// <summary>
        /// Sends a GET request list asynchronously.
        /// </summary>
        /// <param name="httpClient">httpClient.</param>
        /// <param name="endpoint">endpoint.</param>
        /// <param name="apiKey">apiKey.</param>
        /// <param name="tokenType">tokenType.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public static async Task<ApiResponse> GetAsyncList(this HttpClient httpClient, string endpoint, string apiKey, string tokenType)
        {
            if (httpClient == null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }
            try
            {
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, endpoint);

                if (!string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(tokenType))
                {
                    httpRequestMessage.Headers.Add(tokenType, apiKey);
                }

                var cancellationToken = new CancellationTokenSource(TimeSpan.FromMinutes(5)).Token;

                HttpResponseMessage response = await httpClient.SendAsync(httpRequestMessage, cancellationToken);
                var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                var responseData = JsonConvert.DeserializeObject<JArray>(responseString);
                return new ApiResponse(response.ReasonPhrase, responseData, (int)response.StatusCode);
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, ex.Message);
            }
        }
    }
}
