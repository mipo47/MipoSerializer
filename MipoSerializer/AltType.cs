//#define SAFE_COPY
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using System.Web;
using System.ComponentModel;
using MipoSerializer.Common;
using MipoSerializer.Serialize;
//using System.Data.Objects.DataClasses;

namespace MipoSerializer
{
	public class AltTypes : Dictionary<Type, AltType>
	{ }

	public class AltType
	{
		public static bool INCLUDE_NONSERIALIZED = true;
		//public static volatile object locker = new object();

		AltTypes AltTypes;

		Type Type;
		AltType BaseType;
		IntPtr Declaration;

		byte[] data;
		//Queue<byte[]> datas = new Queue<byte[]>();

		public FieldInfo[] RefFields;
		public List<FieldInfo> AllRefFields;
		//public FieldInfo[] ValueFields;
		public ArraysQueue<object> AllRefValues;

		int fieldsSize;
		int valueFieldsSize;
		int valueFieldsStart;

		static unsafe IntPtr GetPtr(object value)
		{
			IntPtr ptr;
			TypedReference reference = __makeref(value);
			ptr = *(IntPtr*)(&reference);
			ptr = Marshal.ReadIntPtr(ptr);
			return ptr;
		}

		public static AltType GetAltType(AltTypes altTypes, Type type, object value)
		{
			if (altTypes.ContainsKey(type))
				return altTypes[type];

			var ptr = GetPtr(value);
			IntPtr declaration = IntPtr.Zero;
#if !SAFE_COPY
			//lock (locker)
			declaration = Marshal.ReadIntPtr(ptr);
#endif
			AltType altType = new AltType(altTypes, type, declaration);
			return altType;
		}

		static AltType GetAltType(AltTypes altTypes, Type type, IntPtr declaration)
		{
			if (altTypes.ContainsKey(type))
				return altTypes[type];

			var altType = new AltType(altTypes, type, declaration);

			//altTypes[type] = altType;
			return altType;
		}

		private AltType(AltTypes altTypes, Type type, IntPtr declaration)
		{
			Type = type;
			AltTypes = altTypes;
			Declaration = declaration;

			var allFields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			AllRefFields = new List<FieldInfo>(allFields.Where(f => !f.FieldType.IsValueType && f.DeclaringType == type && (INCLUDE_NONSERIALIZED || !f.IsNotSerialized)));

			//add fields from inherited classes
			var bType = type.BaseType;
			IntPtr baseDecl = IntPtr.Zero;
			if (bType != null && bType != typeof(object) && bType != typeof(ValueType))
			{
#if !SAFE_COPY
				//lock (locker)
				baseDecl = Marshal.ReadIntPtr(declaration, 16);
#endif
				BaseType = GetAltType(altTypes, bType, baseDecl);
				AllRefFields.AddRange(BaseType.AllRefFields);
			}
			AllRefValues = new ArraysQueue<object>(AllRefFields.Count);
			RefFields = AllRefFields.Where(f => f.DeclaringType == type).ToArray();

#if !SAFE_COPY
			//lock (locker)
			fieldsSize = Marshal.ReadInt32(declaration, 4) - 2 * IntPtr.Size; // without sync and declaration
			valueFieldsSize = fieldsSize - RefFields.Length * IntPtr.Size;
			if (BaseType != null)
				valueFieldsSize -= BaseType.fieldsSize;

			if (valueFieldsSize > 0)
			{
				data = new byte[valueFieldsSize];
				valueFieldsStart = IntPtr.Size + fieldsSize - valueFieldsSize;
			}
			AltTypes.Add(type, this);
#else
			ValueFields = allFields.Where(f => f.FieldType.IsValueType && f.DeclaringType == type && !f.IsNotSerialized).ToArray();
#endif
		}

		public void SerializeValueFields(object value, AltSerialization serialization)
		{
			IntPtr ptr = IntPtr.Zero;
#if !SAFE_COPY
			ptr = GetPtr(value);
#endif
			SerializeValueFields(value, serialization, ptr);
		}

		void SerializeValueFields(object value, AltSerialization serialization, IntPtr ptr)
		{
			if (BaseType != null)
				BaseType.SerializeValueFields(value, serialization, ptr);

#if SAFE_COPY
			foreach (var field in ValueFields)
				serialization.Serialize(field.GetValue(value), field.FieldType.IsSealed ? field.FieldType : null);
#else
			if (valueFieldsSize > 0)
			{
				//lock (locker)
				Marshal.Copy(ptr + valueFieldsStart, data, 0, valueFieldsSize);
				serialization.Writer.Write(data, 0, valueFieldsSize);
			}
#endif
		}

		public void DeserializeValueFields(object value, AltSerialization serialization)
		{
			IntPtr ptr = IntPtr.Zero;
#if !SAFE_COPY
			ptr = GetPtr(value);
#endif
			DeserializeValueFields(value, serialization, ptr);
		}

		void DeserializeValueFields(object value, AltSerialization serialization, IntPtr ptr)
		{
			if (BaseType != null)
				BaseType.DeserializeValueFields(value, serialization, ptr);

#if SAFE_COPY
			foreach (var field in ValueFields)
				field.SetValue(value, serialization.Deserialize(field.FieldType.IsSealed ? field.FieldType : null));
#else
			if (valueFieldsSize > 0)
			{
				//var data = new byte[valueFieldsSize];
				serialization.Reader.Read(data, 0, valueFieldsSize);
				//lock (locker)
				Marshal.Copy(data, 0, ptr + valueFieldsStart, valueFieldsSize);
			}
#endif
		}

		public override string ToString()
		{
			return string.Format("fieldSize = {0}, valueFieldSize = {1}, valueFieldStart = {2}", fieldsSize, valueFieldsSize, valueFieldsStart);
		}
	}
}
