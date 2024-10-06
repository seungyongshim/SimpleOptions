using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class ExtensionMethods
{
    public static IServiceCollection AddSimpleOptions<T>(this IServiceCollection          services,
        string key,
        Func<T, IServiceProvider, T>? post = null,
        Func<T, IServiceProvider, bool> validate = null
        ) where T : class
    {
        post ??= (v, _) => v;
        validate ??= (_, _) => true;

        services.AddOptions<T>(key)
                .BindConfiguration(key, option =>
                {
                    option.BindNonPublicProperties = true;
                })
                .ValidateDataAnnotations()
                .Validate((T v, IServiceProvider sp) =>
                {
                    v = post.Invoke(v, sp);
                    return validate.Invoke(v, sp);
                })
                .ValidateOnStart();
        
        services.TryAddKeyedSingleton<T>(key, (sp, _) =>
        {
            var v = sp.GetRequiredService<IOptionsMonitor<T>>().Get(key);
            v = post.Invoke(v, sp);
            return v;
        });

        return services;
    }
}

