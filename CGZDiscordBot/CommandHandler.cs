using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using StandardLibrary.Data;
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
		public async Task CreateChanel(CommandContext ctx, string name)
		{
			if(ctx.Channel != BotInitSettings.ServersData[ctx.Guild.Id].VoiceChannelCreationChannel) return;

			var overwrites = new DiscordOverwriteBuilder[] { new DiscordOverwriteBuilder() { Allowed = Permissions.All }.For(ctx.Member) };
			await ctx.Guild.CreateChannelAsync(name, ChannelType.Voice, overwrites: overwrites, parent: BotInitSettings.ServersData[ctx.Guild.Id].VoiceChannelCategory).ThrowTaskException();
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

		[Command("st-init-dialog")]
		public async Task InitBot(CommandContext ctx)
		{

			//init check
			if(BotInitSettings.ServersData.ContainsKey(ctx.Guild.Id)) return;

			BotInitSettings.ServersData.Add(ctx.Guild.Id, new BotInitSettings());

			var direct = await ctx.Member.CreateDmChannelAsync().ThrowTaskException();
			await direct.SendMessageAsync("Enter START to start setup or CLOSE to dismiss. Auto dismiss after 3 mins").ThrowTaskException();

			var interact = ctx.Client.GetInteractivity();

			var initStep = await interact.WaitForMessageAsync((s) => s.Channel == direct, new TimeSpan(0, 3, 0));
			if(initStep.TimedOut || initStep.Result.Content == "CLOSE")
			{
				await direct.SendMessageAsync("Setup dismissed").ThrowTaskException();
				return;
			}
			else
			{
				//step 1
				await direct.SendMessageAsync("Enter \"/bot-init#select-channel\" in channel for voice channel creation");
				var step = (await interact.WaitForMessageAsync((s) => s.Author == ctx.Member && s.Content == "/bot-init#select-channel").ThrowTaskException()).Result;

				await step.Channel.SendMessageAsync("Channel selected");

				BotInitSettings.ServersData[ctx.Guild.Id].VoiceChannelCreationChannel = step.Channel;

				//step 2
				await direct.SendMessageAsync("Enter \"/bot-init#select-caterogy\" in any channel in category for voice channel creation");
				step = (await interact.WaitForMessageAsync((s) => s.Author == ctx.Member && s.Content == "/bot-init#select-caterogy").ThrowTaskException()).Result;

				await step.Channel.SendMessageAsync("Channel selected");

				BotInitSettings.ServersData[ctx.Guild.Id].VoiceChannelCategory = step.Channel.Parent;


				await direct.SendMessageAsync("Setup end");
			}

			Program.SettingsSaver.Save("init", BotInitSettings.ServersData);
		}
	}
}
