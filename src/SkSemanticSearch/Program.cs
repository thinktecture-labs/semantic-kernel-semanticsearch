﻿using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Memory.Sqlite;
using Microsoft.SemanticKernel.KernelExtensions;
using Microsoft.SemanticKernel.Orchestration;

const string COLLECTION = "documentation";

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", false)
    .AddUserSecrets<Program>()
    .Build();

var index = !File.Exists("index.db");
var store = await SqliteMemoryStore.ConnectAsync("index.db");
var kernel = new KernelBuilder()
    .Configure(c =>
    {
        c.AddOpenAITextEmbeddingGenerationService("ada", "text-embedding-ada-002", config["OpenAi:ApiKey"]);
        c.AddOpenAITextCompletionService("davinci-openai", "text-davinci-003", config["OpenAi:ApiKey"]);
    })
    .WithMemoryStorage(store)
    .Build();

kernel.ImportSemanticSkillFromDirectory("skills", "qa");

if (index)
{
    await Index();
}

while (true)
{
    Console.Write("Enter a question or press enter to quit: ");
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input))
    {
        Console.WriteLine("Bye 👋");
        break;
    }
    var answer = await Answer(input);
    Console.WriteLine($"🤖:{answer}");
}

async Task<string> Answer(string question)
{
    var results = await kernel.Memory.SearchAsync(COLLECTION, question, limit: 2).ToListAsync();
    var variables = new ContextVariables(question)
    {
        ["context"] = results.Any() 
            ? string.Join("\n", results.Select(r => r.Metadata.Text)) 
            : "No context found for this question."
    };
    
    var result = await kernel.RunAsync(variables, kernel.Skills.GetFunction("qa", "answer"));
    return result.Result;    
}

async Task Index()
{
    Console.WriteLine("Indexing urls");
    var urls = config.GetSection("urls").Get<IndexUrl[]>() ?? Array.Empty<IndexUrl>();
    using var client = new HttpClient();
    foreach (var url in urls)
    {
        Console.WriteLine($"Indexing {url.Url}");
        await IndexUrl(client, url.Url, url.Selector);
    }
    
    Console.WriteLine("Indexing done");
}

async Task IndexUrl(HttpClient client, string url, string contentSelector)
{
    var content = await client.GetStringAsync(url);
    var title = string.Empty;
    
    if (!string.IsNullOrWhiteSpace(contentSelector))
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(content);
        var mainElement = doc.DocumentNode.SelectSingleNode(contentSelector);
        title = mainElement.SelectSingleNode("//h1").InnerText;
        content = Cleanup(mainElement.InnerText);
    }
    
    await kernel.Memory.SaveInformationAsync(COLLECTION, content, url, title);
    
    static string Cleanup(string content) => content.Replace("\t", "").Replace("\r\n\r\n", "");
}

record IndexUrl(string Url, string Selector);