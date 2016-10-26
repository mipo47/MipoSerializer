using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;

namespace MipoSerializer.Serialize
{
	public partial class AltSerialization
	{
#if TRACE
		int indent = 0;
#endif

		public void Serialize(object value, Type type = null, TypeID? typeId = null)
		{
			if (value == null)
			{
				typeId = TypeID.Null;
				Writer.Write((byte)typeId.Value);
			}
			else if (typeId == null)
			{
				if (type != null)
					typeId = GetTypeID(type);
				else
				{
					typeId = GetTypeID(value.GetType());
					Writer.Write((byte)typeId.Value);
				}
			}				

#if TRACE
			string tostr = value != null ? value.GetType().Name : string.Empty;
			try
			{
				tostr = value + " " + tostr;
			}
			catch { }
			if (field != null)
			{
				tostr = tostr + " (Field " + field.Name + ")";
				field = null;
			}
			tostr = new string('\t', indent) + "Serialize " + tostr + " as " + typeId;
			Debug.WriteLine(tostr);
			indent++;
#endif
			switch (typeId.Value)
			{
				case TypeID.Null: break;
				case TypeID.DBNull: break;
				case TypeID.String:
					var str = (string)value;
					Writer.Write(str);
					//var bytes = Encoding.UTF8.GetBytes(str);
					//Writer.Write(bytes.Length);
					//Writer.Write(bytes);
					break;
				case TypeID.Int32: Writer.Write((int)value); break;
				case TypeID.Boolean: Writer.Write((bool)value); break;
				case TypeID.DateTime: Writer.Write(((DateTime)value).Ticks); break;
				case TypeID.Decimal:
					int[] bits = Decimal.GetBits((Decimal)value);
					for (int index = 0; index < 4; ++index)
						Writer.Write(bits[index]);
					break;
				case TypeID.Byte: Writer.Write((byte)value); break;
				case TypeID.Char: Writer.Write((char)value); break;
				case TypeID.Single: Writer.Write((float)value); break;
				case TypeID.Double: Writer.Write((double)value); break;
				case TypeID.SByte: Writer.Write((sbyte)value); break;
				case TypeID.Int16: Writer.Write((short)value); break;
				case TypeID.Int64: Writer.Write((long)value); break;
				case TypeID.UInt16: Writer.Write((ushort)value); break;
				case TypeID.UInt32: Writer.Write((uint)value); break;
				case TypeID.UInt64: Writer.Write((ulong)value); break;
				case TypeID.TimeSpan: Writer.Write(((TimeSpan)value).Ticks); break;
				case TypeID.Guid:
					byte[] buffer = ((Guid)value).ToByteArray();
					Writer.Write(buffer);
					break;
				case TypeID.IntPtr:
					IntPtr num = (IntPtr)value;
					if (IntPtr.Size == 4)
						Writer.Write(num.ToInt32());
					else
						Writer.Write(num.ToInt64());
					break;
				case TypeID.UIntPtr:
					UIntPtr unum = (UIntPtr)value;
					if (UIntPtr.Size == 4)
						Writer.Write(unum.ToUInt32());
					else
						Writer.Write(unum.ToUInt64());
					break;
#if Bluedragon
				case TypeID.FastMap: SerializeFastMap((com.nary.util.FastMap)value); break;
				case TypeID.VectorArray: SerializeVectorArray((com.nary.util.VectorArrayList)value); break;
#endif
				case TypeID.Dictionary: SerializeDictionary((IDictionary)value); break;
				case TypeID.Array: SerializeArray((Array)value, type); break;
				case TypeID.List: SerializeList((IList)value); break;
				case TypeID.ISerializable: SerializeSerializable((ISerializable)value, type); break;
				case TypeID.DataTable: SerializeDataTable((System.Data.DataTable)value); break;
				case TypeID.Binary: SerializeBinary(value); break;
				case TypeID.Object: SerializeObject(value, type); break;
			}
#if TRACE
			indent--;
#endif
		}
	}
}
