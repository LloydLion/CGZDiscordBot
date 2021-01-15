using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZDiscordBot
{
	class CommandHandler : BaseCommandModule
	{
		[Command("hello")]
		public async Task Hello(CommandContext ctx)
		{
			var targetMember = ctx.Member;

			await ctx.Channel.SendMessageAsync($"Hello {targetMember.Mention}");
		}

		[Command("create")]
		public async Task CreateChanel(CommandContext ctx, string typeStr, string name)
		{
			//if(ctx.Channel != ServerDefine.CreateChannelChannel) return;

			var type = (ChannelType)Enum.Parse(typeof(ChannelType), typeStr);

			var overwrites = new DiscordOverwriteBuilder[] { new DiscordOverwriteBuilder() { Allowed = Permissions.All ^ Permissions.Administrator }.For(ctx.Member) };

			switch (type)
			{
				case ChannelType.Text:
					await ctx.Guild.CreateChannelAsync(name, ChannelType.Text, overwrites: overwrites).ThrowTaskException();
					break;

				case ChannelType.Voice:
					await ctx.Guild.CreateChannelAsync(name, ChannelType.Voice, overwrites: overwrites).ThrowTaskException();
					break;

				default: return;
			}

			await ctx.Channel.SendMessageAsync("Channel created").ThrowTaskException();
		}

		[Command("msg-info")]
		public async Task MessageInfo(CommandContext ctx, params string[] msg)
		{
			StringBuilder builder = new StringBuilder();

			builder.AppendLine("Original message - " + ctx.Message.Content);
			builder.AppendLine("Modificated original message - " + string.Join(" ", ctx.Message.Content.ToCharArray()));
			builder.AppendLine("Message Id - " + ctx.Message.Id);
			builder.AppendLine("Chanel Id/Guid id - " + ctx.Channel.Id + "\t\t" + ctx.Channel.GuildId);
			builder.AppendLine("Author Id/Guid id - " + ctx.Member.Id + "\t\t" + ctx.Member.Guild);

			await ctx.Channel.SendMessageAsync(builder.ToString());
		}
	}
}
