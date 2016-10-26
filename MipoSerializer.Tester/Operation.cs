using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MipoSerializer.Tester
{
	class Operation
	{
		public string Name { get; set; }

		public Func<object> Action { get; set; }

		public TimeSpan Min { get; set; }
		public TimeSpan Max { get; set; }
		public TimeSpan Avg { get; set; }
		public TimeSpan AvgSans { get; set; }

		public string Results
		{
			get
			{
				return string.Format(@"{0}:
Min: {1:ss\.fff}
Max: {2:ss\.fff}
Avg: {3:ss\.fff}
AvgSans: {4:ss\.fff}
{5}
", Name, Min, Max, Avg, AvgSans, Action());
			}
		}

		public void SetTimes(IEnumerable<double> millis)
		{
			var times = new List<double>(millis);
			Min = TimeSpan.FromMilliseconds(times.Min());
			Max = TimeSpan.FromMilliseconds(times.Max());
			Avg = TimeSpan.FromMilliseconds(times.Average());
			if (times.Count >= 3)
			{
				times.Remove(times.Min());
				times.Remove(times.Max());
				AvgSans = TimeSpan.FromMilliseconds(times.Average());
			}
		}
	}
}
