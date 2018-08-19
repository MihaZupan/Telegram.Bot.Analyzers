using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MihaZupan.CodeAnalysis.Framework
{
    public sealed class ActionType<T>
    {
        public readonly AnalysisContextType Type;

        private ActionType(AnalysisContextType type)
        {
            Type = type;
        }

        public OperationKind[] OperationKinds { get; private set; }
        public SymbolKind[] SymbolKinds { get; private set; }
        public SyntaxKind[] SyntaxKinds { get; private set; }


        public static ActionType<CodeBlockAnalysisContext> CodeBlockInstance =
            new ActionType<CodeBlockAnalysisContext>(AnalysisContextType.CodeBlock);

        public static readonly ActionType<CompilationAnalysisContext> CompilationInstance =
            new ActionType<CompilationAnalysisContext>(AnalysisContextType.Compilation);

        public static ActionType<OperationAnalysisContext> Create(params OperationKind[] operationKinds)
            => new ActionType<OperationAnalysisContext>(AnalysisContextType.Operation) { OperationKinds = operationKinds };

        public static ActionType<OperationBlockAnalysisContext> OperationBlockInstance =
            new ActionType<OperationBlockAnalysisContext>(AnalysisContextType.OperationBlock);

        public static ActionType<SemanticModelAnalysisContext> SemanticModelInstance =
            new ActionType<SemanticModelAnalysisContext>(AnalysisContextType.SemanticModel);

        public static ActionType<SymbolAnalysisContext> Create(params SymbolKind[] symbolKinds)
            => new ActionType<SymbolAnalysisContext>(AnalysisContextType.Symbol) { SymbolKinds = symbolKinds };

        public static ActionType<SyntaxNodeAnalysisContext> Create(params SyntaxKind[] syntaxKinds)
            => new ActionType<SyntaxNodeAnalysisContext>(AnalysisContextType.SyntaxNode) { SyntaxKinds = syntaxKinds };

        public static ActionType<SyntaxTreeAnalysisContext> SyntaxTreeInstance =
            new ActionType<SyntaxTreeAnalysisContext>(AnalysisContextType.SyntaxTree);

    }
}
