using FizzWare.NBuilder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace MipoSerializer.Tester
{
	class Program
	{
		static void Main(string[] args)
		{
			var program = new Program();
			
			program.Peformance();
			//program.Test();

			Console.ReadKey(true);
		}

		public void Test()
		{
			var obj = Builder<SampleData>.CreateNew().Build();
			var bytes = Serialization.SerializeToBytes(obj, 0, true);
			Console.WriteLine(BitConverter.ToString(bytes));
		}

		public void Peformance()
		{
			var meter = new PerformanceMeter();
			var smallList = Builder<SampleData>.CreateListOfSize(100).Build().ToList();
			var list = Builder<SampleData>
				.CreateListOfSize(1000 * 10)
				.All()
				//TODO: fix performance with this array
				//.With(d => d.Scores = new double[100])
				//.With(d => d.Children = smallList)
				.Build().ToList();

			meter.AddAction(() =>
			{
				var serializer = new BinaryFormatter();
				var memory = new MemoryStream();
				serializer.Serialize(memory, list);
				memory.Seek(0, SeekOrigin.Begin);
				var restore = serializer.Deserialize(memory);
				return "Size: " + memory.Length;
			}, "BinaryFormatter");

			meter.AddAction(() =>
			{
				var bytes = Serialization.SerializeToBytes(list, 0, true);
				Serialization.DeserializeFromBytes(bytes);
				return "Size: " + bytes.Length;
			}, "MipoSerializer");

			//ProtoBuf.Serializer.PrepareSerializer<List<SampleData>>();
			meter.AddAction(() =>
			{
				var memory = new MemoryStream();
				ProtoBuf.Serializer.Serialize<List<SampleData>>(memory, list);
				memory.Seek(0, SeekOrigin.Begin);
				var restore = ProtoBuf.Serializer.Deserialize<List<SampleData>>(memory);
				return "Size: " + memory.Length;
			}, "ProtoBuf");

			meter.Run(TimeSpan.FromSeconds(2));
			Console.WriteLine();
			Console.WriteLine(meter.Results);
		}
	}
}
