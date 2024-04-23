namespace SImpleOptions.FluentValidation;
using UserId = string;
using ProjectId = string;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using global::FluentValidation;

public static class Extension
{
    public static IServiceCollection AddSimpleOptions<T>(this IServiceCollection services,
       string key,
       Func<T, IServiceProvider, T>? post = null
   ) where T : class
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
            var validate = sp.GetRequiredService<IValidator<T>>();
            return () => validate.Validate(v);
        });

        services.AddHostedService<ValidatorHostedService>();

        return services;
    }
}
        
        
