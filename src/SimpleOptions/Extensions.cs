using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

internal delegate void SimpleOptionValid();
public static class ExtensionMethods
{
    public static IServiceCollection AddSimpleOptions<T>(this IServiceCollection services,
        string key,
        Func<T, IServiceProvider, T>? post = null,
        Action<T, IServiceProvider> validate = null
        ) where T : class
    {
        post ??= (v, _) => v;
        validate ??= (_, _) => { };

        services.TryAddKeyedSingleton<T>(key, (sp, _) =>
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


