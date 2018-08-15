using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace Telegram.Bot.Analyzers.Test
{
    [TestClass]
    public class MessageChatAndIdToMessageTests : CodeFixVerifier
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
            var bot = new Telegram.Bot.TelegramBotClient(""API Token"");
            Telegram.Bot.Types.Message message = null;

            await bot.EditMessageTextAsync(123, 123, """");
            await bot.SendTextMessageAsync(123, """");
            await bot.EditMessageLiveLocationAsync(message.Chat, message.MessageId, 123, 123);
            await bot.ForwardMessageAsync(message.Chat, fromChatId: message.Chat, messageId: message.MessageId);
        }
    }
}";
            var expected = new []
            {
                new DiagnosticResult()
                {
                    Id = DiagnosticIDs.MessageChatAndIdToMessage,
                    Message = "Use an overload for EditMessageLiveLocationAsync that takes a Message parameter, instead of Chat and MessageId",
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[] {
                            new DiagnosticResultLocation("Test0.cs", 13, 52)
                        }
                },
                new DiagnosticResult()
                {
                    Id = DiagnosticIDs.MessageChatAndIdToMessage,
                    Message = "Use an overload for ForwardMessageAsync that takes a Message parameter, instead of Chat and MessageId",
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[] {
                            new DiagnosticResultLocation("Test0.cs", 14, 57)
                        }
                }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
namespace TestNamespace
{
    class TestClass
    {
        static async void Test()
        {
            var bot = new Telegram.Bot.TelegramBotClient(""API Token"");
            Telegram.Bot.Types.Message message = null;

            await bot.EditMessageTextAsync(123, 123, """");
            await bot.SendTextMessageAsync(123, """");
            await bot.EditMessageLiveLocationAsync(message, 123, 123);
            await bot.ForwardMessageAsync(message.Chat, message);
        }
    }
}";
            VerifyCSharpFix(test, fixtest);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return Analyzers.MessageChatAndIdToMessage.Instance;
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new TelegramBotAnalyzers();
        }
    }
}
