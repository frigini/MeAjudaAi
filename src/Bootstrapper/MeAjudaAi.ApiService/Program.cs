using MeAjudaAi.ApiService.Extensions;
using MeAjudaAi.Modules.Users.API;
using MeAjudaAi.Shared.Extensions;
using MeAjudaAi.ServiceDefaults;
using MeAjudaAi.Shared.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddSharedServices(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddUsersModule(builder.Configuration);

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseSharedServices();
app.UseApiServices(app.Environment);
app.UseUsersModule();
app.MapAllEndpoints();

app.Run();