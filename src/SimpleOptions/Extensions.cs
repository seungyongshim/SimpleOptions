using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using SimpleOptions;

namespace Microsoft.Extensions.DependencyInjection;

public static class ExtensionMethods
{
    public static IServiceCollection AddSimpleOptions<T>
    (
        this IServiceCollection services,
        Enum enumkey,
        Func<T, IServiceProvider, T>? post = null,
        Func<T, IServiceProvider, bool>? validate = null
    ) where T : class
    {
        var key = Enum.GetName(enumkey.GetType(), enumkey) ?? throw new NullReferenceException();
        services.AddSimpleOptions(key, post, validate);
        services.TryAddKeyedSingleton(enumkey, (sp, _) =>
        {
            var v = sp.GetRequiredService<IOptionsMonitor<T>>().Get(key);
            return v;
        });
        return services;
    }

    public static IServiceCollection AddSimpleOptions<T>
    (
        this IServiceCollection services,
        string key,
        Func<T, IServiceProvider, T>? post = null,
        Func<T, IServiceProvider, bool>? validate = null
    ) where T : class
    {
        post ??= (v, _) => v;
        validate ??= (_, _) => true;

        services.TryAddTransient<IOptionsFactory<T>, KeyedOptionsFactory<T>>();
        services.AddKeyedSingleton<FuncPostConfigure<T>>(key, (sp, key) => v => post.Invoke(v, sp));
        services.AddTransient<IValidateOptions<T>, ValidateOptions<T>>(sp =>
        {
            return new ValidateOptions<T>(key, v => validate(v, sp), $"{key}({typeof(T)}) failed");
        });

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

        return services;
    }
}

