using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;
using MihaZupan.CodeAnalysis.Framework;

namespace Telegram.Bot.Analyzers.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MessageChatAndIdToMessage)), Shared]
    public class MessageChatAndIdToMessage : DiagnosticBase<SyntaxNodeAnalysisContext>
    {
        public override DiagnosticConfig<SyntaxNodeAnalysisContext> Configuration =>
            new DiagnosticConfig<SyntaxNodeAnalysisContext>(
                ActionType<SyntaxNodeAnalysisContext>.Create(SyntaxKind.InvocationExpression),
                "TG0002",
                "Method call parameters can be simplified",
                "Use an overload for {0} that takes a Message parameter, instead of Chat and MessageId",
                "Simplify method call");

        private static readonly ImmutableHashSet<string> ValidMethods =
            ImmutableHashSet.Create(
                "ForwardMessageAsync",
                "StopMessageLiveLocationAsync",
                "EditMessageTextAsync",
                "EditMessageCaptionAsync",
                "EditMessageMediaAsync",
                "EditMessageReplyMarkupAsync",
                "EditMessageLiveLocationAsync",
                "DeleteMessageAsync",
                "PinChatMessageAsync");

        public override void Analyze(SyntaxNodeAnalysisContext context)
        {
            var invocation = context.Node as InvocationExpressionSyntax;
            var methodAccess = invocation.Expression as MemberAccessExpressionSyntax;

            if (methodAccess?.CalleeParentTypeName(context) != "TelegramBotClient") return;

            string methodName = methodAccess.AccessedMemberName();
            if (!ValidMethods.Contains(methodName)) return;

            var argumentList = invocation.ArgumentList;
            if (argumentList.IsMissing) return;

            var arguments = argumentList.Arguments.ToArray();
            if (arguments.Length < 2) return;

            for (int i = 0; i < arguments.Length - 1; i++)
            {
                if (arguments[i].Expression is MemberAccessExpressionSyntax chatIdAccess &&
                    arguments[i + 1].Expression is MemberAccessExpressionSyntax messageIdAccess &&
                    chatIdAccess.AccessedMemberName() == "Chat" &&
                    messageIdAccess.AccessedMemberName() == "MessageId" &&
                    chatIdAccess.ExpressionString() == messageIdAccess.ExpressionString())
                {
                    int start = arguments[i].SpanStart;
                    int length = arguments[i + 1].Span.End - start;
                    Location location = Location.Create(invocation.SyntaxTree, new TextSpan(start, length));

                    context.ReportDiagnostic(GetDiagnostic(location, methodName));
                    break;
                }
            }
        }

        protected override async Task<Document> ExecuteCodeFixAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var root = await document.GetSyntaxRootAsync(cancellationToken);

            var argumentList = root.FindNode(diagnosticSpan) as ArgumentListSyntax;
            var arguments = argumentList.Arguments;

            int messageChatArgumentIndex = arguments.IndexOf(a => a.SpanStart == diagnosticSpan.Start);

            var memberAccess = arguments[messageChatArgumentIndex].Expression as MemberAccessExpressionSyntax;
            var accessorName = memberAccess.ExpressionString();

            var generator = SyntaxGenerator.GetGenerator(document);
            var identifierName = generator.IdentifierName(accessorName);
            var newArgument = generator.Argument(identifierName) as ArgumentSyntax;

            var newArguments = arguments
                .RemoveAt(messageChatArgumentIndex)
                .RemoveAt(messageChatArgumentIndex)
                .Insert(messageChatArgumentIndex, newArgument);

            var newArgumentList = argumentList.WithArguments(newArguments);

            var newRoot = root.ReplaceNode(argumentList, newArgumentList);
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
