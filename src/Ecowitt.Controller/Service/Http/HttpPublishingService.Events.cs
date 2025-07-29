using Ecowitt.Controller.Model.Message.Data;
using Ecowitt.Controller.Model.Message.Event;

namespace Ecowitt.Controller.Service.Http
{
    public partial class HttpPublishingService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting SubdeviceService");

            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_config.PollingInterval));
            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    foreach (var host in _config.Hosts)
                    {
                        try
                        {
                            var data = await GetSubdeviceData(host, stoppingToken);
                            if(data.Count == 0)
                            {
                                _logger.LogWarning($"No subdevice data received from {host.BaseUrl}");
                            }

                            var aggregate = new SubdeviceApiAggregate();
                            aggregate.Subdevices.AddRange(data);
                            await _messageBus.Publish(aggregate,cancellationToken: stoppingToken);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError($"Failed to get subdevicedata for {host.BaseUrl}", e);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Stopping SubdeviceService");
            }
        }

        public async Task StartedAsync(CancellationToken cancellationToken)
        {
            await _messageBus.Publish(new HttpServiceEvent { EventType = HttpServiceEventType.Started }, cancellationToken: cancellationToken);
        }

        public Task StartingAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task StoppedAsync(CancellationToken cancellationToken)
        {
            await _messageBus.Publish(new HttpServiceEvent { EventType = HttpServiceEventType.Stopped }, cancellationToken: cancellationToken);
        }

        public Task StoppingAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
