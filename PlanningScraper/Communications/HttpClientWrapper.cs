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

        public HttpClientWrapper(HttpMessageHandler handler, ILogger logger) : base(handler)
        {
            _logger = logger;
        }

        public override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            _attempt = 0;
            
            return await Policy
                .Handle<HttpRequestException>()
                .Or<TaskCanceledException>()
                .OrResult<HttpResponseMessage>(x =>
                    !x.IsSuccessStatusCode && x.StatusCode != HttpStatusCode.Accepted &&
                    x.StatusCode != HttpStatusCode.Found)
                .WaitAndRetryAsync(3, retryCount => TimeSpan.FromSeconds(Math.Pow(3, retryCount)))
                .ExecuteAsync(async () =>
                {
                    _attempt++;

                    if (_attempt > 1)
                    {
                        await _logger.LogInformationAsync("Retrying...", cancellationToken);
                        request = request.Clone();
                    }

                    await _logger.LogInformationAsync("Sending request...", cancellationToken);
                    var response = await base.SendAsync(request, cancellationToken);
                    if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Accepted ||
                        response.StatusCode == HttpStatusCode.Found)
                    {
                        await _logger.LogInformationAsync($"Response success with status code {response.StatusCode}",
                            cancellationToken);
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
