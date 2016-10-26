//#define SAFE_COPY
using System;
using System.Collections.Generic;

using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Compression;
using MipoSerializer.Serialize;

namespace MipoSerializer
{
    public static class Serialization
    {
		const bool USE_ALT = true;

		public static void SerializeToFile(string path, object value, byte useCompression, bool altSerialization = USE_ALT)
		{
			var file = new FileStream(path, FileMode.CreateNew);
			var data = SerializeToStream(value, useCompression, altSerialization).ToArray();
			file.Write(data, 0, data.Length);
			file.Close();
		}

		public static byte[] SerializeToBytes(object value, byte useCompression, bool altSerialization = USE_ALT)
		{
//#if TRACE
			//var path = @"Z:\web\web3.draugas.lt\temp\" + value.GetType().Name + "_" + DateTime.Now.Ticks + "_" + (altSerialization ? 'A' : 'B') + ".ser";
			//SerializeToFile(path, value, useCompression, altSerialization);
//#endif

			var stream = SerializeToStream(value, useCompression, altSerialization);
			var array = stream.ToArray();
			return array;
		}

		public static MemoryStream SerializeToStream(object value, byte useCompression, bool altSerialization = USE_ALT)
        {
            using (var memory = new MemoryStream())
            {
				if (altSerialization)
				{
					memory.WriteByte((byte)'a');
					memory.WriteByte(useCompression);
				}
				else
				{
					if (useCompression > 0)
					{
						memory.WriteByte((byte)'c');
						memory.WriteByte(useCompression);
					}
					else
						memory.WriteByte((byte)'b');
				}

				Stream gzip = null;
				switch (useCompression)
				{
					case 0: gzip = memory; break;
					case 1: gzip = new GZipStream(memory, CompressionMode.Compress); break;
					case 2: gzip = new DeflateStream(memory, CompressionMode.Compress); break;
					//case 3: gzip = new SevenZip.LzmaEncodeStream(memory); break;
				}
                using (gzip)
                {
					if (altSerialization)
#if !SAFE_COPY
						lock (AltSerialization.Locker) 
#endif
							AltSerialization.Serialize(gzip, value);
					else
					{
						var bi = new BinaryFormatter();
						bi.Serialize(gzip, value);
					}
                }
                return memory;
            }
        }

		public static object DeserializeFromFile(string path)
		{
			var stream = new FileStream(path, FileMode.Open);
			return DeserializeFromStream(stream);
		}

		public static object DeserializeFromBytes(byte[] data)
		{
			if (data == null || data.Length == 0)
				return null;

			var stream = new MemoryStream(data);
			return DeserializeFromStream(stream);
		}

		public static object DeserializeFromStream(Stream stream)
		{
			int dataType = stream.ReadByte();
			if (dataType < 0)
				return null;

			object o = null;
			byte useCompression = 0;
			Stream gzip = null;

			if (dataType == 'c' || dataType == 'a')
				useCompression = (byte)stream.ReadByte();

			switch (useCompression)
			{
				case 0: gzip = stream; break;
				case 1: gzip = new GZipStream(stream, CompressionMode.Decompress); break;
				case 2: gzip = new DeflateStream(stream, CompressionMode.Decompress); break;
				//case 3: gzip = new SevenZip.LzmaDecodeStream(stream); break;
			}
			using (gzip)
			{
				if (dataType == 'a')
#if !SAFE_COPY
					lock (AltSerialization.Locker) 
#endif
						o = AltSerialization.Deserialize(gzip);
				else
				{
					var bi = new BinaryFormatter();
					o = bi.Deserialize(gzip);
				}
			}
			return o;
		}
    }
}
