#define USE_NULL_MAP
//#define SERIALIZE_STATIC

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;
using System.Collections;

namespace MipoSerializer.Serialize
{
	public partial class AltSerialization
	{
		readonly bool ALL_SEALED = false;

#if TRACE
		FieldInfo field = null;
#endif

		object fieldValue;
		static byte[] nullMap = new byte[(UInt16.MaxValue + 1) / 8];

		void SerializeBinary(object value)
		{
			var binary = new BinaryFormatter();
			binary.Serialize(Stream, value);
		}

		object DeserializeBinary()
		{
			var binary = new BinaryFormatter();
			return binary.Deserialize(Stream);
		}

		void SerializeObject(object value, Type type = null)
		{
			ushort valueNr;
			if (!values.TryGetValue(value, out valueNr))
			{
				AltType altType;
				List<FieldInfo> fields;

				Writer.Write((byte)0);
				values.Add(value, (UInt16)(values.Count + 1));

				if (type == null)
				{
					type = value.GetType();
					SerializeType(type);
				}

				altType = AltType.GetAltType(AltTypes, type, value);
				altType.SerializeValueFields(value, this);
				fields = altType.AllRefFields;
				if (fields.Count == 0)
					return;

#if USE_NULL_MAP
				// Prepare null map
				int bitNr = 0;
				byte bitInByte;
				object[] fieldValues = altType.AllRefValues.Take();
				for (int i = 0; i < fields.Count; i++)
				{
					fieldValue = fields[i].GetValue(value);
					fieldValues[i] = fieldValue;
					bitInByte = (byte)(0x1 << (bitNr % 8));
					if (fieldValue != null)
						nullMap[bitNr / 8] |= bitInByte;
					else
						nullMap[bitNr / 8] &= (byte)(0xFF ^ bitInByte);
					bitNr++;
				}
				Writer.Write(nullMap, 0, 1 + (fields.Count - 1) / 8);
				bitNr = 0;
#endif
				
				// write non null reference values
				for (int i = 0; i < fields.Count; i++)
				{
					Type itemType = null;
#if USE_NULL_MAP
					fieldValue = fieldValues[i];
					if (fieldValue == null)
						continue;

					if (ALL_SEALED || fields[i].FieldType.IsSealed)
						itemType = fields[i].FieldType;
#else
					fieldValue = fields[i].GetValue(value);
#endif

#if TRACE
					field = fields[i];
#endif
					Serialize(fieldValue, itemType);
				}

#if USE_NULL_MAP
				altType.AllRefValues.Return(fieldValues);
#endif
			}
			else
			{
				Writer.Write((byte)(valueNr & 0xFF));
				Writer.Write((byte)(valueNr >> 8));
			}
		}

		object DeserializeObject(Type type = null)
		{
			UInt16 valueNr = Reader.ReadByte();
			if (valueNr == 0)
			{
				object value;
				AltType altType;
				List<FieldInfo> fields;

				if (type == null)
					type = DeserializeType();

				//var constructor = type.GetConstructor(Type.EmptyTypes);
				//if (constructor != null)
				//    value = constructor.Invoke(null);
				//else
					value = FormatterServices.GetSafeUninitializedObject(type);
				
				valueList.Add(value);

				altType = AltType.GetAltType(AltTypes, type, value);
				altType.DeserializeValueFields(value, this);
				fields = altType.AllRefFields;
				if (fields.Count == 0)
					return value;

#if USE_NULL_MAP
				int bitNr = 0;
				byte bitInByte;
				var nullMap = new byte[1 + (fields.Count - 1) / 8];
				Reader.Read(nullMap, 0, 1 + (fields.Count - 1) / 8);
#endif
				object fieldValue;
				for (int i = 0; i < fields.Count; i++)
				{
					Type itemType = null;
#if USE_NULL_MAP
					bitInByte = (byte)(0x1 << (bitNr % 8));
					if ((nullMap[bitNr++ / 8] & bitInByte) == 0)
						continue;

					if (ALL_SEALED || fields[i].FieldType.IsSealed)
						itemType = fields[i].FieldType;
#endif
#if TRACE
					field = fields[i];
#endif
					fieldValue = Deserialize(itemType);
					fields[i].SetValue(value, fieldValue);
				}
				return value;
			}
			else
			{
				valueNr |= (ushort)(Reader.ReadByte() << 8);
				return valueList[valueNr - 1];
			}
		}

		void SerializeType(Type type)
		{
			byte typeNr;
			if (!types.TryGetValue(type, out typeNr))
			{
				Writer.Write((byte)0);
				if (type.Name == "System" || type.Assembly.FullName.StartsWith("mscorlib,"))
					Writer.Write(type.FullName);
				else
					Writer.Write(type.AssemblyQualifiedName);

				types.Add(type, (byte)(types.Count + 1));

#if SERIALIZE_STATIC
				var staticFields = type.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
				foreach (var field in staticFields)
				{
					if (field.IsLiteral)
						continue;

					Type fieldType = null;
					if (ALL_SEALED || field.FieldType.IsSealed)
						fieldType = field.FieldType;

					var value = field.GetValue(null);
					Serialize(value, fieldType);
				}
#endif
			}
			else
				Writer.Write(typeNr);
		}

		Type DeserializeType()
		{
			var typeNr = Reader.ReadByte();
			Type type = null;
			if (typeNr == 0)
			{
				string typeName = Reader.ReadString();
				if (!TypesByName.TryGetValue(typeName, out type))
				{
					try
					{
						type = Type.GetType(typeName, true);
					}
					catch (Exception exc)
					{
						throw new Exception("Type not found: " + typeName, exc);
					}
					TypesByName.Add(typeName, type);
				}
				typeList.Add(type);

#if SERIALIZE_STATIC
				var staticFields = type.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
				foreach (var field in staticFields)
				{
					if (field.IsLiteral)
						continue;

					Type fieldType = null;
					if (ALL_SEALED || field.FieldType.IsSealed)
						fieldType = field.FieldType;

					var value = Deserialize(fieldType);
					field.SetValue(null, value);
				}
#endif
			}
			else
				type = typeList[typeNr - 1];
			return type;
		}
	}
}
