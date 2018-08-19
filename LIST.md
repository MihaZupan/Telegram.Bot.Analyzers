#### TG0001

Defined in [`MessageChatInsteadOfMessageChatId.cs`]: *Message.Chat should be used instead of Message.Chat.Id*
```csharp
await Bot.SendTextMessageAsync(message.Chat.Id, "Hello");            
// is converted to
await Bot.SendTextMessageAsync(message.Chat, "Hello");
```

#### TG0002

Defined in [`MessageChatAndIdToMessage.cs`]: *Method call parameters can be simplified*

**This feature requires new extensions from the more-extensions branch**
```csharp
await Bot.EditMessageCaptionAsync(message.Chat, message.MessageId, "New caption");
// is converted to
await Bot.EditMessageCaptionAsync(message, "New caption");           
```


[`MessageChatInsteadOfMessageChatId.cs`]: https://github.com/MihaZupan/Telegram.Bot.Analyzers/blob/master/src/Telegram.Bot.Analyzers/Diagnostics/MessageChatInsteadOfMessageChatId.cs
[`MessageChatAndIdToMessage.cs`]: https://github.com/MihaZupan/Telegram.Bot.Analyzers/blob/master/src/Telegram.Bot.Analyzers/Diagnostics/MessageChatAndIdToMessage.cs