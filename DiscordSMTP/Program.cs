// See https://aka.ms/new-console-template for more information

using System.Buffers;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MimeKit;
using SmtpServer;
using SmtpServer.Authentication;
using SmtpServer.ComponentModel;
using SmtpServer.Mail;
using SmtpServer.Protocol;
using SmtpServer.Storage;

Console.WriteLine("Hello, World!");

var options = new SmtpServerOptionsBuilder()
    .ServerName("localhost")
    .Endpoint(builder =>
        builder
            .Port(9025, false)
            .AllowUnsecureAuthentication(true))
    .Build();

var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .Build();

var builder = DiscordClientBuilder.CreateDefault(configuration["DISCORD_TOKEN"]!, DiscordIntents.None);
var client = builder.Build();

var services = new ServiceCollection();

services.AddSingleton<IMessageStore>(new SampleMessageStore(client));
services.AddSingleton<IMailboxFilterFactory>(new DelegatingMailboxFilterFactory(ctx => MailboxFilter.Default));
services.AddSingleton<IUserAuthenticatorFactory>(new DelegatingUserAuthenticatorFactory(ctx => UserAuthenticator.Default));

var smtpServer = new SmtpServer.SmtpServer(options, services.BuildServiceProvider());

await Task.WhenAll(
    smtpServer.StartAsync(CancellationToken.None),
    client.ConnectAsync(),
    Task.Delay(-1)
);

public class SampleMessageStore(DiscordClient client) : MessageStore
{
    public override async Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
    {
        await using var stream = new MemoryStream();

        var position = buffer.GetPosition(0);
        while (buffer.TryGet(ref position, out var memory))
        {
            await stream.WriteAsync(memory, cancellationToken);
        }

        stream.Position = 0;

        var message = await MimeMessage.LoadAsync(stream, cancellationToken);
        // Console.WriteLine(message.TextBody);

        var emlMs = new MemoryStream();
        await message.WriteToAsync(FormatOptions.Default, emlMs, cancellationToken);
        emlMs.Position = 0;

        foreach (var mailbox in transaction.To)
        {
            if (mailbox.Host == "discord.com")
            {
                var user = await client.GetUserAsync(ulong.Parse(mailbox.User));
                var dmChannel = await user.CreateDmChannelAsync();
                await dmChannel.SendMessageAsync(builder => builder
                    .WithContent($"Received an email from <mailto:{transaction.From.AsAddress()}>")
                    .AddFile("email.eml", emlMs, AddFileOptions.CopyStream | AddFileOptions.ResetStream));
            }
        }
        
        return SmtpResponse.Ok;
    }
}