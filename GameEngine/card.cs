using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    public interface ISinglePlayCard
    {
        bool CanPlay(out Action onplay);
    }
    public interface ITargetPlayCard
    {
        TargetSpell.TargetTypes TargetType { get; }
        bool CanPlay(Creature c, out Action onplay);
    }
	public abstract class Card
	{
		string name;
		public virtual string Name
		{
			get
			{
				if(name == null) { name = this.GetType().Name; }
				return name;
			}
		}
		public abstract int ID { get; }
		
		public Card(Player parent)
		{
			Parent = parent;
		}

		public abstract int Cost { get; }

		public Player Parent;

		public virtual void OnDraw() { }

#if sdebug
		public static void Burn(Card c)
		{
			Console.WriteLine($"You've just burned this card : '{c.Name}'");
		}
#endif

        public virtual Card Copy(Player newParent)
		{
			var re = (Card)MemberwiseClone();
			re.Parent = newParent;
			return re;
		}
	}
	public sealed class MinionCard : Card, ISinglePlayCard
	{
		public override string Name => Minion.Name;
		Minion Minion;
		public Minion GetMinion() => Minion.Copy(Parent);

		public override int ID => Minion.ID;
        public override int Cost => Minion.Cost;

        public MinionCard(Minion m) : base(m.Parent)
		{
			Minion = m;
		}

#if sdebug
		public override bool CustomCanPlay()
		{
			if (Parent.Count < Field.MaxMinions) { return true; }
			else { Exceptions.TooManyMinionsOnBoard(Minion); return false; }
		}
#else
        public bool CanPlay(out Action onplay)
        {
            if (Parent.CanPlay(this, out var onplay1))
            {
                if (CustomCanPlay(out var onplay2))
                {
                    onplay = onplay1 + onplay2;
                    return true;
                }
            }

            onplay = null;
            return false;
        }
#endif
        bool CustomCanPlay(out Action onplay)
        {
            if (Parent.Count < Field.MaxMinions)
            {
                onplay = () => Parent.Summon(GetMinion());
                return true;
            }
            onplay = null;
            return false;
        }

		public override Card Copy(Player newParent)
		{
			var re = (MinionCard)base.Copy(newParent);
			re.Minion = Minion.Copy(newParent);
			return re;
		}
	}
    public abstract class Spell : Card, IDealDamage
    {
        internal Spell(Player parent) : base(parent) { }
    }
    public abstract class SingleSpell : Spell, ISinglePlayCard
	{
		public SingleSpell(Player p) : base(p) { }

		public bool CanPlay(out Action onplay)
        {
            if (Parent.CanPlay(this, out var onplay1))
            {
                if(CustomCanPlay(out var onplay2))
                {
                    onplay = onplay1 + onplay2;
                    return true;
                }
            }

            onplay = null;
            return false;
        }
        protected abstract bool CustomCanPlay(out Action onplay);
	}
    public abstract class TargetSpell : Spell, ITargetPlayCard
    {
        public enum TargetTypes { FriendHero, FriendMinions, FriendCreatures, EnemyHero, EnemyMinions, EnemyCreatures, All, AllMinions }
        public abstract TargetTypes TargetType { get; }
        
        public TargetSpell(Player parent) : base(parent)
        {
        }

        public bool CanPlay(Creature c, out Action onplay)
        {
            if (Parent.CanPlay(this, out var onplay1))
            {
                onplay = onplay1 + re_play;
                return true;
            }

            onplay = null;
            return false;

            void re_play() => Play(c);
        }
        protected abstract void Play(Creature c);
    }
}
