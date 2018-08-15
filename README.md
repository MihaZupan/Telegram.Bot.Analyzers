# Telegram.Bot.Analyzers
Roslyn analyzers and code fixes for the Telegram.Bot library

## Installation

Install as [NuGet package](https://www.nuget.org/packages/Telegram.Bot.Analyzers/):

Package manager:

```powershell
Install-Package Telegram.Bot.Analyzers
```

## Diagnostics examples

#### TG0001 - Message.Chat should be used instead of Message.Chat.Id

```csharp
await Bot.SendTextMessageAsync(message.Chat.Id, "Hello");            
```
becomes
```csharp
await Bot.SendTextMessageAsync(message.Chat, "Hello");
```

#### TG0002 - Method call parameters can be simplified

```csharp
await Bot.EditMessageCaptionAsync(message.Chat, message.MessageId, "New caption");
```
becomes
```csharp
await Bot.EditMessageCaptionAsync(message, "New caption");           
```