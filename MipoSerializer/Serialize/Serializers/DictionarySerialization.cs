using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

namespace MipoSerializer.Serialize
{
	public partial class AltSerialization
	{
		//TODO: support null values (null string as value)
		void SerializeDictionary(IDictionary dic)
		{
			Type type = dic.GetType();
			var generic = type.GetGenericArguments();
			var keyType = generic[0];
			var valueType = generic[1];
			keyType = keyType.IsSealed ? keyType : null;
			valueType = valueType.IsSealed ? valueType : null;

			SerializeType(type);
			Writer.Write(dic.Count);
			foreach (var key in dic.Keys)
			{
				Serialize(key, keyType);
				Serialize(dic[key], valueType);
			}
			//SerializeObject(dic);
		}

		IDictionary DeserializeDictionary()
		{
			Type type = DeserializeType();
			var generic = type.GetGenericArguments();
			var keyType = generic[0];
			var valueType = generic[1];
			keyType = keyType.IsSealed ? keyType : null;
			valueType = valueType.IsSealed ? valueType : null;

			var dic = (IDictionary)Activator.CreateInstance(type);

			int count = Reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				var key = Deserialize(keyType);
				var value = Deserialize(valueType);
				dic.Add(key, value);
			}
			return dic;
			//return (IDictionary)DeserializeObject(reader);
		}
	}
}
