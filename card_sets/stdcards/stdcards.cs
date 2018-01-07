using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GameEngine;

namespace stdcards
{
    interface IBattleCry
    {
        void Battlecry();
    }
    class NoviceEngeneer : Minion, IBattleCry
    {
        public NoviceEngeneer(Player p) : base(p)
        {
        }

        public override int Cost => 2;

        static int id;
        public override int ID => id;

        public override int StdAtack => 1;
        public override int StdHealth => 1;

        public void Battlecry()
        {
            new RandomDrawEvent(Parent);
        }

    }
    class RandomDrawEvent : RandomEvent
    {
        public RandomDrawEvent(Player p) : base(p) { }

        static int id;
        public override int GetID => id;

        public override bool Action(double random)
        {
            Parent.Draw();
            return false;
        }
    }
}
