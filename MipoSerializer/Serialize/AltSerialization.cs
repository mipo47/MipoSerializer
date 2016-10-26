using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Collections;
using System.Runtime.Serialization;
using System.Collections.Generic;
using MipoSerializer.Common;

namespace MipoSerializer.Serialize
{
	public partial class AltSerialization
	{
		public static object Locker = new object();

		public Stream Stream { get; protected set; }
		public BinaryReader Reader { get; protected set; }
		public BinaryWriter Writer { get; protected set; }

		public static AltTypes AltTypes = new AltTypes();
		public static Dictionary<string, Type> TypesByName = new Dictionary<string, Type>();

		Dictionary<Type, byte> types;
		List<Type> typeList;

		Dictionary<object, UInt16> values;
		List<object> valueList;

		//Stack<Type> currentTypes = new Stack<Type>();
		//volatile Type currentType = null;

		public AltSerialization(BinaryReader reader)
		{
			Reader = reader;
			Stream = reader.BaseStream;
			valueList = new List<object>();
			typeList = new List<Type>();
		}

		public AltSerialization(BinaryWriter writer)
		{
			Writer = writer;
			Stream = writer.BaseStream;
			values = new Dictionary<object, UInt16>(new AltComparer());
			types = new Dictionary<Type, byte>();
		}

		public static byte[] SerializeToBytes(object value)
		{
			var memory = new MemoryStream();
			Serialize(memory, value);
			return memory.ToArray();
		}

		public static object DeserializeFromBytes(byte[] bytes)
		{
			var memory = new MemoryStream(bytes);
			return Deserialize(memory);
		}

		public static void Serialize(Stream stream, object value)
		{
			var writer = new BinaryWriter(stream);
			var s = new AltSerialization(writer);
			s.Serialize(value);
			s.Dispose();
		}

		public static object Deserialize(Stream stream)
		{
			var reader = new BinaryReader(stream);
			var s = new AltSerialization(reader);
			var value = s.Deserialize();
			s.Dispose();
			return value;
		}

		public void Dispose()
		{
			if (Writer != null)
			{
				Writer.Flush();
				Writer.Close();
				values = null;
				types = null;
			}
			else if (Reader != null)
			{
				Reader.Close();
				typeList = null;
				valueList = null;
			}
		}

		//TypeID GetTypeID(object value)
		//{
		//    TypeID typeID;
		//    if (value == null) typeID = TypeID.Null;
		//    else if (value is string) typeID = TypeID.String;
		//    else if (value is int) typeID = TypeID.Int32;
		//    else if (value is bool) typeID = TypeID.Boolean;
		//    else if (value is DateTime) typeID = TypeID.DateTime;
		//    else if (value is Decimal) typeID = TypeID.Decimal;
		//    else if (value is byte) typeID = TypeID.Byte;
		//    else if (value is char) typeID = TypeID.Char;
		//    else if (value is float) typeID = TypeID.Single;
		//    else if (value is double) typeID = TypeID.Double;
		//    else if (value is sbyte) typeID = TypeID.SByte;
		//    else if (value is short) typeID = TypeID.Int16;
		//    else if (value is long) typeID = TypeID.Int64;
		//    else if (value is ushort) typeID = TypeID.UInt16;
		//    else if (value is uint) typeID = TypeID.UInt32;
		//    else if (value is ulong) typeID = TypeID.UInt64;
		//    else if (value is TimeSpan) typeID = TypeID.TimeSpan;
		//    else if (value is Guid) typeID = TypeID.Guid;
		//    else if (value is IntPtr) typeID = TypeID.IntPtr;
		//    else if (value is UIntPtr) typeID = TypeID.UIntPtr;
		//    //else if (value is IDictionary) typeID = TypeID.Dictionary;
		//    else if (value is Array) typeID = TypeID.Array;
		//    //else if (value is IList) typeID = TypeID.List;
		//    else typeID = TypeID.Object;
		//    return typeID;
		//}

		TypeID GetTypeID(Type type)
		{
			TypeID typeID;
			if (type == null) typeID = TypeID.Null;
			else if (type == typeof(DBNull)) typeID = TypeID.DBNull;
			else if (type == typeof(string)) typeID = TypeID.String;
			else if (type == typeof(int)) typeID = TypeID.Int32;
			else if (type == typeof(bool)) typeID = TypeID.Boolean;
			else if (type == typeof(DateTime)) typeID = TypeID.DateTime;
			else if (type == typeof(Decimal)) typeID = TypeID.Decimal;
			else if (type == typeof(byte)) typeID = TypeID.Byte;
			else if (type == typeof(char)) typeID = TypeID.Char;
			else if (type == typeof(float)) typeID = TypeID.Single;
			else if (type == typeof(double)) typeID = TypeID.Double;
			else if (type == typeof(sbyte)) typeID = TypeID.SByte;
			else if (type == typeof(short)) typeID = TypeID.Int16;
			else if (type == typeof(long)) typeID = TypeID.Int64;
			else if (type == typeof(ushort)) typeID = TypeID.UInt16;
			else if (type == typeof(uint)) typeID = TypeID.UInt32;
			else if (type == typeof(ulong)) typeID = TypeID.UInt64;
			else if (type == typeof(TimeSpan)) typeID = TypeID.TimeSpan;
			else if (type == typeof(Guid)) typeID = TypeID.Guid;
			else if (type == typeof(IntPtr)) typeID = TypeID.IntPtr;
			else if (type == typeof(UIntPtr)) typeID = TypeID.UIntPtr;
			else if (type == typeof(System.Data.DataTable)) typeID = TypeID.DataTable;
			else if (typeof(System.Data.DataTable).IsAssignableFrom(type)) typeID = TypeID.DataTable;
			else if (type.IsGenericType && type.BaseType == typeof(object) && typeof(IDictionary).IsAssignableFrom(type)) typeID = TypeID.Dictionary;
			else if (type.IsArray) typeID = TypeID.Array;
			//else if (type == typeof(IList)) typeID = TypeID.List;
			//else if (typeof(ISerializable).IsAssignableFrom(type)) typeID = TypeID.Binary;
			else typeID = TypeID.Object;
			return typeID;
		}

		public enum TypeID : byte
		{
			Null = 128,
			Object = 0,
			String = 1, Int32 = 2, Boolean = 3, DateTime = 4,
			Decimal = 5, Byte = 6, Char = 7, Single = 8, 
			Double = 9, SByte = 10, Int16 = 11, Int64 = 12, 
			UInt16 = 13, UInt32 = 14, UInt64 = 15, TimeSpan = 16, 
			Guid = 17, IntPtr = 18, UIntPtr = 19, 
			Array = 21, ISerializable = 22,
			Dictionary = 23, List = 24, Binary = 25, DataTable = 26, DBNull = 27,
			FastMap = 40, VectorArray = 41
		}
	}
}