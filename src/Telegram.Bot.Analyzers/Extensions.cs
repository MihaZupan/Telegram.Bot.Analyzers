using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Telegram.Bot.Analyzers
{
    public static class Extensions
    {
        public static string CalleeParentTypeName(this MemberAccessExpressionSyntax access, SyntaxNodeAnalysisContext context)
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
            => access.Name.Identifier.ValueText;

        public static string ExpressionString(this MemberAccessExpressionSyntax access)
            => access.Expression.ToString();
    }
}
