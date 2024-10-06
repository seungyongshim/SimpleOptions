using System.Xml.Linq;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public class PostFuncConfigureOptions<T>(string? _name, IServiceProvider sp, Func<T, IServiceProvider, T> func) : IPostConfigureOptions<T> where T : class
{
    public void PostConfigure(string? name, T v)
    {
        if (_name == null || name == _name)
        {
            v = func.Invoke(v, sp);
        }
    }
}

