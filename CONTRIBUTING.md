## Framework

This analyzer project uses a custom framework internally.

The framework removes unnecesarry code from diagnostics and code fix providers.

A single diagnostic (with or without a code fix) thus only requires a single file change - the diagnostic itself.

The framework lies in the `MihaZupan.CodeAnalysis.Framework` namespace.

## Making your own diagnostic

Every diagnostic must implement the abstract class `DiagnosticBase<T>`.

**`T`** is the `AnalysisContext` you want for your analyzer. (E.g. `SyntaxNodeAnalysisContext`)

To implement the class, it needs to implement 3 things:
* Property `Configuration<T>`
* Method `Analyze(T context)`
* Method `ExecuteCodeFixAsync(Document, Diagnostic, CancellationToken)`.

You must also add the `ExportCodeFixProvider` attribute to your diagnostic.
*(Sadly, Visual Studio doesn't register code fixes if the attribute is added with reflection at runtime)*

For an example, see any diagnostic in the [Analyzers](https://github.com/MihaZupan/Telegram.Bot.Analyzers/tree/master/src/Telegram.Bot.Analyzers/Analyzers) folder.

If you wish to create a diagnostic *without a code fix*, you **must** set the `ImplementsCodeFix` on `Configuration` to **`false`**.
You can then return `null` in `ExecuteCodeFixAsync`.