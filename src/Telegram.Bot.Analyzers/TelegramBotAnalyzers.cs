using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Telegram.Bot.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TelegramBotAnalyzers : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Analyzers.MessageChatInsteadOfMessageChatId.Instance.DiagnosticDescriptor,
            Analyzers.MessageChatAndIdToMessage.Instance.DiagnosticDescriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                AnalyzeInvocationExpression,
                SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeInvocationExpression(SyntaxNodeAnalysisContext context)
        {
            var invocation = context.Node as InvocationExpressionSyntax;

            if (invocation.Expression is MemberAccessExpressionSyntax methodAccess)
            {
                // Make sure that the method is a part of the TelegramBotClient class
                if (methodAccess.CaleeParentTypeName(context) != "TelegramBotClient") return;

                Analyzers.MessageChatInsteadOfMessageChatId.Instance.Analyze(context);
                Analyzers.MessageChatAndIdToMessage.Instance.Analyze(context);
            }
        }
    }
}
