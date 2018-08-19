using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

[module: SuppressMessage("Build", "RS1022")]

namespace MihaZupan.CodeAnalysis.Framework
{
    [SuppressMessage("Correctness", "RS1016")]
    public abstract class DiagnosticBase : CodeFixProvider
    {
        public abstract AnalysisContextType ContextType { get; }
        public abstract DiagnosticDescriptor DiagnosticDescriptor { get; }
    }
    public abstract class DiagnosticBase<T> : DiagnosticBase
    {
        public abstract DiagnosticConfig<T> Configuration { get; }

        public abstract void Analyze(T context);

        protected abstract Task<Document> ExecuteCodeFixAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken);


        public override ImmutableArray<string> FixableDiagnosticIds =>
            Configuration.ImplementsCodeFix ? ImmutableArray.Create(Configuration.Id) : ImmutableArray<string>.Empty;

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            if (Configuration.ImplementsCodeFix)
            {
                var diagnostic = context.Diagnostics.First();

                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: Configuration.CodeFixTitle,
                        createChangedDocument: c => ExecuteCodeFixAsync(context.Document, diagnostic, c),
                        equivalenceKey: Configuration.Id),
                    diagnostic);
            }
            return Task.CompletedTask;
        }

        public override AnalysisContextType ContextType => Configuration.ActionType.Type;

        public override DiagnosticDescriptor DiagnosticDescriptor => new DiagnosticDescriptor(
                Configuration.Id,
                Configuration.DiagnosticTitle,
                Configuration.MessageFormat,
                Configuration.Category,
                Configuration.DefaultSeverity,
                Configuration.IsEnabledByDefault);
    }
}
