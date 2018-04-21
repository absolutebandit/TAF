using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using PlanningScraper.Interfaces;
using Polly;

namespace PlanningScraper.Communications
{
    public class HttpClientWrapper : HttpClient
    {
        private int _attempt = 0;
        private readonly ILogger _logger;
        private readonly ISystemConfig _systemConfig;

        public HttpClientWrapper(string baseAddress, HttpMessageHandler handler, ILogger logger, ISystemConfig systemConfig) : base(handler, false)
        {
            _logger = logger;
            _systemConfig = systemConfig;
            this.BaseAddress = new Uri(baseAddress);
            AddDefaultRequestHeaders();
        }

        private void AddDefaultRequestHeaders()
        {
            this.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
            this.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/65.0.3325.181 Safari/537.36");
            this.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            this.DefaultRequestHeaders.Add("Accept-Language", "en-GB,en;q=0.9,en-US;q=0.8,fr;q=0.7");
        }

        public async Task<HttpResponseMessage> PostAsync(Func<Task<HttpRequestMessage>> requestBuilder, CancellationToken cancellationToken)
        {
            _attempt = 0;

            // add ethical scraping delay
            await Task.Delay(_systemConfig.RequestDelayTimeSpan, cancellationToken);

            // TODO: this should throw after final retry
            // execute send policy
            return await Policy
                .Handle<HttpRequestException>()
                .Or<TaskCanceledException>()
                .OrResult<HttpResponseMessage>(x => !x.IsSuccessStatusCode && x.StatusCode != HttpStatusCode.Accepted && x.StatusCode != HttpStatusCode.Found)
                .WaitAndRetryAsync(3, retryCount => TimeSpan.FromSeconds(Math.Pow(3, retryCount)))
                .ExecuteAsync(async () =>
                {
                    _attempt++;

                    if (_attempt > 1)
                    {
                        await _logger.LogInformationAsync("Retrying...", cancellationToken);
                    }

                    await _logger.LogInformationAsync("Sending request...", cancellationToken);
                    var response = await base.SendAsync(await requestBuilder(), cancellationToken);
                    if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.Found)
                    {
                        await _logger.LogInformationAsync($"Response success with status code {response.StatusCode}", cancellationToken);
                    }
                    else
                    {
                        await _logger.LogInformationAsync(
                            $"Response failed with status code {response.StatusCode}, response content: {await response.Content.ReadAsStringAsync()}",
                            cancellationToken);
                    }

                    return response;
                });
        }

        public new async Task<HttpResponseMessage> GetAsync(string requestPath, CancellationToken cancellationToken)
        {
            _attempt = 0;

            // add ethical scraping delay
            await Task.Delay(_systemConfig.RequestDelayTimeSpan, cancellationToken);

            // TODO: this should throw after final retry
            // execute send policy
            return await Policy
                .Handle<HttpRequestException>()
                .Or<TaskCanceledException>()
                .OrResult<HttpResponseMessage>(x => !x.IsSuccessStatusCode && x.StatusCode != HttpStatusCode.Accepted && x.StatusCode != HttpStatusCode.Found)
                .WaitAndRetryAsync(3, retryCount => TimeSpan.FromSeconds(Math.Pow(3, retryCount)))
                .ExecuteAsync(async () =>
                {
                    _attempt++;

                    if (_attempt > 1)
                    {
                        await _logger.LogInformationAsync("Retrying...", cancellationToken);
                    }

                    await _logger.LogInformationAsync("Sending request...", cancellationToken);
                    var response = await base.GetAsync(requestPath, cancellationToken);
                    if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.Found)
                    {
                        await _logger.LogInformationAsync($"Response success with status code {response.StatusCode}", cancellationToken);
                    }
                    else
                    {
                        await _logger.LogInformationAsync(
                            $"Response failed with status code {response.StatusCode}, response content: {await response.Content.ReadAsStringAsync()}",
                            cancellationToken);
                    }

                    return response;
                });
        }
    }
}
