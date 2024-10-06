using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace SimpleOptions;

internal delegate T FuncPostConfigure<T>(T options);

internal class KeyedOptionsFactory<TOptions>
(
    IServiceProvider sp
) : IOptionsFactory<TOptions> where TOptions : class
{
    public TOptions Create(string name)
    {
        var setups = sp.GetRequiredService<IEnumerable<IConfigureOptions<TOptions>>>();
        var postConfigures = sp.GetRequiredKeyedService<IEnumerable<FuncPostConfigure<TOptions>>>(name);
        var validations = sp.GetKeyedService<IEnumerable<IValidateOptions<TOptions>>>(name) ?? [];
        var options = CreateInstance(name);

        foreach (var setup in setups)
        {
            if (setup is IConfigureNamedOptions<TOptions> namedSetup)
            {
                namedSetup.Configure(name, options);
            }
            else if (name == Options.DefaultName)
            {
                setup.Configure(options);
            }
        }

        options = postConfigures.Aggregate(options, (current, post) => post(current));

        if (validations.Any())
        {
            var failures = new List<string>();
            foreach (var validate in validations)
            {
                var result = validate.Validate(name, options);
                if (result is not null && result.Failed)
                {
                    failures.AddRange(result.Failures);
                }
            }
            if (failures.Count > 0)
            {
                throw new OptionsValidationException(name, typeof(TOptions), failures);
            }
        }

        return options;
    }

    protected virtual TOptions CreateInstance(string name) => Activator.CreateInstance<TOptions>();
}
