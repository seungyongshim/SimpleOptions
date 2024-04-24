using Microsoft.Extensions.Configuration;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class ExtensionMethods
{
    public static IServiceCollection AddSimpleOptions<T>(this IServiceCollection services,
       string key,
       bool useFluentValidation,
       Func<T, IServiceProvider, T>? post = null) where T : class
    {
        post ??= (v, _) => v;

        services.TryAddKeyedSingleton<T>(key, (sp, _) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();

            var v = configuration.GetRequiredSection(key).Get<T>(option =>
            {
                option.BindNonPublicProperties = true;
            });

            return post.Invoke(v ?? throw new ArgumentNullException(key), sp);
        });

        if (useFluentValidation)
        {
            services.AddTransient<SimpleOptionValid>(sp =>
            {
                var v = sp.GetRequiredKeyedService<T>(key);
                var validate = sp.GetRequiredService<IValidator<T>>();
                return () => validate.ValidateAndThrow(v);
            });

            services.AddHostedService<ValidatorHostedService>();
        }
        return services;
    }
}
        
        
