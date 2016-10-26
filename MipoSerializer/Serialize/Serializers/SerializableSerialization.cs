using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;

namespace MipoSerializer.Serialize
{
	public partial class AltSerialization
	{
		static FieldInfo m_dataField = typeof(SerializationInfo).GetField("m_data", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
		static FieldInfo m_typesField = typeof(SerializationInfo).GetField("m_types", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

		//SerializationInfo si;

		void SerializeSerializable(ISerializable obj, Type type)
		{
			if (type == null)
			{
				type = obj.GetType();
				SerializeType(type);
			}
			var si = new SerializationInfo(type, new FormatterConverter());
			var sc = new StreamingContext();
			obj.GetObjectData(si, sc);

			var m_data = (object[]) m_dataField.GetValue(si);
			//var m_types = (Type[]) m_typesField.GetValue(si);
			//Writer.Write((UInt16) m_data.Length);
			for (int i = 0; i < m_data.Length; i++)
				Serialize(m_data[i]);
		}

		ISerializable DeserializeSerializable(Type type)
		{
			if (type == null)
				type = DeserializeType();

			var si = new SerializationInfo(type, new FormatterConverter());
			var sc = new StreamingContext();

			var m_data = (object[])m_dataField.GetValue(si);
			for (int i = 0; i < m_data.Length; i++)
				m_data[i] = Deserialize();
		
			var constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new Type[] { typeof(SerializationInfo), typeof(StreamingContext) }, null);
			object obj = constructor.Invoke(new object[] { si, sc });
			return (ISerializable) obj;
		}
	}
}
