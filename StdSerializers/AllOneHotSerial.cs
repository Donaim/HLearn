using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GameEngine;
using GameRunner;

namespace StdSerializers
{
    public class AllOneHotSerial : ISerializer
    {
        readonly GameData gameData;
        public GameData Game => gameData;
        public AllOneHotSerial (GameData data)
        {
            //if (!CardsLoader.Register.Loaded) { throw new Exception("Cards are not loaded!!"); }
            gameData = data;

            BasicSerialization.GetCounts(data, SpellCount: out SpellsCount, MinionCount: out _, REventsCount: out REventsCount, CardCount: out CardOneHot);
            CardLength = CardOneHot;

            MinionOneHot = data.Reg.Minions.Count;
            MinionLenght = MinionOneHot + 1 + 2;

            inputSize = REventsCount * 2 + (BasicSerialization.WritePlayerLenght + MinionLenght * Field.MaxMinions) * 2 + (Hand.MaxHand * CardLength + 1);
        }
        readonly int REventsCount = -1000;

        public string Name => nameof(AllOneHotSerial);
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
            SpellsCount = -1000,
            CardOneHot = -1000,
            CardLength = -1000;
        void WriteMCard(MinionCard m, DoubleStream stream)
        {
            int index = SpellsCount + m.ID;
            stream.WriteOneHot(CardOneHot, index);
        }
        void WriteSpell(Spell s, DoubleStream stream)
        {
            int index = s.ID;
            stream.WriteOneHot(CardOneHot, index);
        }
        void WriteHand(Player p, DoubleStream stream)
        {
            foreach (var card in p.HandCards)
            {
                if (card is Spell s) { WriteSpell(s, stream); }
                else if (card is MinionCard m) { WriteMCard(m, stream); }
                else { throw new Exception("LOL WHAT?"); }
            }
            for (int i = 0, to = Hand.MaxHand - p.HandCount; i < to; i++)
            {
                stream.WriteEmpty(CardLength);
            }
        }
        void WriteEnemyHand(Player enemy, DoubleStream stream)
        {
            stream.Write(enemy.HandCount / (double)Hand.MaxHand);
        }

        readonly int
            MinionOneHot = -1000,
            MinionLenght = -1000;
        public const double MinionMaxAttack = 10, MinionMaxHealth = 10;
        void WriteMinion(Minion m, DoubleStream stream)
        {
            int index = m.ID;
            stream.WriteOneHot(MinionOneHot, index);

            stream.Write(m.Sleeping ? 0 : 1);

            stream.Write(m.Atack / MinionMaxAttack);
            stream.Write(m.Health / MinionMaxHealth);
        }
        void WriteBoard(Player p, DoubleStream stream)
        {
            foreach (var m in p.Minions)
            {
                WriteMinion(m, stream);
            }
            for (int i = 0, to = Field.MaxMinions - p.Count; i < to; i++)
            {
                stream.WriteEmpty(MinionLenght); //empty
            }
        }

        public readonly int inputSize = -1000;
    }
}
