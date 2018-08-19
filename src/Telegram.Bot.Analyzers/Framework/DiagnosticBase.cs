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
        public DiagnosticBase()
        {
            _configuration = Configuration;
            _diagnosticDescriptor = DiagnosticDescriptor;
        }

        private readonly DiagnosticConfig<T> _configuration;
        public abstract DiagnosticConfig<T> Configuration { get; }

        public abstract void Analyze(T context);

        protected abstract Task<Document> ExecuteCodeFixAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken);


        public override ImmutableArray<string> FixableDiagnosticIds =>
            _configuration.ImplementsCodeFix ? ImmutableArray.Create(_configuration.Id) : ImmutableArray<string>.Empty;

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            if (_configuration.ImplementsCodeFix)
            {
                var diagnostic = context.Diagnostics.First();

                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: _configuration.CodeFixTitle,
                        createChangedDocument: c => ExecuteCodeFixAsync(context.Document, diagnostic, c),
                        equivalenceKey: _configuration.Id),
                    diagnostic);
            }
            return Task.CompletedTask;
        }

        public override AnalysisContextType ContextType => _configuration.ActionType.Type;

        private readonly DiagnosticDescriptor _diagnosticDescriptor;
        public override DiagnosticDescriptor DiagnosticDescriptor => new DiagnosticDescriptor(
                _configuration.Id,
                _configuration.DiagnosticTitle,
                _configuration.MessageFormat,
                _configuration.Category,
                _configuration.DefaultSeverity,
                _configuration.IsEnabledByDefault);

        public Diagnostic GetDiagnostic(Location location, string message = "")
            => Diagnostic.Create(_diagnosticDescriptor, location, message);
    }
}
