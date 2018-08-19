using Microsoft.CodeAnalysis;
using System.Reflection;

namespace MihaZupan.CodeAnalysis.Framework
{
    public class DiagnosticConfig
    {
        public static string DefaultCategory;
        static DiagnosticConfig()
        {
            string assemblyName = typeof(DiagnosticConfig).GetTypeInfo().Assembly.GetName().Name;
            int indexOfDot = assemblyName.IndexOf('.');
            DefaultCategory = indexOfDot == -1
                ? assemblyName
                : assemblyName.Substring(0, indexOfDot);
        }

        protected DiagnosticConfig() { }

        // Required
        public string Id { get; protected set; }
        public string DiagnosticTitle { get; protected set; }
        public string MessageFormat { get; protected set; }
        public string CodeFixTitle { get; protected set; }

        // Optional
        public string Category { get; protected set; }
        public DiagnosticSeverity DefaultSeverity { get; protected set; }
        public bool IsEnabledByDefault { get; protected set; }
        public bool ImplementsCodeFix { get; protected set; }
    }
    public class DiagnosticConfig<T> : DiagnosticConfig
    {
        public ActionType<T> ActionType { get; }

        public DiagnosticConfig(
            ActionType<T> actionType,
            string id,
            string diagnosticTitle,
            string messageFormat,
            string codeFixTitle,
            string category = default,
            DiagnosticSeverity defaultSeverity = DiagnosticSeverity.Warning,
            bool isEnabledByDefault = true,
            bool implementsCodeFix = true)
        {
            ActionType = actionType;
            Id = id;
            DiagnosticTitle = diagnosticTitle;
            MessageFormat = messageFormat;
            CodeFixTitle = codeFixTitle;
            Category = category ?? DefaultCategory;
            DefaultSeverity = defaultSeverity;
            IsEnabledByDefault = isEnabledByDefault;
            ImplementsCodeFix = implementsCodeFix;
        }
    }
}
