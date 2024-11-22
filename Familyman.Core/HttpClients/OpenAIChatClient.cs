﻿namespace Familyman.Core.HttpClients;

using Familyman.Core.Models;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public interface IOpenAIChatClient
{
    Task<JsonDocument> SendMessageAsync(IEnumerable<ChatMessage> messages, string model = "gpt-4", double temperature = 0.7);
}

public class OpenAIChatClient : IOpenAIChatClient
{
    private readonly HttpClient _httpClient;

    public OpenAIChatClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<JsonDocument> SendMessageAsync(
        IEnumerable<ChatMessage> messages,
        string model = "gpt-4",
        double temperature = 0.7)
    {
        var requestBody = new
        {
            model,
            temperature,
            messages = messages.Select(
                message => new { role = message.Role.ToString().ToLower(), content = message.Content }),
        };


        var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(new Uri("completions", UriKind.Relative), jsonContent);

        try
        {
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            throw new Exception(await response.Content.ReadAsStringAsync());
        }

        var responseContent = await response.Content.ReadFromJsonAsync<JsonDocument>();
        return responseContent;
    }
}
