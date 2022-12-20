using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ConfigurationService.Hosting
{
    public class HostedConfigurationService : IHostedService, IDisposable
    {
        private readonly ILogger<HostedConfigurationService> _logger;

        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly IConfigurationService _configurationService;

        private Task _executingTask;
        private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();
        private bool _disposed;

        public HostedConfigurationService(ILogger<HostedConfigurationService> logger, IHostApplicationLifetime applicationLifetime, IConfigurationService configurationService)
        {
            _logger = logger;
            _applicationLifetime = applicationLifetime;
            _configurationService = configurationService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Configuration Service");

            _applicationLifetime.ApplicationStarted.Register(OnStarted);
            _applicationLifetime.ApplicationStopping.Register(OnStopping);
            _applicationLifetime.ApplicationStopped.Register(OnStopped);

            _executingTask = ExecuteAsync(_stoppingCts.Token);

            if (_executingTask.IsCompleted)
            {
                return _executingTask;
            }

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_executingTask == null)
            {
                return;
            }

            try
            {
                // Signal cancellation to the executing method
                _stoppingCts.Cancel();
            }
            finally
            {
                // Wait until the task completes or the stop token triggers
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
            }
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await _configurationService.Initialize(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred while attempting to initialize the configuration provider");

                _logger.LogInformation("The application will be terminated");

                await StopAsync(stoppingToken);
                _applicationLifetime.StopApplication();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _stoppingCts.Cancel();
            }

            _disposed = true;
        }

        private void OnStarted()
        {
            _logger.LogInformation("Configuration Service started");
        }

        private void OnStopping()
        {
            _logger.LogInformation("Configuration Service is stopping...");
        }

        private void OnStopped()
        {
            _logger.LogInformation("Configuration Service stopped");
        }
    }
}