using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.VoiceNext;
using StandardLibrary.Data;
using StandardLibrary.Other;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode;

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
				parent: BotInitSettings.GetVoiceChannelCategory(ctx.Guild)).ThrowTaskException();

			await ctx.Message.DeleteAsync();
			var msg = await ctx.Channel.SendMessageAsync("Канал " + name + " создан!").ThrowTaskException();

			await Task.Delay(10000);

			await msg.DeleteAsync();

			while(channel.Users.Any()) Thread.Sleep(1);

			await channel.DeleteAsync();
		}

		[Command("mute")]
		public async Task MuteMember(CommandContext ctx, DiscordMember member, string reason, string time)
		{
			if(ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.KickMembers) == false) return;

			time = time == "-1" ? null : time;

			await member.RevokeRoleAsync(BotInitSettings.GetDefaultMemberRole(ctx.Guild)).ThrowTaskException();
			await member.GrantRoleAsync(BotInitSettings.GetMutedMemberRole(ctx.Guild)).ThrowTaskException();

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

		[Command("music")]
		public async Task PlayMusic(CommandContext ctx, string query)
		{
			await ctx.Message.DeleteAsync();

			var youClient = new YoutubeClient();

			var voice = ctx.Client.GetVoiceNext();

			if (voice.GetConnection(ctx.Guild) != null)
			{
				await ctx.Channel.SendMessageAsync(ctx.Member.Mention + " Бот занят! Он уже играет музыку. Подождите или присоединяйтесь.")
					.ContinueWith(s => { Thread.Sleep(5000); s.Result.DeleteAsync().Wait(); });
			}
			else
			{
				var video = (await youClient.Search.GetVideosAsync(query).BufferAsync(1))[0];

				var manifest = await youClient.Videos.Streams.GetManifestAsync(video.Id);
				var audioInfo = manifest.GetAudioOnly().First();

				var msg = await ctx.Channel.SendMessageAsync("Идёт скачивание подождите.....");

				await youClient.Videos.Streams.DownloadAsync(audioInfo, "temp.music");

				await msg.DeleteAsync();
				await ctx.Channel.SendMessageAsync("Скачивание завершено").ContinueWith(s => { Thread.Sleep(2000); s.Result.DeleteAsync().Wait(); });

				var connection = await voice.ConnectAsync(BotInitSettings.GetMusicChannel(ctx.Guild));
				var sink = connection.GetTransmitSink();

				await connection.SendSpeakingAsync(true);

				var ffmpeg = Process.Start(new ProcessStartInfo
				{
					FileName = "ffmpeglib\\ffmpeg.exe",
					Arguments = $@"-i ""temp.music"" -ac 2 -f s16le -ar 48000 pipe:1 -loglevel quiet",
					RedirectStandardOutput = true,
					UseShellExecute = false
				});

				var ffout = ffmpeg.StandardOutput.BaseStream;

				await ffout.CopyToAsync(sink);

				ffmpeg.Kill();

				await sink.FlushAsync();
				await connection.WaitForPlaybackFinishAsync();
			}
		}

		[Command("unmute")]
		public async Task UnmuteMember(CommandContext ctx, DiscordMember member)
		{
			if(ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.KickMembers) == false) return;

			await member.GrantRoleAsync(BotInitSettings.GetDefaultMemberRole(ctx.Guild)).ThrowTaskException();
			await member.RevokeRoleAsync(BotInitSettings.GetMutedMemberRole(ctx.Guild)).ThrowTaskException();

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
			await ctx.Member.GrantRoleAsync(BotInitSettings.GetDefaultMemberRole(ctx.Guild)).ThrowTaskException();
			await ctx.Message.DeleteAsync().ThrowTaskException();
			(await ctx.Channel.GetMessagesAsync().ThrowTaskException())
				.Where(s => s.Author.Id != BotInitSettings.ServersData[ctx.Guild.Id].Administrator).InvokeForAll(s => s.DeleteAsync().Wait());
		}

		[Command("subscribe-streams")]
		public async Task SubscribeToStreams(CommandContext ctx)
		{
			await ctx.Member.GrantRoleAsync(BotInitSettings.GetStreamSubscriberRole(ctx.Guild));
			var msg = await ctx.Channel.SendMessageAsync(ctx.Member.Mention + " подписался на стримы");
			await ctx.Message.DeleteAsync();

			await Task.Delay(3000);

			await msg.DeleteAsync();
		}

		[Command("unsubscribe-streams")]
		public async Task UnsubscribeFromStreams(CommandContext ctx)
		{
			await ctx.Member.RevokeRoleAsync(BotInitSettings.GetStreamSubscriberRole(ctx.Guild));
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
				await BotInitSettings.GetAnnountmentsChannel(ctx.Guild)
					.SendMessageAsync(BotInitSettings.GetStreamSubscriberRole(ctx.Guild).Mention + " " +
					ctx.Member.Mention + " стримт " + gameName + " [" + streamName + "] " + link).ThrowTaskException();

				await ctx.Message.DeleteAsync();
				//don't write anything after it
			}
			else
			{
				var msg = await BotInitSettings.GetAnnountmentsChannel(ctx.Guild)
					.SendMessageAsync(BotInitSettings.GetStreamSubscriberRole(ctx.Guild).Mention + " " +
					ctx.Member.Mention + " будует стримить " + gameName + " [" + streamName + "] через " + time.Value.ToString() + " мин.")
					.ThrowTaskException();

				var emoji = DiscordEmoji.FromName(ctx.Client, ":eyes:");
				await msg.CreateReactionAsync(emoji).ThrowTaskException();

				await ctx.Message.DeleteAsync();

				await Task.Delay(time.Value * 60 * 1000);

				await Announce(ctx, gameName, streamName, link);
			}
		}

		[Command("team-game")]
		public async Task CreateTeamGame(CommandContext ctx, string gameName, int targetMemberCount, string description)
		{
			var channel = BotInitSettings.GetTeamFindingChannel(ctx.Guild);

			var msg = await channel.SendMessageAsync(ctx.Member.Mention + " ищет напарника(ов) для игры в " +
				gameName + "\n" + "Доп. иформация: " + description +
				(targetMemberCount == -1 ? "" : ("\n\n" + "Для игры нужно " + targetMemberCount + " человек(а)")));

			await msg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":ok_hand:"));

			if (targetMemberCount == -1)
				await msg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":arrow_forward:"));

			await ctx.Message.DeleteAsync();

			var interact = ctx.Client.GetInteractivity();
			var dm = await ctx.Member.CreateDmChannelAsync();
			List<DiscordUser> userList;

			if(targetMemberCount != -1)
				while(true)
				{
					Func<MessageReactionAddEventArgs, bool> predecate = (s) => s.Message == msg && s.Emoji == DiscordEmoji.FromName(ctx.Client, ":ok_hand:");
					await interact.WaitForReactionAsync(predecate);

					var users = await msg.GetReactionsAsync(DiscordEmoji.FromName(ctx.Client, ":ok_hand:"));
					if (users.Count - 1 >= targetMemberCount)
					{
						userList = users.Where((s, i) => 0 <= i && i <= targetMemberCount - 1).ToList(); //Bot reaction ignore

						await dm.SendMessageAsync("Ваша игра готова к старту! Зайдите в канал и одобрите старт!");

						await msg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":arrow_forward:"));

						var retry = true;
						while(retry)
							retry = (await interact.WaitForReactionAsync
								(s => s.Message == msg && s.User == ctx.User && s.Emoji == DiscordEmoji.FromName(ctx.Client, ":arrow_forward:"))).TimedOut;

						break;
					}
				}
			else
			{
				Func<MessageReactionAddEventArgs, bool> predecate = (s) => s.Message == msg && s.Emoji == DiscordEmoji.FromName(ctx.Client, ":arrow_forward:");

				var retry = true;
				while(retry)
				{
					retry = (await interact.WaitForReactionAsync
						(s => s.Message == msg && s.Emoji == DiscordEmoji.FromName(ctx.Client, ":arrow_forward:"), new TimeSpan(0, 10, 0))).TimedOut;

					await dm.SendMessageAsync("Незабывайте про созданую игру. Возможно уже пора начинать!");
				}

				var users = await msg.GetReactionsAsync(DiscordEmoji.FromName(ctx.Client, ":ok_hand:"));
				userList = users.Where((s, i) => 0 <= i && i <= targetMemberCount - 1).ToList(); //Bot reaction ignore

			}

			var msg2 = await
				channel.SendMessageAsync("Игра в " + gameName + " запущена!" + "\n\n" + "Участники: " + string.Join(", ", userList.Select(s => s.Mention)));

			var name = "Играем в " + gameName;

			var overwrites = new DiscordOverwriteBuilder[] { new DiscordOverwriteBuilder() { Allowed = Permissions.All }.For(ctx.Member) };
			var voiceChannel = await ctx.Guild.CreateChannelAsync(name, ChannelType.Voice, overwrites: overwrites,
				parent: BotInitSettings.GetVoiceChannelCategory(ctx.Guild)).ThrowTaskException();

			var deleteTask = Task.Run(() =>
			{
				Thread.Sleep(10000);

				while(voiceChannel.Users.Any()) Thread.Sleep(1);
					voiceChannel.DeleteAsync();
			});

			await Task.Delay(3000);
			await msg.DeleteAsync();
			await Task.Delay(120000);
			await msg2.DeleteAsync();

			await deleteTask;
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


				//step 6
				await direct.SendMessageAsync("Enter \"/bot-init#select-channel\" in channel for team finding");
				step = (await interact.WaitForMessageAsync((s) => s.Author == ctx.Member && s.Content == "/bot-init#select-channel").ThrowTaskException()).Result;

				await step.Channel.SendMessageAsync("Channel selected");

				BotInitSettings.ServersData[ctx.Guild.Id].TeamFindingChannel = step.Channel.Id;


				//step 6
				await direct.SendMessageAsync("Enter \"/bot-init#select-channel\" in uncensor channel");
				step = (await interact.WaitForMessageAsync((s) => s.Author == ctx.Member && s.Content == "/bot-init#select-channel").ThrowTaskException()).Result;

				await step.Channel.SendMessageAsync("Channel selected");

				BotInitSettings.ServersData[ctx.Guild.Id].UncensorChannel = step.Channel.Id;

				//step auto 2
				BotInitSettings.ServersData[ctx.Guild.Id].MusicChannel =
					(await ctx.Guild.CreateChannelAsync("музыкальный канал", ChannelType.Voice,
					BotInitSettings.GetVoiceChannelCategory(ctx.Guild))).Id;

				await direct.SendMessageAsync("Setup end");
			}

			Program.SettingsSaver.Save("init", BotInitSettings.ServersData);
		}
	}
}
