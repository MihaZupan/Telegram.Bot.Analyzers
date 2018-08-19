using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace MihaZupan.CodeAnalysis.Framework
{
    public sealed class AnalyzerBase
    {
        public AnalyzerBase()
            : this(_ => true) { }
        public AnalyzerBase(string diagnosticsNamespace)
            : this(name => name == diagnosticsNamespace) { }
        public AnalyzerBase(Func<string, bool> namespacePredicate)
        {
            {
                var possibleContextTypes = Enum.GetValues(typeof(AnalysisContextType)) as AnalysisContextType[];
                AnalyzersByContextType = new Dictionary<AnalysisContextType, List<DiagnosticBase>>(possibleContextTypes.Length);
                foreach (AnalysisContextType type in possibleContextTypes)
                {
                    AnalyzersByContextType.Add(type, new List<DiagnosticBase>());
                }

                var possibleOperationKinds = Enum.GetValues(typeof(OperationKind)) as OperationKind[];
                AnalyzersByOperationKind = new Dictionary<OperationKind, List<DiagnosticBase<OperationAnalysisContext>>>(possibleOperationKinds.Length);
                foreach (OperationKind type in possibleOperationKinds)
                {
                    AnalyzersByOperationKind.Add(type, new List<DiagnosticBase<OperationAnalysisContext>>());
                }

                var possibleSymbolKinds = Enum.GetValues(typeof(SymbolKind)) as SymbolKind[];
                AnalyzersBySymbolKind = new Dictionary<SymbolKind, List<DiagnosticBase<SymbolAnalysisContext>>>(possibleOperationKinds.Length);
                foreach (SymbolKind type in possibleSymbolKinds)
                {
                    AnalyzersBySymbolKind.Add(type, new List<DiagnosticBase<SymbolAnalysisContext>>());
                }

                var possibleSyntaxKinds = Enum.GetValues(typeof(SyntaxKind)) as SyntaxKind[];
                AnalyzersBySyntaxKind = new Dictionary<SyntaxKind, List<DiagnosticBase<SyntaxNodeAnalysisContext>>>(possibleOperationKinds.Length);
                foreach (SyntaxKind type in possibleSyntaxKinds)
                {
                    AnalyzersBySyntaxKind.Add(type, new List<DiagnosticBase<SyntaxNodeAnalysisContext>>());
                }
            }

            List<DiagnosticDescriptor> diagnostics = new List<DiagnosticDescriptor>();
            HashSet<OperationKind> operationKinds = new HashSet<OperationKind>();
            HashSet<SymbolKind> symbolKinds = new HashSet<SymbolKind>();
            HashSet<SyntaxKind> syntaxKinds = new HashSet<SyntaxKind>();

            Type[] analyzers = typeof(AnalyzerBase).GetTypeInfo().Assembly.DefinedTypes
                .Where(type =>
                    type.IsClass &&
                    type.BaseType != null &&
                    type.IsSubclassOf(typeof(DiagnosticBase)) &&
                    type.Namespace != "MihaZupan.CodeAnalysis.Framework" &&
                    namespacePredicate(type.Namespace))
                .Select(type => type.AsType())
                .ToArray();

            foreach (var analyzerType in analyzers)
            {
                var analyzer = Activator.CreateInstance(analyzerType) as DiagnosticBase;

                diagnostics.Add(analyzer.DiagnosticDescriptor);
                AnalyzersByContextType[analyzer.ContextType].Add(analyzer);

                switch (analyzer.ContextType)
                {
                    case AnalysisContextType.Operation:
                        var operationAnalyzer = analyzer as DiagnosticBase<OperationAnalysisContext>;
                        foreach (var operationKind in operationAnalyzer.Configuration.ActionType.OperationKinds)
                        {
                            if (!operationKinds.Contains(operationKind)) operationKinds.Add(operationKind);
                            AnalyzersByOperationKind[operationKind].Add(operationAnalyzer);
                        }
                        break;

                    case AnalysisContextType.Symbol:
                        var symbolAnalyzer = analyzer as DiagnosticBase<SymbolAnalysisContext>;
                        foreach (var symbolKind in symbolAnalyzer.Configuration.ActionType.SymbolKinds)
                        {
                            if (!symbolKinds.Contains(symbolKind)) symbolKinds.Add(symbolKind);
                            AnalyzersBySymbolKind[symbolKind].Add(symbolAnalyzer);
                        }
                        break;

                    case AnalysisContextType.SyntaxNode:
                        var syntaxNodeAnalyzer = analyzer as DiagnosticBase<SyntaxNodeAnalysisContext>;
                        foreach (var syntaxKind in syntaxNodeAnalyzer.Configuration.ActionType.SyntaxKinds)
                        {
                            if (!syntaxKinds.Contains(syntaxKind)) syntaxKinds.Add(syntaxKind);
                            AnalyzersBySyntaxKind[syntaxKind].Add(syntaxNodeAnalyzer);
                        }
                        break;
                }
            }

            SupportedDiagnostics = ImmutableArray.CreateRange(diagnostics);
            NeededOperationKinds = ImmutableArray.CreateRange(operationKinds);
            NeededSymbolKinds = ImmutableArray.CreateRange(symbolKinds);
            NeededSyntaxKinds = ImmutableArray.CreateRange(syntaxKinds);        
        }

        public readonly ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics;

        private readonly Dictionary<AnalysisContextType, List<DiagnosticBase>> AnalyzersByContextType;

        private readonly ImmutableArray<OperationKind> NeededOperationKinds;
        private readonly ImmutableArray<SymbolKind> NeededSymbolKinds;
        private readonly ImmutableArray<SyntaxKind> NeededSyntaxKinds;

        private readonly Dictionary<OperationKind, List<DiagnosticBase<OperationAnalysisContext>>> AnalyzersByOperationKind;
        private readonly Dictionary<SymbolKind, List<DiagnosticBase<SymbolAnalysisContext>>> AnalyzersBySymbolKind;
        private readonly Dictionary<SyntaxKind, List<DiagnosticBase<SyntaxNodeAnalysisContext>>> AnalyzersBySyntaxKind;


        public void Initialize(AnalysisContext context)
        {
            if (SupportedDiagnostics.Length == 0) return;

            if (AnalyzersByContextType[AnalysisContextType.CodeBlock].Count != 0) context.RegisterCodeBlockAction(CodeBlockAction);
            if (AnalyzersByContextType[AnalysisContextType.Compilation].Count != 0) context.RegisterCompilationAction(CompilationAction);
            if (AnalyzersByContextType[AnalysisContextType.Operation].Count != 0) context.RegisterOperationAction(OperationAction, NeededOperationKinds);
            if (AnalyzersByContextType[AnalysisContextType.OperationBlock].Count != 0) context.RegisterOperationBlockAction(OperationBlockAction);
            if (AnalyzersByContextType[AnalysisContextType.SemanticModel].Count != 0) context.RegisterSemanticModelAction(SemanticModelAction);
            if (AnalyzersByContextType[AnalysisContextType.Symbol].Count != 0) context.RegisterSymbolAction(SymbolAction, NeededSymbolKinds);
            if (AnalyzersByContextType[AnalysisContextType.SyntaxNode].Count != 0) context.RegisterSyntaxNodeAction(SyntaxNodeAction, NeededSyntaxKinds);
            if (AnalyzersByContextType[AnalysisContextType.SyntaxTree].Count != 0) context.RegisterSyntaxTreeAction(SyntaxTreeAction);
        }

        private void CodeBlockAction(CodeBlockAnalysisContext context)
        {
            foreach (var analyzer in AnalyzersByContextType[AnalysisContextType.CodeBlock])
            {
                (analyzer as DiagnosticBase<CodeBlockAnalysisContext>).Analyze(context);
            }
        }
        private void CompilationAction(CompilationAnalysisContext context)
        {
            foreach (var analyzer in AnalyzersByContextType[AnalysisContextType.Compilation])
            {
                (analyzer as DiagnosticBase<CompilationAnalysisContext>).Analyze(context);
            }
        }
        private void OperationAction(OperationAnalysisContext context)
        {
            foreach (var analyzer in AnalyzersByOperationKind[context.Operation.Kind])
            {
                analyzer.Analyze(context);
            }
        }
        private void OperationBlockAction(OperationBlockAnalysisContext context)
        {
            foreach (var analyzer in AnalyzersByContextType[AnalysisContextType.OperationBlock])
            {
                (analyzer as DiagnosticBase<OperationBlockAnalysisContext>).Analyze(context);
            }
        }
        private void SemanticModelAction(SemanticModelAnalysisContext context)
        {
            foreach (var analyzer in AnalyzersByContextType[AnalysisContextType.SemanticModel])
            {
                (analyzer as DiagnosticBase<SemanticModelAnalysisContext>).Analyze(context);
            }
        }
        private void SymbolAction(SymbolAnalysisContext context)
        {
            foreach (var analyzer in AnalyzersBySymbolKind[context.Symbol.Kind])
            {
                analyzer.Analyze(context);
            }
        }
        private void SyntaxNodeAction(SyntaxNodeAnalysisContext context)
        {
            foreach (var analyzer in AnalyzersBySyntaxKind[context.Node.Kind()])
            {
                analyzer.Analyze(context);
            }
        }
        private void SyntaxTreeAction(SyntaxTreeAnalysisContext context)
        {
            foreach (var analyzer in AnalyzersByContextType[AnalysisContextType.SyntaxTree])
            {
                (analyzer as DiagnosticBase<SyntaxTreeAnalysisContext>).Analyze(context);
            }
        }
    }
}
