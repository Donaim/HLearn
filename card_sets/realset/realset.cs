using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GameEngine;

namespace realset
{
    class PrimitivePlayer : Player
    {
        public override void Init(Board board)
        {
            base.Init(board);

            if (IsA)
            {
                //Hero.Health -= 2;
                for (int i = 0; i < 3; i++) { Deck.Draw(); }
            }
            else
            {
                for (int i = 0; i < 4; i++) { Deck.Draw(); }
                Draw(new Coin(this));
            }
        }

        public override Card[] StartingCards => new Card[]
        {
            new MinionCard(new CoreHound(this)),
            new MinionCard(new PrimodrialDrake_Modified(this)),
            new MinionCard(new Voidwalker(this)),
            new MinionCard(new NoviceEngeneer(this)),
            new MinionCard(new LootHoarder(this)),
            new MinionCard(new MontainGiant(this)),
            new MinionCard(new RenoJackson(this)),
            new MinionCard(new FlameImp(this)),
            new MinionCard(new Wolfrider(this)),
            new MinionCard(new AbyssalEnforcer(this)),
            new MinionCard(new Doomguard(this)),
            new MinionCard(new ChillwindYety(this)),
            new MinionCard(new AcolyteOfPain(this)),
            new MinionCard(new MagmaRager(this)),
            new MinionCard(new TwilightDrake(this)),
            new MinionCard(new MistressOfMixtures(this)),
            new GreatherHealingPotion(this),
            new BlastCrystalPotion(this),
            new TwistingNether(this),
            new ShieldBlock_Modified(this),
            new Volcano_Modified(this),
            new BattleRage(this),
            new MortalCoil(this),
            new WildGrowth(this),
            new FireBall(this),
            new Soulfire(this),
            new Naturalize(this),
        };

        public override Hero StartingHero => new Guldan(this);

        public override void Summon(Minion m)
        {
            base.Summon(m);
            if (m is IBattleCry bc) { bc.Battlecry(); }
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

    class Guldan : Hero
    {
        public Guldan(Player p) : base(p)
        {
        }

        public override HeroPower StdPower => new LifeDrain(this);

        static int id;
        public override int ID => id;

        class LifeDrain : HeroPower
        {
            public LifeDrain(Hero h) : base(h)
            {
            }

            protected override void Pure()
            {
                Hero.RecieveDamage(this, 2);
                new RandomDrawEvent(Hero.Parent);
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
    //interface IBubleMinion
    //{
    //    bool BubleActive { get; set; }
    //}
    interface IDeathrattle
    {
        void Deathrattle();
    }

    abstract class PrimitiveMinion : Minion
    {
        public PrimitiveMinion(Player p) : base(p)
        {
            if (MeCharge) { Sleeping = false; }
        }

        public virtual bool MeCharge => false;
        
        //public override void RecieveDamage(IDealDamage from, int dmg)
        //{
        //    if (this is IBubleMinion bm && bm.BubleActive) { bm.BubleActive = false; }
        //    else
        //    {
        //        base.RecieveDamage(from, dmg);
        //    }
        //}

        public override void Die()
        {
            if(this is IDeathrattle dr) { dr.Deathrattle(); }

            base.Die();
        }

        public override IEnumerable<string> GetModificatorStrings
        {
            get
            {
                foreach (var o in base.GetModificatorStrings) { yield return o; }
                if (MeCharge) { yield return nameof(MeCharge); }
                if (this is ITauntMinion) { yield return nameof(ITauntMinion); }
                //if (this is IBubleMinion bm && bm.BubleActive) { yield return nameof(IBubleMinion); }
            }
        }

    }
}
