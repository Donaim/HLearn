using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static System.Console;

namespace GameEngine
{
	public static class Exceptions
	{
		public static void Win(Hero h)
		{
			WriteLine($"Player {(h.Parent.IsA ? 'A' : 'B')} won with {h.Name}!");
		}

		public static void HeroPowerNeedsToReload(Hero.HeroPower power)
		{
			WriteLine($"\"{power.GetType().Name}\" - the {power.Hero.Name}'s heropower has to reload! Wait a turn");
		}
		public static void NoManaUseHeroPower(Hero.HeroPower power)
		{
			WriteLine($"Not enough mana to use heropower since its cost = {power.Cost} when you have only {power.Hero.Parent.Mana} mana");
		}

		public static void CreatureCannotAtack(Creature m, string message = "")
		{
			WriteLine($"Crature {m.GetType().Name} cannot attack! " + message);
		}

		public static void NoManaToPlayCard(Card c, string message = "")
		{
			WriteLine($"Not enough mana to play this card: {c.Name}'s cost = {c.Cost} when you have only {c.Parent.Mana} mana" + message);
		}
		public static void TooManyMinionsOnBoard(Minion c, string message = "")
		{
			WriteLine($"Too many minions on board: cannot play {c.GetType().Name}! " + message);
		}
	}
}
