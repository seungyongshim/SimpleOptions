using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.Text;
using FluentValidation;
using System.Diagnostics.CodeAnalysis;

namespace SimpleOptions.Tests;

public class UnitTest1
{
    [Fact]
    public async Task Test1()
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Configuration.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes("""
        {
            "Section1" :
            {
                "Value1" : "Hello",
                "Value4" : ["A", "B"]
            },
            "Section2" :
            {
                "Value2" : "Hello2"
            }
        }
        """)));

        builder.Services.AddSingleton<IValidator<TestOption>, TestOptionValidator>();
        builder.Services.AddSimpleOptions<TestOption>("Section1", (v, sp) => v with
        {
            Value3 = "World"
        }, (v, sp) =>
        {
            var validator = sp.GetRequiredService<IValidator<TestOption>>();
            return validator.Validate(v).IsValid;
        });
        builder.Services.AddSimpleOptions<TestOption>(TestOptionTypes.Section2);

        var app = builder.Build();

        await app.StartAsync();

        var option1 = app.Services.GetKeyedService<TestOption>("Section1");
        var option2 = app.Services.GetKeyedService<TestOption>(TestOptionTypes.Section2);

        await app.StopAsync();
    }
}

public class TestOptionValidator: AbstractValidator<TestOption>
{
    public TestOptionValidator()
    {
        RuleFor(x => x.Value3).NotNull();
    }
}


public record TestOption
{
    public required string Value1 { get; init; }
    [NotNull]
    public string? Value2 { get; init; }
    public string? Value3 { get; init; }
    public IReadOnlyCollection<string> Value4 { get; init; } = [];
}

public enum TestOptionTypes
{
    Section2
}
