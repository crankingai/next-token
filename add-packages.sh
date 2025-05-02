#!/bin/bash

echo "➖➖➖➖➖➖➖➖➖➖➖➖➖➖➖➖➖"
echo "❮ Microsoft.SemanticKernel ❯"
dotnet add package Microsoft.SemanticKernel 

echo "➖➖➖➖➖➖➖➖➖➖➖➖➖➖➖➖➖➖➖➖➖➖➖➖➖➖➖➖➖➖"
echo "❮ Microsoft.SemanticKernel.Connectors.SqlServer ❯"
#dotnet add package Microsoft.SemanticKernel.Connectors.SqlServer
# as of 16-Jan-2025
#dotnet add package Microsoft.SemanticKernel.Connectors.SqlServer --version 1.33.0-alpha
dotnet add package Microsoft.SemanticKernel.Connectors.SqlServer --prerelease

echo "➖➖➖➖➖➖➖➖➖➖➖➖➖➖➖➖➖➖➖➖➖➖➖➖"
echo "❮ M  icrosoft.Extensions.Logging.Console ❯"
# as of 16-Jan-2025
# dotnet add package Microsoft.Extensions.Logging.Console --version 9.0.1
dotnet add package Microsoft.Extensions.Logging.Console --version 9.0.1

echo "➖➖➖➖➖➖"
echo "❮ Polly ❯"
dotnet add package Polly

echo "➖➖➖➖➖➖"
echo "❮ Handlebars ❯"
dotnet add package Microsoft.SemanticKernel.PromptTemplates.Handlebars

dotnet add package Microsoft.Extensions.Logging
dotnet add package Microsoft.Extensions.Logging.Console
dotnet add package Microsoft.SemanticKernel
dotnet add package Microsoft.SemanticKernel.Plugins.Web
dotnet add package Microsoft.SemanticKernel.Prompty
dotnet add package OpenTelemetry.Exporter.Console
dotnet add package System.Configuration.ConfigurationManager

# /Users/billdev/repos/devpartners/PROJECTS/crankingai/next-token/Program.cs(7,32): error CS0234: The type or namespace name 'Plugins' does not exist in the namespace 'Microsoft.SemanticKernel' (are you missing an assembly reference?)
## old ## dotnet add package Microsoft.SemanticKernel.Connectors.Bing
dotnet add package Microsoft.SemanticKernel.Plugins.Web --prerelease

