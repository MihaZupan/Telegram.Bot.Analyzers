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
using MihaZupan.CodeAnalysis.Framework;

namespace Telegram.Bot.Analyzers.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MessageChatInsteadOfMessageChatId)), Shared]
    public class MessageChatInsteadOfMessageChatId : DiagnosticBase<SyntaxNodeAnalysisContext>
    {
        public override DiagnosticConfig<SyntaxNodeAnalysisContext> Configuration =>
            new DiagnosticConfig<SyntaxNodeAnalysisContext>(
                ActionType<SyntaxNodeAnalysisContext>.Create(SyntaxKind.InvocationExpression),
                "TG0001",
                "Message.Chat should be used instead of Message.Chat.Id",
                "Use {0}.Chat instead of {0}.Chat.Id",
                "Use Message.Chat instead of Message.Chat.Id");

        public override void Analyze(SyntaxNodeAnalysisContext context)
        {
            var invocation = context.Node as InvocationExpressionSyntax;
            var methodAccess = invocation.Expression as MemberAccessExpressionSyntax;

            if (methodAccess?.CalleeParentTypeName(context) != "TelegramBotClient") return;
            
            var argumentList = invocation.ArgumentList;
            if (argumentList.IsMissing) return;

            var arguments = argumentList.Arguments.ToArray();

            for (int i = 0; i < arguments.Length; i++)
            {
                if (arguments[i].Expression is MemberAccessExpressionSyntax chatIdAccess &&
                    chatIdAccess.AccessedMemberName() == "Id" &&
                    chatIdAccess.Expression is MemberAccessExpressionSyntax chatAccess &&
                    chatAccess.AccessedMemberName() == "Chat")
                {
                    context.ReportDiagnostic(GetDiagnostic(arguments[i].GetLocation(), chatAccess.ExpressionString()));
                }
            }
        }
        
        protected override async Task<Document> ExecuteCodeFixAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var root = await document.GetSyntaxRootAsync(cancellationToken);

            var argument = root.FindNode(diagnosticSpan) as ArgumentSyntax;
            
            var chatAccess = argument.Expression as MemberAccessExpressionSyntax;
            var memberAccess = chatAccess.Expression as MemberAccessExpressionSyntax;
            var accessorName = memberAccess.ExpressionString();

            var generator = SyntaxGenerator.GetGenerator(document);
            var identifierName = generator.IdentifierName(accessorName);
            var newMemberAccess = generator.MemberAccessExpression(identifierName, "Chat");
            var newArgument = generator.Argument(newMemberAccess) as ArgumentSyntax;
            
            var newRoot = root.ReplaceNode(argument, newArgument);
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
