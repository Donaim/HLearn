using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GameEngine;

namespace realset
{
    class Coin : SingleSpell
    {
        public Coin(Player p) : base(p)
        {
        }

        static int id;
        public override int ID => id;

        public override int Cost => 0;

        protected override bool CustomCanPlay(out Action onplay)
        {
            onplay = act;
            return true;
        }
        void act()
        {
            Parent.Mana++;
        }
    }
    class RandomDiscardEvent : RandomEvent
    {
        public RandomDiscardEvent(Player p) : base(p)
        {
        }

        static int id;
        public override int GetID => id;

        public override bool Action(double random)
        {
            int cardcount = Parent.HandCount;
            if (cardcount > 0)
            {
                ((List<Card>)Parent.HandCards).RemoveAt((int)(random * cardcount));
            }

            return false;
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
