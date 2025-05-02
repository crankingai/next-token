using System;

namespace CrankingAI.Config;

public static class EnvVarReader
{
    public static bool debug = false;

    public static (string, string, string) GetAzureOpenAIConfig()
    {
        var modelId = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME");
        var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
        var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");

        if (modelId is null || endpoint is null || apiKey is null)
        {
            throw new InvalidOperationException("Please set the environment variables for the Azure OpenAI deployment name, endpoint, and API key.");
        }

        if (debug)
        {
            Console.WriteLine($"Model ID: {modelId}");
            Console.WriteLine($"Endpoint: {endpoint}");
            Console.WriteLine($"API Key: {apiKey.Substring(0, 2)}...{apiKey.Substring(apiKey.Length - 2)}");
        }

        return (modelId, endpoint, apiKey);
    }

    public static (string, string) GetOpenAIConfig()
    {
        var modelIdEnvVar = "OPENAI_MODEL_ID";
        var apiKeyEnvVar = "OPENAI_API_KEY";
        var modelId = Environment.GetEnvironmentVariable(modelIdEnvVar);
        var apiKey = Environment.GetEnvironmentVariable(apiKeyEnvVar);

        if (modelId is null || apiKey is null)
        {
            throw new InvalidOperationException($"Did not find one or both of {modelIdEnvVar} and {apiKeyEnvVar} environment variable for OpenAI API access.");
        }

        if (debug)
        {
            Console.WriteLine($"Model ID: {modelId}");
            Console.WriteLine($"API Key: {apiKey.Substring(0, 12)}...{apiKey.Substring(apiKey.Length - 2)}");
        }

        return (modelId, apiKey);
    }
}
