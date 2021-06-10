using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using SharpFuzz;

namespace CoreFX
{
	public class Program
	{
		[DataContract]
		public class Obj
		{
			[DataMember] public int A = 0;
			[DataMember] public double B = 0;
			[DataMember] public DateTime C = DateTime.MinValue;
			[DataMember] public bool D = false;
			[DataMember] public List<int> E = null;
			[DataMember] public string[] F = null;
		}

		public static void Main(string[] args)
		{
			Run(XmlSerializer_Deserialize);
		}

		private static void Run(Action<Stream> action) => Fuzzer.OutOfProcess.Run(action);
		private static void Run(Action<String> action) => Fuzzer.OutOfProcess.Run(action);

		private static void XmlSerializer_Deserialize(Stream stream)
		{
			var serializer = new XmlSerializer(typeof(Obj));

			try
			{
				serializer.Deserialize(stream);
			}
			catch (IndexOutOfRangeException) { }
			catch (InvalidOperationException) { }
			catch (XmlException) { }
		}

		private static void XmlSerializer_Deserialize(ReadOnlySpan<byte> data)
		{
			var serializer = new XmlSerializer(typeof(Obj));

			try
			{
				using (var stream = new MemoryStream(data.ToArray()))
				{
					serializer.Deserialize(stream);
				}
			}
			catch (IndexOutOfRangeException) { }
			catch (InvalidOperationException) { }
			catch (XmlException) { }
		}
	}
}