using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

namespace MipoSerializer.Common
{
	class AltComparer : IEqualityComparer<object>
	{
		new public bool Equals(object x, object y)
		{
			return RuntimeHelpers.GetHashCode(x) == RuntimeHelpers.GetHashCode(y);
			//return x == y || x.Equals(y);
		}

		public int GetHashCode(object obj)
		{
			//return obj.GetHashCode();
			return RuntimeHelpers.GetHashCode(obj);
		}
	}

	class TextComparer : IEqualityComparer<string>
	{
		/// <summary>
		/// Has a good distribution.
		/// </summary>
		const int _multiplier = 89;

		/// <summary>
		/// Whether the two strings are equal
		/// </summary>
		public bool Equals(string x, string y)
		{
			return x == y;
		}

		/// <summary>
		/// Return the hash code for this string.
		/// </summary>
		public int GetHashCode(string obj)
		{
			// Stores the result.
			int result = 0;

			// Don't compute hash code on null object.
			if (obj == null)
			{
				return 0;
			}

			// Get length.
			int length = obj.Length;

			// Return default code for zero-length strings [valid, nothing to hash with].
			if (length > 0)
			{
				// Compute hash for strings with length greater than 1
				char let1 = obj[0];          // First char of string we use
				char let2 = obj[length - 1]; // Final char

				// Compute hash code from two characters
				int part1 = let1 + length;
				result = (_multiplier * part1) + let2 + length;
			}
			return result;
		}
	}
}
