using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZDiscordBot
{
	public static class Extensions
	{
		public static async Task<T> ThrowTaskException<T>(this Task<T> task)
		{
			var res = await task;
			if (task.Exception == null) return res;
			else throw task.Exception;
		}
	}
}
