#define USE_NULL_MAP

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;

namespace MipoSerializer.Serialize
{
	public partial class AltSerialization
	{
		void SerializeList(IList list)
		{
			Type type = list.GetType();
			SerializeType(type);
			Writer.Write(list.Count);

			if (list.Count == 0)
				return;

			var enumerator = list.GetEnumerator();
			while (enumerator.MoveNext())
				Serialize(enumerator.Current);
		}

		IList DeserializeList()
		{
			Type type = DeserializeType();
			int count = Reader.ReadInt32();
			IList list = (IList)Activator.CreateInstance(type);
			for (int i = 0; i < count; i++)
			{
				var value = Deserialize();
				list.Add(value);
			}
			return list;
		}

		void SerializeArray(Array array, Type type = null)
		{
			//TODO: support muldtidimensional arrays
			if (array.Rank > 1)
				throw new Exception("Multidimentional arrays are not supported");

			if (type == null)
			{
				type = array.GetType();
				SerializeType(type);
			}

			Writer.Write(array.Length);
			if (array.Length == 0)
				return;

			var itemType = type.GetElementType();
			TypeID? typeId = null;
			if (!itemType.IsSealed)
				itemType = null;
			else
				typeId = GetTypeID(itemType);

			object value;
#if USE_NULL_MAP
			byte[] nullMap = null;
			if (itemType == null || !itemType.IsPrimitive)
			{
				nullMap = AltSerialization.nullMap;
				if (array.Length > nullMap.Length * 8)
					nullMap = new byte[1 + (array.Length - 1) / 8];

				int bitNr = 0;
				byte bitInByte;
				for (int i = 0; i < array.Length; i++)
				{
					value = array.GetValue(i);
					bitInByte = (byte)(0x1 << (bitNr % 8));
					if (value == null)
						nullMap[bitNr / 8] &= (byte)(0xFF ^ bitInByte);
					else
						nullMap[bitNr / 8] |= bitInByte;
					bitNr++;
				}
				Writer.Write(nullMap, 0, 1 + (array.Length - 1) / 8);
			}
#endif
			for (int i = 0; i < array.Length; i++)
			{
				value = array.GetValue(i);
#if USE_NULL_MAP
				if (value != null || nullMap == null)
#endif
					Serialize(value, itemType, typeId);
			}
		}

		Array DeserializeArray(Type type = null)
		{
			if (type == null)
				type = DeserializeType();

			int count = Reader.ReadInt32();

			var itemType = type.GetElementType();
			Array array;
			array = Array.CreateInstance(itemType, count);

			if (count == 0)
				return array;

			TypeID? typeId = null;
			if (!itemType.IsSealed)
				itemType = null;
			else
				typeId = GetTypeID(itemType);

#if USE_NULL_MAP
			byte[] nullMap = null;
			int bitNr = 0;
			byte bitInByte;
			if (itemType == null || !itemType.IsPrimitive)
			{
				nullMap = new byte[1 + (array.Length - 1) / 8];
				Reader.Read(nullMap, 0, 1 + (array.Length - 1) / 8);
			}
#endif
			object value;
			for (int i = 0; i < count; i++)
			{
#if USE_NULL_MAP
				if (nullMap != null)
				{
					bitInByte = (byte)(0x1 << (bitNr % 8));
					if ((nullMap[bitNr++ / 8] & bitInByte) == 0)
						continue;
				}
#endif
				value = Deserialize(itemType, typeId);
				array.SetValue(value, i);
			}
			return array;
		}
	}
}
