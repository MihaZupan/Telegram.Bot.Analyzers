using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Telegram.Bot.Analyzers
{
    public static class Helpers
    {
        public static string CaleeParentTypeName(this MemberAccessExpressionSyntax access, SyntaxNodeAnalysisContext context)
        {
            var identifierType = context.SemanticModel.GetTypeInfo(access.Expression);
            return identifierType.Type.Name;
        }
        public static string VariableName(this MemberAccessExpressionSyntax access)
        {
            var identifierName = access.Expression as IdentifierNameSyntax;
            return identifierName.Identifier.ValueText;
        }
        public static string AccessedMemberName(this MemberAccessExpressionSyntax access)
        {
            return access.Name.Identifier.ValueText;
        }
    }
}
