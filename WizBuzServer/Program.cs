using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}

app.UseHttpsRedirection();

app.MapGet("/getMessageById", async Task<MessageDto?> ([FromQuery] string to, [FromQuery] string messageId, [FromQuery] bool? remove, [FromServices] IMemoryCache cache)
    =>
{
    List<MessageDto>? messages;


    if (cache.TryGetValue(to, out messages))
    {
        for (var i = 0; i < messages!.Count; i++)
        {
            if (messages[i].Id == messageId)
            {
                var foundMessage = messages[i];
                //Defult to purge
                if (remove == null || remove == true)
                {
                    messages.Remove(foundMessage);
                }
                return foundMessage;
            }
        }
    }
    return null;
});

app.MapPut("/setMessage", ([FromBody] MessageDto message, [FromServices] IMemoryCache cache) =>
{
    message.Id = Guid.NewGuid().ToString();

    List<MessageDto>? messages;
    bool messageQueueExists = cache.TryGetValue(message.To!, out messages);

    if (false == messageQueueExists)
    {
        messages = new List<MessageDto>();
    }

    messages!.Add(message);
    cache.Set(message.To!, messages, DateTime.Now.AddMinutes(10));
    return message.Id;
});
app.Run();
