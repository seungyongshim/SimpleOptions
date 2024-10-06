using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.Text;
using FluentValidation;

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
                "Value1" : "Hello"
            },
            "Section2" :
            {
                "Value2" : "Hello2"
            }
        }
        """)));

        builder.Services.AddSimpleOptions<Option1>("Section1",
            (v, sp) => v with
            {
                Value3 = "World"
            },
            (v, sp) =>
            {
                var validator = new Option1Validator();
                return validator.Validate(v).IsValid;
            });
        builder.Services.AddSimpleOptions<Option1>("Section2");

        var app = builder.Build();

        await app.StartAsync();

        var option1 = app.Services.GetKeyedService<Option1>("Section1");

        await app.StopAsync();
    }
}

public class Option1Validator: AbstractValidator<Option1>
{
    public Option1Validator()
    {
        RuleFor(x => x.Value3).NotNull();
    }
}


public record Option1
{
    public required string Value1 { get; init; }
    public required string Value2 { get; init; }
    public string? Value3 { get; init; }
}
