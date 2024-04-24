using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSimpleOptions<SimpleImmutableOption>("microservice1", true);
builder.Services.AddSimpleOptions<SimpleImmutableOption>("microservice1:ExternalService1", true);
builder.Services.AddSingleton<IValidator<SimpleImmutableOption>, SimpleImmutableOptionValidator>();

var app = builder.Build();

await app.StartAsync();

Console.WriteLine(app.Services.GetRequiredKeyedService<SimpleImmutableOption>("microservice1"));
Console.WriteLine(app.Services.GetRequiredKeyedService<SimpleImmutableOption>("microservice1:ExternalService1"));


await app.StopAsync();
