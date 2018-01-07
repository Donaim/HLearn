using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
	public class Board
	{
		public Board(Player a, Player b)
		{
			A = a; B = b;

			a.Init(this);
			b.Init(this);

			GameGoing = true;
		}
		public bool GameGoing = false;
		public bool TurnA = true;
        public int MoveCount = 0;
        public int StepCount = 0;
		public bool Won;
		public Player A;
		public Player B;

		public Board Copy()
		{
			var re = (Board)MemberwiseClone();

			re.A = A.Copy(re);
			re.B = B.Copy(re);

			re.A.Enemy = re.B;
			re.B.Enemy = re.A;

            re.randomList = new Queue<RandomEvent>(this.randomList.Count);
            foreach(var r in this.randomList)
            {
                re.randomList.Enqueue(r.Copy(r.Parent.IsA ? re.A : re.B));
            }

			return re;
		}

        Queue<RandomEvent> randomList = new Queue<RandomEvent>(4);
        public IEnumerable<RandomEvent> RandomList => randomList;
        internal void AddRandomEvent(RandomEvent e) => randomList.Enqueue(e);
        public void InvokeRandomEvents()
        {
            int index = 0;
            while (randomList.Count > index++)
            {
                var o = randomList.Dequeue();
                if (o.Action(rng.Double())) { randomList.Enqueue(o); }
            }
        }
    }
	
	public abstract class Player
	{
		public Player()
		{
			Hand = new Hand(this);
			field = new Field(this, Hand, Deck);

			Deck = new Deck(this, StartingCards, true);
			Hero = StartingHero;
		}

        public abstract Card[] StartingCards { get; }
		public abstract Hero StartingHero { get; }
        
		//init
		public Board Board;
		public Player Enemy;
		public virtual void Init(Board board)
		{
			if(Board != null) { throw new Exception("Double initialization of the same player!!"); }

			Board = board;
			if(this == Board.A) { IsA = true; }
			else if(this == Board.B) { IsA = false; } else { throw new Exception("Board does not contain this player!"); }
			Enemy = IsA ? Board.B : Board.A;
		}
		public bool IsA { get; private set; }

		//hero
		public Hero Hero;
		public virtual bool TryHeroPower(out Action powerAction)
		{
			powerAction = null;

			if (Mana < Hero.Power.Cost)
			{
#if sdebug
				Exceptions.NoManaUseHeroPower(Hero.Power); 
#endif
				return false;
			}

			if (!Hero.Power.CanUse()) { return false; }

			powerAction = stdHeropower;
			return true;

			//
			void stdHeropower()
			{
				Hero.Power.Use();
			}
		}

		//minions
		protected Field field;
		public IReadOnlyList<Minion> Minions => field.Minions;
		public int Count => field.Count;
        public virtual void Summon(Minion m) => field.Add(m);
        public virtual void Remove(Minion m) => field.Remove(m);

		//cards
		public const int ManaCap = 10;
		public int Mana = 0, MaxMana = 0;

		protected Hand Hand;
		public IReadOnlyList<Card> HandCards => Hand.cards;
		public int HandCount => Hand.cards.Count;

		protected Deck Deck;
		public IReadOnlyList<Card> DeckCards => Deck.cards;
		public int DeckCount => Deck.cards.Count;

		public virtual bool CanPlay(Card c, out Action PlayAction)
		{
			if (Mana >= c.Cost)
			{
                PlayAction = stdPlayAction; return true;
			}
#if sdebug
			else
			{
				Exceptions.NoManaToPlayCard(c);
			}
#endif
			PlayAction = null;
			return false;

			void stdPlayAction()
			{
				Hand.Play(c);
			}
		}
        public virtual bool CanHit(Creature agressor, Creature target) => true;

		public virtual void Draw() { Deck.Draw(); } //draw first card from deck
		public virtual void Draw(Card c) { Hand.Draw(c); } //add new card to hand

		//game
		public virtual void StartTurn()
		{
			MaxMana = Math.Min(MaxMana + 1, ManaCap);
			Mana = MaxMana;

			Deck.Draw();

            foreach (var m in Minions) { m.StartTurn(); }
            Hero.StartTurn();
		}
		public virtual void EndTurn() { }

		public Player Copy(Board board)
		{
			var re = (Player)MemberwiseClone();

			re.Hero = Hero.Copy(re);
			re.Hand = Hand.Copy(re);
			re.Deck = Deck.Copy(re);
			re.field = field.Copy(re, re.Hand, re.Deck);

			re.Board = board;

			return re;
		}
	}
	public sealed class Field
	{
		public const int MaxMinions = 7;

		public Player Parent;
		public Hand Hand;
		public Deck Deck;
		public Field(Player p, Hand h, Deck d)
		{
			Parent = p;
			Hand = h;
			Deck = d;
			minions = new List<Minion>();
		}

		List<Minion> minions;
		public IReadOnlyList<Minion> Minions => minions;
		public int Count => minions.Count;

		public void Add(Minion m)
		{
			minions.Add(m);
		}
		public void Remove(Minion m)
		{
			minions.Remove(m);
		}

		public Field Copy(Player newParent, Hand newHand, Deck newDeck)
		{
			var re = new Field(newParent, newHand, newDeck);

			foreach (var m in minions)
			{
				re.Add(m.Copy(newParent));
			}

			return re;
		}
	}
}
