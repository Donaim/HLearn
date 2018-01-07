using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GameEngine;

namespace primitive
{
	class PrimitivePlayer : Player
	{
		public override void Init(Board board)
		{
			base.Init(board);
				
			if (IsA)
			{
                Hero.Health -= 2;
                for (int i = 0; i < 3; i++) { Deck.Draw(); }
			}
			else
			{
				for (int i = 0; i < 4; i++) { Deck.Draw(); }
			}
		}

		public override Card[] StartingCards => new Card[]
		{
            //new MinionCard(new NoviceEngeneer(this)),
            //new MinionCard(new NoviceEngeneer(this)),
            //new MinionCard(new NoviceEngeneer(this)),
            //new MinionCard(new NoviceEngeneer(this)),
            //new MinionCard(new NoviceEngeneer(this)),
            //new MinionCard(new NoviceEngeneer(this)),
            //new MinionCard(new NoviceEngeneer(this)),

            //new MinionCard(new Wolfrider(this)),
            //new MinionCard(new Wolfrider(this)),
            //new MinionCard(new Wolfrider(this)),
            //new MinionCard(new Wolfrider(this)),
            //new MinionCard(new Wolfrider(this)),
            //new MinionCard(new Wolfrider(this)),

            //new MinionCard(new AgentSquire(this)),
            //new MinionCard(new AgentSquire(this)),
            //new MinionCard(new AgentSquire(this)),
            //new MinionCard(new AgentSquire(this)),
            //new MinionCard(new AgentSquire(this)),
            //new MinionCard(new AgentSquire(this)),

            new MinionCard(new GoldshireFootman(this)),
            new MinionCard(new GoldshireFootman(this)),
            new MinionCard(new GoldshireFootman(this)),
            new MinionCard(new GoldshireFootman(this)),
            new MinionCard(new GoldshireFootman(this)),

            new MinionCard(new SmallRenoJackson(this)),

            //new WildGrowth(this),
            //new WildGrowth(this),
            //new WildGrowth(this),
            //new WildGrowth(this),
            //new WildGrowth(this),
            //new WildGrowth(this),

            //new MinionCard(new BAbyssalEnforcer(this)),
            //new MinionCard(new BAbyssalEnforcer(this)),
            //new MinionCard(new BAbyssalEnforcer(this)),
            //new MinionCard(new BAbyssalEnforcer(this)),
            //new MinionCard(new BAbyssalEnforcer(this)),
            //new MinionCard(new BAbyssalEnforcer(this)),

            new MinionCard(new FlameImp(this)),
            new MinionCard(new FlameImp(this)),
            new MinionCard(new FlameImp(this)),
            new MinionCard(new FlameImp(this)),
            new MinionCard(new FlameImp(this)),
            new MinionCard(new FlameImp(this)),
            new MinionCard(new FlameImp(this)),
            new MinionCard(new FlameImp(this)),
            new MinionCard(new FlameImp(this)),
            new MinionCard(new FlameImp(this)),
            new MinionCard(new FlameImp(this)),
            new MinionCard(new FlameImp(this)),

            //new DestroyingSecret(this),
            //new DestroyingSecret(this),
            //new DestroyingSecret(this),
            //new DestroyingSecret(this),
            //new DestroyingSecret(this),
            //new DestroyingSecret(this),
            //new DestroyingSecret(this),

            //new FireBall(this),
            //new FireBall(this),
            //new FireBall(this),
            //new FireBall(this),
            //new FireBall(this),
            //new FireBall(this),
            //new FireBall(this),
            //new FireBall(this),
            //new FireBall(this),
            //new FireBall(this),
        };

		public override Hero StartingHero => new Rexxar(this);

        public override void Summon(Minion m)
        {
            base.Summon(m);
            if(m is IBattleCry bc) { bc.Battlecry(); }
        }

        public override bool CanHit(Creature agressor, Creature target)
        {
            if (target is ITauntMinion) { return true; }
            else
            {
                if (target.Parent.Minions.Any(o => o is ITauntMinion)) { return false; }
                else { return true; }
            }
        }
    }
    class Rexxar : Hero
    {
        public Rexxar(Player p) : base(p) { }

        static int id;
        public override int ID => id;

        public override HeroPower StdPower => new SteadyShot(this);

        class SteadyShot : HeroPower
        {
            public SteadyShot(Hero h) : base(h) { }

            protected override void Pure()
            {
                Hero.Parent.Enemy.Hero.RecieveDamage(this, 2);
            }
        }
    }

    interface IBattleCry
    {
        void Battlecry();
    }
    interface ITauntMinion { }
    interface IBubleMinion
    {
        bool Active { get; set; }
    }

    abstract class PrimitiveMinion : Minion
    {
        public PrimitiveMinion(Player p) : base(p)
        {
            if(MeCharge) { Sleeping = false; }
        }

        public virtual bool MeCharge => false;

        public override void RecieveDamage(Creature from)
        {
            if(this is IBubleMinion bm && bm.Active) { bm.Active = false; }
            else
            {
                base.RecieveDamage(from);
            }
        }

        public override IEnumerable<string> GetModificatorStrings
        {
            get
            {
                foreach(var o in base.GetModificatorStrings) { yield return o; }
                if (MeCharge) { yield return nameof(MeCharge); }
                if (this is ITauntMinion) { yield return nameof(ITauntMinion); }
                if (this is IBubleMinion bm && bm.Active) { yield return nameof(IBubleMinion); }
            }
        }
    }

    class SmallRenoJackson : PrimitiveMinion, IBattleCry
    {
        public SmallRenoJackson(Player p) : base(p)
        {
        }

        public override int Cost => 1;

        static int id;
        public override int ID => id;

        public override int StdAtack => 1;
        public override int StdHealth => 1;

        public void Battlecry()
        {
            Parent.Hero.Health = Parent.Hero.StdHealth;
        }
    }
    class FlameImp : Minion, IBattleCry
    {
        public FlameImp(Player p) : base(p)
        {
        }

        public override int Cost => 1;

        static int id;
        public override int ID => id;

        public override int StdAtack => 3;
        public override int StdHealth => 2;

        public void Battlecry()
        {
            Parent.Hero.RecieveDamage(this, 3);
        }
    }
    class GoldshireFootman : PrimitiveMinion, ITauntMinion
    {
        public GoldshireFootman(Player p) : base(p)
        {

        }

        public override int Cost => 1;

        static int id;
        public override int ID => id;

        public override int StdAtack => 1;
        public override int StdHealth => 2;
    }

    /*
    class NoviceEngeneer : PrimitiveMinion, IBattleCry
    {
        public NoviceEngeneer(Player p) : base(p)
        {

        }
        public override int StdCost => 2;

        static int id;
        public override int ID => id;

        public override int StdAtack => 1;
        public override int StdHealth => 1;

        public void Battlecry()
        {
            Parent.Board.RandomList.Enqueue(new RandomDrawEvent(Parent));
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


    class AgentSquire : PrimitiveMinion, IBubleMinion
    {
        public AgentSquire(Player p) : base(p)
        {
        }

        public override int StdCost => 1;

        static int id;
        public override int ID => id;

        public override int StdAtack => 1;
        public override int StdHealth => 1;

        public bool Active { get; set; } = true;
    }
    class Wolfrider : PrimitiveMinion
    {
        public Wolfrider(Player p) : base(p)
        {
        }

        public override int StdCost => 3;

        static int id;
        public override int ID => id;

        public override int StdAtack => 3;
        public override int StdHealth => 1;

        public override bool MeCharge => true;
    }


    class WildGrowth : SingleSpell
    {
        public WildGrowth(Player p) : base(p)
        {
        }

        static int id;
        public override int ID => id;
        public override int StdCost => 2;

        protected override bool CustomCanPlay(out Action onplay)
        {
            onplay = this.onplay;
            return true;
        }
        void onplay() => Parent.MaxMana++;
    }

    class FireBall : TargetSpell
    {
        public FireBall(Player p) : base(p) { }

        static int id;
        public override int ID => id;

        public override int StdCost => 4;

        public override TargetTypes TargetType => TargetTypes.EnemyCreatures;
        protected override void Play(Creature c)
        {
            c.RecieveDamage(this, 6);
        }
    }

    /// <summary> improved version </summary>
    public class BAbyssalEnforcer : Minion, IBattleCry
    {
        public BAbyssalEnforcer(Player p) : base(p) { }

        static int id;
        public override int ID => id;

        public override int StdCost => 8;

        public override int StdAtack => 6;
        public override int StdHealth => 6;

        const int basicDmg = 3;
        public void Battlecry()
        {
            int damage = basicDmg;

            foreach (var o in Parent.Enemy.Minions.ToArray())
            {
                o.RecieveDamage(this, damage);
            }

            Parent.Enemy.Hero.RecieveDamage(this, damage);
        }
    }

    public class DestroyingSecret : Secret
    {
        public DestroyingSecret(Player parent) : base(parent)
        {
        }

        public override RandomEvent Event => new DestroyingEvent(Parent);

        static int id;
        public override int ID => id;

        public override int StdCost => 1;

        public class DestroyingEvent : RandomEvent
        {
            readonly int enemy_starting_minions = -100;
            public DestroyingEvent(Player p) : base(p)
            {
                enemy_starting_minions = p.Enemy.Count;
            }

            static int id;
            public override int GetID => id;

            public override bool Action(double random)
            {
                if (Parent.Enemy.Hero.Power.UsedThisTurn)
                {
                    Parent.Enemy.Hero.Health = 1;
                    return false;
                }
                else if (Parent.Enemy.Count <= enemy_starting_minions)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
    */
}
