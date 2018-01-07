using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Windows.Forms;

using GameEngine;
using GameRunner;

namespace GameVisualizer
{
	public class HeroPortret : Control, ISelectable
	{
		public static Size StdSize = new Size(150, 200);
		protected override Size DefaultSize => StdSize;

		public IDealDamage GetTarget => hero;

		public Player player;
		public Hero hero;
		bool IsA;
		public ControlCenter center;
		public HeroPowerPortret powerC;
		public HeroPortret(ControlCenter c, bool isA)
		{
			center = c;
			IsA = isA;
			center.game.OnStart += loadHeroes;
			powerC = new HeroPowerPortret(this);

			DoubleBuffered = true;
			center.parent.Controls.Add(this);

			MouseUp += HeroPortret_MouseUp;
		}

		public bool MyMoveQ => hero.Parent.Board.TurnA == hero.Parent.IsA;
		private void HeroPortret_MouseUp(object sender, MouseEventArgs e)
		{
			center.SelectMe(this);
		}

		void loadHeroes(GameInstance gi)
		{
			player = IsA ? gi.board.A : gi.board.B;
			hero = player.Hero;

            heroname = "\n\n" + (IsA ? "-A-" : "-B-") + '\n' + hero.Name;
		}
		string heroname;

		const int DBoarder = 30, DX = 20;
		static readonly Font font = new Font("Georgia", 14f, FontStyle.Bold);
        static readonly Font randomFont = new Font("Consolas", 9);
        protected static readonly StringFormat NameFormat = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near };
        protected static readonly StringFormat RandomFormat = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far };
        public static readonly Color ColorUnselected = Color.White, ColorSelected = Color.Yellow;
		protected override void OnPaint(PaintEventArgs e)
		{
			if (!center.game.StartedQ) { return; }
			loadHeroes(center.game);

			BackColor = center.S1 == this ? ColorSelected : ColorUnselected;
			e.Graphics.Clear(BackColor);

			e.Graphics.DrawString(hero.Power.Cost.ToString(), font, Brushes.Blue, 10, 10);
			e.Graphics.DrawString(hero.Atack.ToString(), font, Brushes.Red, 10, Height - 30);

			var healthLen = e.Graphics.MeasureString(hero.Health.ToString(), font).Width;
			e.Graphics.DrawString(hero.Health.ToString(), font, Brushes.Green, Width - healthLen, Height - 30);

			string costString = $"{hero.Parent.Mana}/{hero.Parent.MaxMana}";
			var costLen = e.Graphics.MeasureString(costString, font).Width;
			e.Graphics.DrawString(costString, font, Brushes.Black, Width - costLen, 10);

            e.Graphics.DrawString(heroname, font, Brushes.Black, ClientRectangle, NameFormat);

            e.Graphics.DrawString(string.Join("\n", hero.GetModificatorStrings) + '\n', randomFont, Brushes.Blue, ClientRectangle, RandomFormat);
        }

        public static Point DefaultLocation(HeroPortret portret)
		{
			int y;
			if (portret.IsA) { y = DBoarder; }
			else { y = portret.Parent.Height - portret.Height - DBoarder - 30; }

			int x = portret.Parent.Width - portret.Width - DBoarder;
			return new Point(x, y);
		}
	}
	public class HeroPowerPortret : Control
	{
		public static readonly Size StdSize = new Size(50, 50);
		protected override Size DefaultSize => StdSize;

		public ControlCenter center;
		public HeroPortret parent;
		public HeroPowerPortret(HeroPortret p)
		{
			parent = p;
			center = parent.center;
			parent.Controls.Add(this);
			MouseUp += HeroPowerPortret_MouseUp;
		}

		private void HeroPowerPortret_MouseUp(object sender, MouseEventArgs e)
		{
			if (parent.MyMoveQ)
			{
				if (parent.hero.Parent.TryHeroPower(out var act))
				{
					act();
					center.ReBuild();
				}
			}
			else
			{
				VisualExceptions.NotMyMoveSelection("HeroPower");
			}
		}

		public static readonly StringFormat format = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
		public static readonly Font font = new Font("Georgia", 14, FontStyle.Bold);
		protected override void OnPaint(PaintEventArgs e)
		{
			if (!center.game.StartedQ) { return; }

			e.Graphics.Clear(parent.BackColor);

			if (parent.hero.Power.UsedThisTurn)
			{
				e.Graphics.FillEllipse(Brushes.Black, ClientRectangle);
			}
			else
			{
				e.Graphics.FillEllipse(Brushes.Blue, ClientRectangle);
				e.Graphics.DrawString(parent.hero.Power.Cost.ToString(), font, Brushes.White, ClientRectangle, format);
			}
		}
	}

	public abstract class CardPortret : Control, ISelectable
	{
		public static Size StdSize = new Size(80, 100);
		protected override Size DefaultSize => StdSize;

		public Card card;
		public ControlCenter center;
		public CardPortret(ControlCenter cc, Card c)
		{
			center = cc;
			card = c;

			DoubleBuffered = true;
			MouseUp += CardPortret_MouseUp;
			BackColor = ColorUnselected;
		}

		public bool MyMoveQ => card.Parent.Board.TurnA == card.Parent.IsA;

        public abstract IDealDamage GetTarget { get; }

        public abstract void CardPortret_MouseUp(object sender, MouseEventArgs e);

		protected static readonly Font font = new Font("Georgia", 12f, FontStyle.Bold);
		protected static readonly Font namefont = new Font("Georgia", 9f, FontStyle.Bold);
		protected static readonly Pen bordersPen = new Pen(Brushes.Black, 3f);
		public static readonly Color ColorUnselected = Color.White, ColorSelected = Color.Yellow;

		public static readonly StringFormat
			nameFormat = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near },
			costFormat = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far },

			atackFormat = new StringFormat() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Far },
			healthFormat = new StringFormat() { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Far };
			
		protected override void OnPaint(PaintEventArgs e)
		{
			BackColor = center.S1 != null && center.S1.GetTarget == this.GetTarget ? ColorSelected : ColorUnselected;
			e.Graphics.Clear(BackColor);
			e.Graphics.DrawRectangle(bordersPen, ClientRectangle);

			e.Graphics.DrawString(card.Cost.ToString(), font, Brushes.Blue, ClientRectangle, costFormat);
			e.Graphics.DrawString(card.Name, namefont, Brushes.Black, ClientRectangle, nameFormat);

			if (card is MinionCard m) { drawMinion(e, m.GetMinion()); }
		}

		void drawMinion(PaintEventArgs e, Minion minion)
		{
			e.Graphics.DrawString(minion.Atack.ToString(), font, Brushes.Red, ClientRectangle, atackFormat);
			e.Graphics.DrawString(minion.Health.ToString(), font, Brushes.Green, ClientRectangle, healthFormat);

			e.Graphics.DrawString(string.Join("\n", minion.GetModificatorStrings), modFont, Brushes.Green, ClientRectangle, ModsFormat);
		}

		protected static readonly StringFormat ModsFormat = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
		static readonly Font modFont = new Font("Consolas", 8);
	}
	public class HandCardPortret : CardPortret
	{
		public HandControl hand;
		public HandCardPortret(HandControl h, Card c) : base(h.center, c)
		{
			hand = h;

            if(card is IDealDamage idd) { dealDamageCard = idd; }
            else { dealDamageCard = null; }

			h.Controls.Add(this);
		}

        readonly IDealDamage dealDamageCard;
        public override IDealDamage GetTarget => dealDamageCard;

        public override void CardPortret_MouseUp(object sender, MouseEventArgs e)
		{
            if(card is ISinglePlayCard spc)
            {
                if(MyMoveQ && spc.CanPlay(out var pl)) { pl(); }
                else { return; }
            }
            else if(card is ITargetPlayCard tpc)
            {
                hand.center.SelectMe(this);
                return;
            }
            else { throw new NotImplementedException(); }

			hand.center.ReBuild();
		}
	}
	public class MinionPortret : CardPortret
	{
		public static new Size StdSize = new Size(100, 150);
		protected override Size DefaultSize => StdSize;

		public BoardControl board;
		public Minion minion;
		public MinionPortret(BoardControl b, Minion m) : base(b.center, new MinionCard(m))
		{
			board = b;
			minion = m;
			board.Controls.Add(this);

			DoubleBuffered = true;
		}

		public override IDealDamage GetTarget => minion;
		public override void CardPortret_MouseUp(object sender, MouseEventArgs e)
		{
			board.center.SelectMe(this);
		}
	}

}
