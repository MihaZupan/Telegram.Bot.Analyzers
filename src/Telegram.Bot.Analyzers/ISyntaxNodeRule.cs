using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Telegram.Bot.Analyzers
{
    public interface ISyntaxNodeRule
    {
        DiagnosticDescriptor DiagnosticDescriptor { get; }
        void Analyze(SyntaxNodeAnalysisContext context);
    }
}
