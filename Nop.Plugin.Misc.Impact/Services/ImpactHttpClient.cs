using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Logging;
using Nop.Services.Logging;

namespace Nop.Plugin.Misc.Impact.Services
{
    /// <summary>
    /// Represents HTTP client to request third-party services
    /// </summary>
    public class ImpactHttpClient
    {
        #region Fields

        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly ImpactSettings _impactSettings;

        #endregion

        #region Ctor

        public ImpactHttpClient(HttpClient httpClient,
            ILogger logger,
            ImpactSettings impactSettings)
        {
            httpClient.Timeout = TimeSpan.FromSeconds(impactSettings.RequestTimeout ?? ImpactDefaults.RequestTimeout);
            httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, ImpactDefaults.UserAgent);

            _httpClient = httpClient;
            _logger = logger;
            _impactSettings = impactSettings;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare the impact WebApi URL and set credential
        /// </summary>
        /// <param name="apiUrl">The API call</param>
        /// <returns>Full WebApi URL to action</returns>
        private string PrepareHttpClient(string apiUrl)
        {
            var url = $"{ImpactDefaults.ApiUrl}{_impactSettings.AccountSId}/{apiUrl}";

            var base64EncodedAuthenticationString =
                Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_impactSettings.AccountSId}:{_impactSettings.AuthToken}"));
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);

            return url;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Send request to impact API
        /// </summary>
        /// <param name="apiUrl">API URL</param>
        /// <param name="method">HTTP method</param>
        /// <param name="data">Data to send</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task SendRequestAsync(string apiUrl, HttpMethod method, Dictionary<string, string> data)
        {
            try
            {
                var requestData = JsonConvert.SerializeObject(data);

                if (_impactSettings.LogRequests)
                    await _logger.InsertLogAsync(LogLevel.Debug, $"{ImpactDefaults.SystemName} request details", requestData);

                var requestContent = new StringContent(requestData, Encoding.UTF8, MimeTypes.ApplicationJson);
                var url = PrepareHttpClient(apiUrl);
                var request = new HttpRequestMessage(method, new Uri(url))
                {
                    Content = requestContent
                };

                var response = await _httpClient.SendAsync(request);

                if (_impactSettings.LogRequests)
                {
                    var responseData = !response.IsSuccessStatusCode
                        ? $"{response.StatusCode}: {response.RequestMessage?.ToString()}"
                        : await response.Content.ReadAsStringAsync();
                    await _logger.InsertLogAsync(LogLevel.Debug, $"{ImpactDefaults.SystemName} response details", responseData);
                }
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync($"{ImpactDefaults.SystemName} error", e);
            }
        }

        #endregion
    }
}