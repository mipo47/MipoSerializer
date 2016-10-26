using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MipoSerializer.Tester
{
	[Serializable, ProtoContract]
	class SampleData
	{
		[ProtoMember(1)]
		public int Id { get; set; }

		[ProtoMember(2)]
		public string Name { get; set; }

		[ProtoMember(3)]
		public DateTime Created { get; set; }

		[ProtoMember(4)]
		public double[] Scores { get; set; }

		//public bool b1, b2, b3, b4, b5, b6;

		//public long l1, l2;

		[ProtoMember(5)]
		public List<SampleData> Children { get; set; } 
	}
}
