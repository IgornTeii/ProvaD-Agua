using ProvaDagua.Application.FloodEvents;

namespace ProvaDagua.Api.Workers;

public sealed class FloodEventEvaluationWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<FloodEventEvaluationWorker> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        do
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();

                var evaluator = scope.ServiceProvider.GetRequiredService<EvaluateSuspectedEventsHandler>();
                var decided = await evaluator.HandleAsync(stoppingToken);
                if (decided > 0)
                    logger.LogInformation("{Count} evento(s) suspeito(s) decidido(s) pela IA.", decided);

                var closer = scope.ServiceProvider.GetRequiredService<CloseRecededEventsHandler>();
                var closed = await closer.HandleAsync(stoppingToken);
                if (closed > 0)
                    logger.LogInformation("{Count} alagamento(s) encerrado(s), com comprovações emitidas.", closed);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Falha ao processar os eventos de alagamento.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}