using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.Text;

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
            }
        }
        """)));

        builder.Services.AddSimpleOptions<Option1>("Section1");

        var app = builder.Build();

        await app.StartAsync();

        var option1 = app.Services.GetKeyedService<Option1>("Section1");

        await app.StopAsync();
    }
}


public record Option1
{
    public string Value1 { get; init; }
    public string Value2 { get; init; }
    public string Value3 { get; init; }
}
