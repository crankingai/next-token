using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel.Data;
// using Microsoft.SemanticKernel.PromptTemplates.Handlebars;

var builder = Kernel.CreateBuilder();

#region Configure OpenAI API

string? modelId = Environment.GetEnvironmentVariable("OPENAI_MODEL_ID");
string? apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
if (string.IsNullOrEmpty(modelId) || string.IsNullOrEmpty(apiKey))
{
    throw new InvalidOperationException("OpenAI model ID and API key must be set in environment variables.");
}
builder.AddOpenAIChatCompletion(modelId, apiKey);

#endregion
#region Configure Bing Plugin (bingSearchPlugin)

// https://learn.microsoft.com/en-us/semantic-kernel/concepts/text-search/text-search-plugins?pivots=programming-language-csharp
var bingApiKey = Environment.GetEnvironmentVariable("BING_SEARCH_KEY") ??
    throw new InvalidOperationException("Bing search key must be set as environment variable `BING_SEARCH_KEY`.");
#pragma warning disable SKEXP0050
var textSearch = new BingTextSearch(apiKey: bingApiKey);
#pragma warning restore SKEXP0050
#pragma warning disable SKEXP0001
var searchOptions = new TextSearchOptions();
#pragma warning restore SKEXP0001

var bingSearchPlugin = KernelPluginFactory.CreateFromFunctions(
    "SearchPlugin", "Bing Web Search Plugin",
    [textSearch.CreateGetTextSearchResults(searchOptions: searchOptions)]);

#endregion
#region Configure Logging - TODO: move to config
builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));
#endregion


Kernel kernel = builder.Build();
kernel.Plugins.Add(bingSearchPlugin);

var prompt = @"What is the capital city of Massachusetts?";
//  prompt = @"What was the capital city of Massachusetts 300 years ago?";
prompt = @"Tell me a joke that a software eneginner would like.";
prompt = @"Pick a number between 1 and 1000.";
prompt = @"Pick a number between a trillion and three googol.";
prompt = @"Tell me a joke about Boston.";

// Console.WriteLine($"╔═════════════════════════════════════════════");
// Console.WriteLine($"║📣🤖 Prompt: 『{prompt}』📢🤖");
// Console.WriteLine($"╚═══");

var MaxTokens = 2500;
var Temperature = 0.7f;
Temperature = 1.7f;
Temperature = 0.0f; // greedy sampling
Temperature = 1.99f; // max "creativity"
Temperature = 0.7f; // common default
var TopP = 1.0f;
TopP = 0.93f;
TopP = 0.0f; // greedy sampling
TopP = 1.0f; // include everything
// TopP = 0.5f; // include half the mass

var arguments = new KernelArguments(new OpenAIPromptExecutionSettings
{
#pragma warning disable SKEXP0010
    Logprobs = true,
    TopLogprobs = 20, // max = 20 :-(
#pragma warning restore SKEXP0010
    MaxTokens = MaxTokens,
    Temperature = Temperature,
    TopP = TopP

   //  FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
   //  ChatSystemPrompt = "You are a web researcher. If you can't find what you are seeking, you retry up to 20 times across many pages."
});


var response = await kernel.InvokePromptAsync(
    prompt,
    arguments);


Console.WriteLine($"╔═════════════════════════════════════════════");
Console.WriteLine($"║✅🤖 Prompt:   『{response.RenderedPrompt}』");
var responseText = response.GetValue<string>();
if (!String.IsNullOrWhiteSpace(responseText))
   // make sure no newlines are in the responseText since it might not be seen in the console for demos that grep
   responseText = responseText.ReplaceLineEndings().Replace('\n', ' ');
Console.WriteLine($"║✅🤖 Response: 『{responseText}』");
Console.WriteLine($"║✅🤖 Inference Params:");

#if false
if (arguments.TryGetValue(typeof(OpenAIPromptExecutionSettings).FullName!, out object? executionSettingsObject))
{
    if (executionSettingsObject is OpenAIPromptExecutionSettings executionSettings)
    {
        Console.WriteLine($"✅ Temperature: {executionSettings.Temperature}");
        Console.WriteLine($"✅ TopP: {executionSettings.TopP}");
        // Access other properties as needed
    }
    else
    {
        Console.WriteLine("Retrieved object is not OpenAIPromptExecutionSettings or is null.");
    }
}
else
{
    Console.WriteLine("OpenAIPromptExecutionSettings was not found in the arguments.");
}
#else
Console.WriteLine($"║✅🤖 MaxTokens:『{MaxTokens}』");
Console.WriteLine($"║✅🤖 Temp:   『{Temperature}』");
Console.WriteLine($"║✅🤖 TopP:   『{TopP}』");
#endif

Console.WriteLine($"╚═══");
if (response.Metadata == null || !response.Metadata.ContainsKey("Usage"))
{
    Console.WriteLine("No token usage information found.");
}
else
{
    var usage = response.Metadata["Usage"] as OpenAI.Chat.ChatTokenUsage;
    if (usage == null)
    {
        Console.WriteLine("Token usage information is not in the expected format.");
        return;
    }
    Console.WriteLine($"✅ {usage.InputTokenCount} [Input Tokens] + {usage.OutputTokenCount} [Output Tokens] = {usage.TotalTokenCount} [Total Tokens]");
   //  Console.WriteLine($"{usage.OutputTokenDetails.ReasoningTokenCount} [Output Token Details]");

    if (response.Metadata != null && response.Metadata.TryGetValue("ContentTokenLogProbabilities", out var tokenProbsObj) && tokenProbsObj != null)
    {
        var probDetails = (List<OpenAI.Chat.ChatTokenLogProbabilityDetails>)tokenProbsObj!;

        var tokenProbs = tokenProbsObj as List<OpenAI.Chat.ChatTokenLogProbabilityDetails>;
        //List<Dictionary<string, float>>;
        if (tokenProbs != null)
            LogProbsDumper.DumpRawLogProbs(tokenProbs, "./dumpy.log");
    }
}
