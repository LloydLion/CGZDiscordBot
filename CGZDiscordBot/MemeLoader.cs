using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using System.IO;
using System.Drawing.Imaging;
using DSharpPlus;
using StandardLibrary.Other;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace CGZDiscordBot
{
	class MemeLoader
	{
		private readonly DiscordClient client;
		private List<DiscordMessage> imageMsgs = new();


		public MemeLoader(DiscordClient client)
		{
			this.client = client;
			BotInitSettings.ServersData.InvokeForAll(s => BotInitSettings.GetBotImagesChannel
				(client.GetGuildAsync(s.Key).Result).GetMessagesAfterAsync(0).Result.InvokeForAll(s => s.DeleteAsync().Wait()));
		}

		public void LoadImage(Image image)
		{
			var guilds = BotInitSettings.ServersData.Keys;

			if (File.Exists("tmp" + Path.DirectorySeparatorChar + "tmp.png")) File.Delete("tmp" + Path.DirectorySeparatorChar + "tmp.png");
			image.Save("tmp" + Path.DirectorySeparatorChar + "tmp.png", ImageFormat.Png);
			var add = guilds.Select(s => BotInitSettings.GetBotImagesChannel(client.GetGuildAsync(s).Result)
				.SendFileAsync("tmp" + Path.DirectorySeparatorChar + "tmp.png").Result);
			imageMsgs.AddRange(add);
		}

		public IReadOnlyList<Uri> GetImages()
		{
			return imageMsgs.Select(s => 
			{
				return new Uri(s.Attachments[0].Url);
			}).ToList();
		}
	}
}
