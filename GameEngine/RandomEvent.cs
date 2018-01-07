using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    public abstract class RandomEvent : IDealDamage
    {
        public Player Parent;
        public RandomEvent(Player p)
        {
            Parent = p;
            Parent.Board.AddRandomEvent(this);
        }
        public abstract int GetID { get; }
        //public abstract Action<double> Action { get; }
        public abstract bool Action(double random);

        public virtual RandomEvent Copy(Player newParent)
        {
            var re = (RandomEvent)MemberwiseClone();
            re.Parent = newParent;
            return re;
        }

        public class EndTurnREvent : RandomEvent
        {
            public EndTurnREvent(Player p) : base(p) { }

            public override int GetID => 0;
            public override bool Action(double random)
            {
                return false;
#if sdebug
                Console.WriteLine($"Turn of player {(Parent.IsA ? "A" : "B")} ended");
#endif
            }
        }
    }
}
