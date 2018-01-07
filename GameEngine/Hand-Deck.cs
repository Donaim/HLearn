using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
	public class Deck
	{
		public Player Parent;
		public Deck(Player p, Card[] cards, bool shuffle = true)
		{
			Parent = p;

			this.cards = cards.ToList();
			if (shuffle) { doshuffle(); }
		}
		static readonly Random rand = new Random();
		void doshuffle()
		{
			for (int i = cards.Count - 1; i > 1; i--)
			{
				int j = rand.Next(i + 1);

				var save = cards[i];

				cards[i] = cards[j];
				cards[j] = save;
			}
		}

		public List<Card> cards;
		public bool EmptyQ => cards.Count == 0;

		public void Draw()
		{
			if(cards.Count > 0)
			{
				var card = cards[0];

				cards.RemoveAt(0);
				Parent.Draw(card);
			}
			else
			{
				new Fatigue(Parent, ++FatigueCounter).OnDraw();
			}
		}

		public int FatigueCounter = 0;
		class Fatigue : SingleSpell
		{
			static int id;
			public override int ID { get => id; }
			public Fatigue(Player p, int fatigue) : base(p) { FatigueCounter = fatigue; }

			public int FatigueCounter = 0;
			public override int Cost => FatigueCounter; 

			public override void OnDraw()
			{
				FatigueCounter++;
				Parent.Hero.RecieveDamage(this, FatigueCounter);
                if (this.CanPlay(out var pl)) { pl(); }
			}

            static void empty() { }
            protected override bool CustomCanPlay(out Action onplay)
            {
                onplay = empty;
                return true;
            }
        }

		public Deck Copy(Player newParent)
		{
			var re = (Deck)MemberwiseClone();

			re.Parent = newParent;

			re.cards = new List<Card>(cards.Count);
			foreach (var c in cards)
			{
				re.cards.Add(c.Copy(newParent));
			}

			return re;
		}
	}
	public class Hand
	{
		public Player Parent;
		public Hand(Player p)
		{
			Parent = p;
			cards = new List<Card>(10);
		}

		public List<Card> cards;
		public bool EmptyQ => cards.Count == 0;

		public const int MaxHand = 10;
		public void Draw(Card c)
		{
			if(cards.Count < MaxHand)
			{
				cards.Add(c);
				c.OnDraw();
			}
#if sdebug
			else { Card.Burn(c); }
#endif
		}
		
		public void Play(Card c)
		{
			Parent.Mana -= c.Cost;
			cards.Remove(c);

            //c.OnPlay();
        }


		public Hand Copy(Player newParent)
		{
			var re = (Hand)MemberwiseClone();

			re.Parent = newParent;

			re.cards = new List<Card>(cards.Count);
			foreach(var c in cards)
			{
				re.cards.Add(c.Copy(newParent));
			}

			return re;
		}
	}
}
