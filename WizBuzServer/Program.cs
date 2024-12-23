using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

using System.Reflection.Metadata.Ecma335;

using WizBuzModels;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<WizCache>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}

app.UseHttpsRedirection();

app.MapGet("/getAllMessagesFromQueue/{queueName}", async Task<MessageDto[]> (string queueName, [FromServices] IMemoryCache cache, [FromQuery] bool? remove) =>
    {
        List<MessageDto> messages;
        bool queueExists = cache.TryGetValue(queueName, out messages);
        if (queueExists)
        {
            if (remove == null || remove == true)
            {
                cache.Remove(queueName);
                var listOfQueues = cache.GetOrCreate<List<string>>(WizBuzModels.WizBusConstants.TO_ENTRIES_KEY, TO_ENTRIES => new List<string>());
                listOfQueues!.Remove(queueName);
                cache.Set(WizBuzModels.WizBusConstants.TO_ENTRIES_KEY, listOfQueues);
            }

                return messages.ToArray() ;
        }
        return Array.Empty<MessageDto>();
    });

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
                    if (messages.Count == 0) 
                    {
                        var listOfQueues = cache.GetOrCreate<List<string>>(WizBuzModels.WizBusConstants.TO_ENTRIES_KEY, TO_ENTRIES => new List<string>());
                        listOfQueues!.Remove(to);
                        cache.Set(WizBuzModels.WizBusConstants.TO_ENTRIES_KEY, listOfQueues);
                    }
                }
                return foundMessage;
            }
        }
    }
    return null;
});


app.MapGet("/getQueues", ([FromServices] IMemoryCache cache) =>
{
    var listOfToEntries = cache.GetOrCreate<List<string>>(WizBuzModels.WizBusConstants.TO_ENTRIES_KEY, TO_ENTRIES => new List<string>());
    return listOfToEntries;
});

app.MapPut("/setMessage", ([FromBody] MessageDto message, [FromServices] IMemoryCache cache) =>
{
    message.Id = Guid.NewGuid().ToString();

    List<MessageDto>? messages;
    bool messageQueueExists = cache.TryGetValue(message.To!, out messages);
    if (false == messageQueueExists)
    {
        messages = new List<MessageDto>();
        
        //Update the TO listOfToEntries
        var listOfToEntries = cache.GetOrCreate<List<string>>(WizBuzModels.WizBusConstants.TO_ENTRIES_KEY, TO_ENTRIES => new List<string>());
        listOfToEntries!.Add(message.To!);
        cache.Set(WizBuzModels.WizBusConstants.TO_ENTRIES_KEY, listOfToEntries);
    }

    messages!.Add(message);
    cache.Set(message.To!, messages, DateTime.Now.AddMinutes(10));
    return message.Id;
});

app.MapPut("/registerListener", string ([FromBody] SetListenerDto listener, [FromServices] WizCache cache) => 
{
    cache.AddListener(listener);
    return null;
});



app.Run();
