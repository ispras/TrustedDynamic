using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using SharpFuzz;

using System.Text.Json;


namespace CoreFX
{
	public class Program
	{

		public static void Main(string[] args)
		{
			Run(JsonDocument_Parse);
		}

		private static void Run(Action<Stream> action) => Fuzzer.OutOfProcess.Run(action);
		private static void Run(Action<String> action) => Fuzzer.OutOfProcess.Run(action);

		private static void JsonDocument_Parse(Stream stream)
		{
			try
			{
				JsonDocument.Parse(stream);
			}
			catch (JsonException) { }
			catch (ArgumentException) { }
		}

		private static void JsonDocument_Parse(ReadOnlySpan<byte> data)
		{
			try
			{
				JsonDocument.Parse(data.ToArray());
			}
			catch (JsonException) { }
			catch (ArgumentException) { }
		}
	}
}