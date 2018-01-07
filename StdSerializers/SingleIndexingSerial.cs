using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GameEngine;
using GameRunner;

namespace StdSerializers
{
    public class SingleIndexingSerial : ISerializer
    {
        public readonly int inputSize = -1000;
        readonly int REventsCount = -1000;

        readonly GameData gameData;
        public GameData Game => gameData;
        public SingleIndexingSerial(GameData data)
        {
            //if (!CardsLoader.Register.Loaded) { throw new Exception("Cards are not loaded!!"); }
            gameData = data;

            BasicSerialization.GetCounts(data, SpellCount: out SpellCount, MinionCount: out MinionCount, REventsCount: out REventsCount, CardCount: out CardCount);

            HandCardsLength = CardCount;
            BoardMinionsLenght = MinionCount * (MinionSingleLength);

            inputSize = REventsCount * 2 + (BasicSerialization.WritePlayerLenght * 2) + (BoardMinionsLenght * 2) + (HandCardsLength) + 1;
        }

        public string Name => nameof(SingleIndexingSerial);
        public int InputLength => inputSize;

        public double[] ConvertPosition(Player p)
        {
            var stream = new DoubleStream(inputSize);

            BasicSerialization.WritePlayer(p, stream);
            BasicSerialization.WritePlayer(p.Enemy, stream);

            WriteHand(p, stream);
            WriteEnemyHand(p.Enemy, stream);

            WriteBoard(p, stream);
            WriteBoard(p.Enemy, stream);

            BasicSerialization.WriteRandomEvents(p, stream, REventsCount);

            return stream.ToArray();
        }

        /// <summary>unique minions count + spells count + isMinion (1)</summary>
        readonly int
            CardCount = -1000,
            SpellCount = -1000,
            HandCardsLength = -1000;
        void WriteMCard(MinionCard m, DoubleStream stream)
        {
            int index = SpellCount + m.ID;
            stream[index]++;
        }
        static void WriteSpell(Spell s, DoubleStream stream)
        {
            int index = s.ID;
            stream[index]++;
        }
        void WriteHand(Player p, DoubleStream stream)
        {
            foreach (var card in p.HandCards)
            {
                if (card is Spell s) { WriteSpell(s, stream); }
                else if (card is MinionCard m) { WriteMCard(m, stream); }
                else { throw new Exception("LOL WHAT?"); }
            }
            stream.Skip(HandCardsLength);
        }
        static void WriteEnemyHand(Player enemy, DoubleStream stream)
        {
            stream.Write(enemy.HandCount / (double)Hand.MaxHand);
        }

        readonly int
            MinionCount = -1000,
            BoardMinionsLenght = -1000;
        const int MinionSingleLength = 3;
        public const double MinionMaxAttack = 10, MinionMaxHealth = 10;
        void WriteMinion(Minion m, DoubleStream stream)
        {
            int index = m.ID;
            int start = index * MinionSingleLength;

            stream[start + 0]++;
            if (!m.Sleeping) { stream[start + 1]++; }

            //stream.Write(m.Atack / MinionMaxAttack);
            stream[start + 2] += m.Health / MinionMaxHealth;
        }
        void WriteBoard(Player p, DoubleStream stream)
        {
            foreach (var m in p.Minions)
            {
                WriteMinion(m, stream);
            }
            stream.Skip(BoardMinionsLenght);
        }

    }
}
