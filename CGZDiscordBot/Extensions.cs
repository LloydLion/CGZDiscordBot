using DSharpPlus.Entities;
using StandardLibrary.Data;
using StandardLibrary.Other;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZDiscordBot
{
	static class Extensions
	{
		public static async Task<T> ThrowTaskException<T>(this Task<T> task)
		{
			var res = await task;
			if (task.Exception == null) return res;
			else throw task.Exception;
		}

		public static async Task ThrowTaskException(this Task task)
		{
			await task;
			if(task.Exception != null) throw task.Exception;
		}

		public static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> obj, Dictionary<TKey, TValue> toAdd)
		{
			toAdd.InvokeForAll(s => obj.Add(s.Key, s.Value));
		}

		public static bool ContainsAnyElementOf<T>(this IEnumerable<T> collection, IEnumerable<T> check)
		{
			foreach(var item in check)
			{
				if(collection.Contains(item))
					return true;
			}

			return false;
		}
	}
}
