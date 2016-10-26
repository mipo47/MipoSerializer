using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MipoSerializer.Tester
{
	class PerformanceMeter
	{
		bool debugToConsole = true;
		List<Operation> operations = new List<Operation>();

		public string Results
		{
			get
			{
				var results = new StringBuilder("----------- RESULTS -----------\n");
				foreach (var operation in operations)
				{
					results.AppendLine(operation.Results);
				}
				return results.ToString();
			}
		}

		public void AddAction(Func<object> action, string name = null)
		{
			operations.Add(new Operation
			{
				Name = name ?? "Action" + (operations.Count + 1),
				Action = action,
			});
		}

		public void Run(TimeSpan duration)
		{
			foreach (var operation in operations)
			{
				var times = new List<double>();

				var until = DateTime.Now + duration;
				do
				{
					var sw = Stopwatch.StartNew();
					operation.Action();
					sw.Stop();

					times.Add(sw.ElapsedMilliseconds);
					if (debugToConsole)
					{
						Console.WriteLine("took {0} ms to run action {1}",
							sw.ElapsedMilliseconds,
							operation.Name);
					}
				} while (DateTime.Now < until);

				operation.SetTimes(times);

				if (debugToConsole)
				{
					Console.WriteLine("average: {0}, min: {1}, max: {2}",
						times.Average(),
						times.Min(),
						times.Max());
				}
			}
		}
	}
}
