using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace Telegram.Bot.Analyzers.Test
{
    [TestClass]
    public class MessageChatInsteadOfMessageChatIdTests : CodeFixVerifier
    {
        [TestMethod]
        public void ActualTest()
        {
            var test = @"
namespace TestNamespace
{
    class TestClass
    {
        static async void Test()
        {
            var bot = new TelegramBotClient(""API Token"");
            Message message = null;

            await bot.SendTextMessageAsync(message.Chat.Id, """");
        }
    }
}";
            var expected = new[]
            {
                new DiagnosticResult()
                {
                    Id = DiagnosticIDs.MessageChatInsteadOfMessageChatId,
                    Message = "Use message.Chat instead of message.Chat.Id",
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[] {
                            new DiagnosticResultLocation("Test0.cs", 11, 44)
                        }
                }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = test.Replace("message.Chat.Id", "message.Chat");
            VerifyCSharpFix(test, fixtest);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return Analyzers.MessageChatInsteadOfMessageChatId.Instance;
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new TelegramBotAnalyzers();
        }
    }
}
