using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using StandardLibrary.Data;
using StandardLibrary.Other;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
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
			if(ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.KickMembers) == false)
			{
				var msg = await ctx.Channel.SendMessageAsync("У вас недостаточно прав для этого");
				await Task.Delay(3000);
				await msg.DeleteAsync();
				return;
			}

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

		[Command("unmute")]
		public async Task UnmuteMember(CommandContext ctx, DiscordMember member)
		{
			if(ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.KickMembers) == false)
			{
				var msg = await ctx.Channel.SendMessageAsync("У вас недостаточно прав для этого");
				await Task.Delay(3000);
				await msg.DeleteAsync();
				return;
			}

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

		[Command("send-meme")]
		public async Task SendMeme(CommandContext ctx, string message, string upTitle, string downTitle)
		{
			await ctx.Message.DeleteAsync();

			var images = Program.MemeLoader.GetImages();

			(Uri, Uri, Uri, Uri, Uri)[] imagesT = new (Uri, Uri, Uri, Uri, Uri)[(int)Math.Ceiling(images.Count / 5f)];

			for(int i = 0; i < images.Count / 5; i++)
			{
				imagesT[i].Item1 = images[i * 5 + 0];
				imagesT[i].Item2 = images[i * 5 + 1];
				imagesT[i].Item3 = images[i * 5 + 2];
				imagesT[i].Item4 = images[i * 5 + 3];
				imagesT[i].Item5 = images[i * 5 + 4];
			}

			if(images.Count % 5 != 0)
			{
				if(images.Count % 5 >= 1)
					imagesT[^1] = (images[(imagesT.Length - 1) * 5 + 0], null, null, null, null);
				if(images.Count % 5 >= 2)
					imagesT[^1] = (imagesT[^1].Item1, images[(imagesT.Length - 1) * 5 + 1], null, null, null);
				if(images.Count % 5 >= 3)
					imagesT[^1] = (imagesT[^1].Item1, imagesT[^1].Item2, images[(imagesT.Length - 1) * 5 + 2], null, null);
				if(images.Count % 5 >= 4)
					imagesT[^1] = (imagesT[^1].Item1, imagesT[^1].Item2, imagesT[^1].Item3, images[(imagesT.Length - 1) * 5 + 3], null);
			}

			DiscordEmbed[][] pages = imagesT.Select(s =>
			{
				var builder = new DiscordEmbedBuilder();
				var ret = new List<DiscordEmbed>();

				if(s.Item1 != null) ret.Add(builder.WithImageUrl(s.Item1).Build());
				if(s.Item2 != null) ret.Add(builder.WithImageUrl(s.Item2).Build());
				if(s.Item3 != null) ret.Add(builder.WithImageUrl(s.Item3).Build());
				if(s.Item4 != null) ret.Add(builder.WithImageUrl(s.Item4).Build());
				if(s.Item5 != null) ret.Add(builder.WithImageUrl(s.Item5).Build());

				return ret.ToArray();
			}).ToArray();

			int currentPage = 0;
			int imageIndex = 0;
			bool end = false;
			
			while(!end)
			{
				var msgs = pages[currentPage].Select(s => ctx.Channel.SendMessageAsync(embed: s).Result).ToArray();
				var targetMsg = msgs[^1];

				await targetMsg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":one:"));
				await targetMsg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":two:"));
				await targetMsg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":three:"));
				await targetMsg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":four:"));
				await targetMsg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":five:"));

				await targetMsg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":arrow_forward:"));
				await targetMsg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":arrow_backward:"));
				await targetMsg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":x:"));

				var interct = ctx.Client.GetInteractivity();

			InvalidEndji:
				var iRes = await interct.WaitForReactionAsync(s => s.Message == targetMsg && s.User == ctx.User);

				if (iRes.TimedOut) goto InvalidEndji;
				
				switch(iRes.Result.Emoji.Name)
				{
					case "▶️":
						currentPage++;
						break;
					case "◀️":
						currentPage--;
						break;
					case "1️⃣":
						imageIndex = 0;
						end = true;
						break;
					case "2️⃣":
						imageIndex = 1;
						end = true;
						break;
					case "3️⃣":
						imageIndex = 2;
						end = true;
						break;
					case "4️⃣":
						imageIndex = 3;
						end = true;
						break;
					case "5️⃣":
						imageIndex = 4;
						end = true;
						break;
					default:
						goto InvalidEndji;
				}

				msgs.InvokeForAll(s => s.DeleteAsync().Wait());
			}

			var targetUrl = pages[currentPage][imageIndex].Image.Url;

			var downloadClient = new WebClient();
			var mem = new MemoryStream(downloadClient.DownloadData(targetUrl.ToUri().OriginalString));
			var image = Image.FromStream(mem);

			var graphics = Graphics.FromImage(image);

			graphics.DrawString(upTitle, new Font("Tahoma", 60, FontStyle.Bold), Brushes.White, new PointF(image.Width / 2, 45),
				new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

			graphics.DrawString(downTitle, new Font("Tahoma", 60, FontStyle.Bold), Brushes.White, new PointF(image.Width / 2, image.Height - 45),
				new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

			graphics.Save();

			image.Save("tmp" + Path.DirectorySeparatorChar + "dtmp.png", System.Drawing.Imaging.ImageFormat.Png);
			await ctx.Channel.SendFileAsync("tmp" + Path.DirectorySeparatorChar + "dtmp.png", content: "[" + ctx.Member.Mention + "]: " + message);
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
				await direct.SendMessageAsync("Enter \"/bot-init#select-caterogy\" in any channel in category for voice channel creation");
				var step = (await interact.WaitForMessageAsync((s) => s.Author == ctx.Member && s.Content == "/bot-init#select-category").ThrowTaskException()).Result;

				await step.Channel.SendMessageAsync("Category selected");

				BotInitSettings.ServersData[ctx.Guild.Id].VoiceChannelCategory = step.Channel.Parent.Id;

				//step 2
				await direct.SendMessageAsync("Enter \"/bot-init#select-role @ROLEMENTION\" for default member role");
				step = (await interact.WaitForMessageAsync((s) => s.Author == ctx.Member && s.Content.StartsWith("/bot-init#select-role <@&")).ThrowTaskException()).Result;

				var role = ctx.Guild.GetRole(ulong.Parse(step.Content["/bot-init#select-role <@&".Length..^1]));

				await step.Channel.SendMessageAsync("Role selected");

				BotInitSettings.ServersData[ctx.Guild.Id].DefaultMemberRole = role.Id;

				//step 3
				await direct.SendMessageAsync("Enter \"/bot-init#select-channel\" in channel for annountments");
				step = (await interact.WaitForMessageAsync((s) => s.Author == ctx.Member && s.Content == "/bot-init#select-channel").ThrowTaskException()).Result;

				await step.Channel.SendMessageAsync("Channel selected");

				BotInitSettings.ServersData[ctx.Guild.Id].AnnountmentsChannel = step.Channel.Id;

				//step 4
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


				//step 7
				await direct.SendMessageAsync("Enter \"/bot-init#select-channel\" in uncensor channel");
				step = (await interact.WaitForMessageAsync((s) => s.Author == ctx.Member && s.Content == "/bot-init#select-channel").ThrowTaskException()).Result;

				await step.Channel.SendMessageAsync("Channel selected");

				BotInitSettings.ServersData[ctx.Guild.Id].UncensorChannel = step.Channel.Id;

				await direct.SendMessageAsync("Setup end");
			}

			Program.SettingsSaver.Save(ctx.Guild.Id, BotInitSettings.ServersData[ctx.Guild.Id]);
			Program.SettingsSaver.FlushToDefault();
		}
	}
}
