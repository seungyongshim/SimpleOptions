using System.Runtime.ExceptionServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

internal class ValidatorHostedService(IEnumerable<SimpleOptionValid> validators) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        List<Exception> exceptions = [];

        foreach (var validator in validators)
        {
            try
            {
                validator();
            }
            catch (OptionsValidationException ex)
            {
                exceptions.Add(ex);
            }
        }

        if (exceptions.Any())
        {
            if (exceptions.Count == 1)
            {
                // Rethrow if it's a single error
                ExceptionDispatchInfo.Capture(exceptions[0]).Throw();
            }

            if (exceptions.Count > 1)
            {
                // Aggregate if we have many errors
                throw new AggregateException(exceptions);
            }
        }

        return Task.CompletedTask;
    }
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    
}
