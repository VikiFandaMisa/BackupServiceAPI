using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;


namespace BackupServiceAPI.Services {
    public abstract class BackgroundService : IHostedService, IDisposable {
        private Task _ExecutingTask;
        private readonly CancellationTokenSource _StoppingCts =
                                                       new CancellationTokenSource();

        protected abstract Task ExecuteAsync(CancellationToken stoppingToken);

        public virtual Task StartAsync(CancellationToken cancellationToken) {
            // Store the task we're executing
            _ExecutingTask = ExecuteAsync(_StoppingCts.Token);

            // If the task is completed then return it,
            // this will bubble cancellation and failure to the caller
            if (_ExecutingTask.IsCompleted) {
                return _ExecutingTask;
            }

            // Otherwise it's running
            return Task.CompletedTask;
        }

        public virtual async Task StopAsync(CancellationToken cancellationToken) {
            // Stop called without start
            if (_ExecutingTask == null) {
                return;
            }

            try {
                // Signal cancellation to the executing method
                _StoppingCts.Cancel();
            }
            finally {
                // Wait until the task completes or the stop token triggers
                await Task.WhenAny(_ExecutingTask, Task.Delay(Timeout.Infinite,
                                                              cancellationToken));
            }
        }

        public virtual void Dispose() {
            _StoppingCts.Cancel();
        }
    }
}
