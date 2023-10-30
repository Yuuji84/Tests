using System;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

List<Service> services = new List<Service>
{
    new() { Id = Guid.NewGuid().ToString(), Name = "Music", State = "Stable", Description = "Music Streaming Service" },
    new() { Id = Guid.NewGuid().ToString(), Name = "Video", State = "Non-Stable", Description = "Video Streaming Service" },
    new() { Id = Guid.NewGuid().ToString(), Name = "Search", State = "Don't Work", Description = "Search Service" }
};

var builder = WebApplication.CreateBuilder();
var app = builder.Build();

app.Run(async (context) =>
{
    var response = context.Response;
    var request = context.Request;
    var path = request.Path;
    
    string expressionForGuid = @"^/api/services/\w{8}-\w{4}-\w{4}-\w{4}-\w{12}$";
    if (path == "/api/services" && request.Method == "GET")
    {
        await GetAllServices(response);
    }
    else if (Regex.IsMatch(path, expressionForGuid) && request.Method == "GET")
    {
        string? id = path.Value?.Split("/")[3];
        await GetService(id, response);
    }
    else if (path == "/api/services" && request.Method == "POST")
    {
        await CreateService(response, request);
    }
    else if (path == "/api/services" && request.Method == "PUT")
    {
        await UpdateService(response, request);
    }
    else if (Regex.IsMatch(path, expressionForGuid) && request.Method == "DELETE")
    {
        string? id = path.Value?.Split("/")[3];
        await DeleteService(id, response);
    }
    else
    {
        response.ContentType = "text/html; charset=utf-8";
        await response.SendFileAsync("html/index.html");
    }
});

app.Run();

async Task GetAllServices(HttpResponse response)
{
    await response.WriteAsJsonAsync(services);
}
async Task GetService(string? id, HttpResponse response)
{
    Service? service = services.FirstOrDefault((u) => u.Id == id);
    
    if (service != null)
        await response.WriteAsJsonAsync(service);
    else
    {
        response.StatusCode = 404;
        await response.WriteAsJsonAsync(new { message = "Сервис не найден" });
    }
}

async Task DeleteService(string? id, HttpResponse response)
{
    Service? service = services.FirstOrDefault((u) => u.Id == id);
    if (service != null)
    {
        services.Remove(service);
        await response.WriteAsJsonAsync(service);
    }
    else
    {
        response.StatusCode = 404;
        await response.WriteAsJsonAsync(new { message = "Сервис не найден" });
    }
}

async Task CreateService(HttpResponse response, HttpRequest request)
{
    try
    {
        var service = await request.ReadFromJsonAsync<Service>();
        if (service != null)
        {
            service.Id = Guid.NewGuid().ToString();
            services.Add(service);
            await response.WriteAsJsonAsync(service);
        }
        else
        {
            throw new Exception("Некорректные данные");
        }
    }
    catch (Exception)
    {
        response.StatusCode = 400;
        await response.WriteAsJsonAsync(new { message = "Некорректные данные" });
    }
}

async Task UpdateService(HttpResponse response, HttpRequest request)
    {
    try
    {
        Service? serviceData = await request.ReadFromJsonAsync<Service>();
        if (serviceData != null)
        {
            var service = services.FirstOrDefault(u => u.Id == serviceData.Id);
            if (service != null)
            {
                service.Description = serviceData.Description;
                service.Name = serviceData.Name;
                service.State = serviceData.State;
                await response.WriteAsJsonAsync(service);
            }
            else
            {
                response.StatusCode = 404;
                await response.WriteAsJsonAsync(new { message = "Сервис не найден" });
            }
        }
        else
        {
            throw new Exception("Некорректные данные");
        }
    }
    catch (Exception)
    {
        response.StatusCode = 400;
        await response.WriteAsJsonAsync(new { message = "Некорректные данные" });
    }
}

public class Service
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string State { get; set; } = "";
    public string Description { get; set; } = "";
}