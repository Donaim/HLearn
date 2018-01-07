using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GameEngine;

namespace realset
{
    class GreatherHealingPotion : TargetSpell
    {
        public GreatherHealingPotion(Player parent) : base(parent)
        {
        }

        public override TargetTypes TargetType => TargetTypes.FriendCreatures;

        static int id;
        public override int ID => id;

        public override int Cost => 4;

        protected override void Play(Creature c)
        {
            c.Health = Math.Min(c.StdHealth, c.Health + 12);
        }
    }
    class MistressOfMixtures : PrimitiveMinion, IDeathrattle
    {
        public MistressOfMixtures(Player p) : base(p)
        {
        }

        public override int Cost => 1;

        static int id;
        public override int ID => id;

        public override int StdAtack => 2;
        public override int StdHealth => 2;

        public void Deathrattle()
        {
            Parent.Hero.Health = Math.Min(Parent.Hero.Health + 4, Parent.Hero.StdHealth);
            Parent.Enemy.Hero.Health = Math.Min(Parent.Enemy.Hero.Health + 4, Parent.Enemy.Hero.StdHealth);
        }
    }
    class BlastCrystalPotion : TargetSpell
    {
        public BlastCrystalPotion(Player parent) : base(parent)
        {
        }

        public override TargetTypes TargetType => TargetTypes.EnemyMinions;

        static int id;
        public override int ID => id;

        public override int Cost => 4;

        protected override void Play(Creature c)
        {
            c.Die();
            Parent.MaxMana--;
        }
    }
    class TwistingNether : SingleSpell
    {
        public TwistingNether(Player p) : base(p)
        {
        }

        static int id;
        public override int ID => id;

        public override int Cost => 8;

        protected override bool CustomCanPlay(out Action onplay)
        {
            onplay = play;
            return true;
        }
        void play()
        {
            foreach(var em in Parent.Enemy.Minions.ToArray()) { em.Die(); }
            foreach(var fm in Parent.Minions.ToArray()) { fm.Die(); }
        }
    }
    class ShieldBlock_Modified : SingleSpell
    {
        public ShieldBlock_Modified(Player p) : base(p)
        {
        }

        static int id;
        public override int ID => id;

        public override int Cost => 3;

        protected override bool CustomCanPlay(out Action onplay)
        {
            onplay = play;
            return true;
        }
        void play()
        {
            Parent.Hero.Health = Math.Min(Parent.Hero.StdHealth, Parent.Hero.StdHealth + 5);
            new RandomDrawEvent(Parent);
        }
    }
    class Volcano_Modified : SingleSpell
    {
        public Volcano_Modified(Player p) : base(p)
        {
        }

        static int id;
        public override int ID => id;

        public override int Cost => 7;

        protected override bool CustomCanPlay(out Action onplay)
        {
            onplay = createVolcano;
            return true;
        }
        void createVolcano() => new RandomVolcanoE(Parent);

        class RandomVolcanoE : RandomEvent
        {
            public RandomVolcanoE(Player p) : base(p)
            {
            }

            static int id;
            public override int GetID => id;

            public override bool Action(double random)
            {
                for (int i = 0; i < 15; i++)
                {
                    if(Parent.Count + Parent.Enemy.Count == 0) { break; }

                    int rand = rng.Int(Parent.Count + Parent.Enemy.Count);
                    if(rand < Parent.Count)
                    {
                        Parent.Minions[rand].RecieveDamage(this, 1);
                    }
                    else
                    {
                        Parent.Enemy.Minions[rand - Parent.Count].RecieveDamage(this, 1);
                    }
                }

                return false;
            }
        }
    }
    class BattleRage : SingleSpell
    {
        public BattleRage(Player p) : base(p)
        {
        }

        static int id;
        public override int ID => id;

        public override int Cost => 2;

        protected override bool CustomCanPlay(out Action onplay)
        {
            onplay = this.onplay;
            return true;
        }
        void onplay()
        {
            int count = 0;
            for (int i = 0, to = Parent.Minions.Count; i < to ; i++)
            {
                if (Parent.Minions[i].Health < Parent.Minions[i].StdHealth) { count++; }
            }
            if(Parent.Hero.Health < Parent.Hero.StdHealth) { count++; }

            for(int i = 0; i < count; i++) { new RandomDrawEvent(Parent); }
        }
    }
    class Soulfire : TargetSpell
    {
        public Soulfire(Player parent) : base(parent)
        {
        }

        public override TargetTypes TargetType => TargetTypes.EnemyCreatures;

        static int id;
        public override int ID => id;

        public override int Cost => 1;

        protected override void Play(Creature c)
        {
            c.RecieveDamage(this, 4);
            new RandomDiscardEvent(Parent);
        }
    }
    class TwilightDrake : PrimitiveMinion, IBattleCry
    {
        public TwilightDrake(Player p) : base(p)
        {
        }

        public override int Cost => 4;

        static int id;
        public override int ID => id;

        public override int StdAtack => 4;
        public override int StdHealth => 1;

        public void Battlecry()
        {
            Health += Parent.HandCards.Count;
        }
    }
    class MagmaRager : PrimitiveMinion
    {
        public MagmaRager(Player p) : base(p)
        {
        }

        public override int Cost => 3;

        static int id;
        public override int ID => id;

        public override int StdAtack => 5;
        public override int StdHealth => 1;
    }
    class AcolyteOfPain : PrimitiveMinion
    {
        public AcolyteOfPain(Player p) : base(p)
        {
        }

        public override int Cost => 3;

        static int id;
        public override int ID => id;

        public override int StdAtack => 1;
        public override int StdHealth => 3;

        public override void RecieveDamage(IDealDamage from, int dmg)
        {
            new RandomDrawEvent(Parent);
            base.RecieveDamage(from, dmg);
        }
    }
    class Naturalize : TargetSpell
    {
        public Naturalize(Player parent) : base(parent)
        {
        }

        public override TargetTypes TargetType => TargetTypes.EnemyMinions;

        static int id;
        public override int ID => id;

        public override int Cost => 1;

        protected override void Play(Creature c)
        {
            c.Die();

            c.Parent.Draw();
            c.Parent.Draw();
        }
    }
    class ChillwindYety : PrimitiveMinion
    {
        public ChillwindYety(Player p) : base(p)
        {
        }

        public override int Cost => 4;

        static int id;
        public override int ID => id;

        public override int StdAtack => 4;
        public override int StdHealth => 5;
    }
    class Doomguard : PrimitiveMinion, IBattleCry
    {
        public Doomguard(Player p) : base(p)
        {
        }

        public override bool MeCharge => true;

        public override int Cost => 5;

        static int id;
        public override int ID => id;

        public override int StdAtack => 5;
        public override int StdHealth => 7;

        public void Battlecry()
        {
            new RandomDiscardEvent(Parent);
            new RandomDiscardEvent(Parent);
        }
    }
    class MortalCoil : TargetSpell
    {
        public MortalCoil(Player parent) : base(parent)
        {
        }

        public override TargetTypes TargetType => TargetTypes.AllMinions;

        static int id;
        public override int ID => id;

        public override int Cost => 1;

        protected override void Play(Creature c)
        {
            c.RecieveDamage(this, 1);
            if (c.Dead) { new RandomDrawEvent(Parent); }
        }
    }
    class MontainGiant : PrimitiveMinion
    {
        public MontainGiant(Player p) : base(p)
        {

        }

        int stdCost = 12;
        public override int Cost => stdCost - Parent.HandCount;

        static int id;
        public override int ID => id;

        public override int StdAtack => 8;
        public override int StdHealth => 8;
    }
    class LootHoarder : PrimitiveMinion, IDeathrattle
    {
        public LootHoarder(Player p) : base(p)
        {
        }

        public override int Cost => 2;

        static int id;
        public override int ID => id;

        public override int StdAtack => 2;
        public override int StdHealth => 1;

        public void Deathrattle()
        {
            new RandomDrawEvent(Parent);
        }
    }
    class RenoJackson : PrimitiveMinion, IBattleCry
    {
        public RenoJackson(Player p) : base(p)
        {
        }

        public override int Cost => 6;

        static int id;
        public override int ID => id;

        public override int StdAtack => 4;
        public override int StdHealth => 6;

        public void Battlecry()
        {
            Parent.Hero.Health = Parent.Hero.StdHealth;
        }
    }
    class FlameImp : PrimitiveMinion, IBattleCry
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
    class Voidwalker : PrimitiveMinion, ITauntMinion
    {
        public Voidwalker(Player p) : base(p)
        {

        }

        public override int Cost => 1;

        static int id;
        public override int ID => id;

        public override int StdAtack => 1;
        public override int StdHealth => 3;
    }
    class NoviceEngeneer : PrimitiveMinion, IBattleCry
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
    class Wolfrider : PrimitiveMinion
    {
        public Wolfrider(Player p) : base(p)
        {
        }

        public override int Cost => 3;

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
        public override int Cost => 2;

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

        public override int Cost => 4;

        public override TargetTypes TargetType => TargetTypes.EnemyCreatures;
        protected override void Play(Creature c)
        {
            c.RecieveDamage(this, 6);
        }
    }
    class AbyssalEnforcer : PrimitiveMinion, IBattleCry
    {
        public AbyssalEnforcer(Player p) : base(p) { }

        static int id;
        public override int ID => id;

        public override int Cost => 7;

        public override int StdAtack => 6;
        public override int StdHealth => 6;

        const int basicDmg = 3;
        public void Battlecry()
        {
            int damage = basicDmg;

            foreach (var em in Parent.Enemy.Minions.ToArray())
            {
                em.RecieveDamage(this, damage);
            }
            foreach (var fm in Parent.Minions.ToArray())
            {
                fm.RecieveDamage(this, damage);
            }

            Parent.Enemy.Hero.RecieveDamage(this, damage);
            Parent.Hero.RecieveDamage(this, damage);
        }
    }
    class PrimodrialDrake_Modified : PrimitiveMinion, IBattleCry
    {
        public PrimodrialDrake_Modified(Player p) : base(p)
        {

        }

        public override int Cost => 8;

        static int id;
        public override int ID => id;

        public override int StdAtack => 4;
        public override int StdHealth => 8;

        public void Battlecry()
        {
            foreach(var em in Parent.Enemy.Minions.ToArray())
            {
                em.RecieveDamage(this, 2);
            }
        }
    }
    class CoreHound : PrimitiveMinion
    {
        public CoreHound(Player p) : base(p)
        {
        }

        public override int Cost => 7;

        static int id;
        public override int ID => id;

        public override int StdAtack => 9;
        public override int StdHealth => 5;
    }
    /*
    class PowerWordShield : TargetSpell
    {
        public PowerWordShield(Player parent) : base(parent)
        {
        }

        public override TargetTypes TargetType => TargetTypes.FriendMinions;

        static int id;
        public override int ID => 1;

        public override int Cost => 1;

        protected override void Play(Creature c)
        {
            c.Health += 2;
            new RandomDrawEvent(Parent);
        }
    }
    class AgentSquire : PrimitiveMinion, IBubleMinion
    {
        public AgentSquire(Player p) : base(p)
        {
        }

        public override int Cost => 1;

        static int id;
        public override int ID => id;

        public override int StdAtack => 1;
        public override int StdHealth => 1;

        public bool BubleActive { get; set; } = true;
    } 
    
    class PsychoTron : PrimitiveMinion, ITauntMinion, IBubleMinion
    {
        public PsychoTron(Player p) : base(p)
        {
        }

        public override int Cost => 5;

        static int id;
        public override int ID => id;

        public override int StdAtack => 3;
        public override int StdHealth => 4;

        public bool BubleActive { get; set; } = true;
    }

    class ArcaneIntellect : SingleSpell
    {
        public ArcaneIntellect(Player p) : base(p)
        {
        }

        static int id;
        public override int ID => id;

        public override int Cost => 3;

        protected override bool CustomCanPlay(out Action onplay)
        {
            onplay = play;
            return true;
        }
        void play()
        {
            new RandomDrawEvent(Parent);
            new RandomDrawEvent(Parent);
        }
    }
    */
}
