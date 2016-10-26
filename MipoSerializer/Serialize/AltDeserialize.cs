//#define SHOWCALLS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using MipoSerializer;

namespace MipoSerializer.Serialize
{
	public partial class AltSerialization
	{
		public object Deserialize(Type type = null, TypeID? typeId = null)
		{
			object obj = null;
			if (typeId == null)
			{
				if (type != null)
					typeId = GetTypeID(type);
				else
					typeId = (TypeID)Reader.ReadByte();
			}

#if TRACE
			string tostr = string.Empty;
			if (field != null)
			{
				tostr = "Field " + field.Name;
				field = null;
			}
			Debug.WriteLine(new string('\t', indent) + "Deserialize " + tostr + " as " + typeId);
			indent++;
#endif
			switch (typeId)
			{
				case TypeID.Null: obj = null; break;
				case TypeID.DBNull: obj = DBNull.Value; break;
				case TypeID.String:
					//int length = Reader.ReadInt32();
					//byte[] bytes = new byte[length];
					//Reader.Read(bytes, 0, length);
					//obj = Encoding.UTF8.GetString(bytes);
					obj = Reader.ReadString();
					break;
				case TypeID.Int32: obj = Reader.ReadInt32(); break;
				case TypeID.Boolean: obj = Reader.ReadBoolean();	break;
				case TypeID.DateTime: obj = new DateTime(Reader.ReadInt64());	break;
				case TypeID.Decimal:
					int[] bits = new int[4];
					for (int index = 0; index < 4; ++index)
						bits[index] = Reader.ReadInt32();
					obj = new Decimal(bits);
					break;
				case TypeID.Byte: obj = Reader.ReadByte(); break;
				case TypeID.Char: obj = Reader.ReadChar(); break;
				case TypeID.Single: obj = Reader.ReadSingle(); break;
				case TypeID.Double: obj = Reader.ReadDouble(); break;
				case TypeID.SByte: obj = Reader.ReadSByte(); break;
				case TypeID.Int16: obj = Reader.ReadInt16(); break;
				case TypeID.Int64: obj = Reader.ReadInt64(); break;
				case TypeID.UInt16: obj = Reader.ReadUInt16(); break;
				case TypeID.UInt32: obj = Reader.ReadUInt32(); break;
				case TypeID.UInt64: obj = Reader.ReadUInt64(); break;
				case TypeID.TimeSpan: obj = new TimeSpan(Reader.ReadInt64()); break;
				case TypeID.Guid: obj = new Guid(Reader.ReadBytes(16)); break;
				case TypeID.IntPtr: 
					obj = IntPtr.Size != 4 ? new IntPtr(Reader.ReadInt64()) : new IntPtr(Reader.ReadInt32());
					break;
				case TypeID.UIntPtr:
					obj = UIntPtr.Size != 4 ? new UIntPtr(Reader.ReadUInt64()) : new UIntPtr(Reader.ReadUInt32());
					break;
#if Bluedragon
				case TypeID.FastMap: obj = DeserializeFastMap(); break;
				case TypeID.VectorArray: obj = DeserializeVectorArray(); break;
#endif
				case TypeID.Dictionary: obj = DeserializeDictionary(); break;
				case TypeID.Array: obj = DeserializeArray(type); break;
				case TypeID.List: obj = DeserializeList(); break;
				case TypeID.ISerializable: obj = DeserializeSerializable(type); break;
				case TypeID.DataTable: obj = DeserializeDataTable(); break;
				case TypeID.Binary: obj = DeserializeBinary(); break;
				case TypeID.Object: obj = DeserializeObject(type); break;
			}
#if TRACE
			indent--;
#endif
			return obj;
		}
	}
}
