using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MipoSerializer.Common
{
	public class ArraysQueue<T> where T: class
	{
		int arrayLength;
		Stack<T[]> queue;

		public ArraysQueue(int arrayLength, int createArrays = 1)
		{
			queue = new Stack<T[]>();
			this.arrayLength = arrayLength;
			for (int i = 0; i < createArrays; i++)
				queue.Push(new T[arrayLength]);
		}

		public T[] Take()
		{
			if (queue.Count > 0)
				return queue.Pop();
			return new T[arrayLength];
		}

		public void Return(T[] array)
		{
			queue.Push(array);
		}
	}
}
