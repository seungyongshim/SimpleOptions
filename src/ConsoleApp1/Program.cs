using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSimpleOptions<SimpleImmutableOption>("MicroService1", true);
builder.Services.AddSingleton<IValidator<SimpleImmutableOption>, SimpleImmutableOptionValidator>();

var app = builder.Build();

app.Run();
