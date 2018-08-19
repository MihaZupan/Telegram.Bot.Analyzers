using MihaZupan.CodeAnalysis.Framework;
using Telegram.Bot.Analyzers.Diagnostics;
using Telegram.Bot.Analyzers.Test.Framework;
using Xunit;

namespace Telegram.Bot.Analyzers.Test
{
    public class MessageChatInsteadOfMessageChatIdTests : CodeFixVerifier
    {
        protected override DiagnosticBase CodeFixProvider => new MessageChatInsteadOfMessageChatId();

        [Fact]
        public void ActualTest()
        {
            var test = @"
static async void Test()
{
    TelegramBotClient bot = null;
    Message message = null;

    bot.SendTextMessageAsync(message.Chat.Id, """");
}";

            var expected = new[]
            {
                GetDiagnosticResult("message", 7, 30)
            };

            VerifyDiagnostic(test, expected);

            var fixtest = test.Replace("message.Chat.Id", "message.Chat");
            VerifyFix(test, fixtest);
        }
    }
}
