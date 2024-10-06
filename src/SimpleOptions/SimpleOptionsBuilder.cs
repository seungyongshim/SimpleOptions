using FluentValidation;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public class SimpleOptionsBuilder<T>
{
    public required string Key { get; init; }
    public required IServiceCollection Services { get; init; }

    public SimpleOptionsBuilder<T> AddFluentValidator<TValidator>
    (
        ServiceLifetime lifetime = ServiceLifetime.Singleton
    ) where TValidator : class, IValidator<T>
    {
        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                Services.AddKeyedSingleton<IValidator<T>, TValidator>(Key);
                break;
            case ServiceLifetime.Scoped:
                Services.AddKeyedScoped<IValidator<T>, TValidator>(Key);
                break;
            case ServiceLifetime.Transient:
                Services.AddKeyedTransient<IValidator<T>, TValidator>(Key);
                break;
        }
        
        return this;
    }
}
