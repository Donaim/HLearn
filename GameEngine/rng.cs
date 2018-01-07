using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
	public static class rng
	{
		public const int randsize = ushort.MaxValue + 1, onesize = 1000;

		static ushort Iter = 0;

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static int Int() => intBuffer[Iter++];

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static int Int(int Max) => (int)(doubleBuffer[Iter++] * Max);

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static bool Bool() => boolBuffer[Iter++];

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static double Double() => doubleBuffer[Iter++];

		static readonly int[] intBuffer = new int[randsize];
		static readonly bool[] boolBuffer = new bool[randsize];
		static readonly double[] doubleBuffer = new double[randsize];
		static rng()
		{
            int tickPre = (int)Environment.WorkingSet;
			int n = 0;
			while (true)
			{
                var tt = Environment.TickCount + tickPre++;
                int ticks = tt + tt % 107;
				Random random = new Random(ticks);
				for (int j = 0; j < onesize; j++, n++)
				{
					if(n >= randsize) { return; }

					var r = random.Next();
					intBuffer[n] = r;
					boolBuffer[n] = r % 2 == 0;
					doubleBuffer[n] = random.NextDouble();
				}
			}
		}

        public static void ShuffleRandomly<T>(this IList<T> cards)
        {
            for (int i = cards.Count - 1; i > 1; i--)
            {
                int j = Int(i + 1);

                var save = cards[i];

                cards[i] = cards[j];
                cards[j] = save;
            }
        }
    }
}
