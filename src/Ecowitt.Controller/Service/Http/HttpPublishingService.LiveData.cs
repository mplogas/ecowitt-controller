using System.Text.Json;
using Ecowitt.Controller.Model.Message.Config;
using Ecowitt.Controller.Model.Message.Data;
using SlimMessageBus;

namespace Ecowitt.Controller.Service.Http
{
    public partial class HttpPublishingService
    {
        private async Task LiveDataPollingLoop(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting LiveData polling loop");
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(Math.Clamp(_config.LiveDataInterval, 5, 600)));
            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    foreach (var host in _config.LiveDataHosts)
                    {
                        try
                        {
                            var data = await GetLiveData(host, stoppingToken);
                            if (data == null)
                            {
                                _logger.LogWarning("No livedata received from {HostBaseUrl}", host.BaseUrl);
                                continue;
                            }
                            await _messageBus.Publish(data, cancellationToken: stoppingToken);
                        }
                        catch (Exception e) when (e is not OperationCanceledException)
                        {
                            _logger.LogError(e, "Failed to get livedata from {HostBaseUrl}", host.BaseUrl);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Stopping LiveData polling loop");
            }
        }

        private async Task<GatewayLiveData?> GetLiveData(HttpHost host, CancellationToken cancellationToken)
        {
            var sem = _gatewaySemaphores.GetOrAdd(host.Host, static _ => new SemaphoreSlim(1, 1));
            var acquired = false;
            try
            {
                await sem.WaitAsync(cancellationToken);
                acquired = true;
                using var client = CreateHttpClient(host);
                var response = await client.GetAsync("get_livedata_info", cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("livedata endpoint returned {StatusCode} on {Host}", response.StatusCode, host.Host);
                    return null;
                }
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                if (string.IsNullOrWhiteSpace(json))
                {
                    _logger.LogWarning("livedata endpoint returned empty body on {Host}", host.Host);
                    return null;
                }
                var data = JsonSerializer.Deserialize<GatewayLiveData>(json);
                if (data == null)
                {
                    _logger.LogWarning("livedata response deserialized to null on {Host}", host.Host);
                    return null;
                }
                data.IpAddress = host.Host;
                data.TimestampUtc = DateTime.UtcNow;
                return data;
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                _logger.LogError(e, "Exception while fetching livedata from {Host}", host.Host);
                return null;
            }
            finally
            {
                if (acquired) sem.Release();
            }
        }
    }
}
