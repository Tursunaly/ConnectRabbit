using Consumer.Api.RabbitMq;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<RabbitMqConsumerService>();

builder.Services.AddSingleton<RabbitMqConsumerService>(provider => 
    provider.GetServices<IHostedService>()
            .OfType<RabbitMqConsumerService>()
            .First());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();