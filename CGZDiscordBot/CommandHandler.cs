using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using StandardLibrary.Data;
using StandardLibrary.Other;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
			if (ctx.Channel.Id != BotInitSettings.ServersData[ctx.Guild.Id].VoiceChannelCreationChannel) return;

			var overwrites = new DiscordOverwriteBuilder[] { new DiscordOverwriteBuilder() { Allowed = Permissions.All }.For(ctx.Member) };
			var channel = await ctx.Guild.CreateChannelAsync(name, ChannelType.Voice, overwrites: overwrites,
				parent: BotInitSettings.ServersData[ctx.Guild.Id].GetVoiceChannelCategory(ctx.Guild)).ThrowTaskException();

			await ctx.Channel.SendMessageAsync("Channel created").ThrowTaskException();

			await Task.Delay(10000);

			while(channel.Users.Any()) Thread.Sleep(1);

			await channel.DeleteAsync();
		}

		[Command("mute")]
		public async Task MuteMember(CommandContext ctx, DiscordMember member, string reason, string time)
		{
			if(ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.KickMembers) == false) return;

			time = time == "-1" ? null : time;

			await member.RevokeRoleAsync(BotInitSettings.ServersData[ctx.Guild.Id].GetDefaultMemberRole(ctx.Guild)).ThrowTaskException();
			await member.GrantRoleAsync(BotInitSettings.ServersData[ctx.Guild.Id].GetMutedMemberRole(ctx.Guild)).ThrowTaskException();

			await ctx.Message.DeleteAsync();

			await ctx.Channel.SendMessageAsync(member.Mention + " заглушон на " + (time?.ToString() ?? "неограниченное время") + " по причине " + (reason ?? "*НЕЗАДАНО*"));

			if(time == null) return;
			else
			{
				var span = TimeSpan.Parse(time);
				await Task.Delay((int)span.TotalMilliseconds);

				await UnmuteMember(ctx, member).ThrowTaskException();
			}
		}

		[Command("unmute")]
		public async Task UnmuteMember(CommandContext ctx, DiscordMember member)
		{
			if(ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.KickMembers) == false) return;

			await member.GrantRoleAsync(BotInitSettings.ServersData[ctx.Guild.Id].GetDefaultMemberRole(ctx.Guild)).ThrowTaskException();
			await member.RevokeRoleAsync(BotInitSettings.ServersData[ctx.Guild.Id].GetMutedMemberRole(ctx.Guild)).ThrowTaskException();

			await ctx.Channel.SendMessageAsync(member.Mention + " теперь может говорить(и писать тоже)!");

			await ctx.Message.DeleteAsync();
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

		[Command("join")]
		public async Task HelloNewMember(CommandContext ctx)
		{
			await ctx.Member.GrantRoleAsync(BotInitSettings.ServersData[ctx.Guild.Id].GetDefaultMemberRole(ctx.Guild)).ThrowTaskException();
			await ctx.Message.DeleteAsync().ThrowTaskException();
			(await ctx.Channel.GetMessagesAsync().ThrowTaskException())
				.Where(s => s.Author.Id != BotInitSettings.ServersData[ctx.Guild.Id].Administrator).InvokeForAll(s => s.DeleteAsync().Wait());
		}

		[Command("subscribe-streams")]
		public async Task SubscribeToStreams(CommandContext ctx)
		{
			await ctx.Member.GrantRoleAsync(BotInitSettings.ServersData[ctx.Guild.Id].GetStreamSubscriberRole(ctx.Guild));
			var msg = await ctx.Channel.SendMessageAsync(ctx.Member.Mention + " подписался на стримы");
			await ctx.Message.DeleteAsync();

			await Task.Delay(3000);

			await msg.DeleteAsync();
		}

		[Command("unsubscribe-streams")]
		public async Task UnsubscribeFromStreams(CommandContext ctx)
		{
			await ctx.Member.RevokeRoleAsync(BotInitSettings.ServersData[ctx.Guild.Id].GetStreamSubscriberRole(ctx.Guild));
			var msg = await ctx.Channel.SendMessageAsync(ctx.Member.Mention + " отподписался от стримов");
			await ctx.Message.DeleteAsync();

			await Task.Delay(3000);

			await msg.DeleteAsync();
		}

		[Command("announce")]
		public async Task Announce(CommandContext ctx, string gameName, string streamName, string link, int? time = null)
		{
			if(time == null)
			{
				await BotInitSettings.ServersData[ctx.Guild.Id].GetAnnountmentsChannel(ctx.Guild)
					.SendMessageAsync(BotInitSettings.ServersData[ctx.Guild.Id].GetStreamSubscriberRole(ctx.Guild).Mention + " " +
					ctx.Member.Mention + " стримт " + gameName + " [" + streamName + "] " + link).ThrowTaskException();

				await ctx.Message.DeleteAsync();
			}
			else
			{
				await BotInitSettings.ServersData[ctx.Guild.Id].GetAnnountmentsChannel(ctx.Guild)
					.SendMessageAsync(BotInitSettings.ServersData[ctx.Guild.Id].GetStreamSubscriberRole(ctx.Guild).Mention + " " +
					ctx.Member.Mention + " будует стримить " + gameName + " [" + streamName + "] через " + time.Value.ToString() + " мин.")
					.ThrowTaskException();

				await Task.Delay(time.Value * 60 * 1000);

				await Announce(ctx, gameName, streamName, link);
			}
		}

		[Hidden]
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
				//step auto
				BotInitSettings.ServersData[ctx.Guild.Id].Administrator = ctx.Member.Id;

				//step 1
				await direct.SendMessageAsync("Enter \"/bot-init#select-channel\" in channel for voice channel creation");
				var step = (await interact.WaitForMessageAsync((s) => s.Author == ctx.Member && s.Content == "/bot-init#select-channel").ThrowTaskException()).Result;

				await step.Channel.SendMessageAsync("Channel selected");

				BotInitSettings.ServersData[ctx.Guild.Id].VoiceChannelCreationChannel = step.Channel.Id;

				//step 2
				await direct.SendMessageAsync("Enter \"/bot-init#select-caterogy\" in any channel in category for voice channel creation");
				step = (await interact.WaitForMessageAsync((s) => s.Author == ctx.Member && s.Content == "/bot-init#select-category").ThrowTaskException()).Result;

				await step.Channel.SendMessageAsync("Category selected");

				BotInitSettings.ServersData[ctx.Guild.Id].VoiceChannelCategory = step.Channel.Parent.Id;

				//step 3
				await direct.SendMessageAsync("Enter \"/bot-init#select-role @ROLEMENTION\" for default member role");
				step = (await interact.WaitForMessageAsync((s) => s.Author == ctx.Member && s.Content.StartsWith("/bot-init#select-role <@&")).ThrowTaskException()).Result;

				var role = ctx.Guild.GetRole(ulong.Parse(step.Content["/bot-init#select-role <@&".Length..^1]));

				await step.Channel.SendMessageAsync("Role selected");

				BotInitSettings.ServersData[ctx.Guild.Id].DefaultMemberRole = role.Id;

				//step 4
				await direct.SendMessageAsync("Enter \"/bot-init#select-channel\" in channel for annountments");
				step = (await interact.WaitForMessageAsync((s) => s.Author == ctx.Member && s.Content == "/bot-init#select-channel").ThrowTaskException()).Result;

				await step.Channel.SendMessageAsync("Channel selected");

				BotInitSettings.ServersData[ctx.Guild.Id].AnnountmentsChannel = step.Channel.Id;

				//step 5
				await direct.SendMessageAsync("Enter \"/bot-init#select-role @ROLEMENTION\" for stream sub role");
				step = (await interact.WaitForMessageAsync((s) => s.Author == ctx.Member && s.Content.StartsWith("/bot-init#select-role <@&")).ThrowTaskException()).Result;

				role = ctx.Guild.GetRole(ulong.Parse(step.Content["/bot-init#select-role <@&".Length..^1]));

				await step.Channel.SendMessageAsync("Role selected");

				BotInitSettings.ServersData[ctx.Guild.Id].StreamSubscriberRole = role.Id;

				//step 5
				await direct.SendMessageAsync("Enter \"/bot-init#select-role @ROLEMENTION\" for muted member");
				step = (await interact.WaitForMessageAsync((s) => s.Author == ctx.Member && s.Content.StartsWith("/bot-init#select-role <@&")).ThrowTaskException()).Result;

				role = ctx.Guild.GetRole(ulong.Parse(step.Content["/bot-init#select-role <@&".Length..^1]));

				await step.Channel.SendMessageAsync("Role selected");

				BotInitSettings.ServersData[ctx.Guild.Id].MutedMemberRole = role.Id;


				await direct.SendMessageAsync("Setup end");
			}

			Program.SettingsSaver.Save("init", BotInitSettings.ServersData);
		}
	}
}
