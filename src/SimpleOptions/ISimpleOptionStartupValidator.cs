namespace Microsoft.Extensions.DependencyInjection;

internal interface ISimpleOptionStartupValidator
{
    Task ValidateAsync();
}
