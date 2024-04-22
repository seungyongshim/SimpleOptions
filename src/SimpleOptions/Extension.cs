using System.Runtime.ExceptionServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class Extension
{
    public static IServiceCollection AddSimpleOptions<T>(this IServiceCollection services,
    string key,
        Func<T, IServiceProvider, T>? post = null,
        Action<T, IServiceProvider> validate = null
        ) where T : class
    {
        post ??= (v, _) => v;
        validate ??= (_, _) => { };



        services.AddKeyedSingleton<T>(key, (sp, _) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();

            var v = configuration.GetSection(key).Get<T>(option =>
            {
                option.BindNonPublicProperties = true;
            });

            return post.Invoke(v, sp);
        });

        services.AddTransient<SimpleOptionValid>(sp =>
        {
            var v = sp.GetRequiredKeyedService<T>(key);
            return () => validate.Invoke(v, sp);
        });

        services.AddHostedService<ValidatorHostedService>();

        return services;
    }


}

file delegate void SimpleOptionValid();

file interface ISimpleOptionStartupValidator
{
    Task ValidateAsync();
}

file class ValidatorHostedService(IEnumerable<SimpleOptionValid> validators) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        List<Exception> exceptions = [];

        foreach (var validator in validators)
        {
            try
            {
                // Execute the validation method and catch the validation error
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
