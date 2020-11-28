using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZDiscordBot
{
	class CommandHandler
	{
		[Command("hello")]
		public async Task Hello(CommandContext ctx)
		{
			var targetMember = ctx.Member;

			await ctx.Channel.SendMessageAsync($"Hello {targetMember.Mention}");
		}

		[Command("create")]
		public async Task CreateChanel(CommandContext ctx)
		{

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
