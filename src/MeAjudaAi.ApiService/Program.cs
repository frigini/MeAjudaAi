using MeAjudaAi.ApiService.Extensions;
using MeAjudaAi.Shared.Extensions;
using MeAjudaAi.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddSharedServices(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

app.UseSharedServices();
app.UseApiServices(app.Environment);
app.MapAllEndpoints();

app.Run();