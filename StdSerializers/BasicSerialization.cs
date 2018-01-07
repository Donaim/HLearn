using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GameEngine;
using System.Runtime.CompilerServices;

namespace StdSerializers
{
    public static class BasicSerialization
    {
        public static void GetCounts(GameRunner.GameData data, out int SpellCount, out int MinionCount, out int REventsCount, out int CardCount)
        {
            SpellCount = data.Reg.Spells.Count;
            MinionCount = data.Reg.Minions.Count;
            REventsCount = data.Reg.RandomEvents.Count;
            CardCount = MinionCount + SpellCount;
        }

        public const int WritePlayerLenght = 5;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WritePlayer(Player p, DoubleStream stream)
        {
            double health = Math.Max(0, p.Hero.Health) / (double)p.Hero.StdHealth;
            stream.Write(health);

            // TODO: Attack

            double mana = p.Mana / (double)Player.ManaCap;
            stream.Write(mana);
            double maxmana = p.MaxMana / (double)Player.ManaCap;
            stream.Write(mana);

            double heropower = p.Hero.Power.UsedThisTurn ? 0 : 1;
            stream.Write(heropower);

            double cardsleft = p.DeckCount / (double)p.StartingCards.Length;
            stream.Write(cardsleft);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRandomEvents(Player p, DoubleStream stream, int eventsCount)
        {
            foreach(var r in p.Board.RandomList)
            {
                writeRandomEvent(p, stream, r, eventsCount);
            }
            stream.Skip(eventsCount + eventsCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void writeRandomEvent(Player p, DoubleStream stream, RandomEvent r, int eventsCount)
        {
            int startIndex = r.Parent.IsA == p.IsA ? 0 : eventsCount;
            stream[startIndex + r.GetID] += 1;
        }
    }
}
