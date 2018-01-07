using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
	public interface IDealDamage
	{

	}
	public abstract class Creature : IDealDamage
	{
		string name;
		public virtual string Name
		{
			get
			{
				if (name == null) { name = this.GetType().Name; }
				return name;
			}
		}
		public abstract int ID { get; }

		public Player Parent;
		protected Creature(Player p)
		{
			Parent = p;
			NormalAtack = StdAtack; NormalHealth = StdHealth;
			Atack = StdAtack; Health = StdHealth;

            Sleeping = true;
            Dead = false;
		}

		public abstract int StdAtack { get; }
		public abstract int StdHealth { get; } //initial base state. also get there if silenced f.e.
		public int NormalAtack = int.MinValue, NormalHealth = int.MinValue; //changed base state. useful in buffs mechanics
		public int Atack, Health; //current state

		public bool Sleeping;
		public bool Dead;
        public abstract IEnumerable<string> GetModificatorStrings { get; }

		public virtual bool CanAttack(Creature target)
		{
			if (Sleeping) {
#if sdebug
				Exceptions.CreatureCannotAtack(this, "It's speeping now!");
#endif
				return false; }
			if (Atack <= 0) {
#if sdebug
				Exceptions.CreatureCannotAtack(this, "It's has no damage!");
#endif
				return false; }
			if (Dead) { return false; }

            return Parent.CanHit(this, target);
		}
		public virtual void Hit(Creature target)
		{
			target.RecieveDamage(this);
			target.Defend(this);

			Sleeping = true; //cant attack second time
		}
		public abstract void Defend(Creature target);

		public virtual void RecieveDamage(Creature from) { RecieveDamage(from, from.Atack); }
		public virtual void RecieveDamage(IDealDamage from, int dmg)
		{
			Health -= dmg;
			if (Health <= 0) { Die(); }
		}
		public virtual void Die() { Dead = true; }

		public virtual void StartTurn() { Sleeping = false; }
	}
	public abstract class Minion : Creature, IDealDamage
	{
		public abstract int Cost { get; }

		public Minion(Player p) : base(p)
		{
		}

        public override IEnumerable<string> GetModificatorStrings
        {
            get
            {
                if (Sleeping) { yield return nameof(Sleeping); }
                yield return null;
            }
        }

        //public virtual bool Charge { get; set; }
        //      public virtual bool DivineShield { get; set; }
        //public virtual bool Taunt        { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        //public virtual bool StdWindfury  { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        //      public virtual bool Adapt        { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        //public virtual bool Discover     { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        //public virtual bool Immune       { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        //public virtual bool Stealth      { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        //public virtual int Overload      { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        //public virtual int SpellDamage   { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Hit(Creature target)
		{
			base.Hit(target);
		}
        
		public override void Defend(Creature target) { target.RecieveDamage(this); }

		public override void Die()
		{
			base.Die();
			Parent.Remove(this);
		}

		public virtual Minion Copy(Player newParent)
		{
			var re = (Minion)MemberwiseClone();
			re.Parent = newParent;
			return re;
		}
	}
	public abstract class SimpleMinion : Minion
	{
		public SimpleMinion(Player p) : base(p)
		{
		}

		public override sealed bool CanAttack(Creature target) => base.CanAttack(target);
		public override sealed void Hit(Creature target) => base.Hit(target);

		public override void RecieveDamage(IDealDamage from, int dmg) => base.RecieveDamage(from, dmg);
		public override void RecieveDamage(Creature from) => base.RecieveDamage(from);
		public override sealed void Defend(Creature target) => base.Defend(target);

		public override sealed Minion Copy(Player newParent) => base.Copy(newParent);
	}

	public abstract class Hero : Creature, IDealDamage
	{
		public override int StdAtack => 0;
		public override int StdHealth => 30;

		public Hero(Player p) : base(p)
		{
			Sleeping = false;
			Power = StdPower;
		}

        public override IEnumerable<string> GetModificatorStrings => Parent.Board.RandomList.Where(o => o.Parent.IsA == Parent.IsA).Select(o => o.GetType().Name);

        public override void Defend(Creature target) { } //heroes does not defend

		public override void Die()
		{
			base.Die();

			if (Parent.IsA) { Parent.Board.Won = false; }
			else { Parent.Board.Won = true; }

			Parent.Board.GameGoing = false;

#if sdebug
			Exceptions.Win(Parent.Enemy.Hero);
#endif
		}

		public override void StartTurn()
		{
			base.StartTurn();
			Power.UsedThisTurn = false;
		}

		public HeroPower Power;
		public abstract HeroPower StdPower { get; }

		public abstract class HeroPower : IDealDamage
		{
			public Hero Hero;
			public HeroPower(Hero h) { Hero = h; }

			public int Cost = 2;
			public bool UsedThisTurn = false;
			public bool CanUse()
			{
				if (UsedThisTurn)
				{
#if sdebug
					Exceptions.HeroPowerNeedsToReload(this);
#endif
					return false;
				}
				else { return true; }
			}

			public void Use()
			{
				Hero.Parent.Mana -= Cost;
				UsedThisTurn = true;
				
				Pure();
			}
			protected abstract void Pure();

			public virtual HeroPower Copy(Hero newHero)
			{
				var re = (HeroPower)MemberwiseClone();
				re.Hero = newHero;
				return re;
			}
		}

		public virtual Hero Copy(Player newParent)
		{
			var re = (Hero)MemberwiseClone();

			re.Parent = newParent;
			re.Power = Power.Copy(re);

			return re;
		}
	}
}
