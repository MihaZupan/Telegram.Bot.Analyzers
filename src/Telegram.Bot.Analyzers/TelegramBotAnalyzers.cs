using System.Collections.Immutable;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using MihaZupan.CodeAnalysis.Framework;

namespace Telegram.Bot.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TelegramBotAnalyzers : DiagnosticAnalyzer
    {
        static TelegramBotAnalyzers()
        {
            // When compiling the project that uses this analyzer, the Microsoft.CodeAnalysis.Workspaces assembly is not loaded.
            // All this try-catch and null checking nonsense is here to remove the warning one would get when compiling
            try
            {
                DiagnosticConfig.DefaultCategory = "Telegram.Bot";
                Manager = new AnalyzerBase();
            }
            catch (FileNotFoundException exception)
            when (exception.FileName.Contains("Microsoft.CodeAnalysis.Workspaces"))
            { }
        }

        static readonly AnalyzerBase Manager;
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Manager?.SupportedDiagnostics ?? ImmutableArray<DiagnosticDescriptor>.Empty;
        public override void Initialize(AnalysisContext context) => Manager?.Initialize(context);
    }
}
