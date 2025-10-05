# DiscordMailer

A SMTP server that forwards sent emails as .eml attachments in Discord DMs.

<img width="703" height="548" alt="image" src="https://github.com/user-attachments/assets/c64751ef-5657-42c9-9860-e957f79166e0" />

## How to use

1. Build with .NET 9
2. Provide Discord bot token as DISCORD_TOKEN environment variable
3. Run and send email via `localhost:9025` to `discord-id@discord.com` where discord-id is a Discord User ID (use Developer Mode in the Discord client to find this out)

## How do I view the emails?

Drag the attachment into your email client usually works.

## What is it for?

Mastodon instance, atproto PDS, etc. if you don't wanna set up a mailpit or a real email service (and deal with all the problems that creates)

## Troubleshooting

If you get a 403 Forbidden error when forwarding emails you probably need to share a server with the bot in order to receive DMs from it.

## Disclaimer

Don't expose the SMTP server publicly! It has no authentication and can easily be used for spam. If you need something more robust feel free to fork the code and add authentication.
