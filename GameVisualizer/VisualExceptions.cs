using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameVisualizer
{
	public static class VisualExceptions
	{
        public static void CannotTargetThatThing(string agressor, string target)
        {
            Console.WriteLine($"{target} is not a valid target for {agressor}!");
        }

        public static void CannotAttackThatOne(string agressor, string target)
        {
            Console.WriteLine($"{agressor} cannot attack {target}!");
        }

		public static void CannotTargetFriendlyMinion(string agressor, string victim)
		{
			Console.WriteLine($"{agressor} Cannot target friendly creature - {victim}!");
		}
		public static void NotMyMoveSelection(string minion)
		{
			Console.WriteLine($"Cannot select enemy minion '{minion}'");
		}
	}
}
