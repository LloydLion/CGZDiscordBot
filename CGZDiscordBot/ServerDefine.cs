using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZDiscordBot
{
	static class ServerDefine
	{
		public static void Init(DiscordClient client)
		{
			//CreateChannelChannel = client.Guilds.Values.ElementAt(0).GetChannel(780833811566166037);
		}


		public static DiscordChannel CreateChannelChannel { get; private set; }
	}
}
