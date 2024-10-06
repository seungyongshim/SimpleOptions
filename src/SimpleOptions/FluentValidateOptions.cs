using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace SimpleOptions;

public class FluentValidateOptions<T>
(
    [ServiceKey]string key,
    IServiceProvider sp
) : IValidateOptions<T> where T : class
{
    public ValidateOptionsResult Validate(string? name, T options)
    {
        if(key != name)
        {
            return ValidateOptionsResult.Skip;
        }

        var validator = sp.GetKeyedService<IValidator<T>>(key);

        var ret = validator?.Validate(options);

        return ret switch
        {
            { IsValid: true } => ValidateOptionsResult.Success,
            null => ValidateOptionsResult.Skip,
            _ => ValidateOptionsResult.Fail(ret.Errors.Select(x => x.ErrorMessage))
        };
    }
}
