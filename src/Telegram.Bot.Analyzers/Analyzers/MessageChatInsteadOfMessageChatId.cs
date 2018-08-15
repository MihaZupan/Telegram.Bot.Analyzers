using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Editing;

namespace Telegram.Bot.Analyzers.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MessageChatInsteadOfMessageChatId)), Shared]
    public class MessageChatInsteadOfMessageChatId : CodeFixProvider, ISyntaxNodeRule
    {
        public static MessageChatInsteadOfMessageChatId Instance { get; } = new MessageChatInsteadOfMessageChatId();

        public MessageChatInsteadOfMessageChatId() { }

        public DiagnosticDescriptor DiagnosticDescriptor { get; } = new DiagnosticDescriptor(
            DiagnosticIDs.MessageChatInsteadOfMessageChatId,
            "Message.Chat should be used instead of Message.Chat.Id",
            "Use {0}.Chat instead of {0}.Chat.Id",
            Constants.Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(DiagnosticIDs.MessageChatInsteadOfMessageChatId);

        public void Analyze(SyntaxNodeAnalysisContext context)
        {
            var invocation = context.Node as InvocationExpressionSyntax;
            var methodAccess = invocation.Expression as MemberAccessExpressionSyntax;

            string methodName = methodAccess.Name.Identifier.ValueText;

            var argumentList = invocation.ArgumentList;
            if (argumentList.IsMissing) return;

            var arguments = argumentList.Arguments.ToArray();

            for (int i = 0; i < arguments.Length; i++)
            {
                if (arguments[i].Expression is MemberAccessExpressionSyntax chatIdAccess &&
                    chatIdAccess.AccessedMemberName() == "Id" &&
                    chatIdAccess.Expression is MemberAccessExpressionSyntax chatAccess &&
                    chatAccess.AccessedMemberName() == "Chat" &&
                    chatAccess.CaleeParentTypeName(context) == "Message")
                {
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptor, arguments[i].GetLocation(), chatAccess.VariableName()));
                }
            }
        }

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var diagnostic = context.Diagnostics.First();
            
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Use Message.Chat instead of Message.Chat.Id",
                    createChangedDocument: c => UseMessageChatInsteadOfMessageChatId(context.Document, diagnostic, c),
                    equivalenceKey: DiagnosticIDs.MessageChatAndIdToMessage),
                diagnostic);
        }

        private async Task<Document> UseMessageChatInsteadOfMessageChatId(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var root = await document.GetSyntaxRootAsync(cancellationToken);

            var argument = root.FindNode(diagnosticSpan) as ArgumentSyntax;
            
            var chatAccess = argument.Expression as MemberAccessExpressionSyntax;
            var memberAccess = chatAccess.Expression as MemberAccessExpressionSyntax;
            var variableName = memberAccess.VariableName();

            var generator = SyntaxGenerator.GetGenerator(document);
            var identifierName = generator.IdentifierName(variableName);
            var newMemberAccess = generator.MemberAccessExpression(identifierName, "Chat");
            var newArgument = generator.Argument(newMemberAccess) as ArgumentSyntax;
            
            var newRoot = root.ReplaceNode(argument, newArgument);
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
