using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZDiscordBot
{
	class SettingsSaver
	{
		public readonly Dictionary<ulong, BotInitSettings> values = new Dictionary<ulong, BotInitSettings>();


		public IReadOnlyDictionary<ulong, BotInitSettings> SettingsDictinary { get => values; }


		public SettingsSaver()
		{

		}


		public void Save(ulong guild, BotInitSettings settings)
		{
			if(values.TryAdd(guild, settings) == false) values[guild] = settings;
		}

		public BotInitSettings Get(ulong guild)
		{
			return values[guild];
		}

		public void FlushTo(Stream stream)
		{
			stream.Write(Encoding.Default.GetBytes(JsonConvert.SerializeObject(values)));
		}

		public void ConcatFrom(byte[] data)
		{
			values.AddRange(JsonConvert.DeserializeObject<Dictionary<ulong, BotInitSettings>>(Encoding.Default.GetString(data)));
		}
	}
}
