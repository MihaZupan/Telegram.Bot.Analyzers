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
using Microsoft.CodeAnalysis.Text;

namespace Telegram.Bot.Analyzers.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MessageChatAndIdToMessage)), Shared]
    public class MessageChatAndIdToMessage : CodeFixProvider, ISyntaxNodeRule
    {
        public static MessageChatAndIdToMessage Instance { get; } = new MessageChatAndIdToMessage();

        public MessageChatAndIdToMessage() { }

        public DiagnosticDescriptor DiagnosticDescriptor { get; } = new DiagnosticDescriptor(
            DiagnosticIDs.MessageChatAndIdToMessage,
            "Method call parameters can be simplified",
            "Use an overload for {0} that takes a Message parameter, instead of Chat and MessageId",
            Constants.Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(DiagnosticIDs.MessageChatAndIdToMessage);

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

        public void Analyze(SyntaxNodeAnalysisContext context)
        {
            var invocation = context.Node as InvocationExpressionSyntax;
            var methodAccess = invocation.Expression as MemberAccessExpressionSyntax;

            string methodName = methodAccess.Name.Identifier.ValueText;
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
                    chatIdAccess.VariableName() == messageIdAccess.VariableName() &&
                    chatIdAccess.CaleeParentTypeName(context) == "Message" &&
                    messageIdAccess.CaleeParentTypeName(context) == "Message")
                {
                    int start = arguments[i].SpanStart;
                    int length = arguments[i + 1].Span.End - start;
                    Location location = Location.Create(invocation.SyntaxTree, new TextSpan(start, length));

                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptor, location, methodName));
                    break;
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
                    title: "Simplify method call",
                    createChangedDocument: c => SimplifyMethodCall(context.Document, diagnostic, c),
                    equivalenceKey: DiagnosticIDs.MessageChatAndIdToMessage),
                diagnostic);
        }

        private async Task<Document> SimplifyMethodCall(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var root = await document.GetSyntaxRootAsync(cancellationToken);

            var argumentList = root.FindNode(diagnosticSpan) as ArgumentListSyntax;
            var arguments = argumentList.Arguments;

            int messageChatArgumentIndex = arguments.IndexOf(a => a.SpanStart == diagnosticSpan.Start);

            var memberAccess = arguments[messageChatArgumentIndex].Expression as MemberAccessExpressionSyntax;
            var variableName = memberAccess.VariableName();

            var generator = SyntaxGenerator.GetGenerator(document);
            var identifierName = generator.IdentifierName(variableName);
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
