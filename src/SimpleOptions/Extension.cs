using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public interface ISimpleOptionsValidation<T>
{
    public void Valid(T v);
}

internal delegate void SimpleOptionValid();
public static class Extension
{
    public static IServiceCollection AddSimpleOptions<T, TValidation>(this IServiceCollection services,
        string key,
        Func<T, IServiceProvider, T>? post = null
    )   where T : class
        where TValidation : ISimpleOptionsValidation<T>
    {
        post ??= (v, _) => v;

        services.AddKeyedSingleton<T>(key, (sp, _) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();

            var v = configuration.GetRequiredSection(key).Get<T>(option =>
            {
                option.BindNonPublicProperties = true;
            });

            return post.Invoke(v ?? throw new ArgumentNullException(key), sp);
        });

        services.AddTransient<SimpleOptionValid>(sp =>
        {
            var v = sp.GetRequiredKeyedService<T>(key);
            var validate = sp.GetRequiredService<TValidation>();
            return () => validate.Valid(v);
        });

        services.AddHostedService<ValidatorHostedService>();

        return services;
    }

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


