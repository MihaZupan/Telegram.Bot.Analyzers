using MihaZupan.CodeAnalysis.Framework;
using Telegram.Bot.Analyzers.Diagnostics;
using Telegram.Bot.Analyzers.Test.Framework;
using Xunit;

namespace Telegram.Bot.Analyzers.Test
{
    public class MessageChatAndIdToMessageTests : CodeFixVerifier
    {
        protected override DiagnosticBase CodeFixProvider => new MessageChatAndIdToMessage();

        [Fact]
        public void ActualTest()
        {
            var test = @"
void Test()
{
    TelegramBotClient bot = null;

    bot.EditMessageTextAsync(123, 123, """");
    bot.EditMessageLiveLocationAsync(message.Chat, message.MessageId, 123, 123);
    bot.SendTextMessageAsync(123, """");
    bot.ForwardMessageAsync(message.Chat, fromChatId: message.Chat, messageId: message.MessageId);
    bot.DeleteMessageAsync(e.CallbackQuery.Message.Chat, e.CallbackQuery.Message.MessageId);
}";

            var expected = new[]
            {
                GetDiagnosticResult("EditMessageLiveLocationAsync", 7, 38),
                GetDiagnosticResult("ForwardMessageAsync", 9, 43),
                GetDiagnosticResult("DeleteMessageAsync", 10, 28)
            };

            VerifyDiagnostic(test, expected);

            var fixtest = @"
void Test()
{
    TelegramBotClient bot = null;

    bot.EditMessageTextAsync(123, 123, """");
    bot.EditMessageLiveLocationAsync(message, 123, 123);
    bot.SendTextMessageAsync(123, """");
    bot.ForwardMessageAsync(message.Chat, message);
    bot.DeleteMessageAsync(e.CallbackQuery.Message);
}";
            VerifyFix(test, fixtest);
        }
    }
}
