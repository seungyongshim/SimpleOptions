using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using SimpleOptions;

namespace Microsoft.Extensions.DependencyInjection;

public static class ExtensionMethods
{
    public static SimpleOptionsBuilder<T> AddSimpleOptions<T>
    (
        this IServiceCollection services,
        Enum enumkey,
        Func<T, IServiceProvider, T>? post = null
    ) where T : class
    {
        var key = Enum.GetName(enumkey.GetType(), enumkey) ?? throw new NullReferenceException();
        services.TryAddKeyedSingleton(enumkey, (sp, _) =>
        {
            var v = sp.GetRequiredService<IOptionsMonitor<T>>().Get(key);
            return v;
        });

        return services.AddSimpleOptions(key, post);
    }

    public static SimpleOptionsBuilder<T> AddSimpleOptions<T>
    (
        this IServiceCollection services,
        string key,
        Func<T, IServiceProvider, T>? post = null
    ) where T : class
    {
        post ??= (v, _) => v;

        services.TryAddTransient<IOptionsFactory<T>, KeyedOptionsFactory<T>>();
        services.AddKeyedSingleton<FuncPostConfigure<T>>(key, (sp, key) => v => post.Invoke(v, sp));
        services.TryAddKeyedTransient(typeof(IValidateOptions<>), key, typeof(FluentValidateOptions<>));

        services.AddOptions<T>(key)
                .BindConfiguration(key, option =>
                {
                    option.BindNonPublicProperties = true;
                })
                .ValidateDataAnnotations()
                .ValidateOnStart();
        
        services.TryAddKeyedSingleton(key, (sp, _) =>
        {
            var v = sp.GetRequiredService<IOptionsMonitor<T>>().Get(key);
            return v;
        });

        return new SimpleOptionsBuilder<T>
        {
            Key = key,
            Services = services
        };
    }
}
