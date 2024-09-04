

using Product.Event.Handler.Service.Services;
using Shared.Services;
using Shared.Services.Abstractions;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<EventStoreBackgorundService>();

builder.Services.AddSingleton<IEventStoreService, EventStoreService>();
builder.Services.AddSingleton<IMongoDBService, MongoDBService>();

var host = builder.Build();
host.Run();
