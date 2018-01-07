using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using System.Drawing;
using System.Windows.Forms;

using GameEngine;
using GameRunner;

namespace GameVisualizer
{
	public class ControlCenter
	{
        public GameInstance game;
        public GamingForm parent;
		public ControlCenter(GameInstance g, GamingForm f)
		{
			parent = f;
			game = g;
			g.Input = GetInput;

			InvokeRebuild = () => parent.Invoke((Action)ReBuild);
            Input = (p) =>
            {
                parent.Invoke(InvokeRebuild);
                return true;
            };

			endButt = new EndTurnButton(this);
			HeroA = new HeroPortret(this, true);
			HeroB = new HeroPortret(this, false);
			AHand = new HandControl(this, true);
			BHand = new HandControl(this, false);
			boardC = new BoardControl(this);
		}
		
		public BoardControl boardC;
		public HandControl AHand, BHand;
		public HeroPortret HeroA, HeroB;
		public EndTurnButton endButt;

		public ISelectable S1 = null, S2 = null;
		void InvokeSelection()
		{
			if(S1 == null || S2 == null)
			{
                ReBuild();
			}
            else 
            {
                if(S2.GetTarget is Creature tc)
                {
			        if(S1.GetTarget is Creature c)
			        {
                        if (c.CanAttack(tc))
                        {
                            c.Hit(tc);
                        }
                        else
                        {
                            VisualExceptions.CannotAttackThatOne(c.Name, tc.Name);
                        }
                    }
                    else if (S1.GetTarget is ITargetPlayCard tpc)
                    {
                        if(tpc.CanPlay(tc, out var play))
                        {
                            play();
                        }
                        else
                        {
                            VisualExceptions.CannotTargetThatThing(tpc.ToString(), tc.Name);
                        }
                    }
                    else { throw new NotImplementedException(); }

                }
                else
                {
                    VisualExceptions.CannotTargetThatThing("Anything", S2.ToString());
                }
                ReBuild();
                S1 = null; S2 = null;
            }
		}
		public void SelectMe(ISelectable s)
		{
            if(S1 == null) { S1 = s; }
            else if(S2 == null) { S2 = s; }
            else { S1 = null; S2 = null; }

            if(S1 != null)
            {
                if (S1.MyMoveQ)
                {
                    if (S1 == S2) { S1 = null; S2 = null; }

                    if (S2 != null)
                    {
                        if ((!(S1.GetTarget is ITargetPlayCard)) && S1.MyMoveQ == S2.MyMoveQ) { S1 = s; S2 = null; }
                    }
                }
                else
                {
                    VisualExceptions.NotMyMoveSelection(s.GetTarget.GetType().Name);
                    S1 = null;
                    S2 = null;
                }
            }

            InvokeSelection();
		}

		public bool TurnGoing = false;
		public Func<Player, bool> Input;
		public bool GetInput(Player p)
		{
            S1 = null; S2 = null;
			TurnGoing = true;

            parent.RefreshTitle();
			return Input(p);
		}

		public Action InvokeRebuild;
		public void ReBuild()
		{
            if (game.board == null) { return; }

			boardC.ReBuild();

			AHand.ReBuild();
			BHand.ReBuild();

			HeroA.Refresh();
			HeroB.Refresh();

			endButt.Refresh();
		}
	}
	public interface ISelectable
	{
		bool MyMoveQ { get; }
		IDealDamage GetTarget { get; }
	}

	public class HandControl : Control
	{
		public static Point StdLocation(HandControl c, bool up)
		{
			int y = up ? c.center.parent.Height - c.Height - 20 - 30 : 20;
			return new Point((c.center.parent.Width - HeroPortret.StdSize.Width - DBoarder) / 2 - c.Width / 2, y);
		}
		protected override Size DefaultSize => new Size(
			Hand.MaxHand * (CardPortret.StdSize.Width + DX), 
			DBoarder + CardPortret.StdSize.Height + DBoarder
			);
		public ControlCenter center;
		public GameInstance game;
		void loadme()
		{
			BackColor = Color.Red;
			DoubleBuffered = true;
		}
		bool IsAPlayer;
		public HandControl(ControlCenter c, bool APlayer)
		{
			IsAPlayer = APlayer;
			center = c;
			game = center.game;
			center.parent.Controls.Add(this);

			//center.game.OnStart += loadHand;
			loadme();
		}

		private bool active = false;
		public bool Active { get => active; set
			{
				active = value;
				BackColor = active ? Color.Red : center.parent.BackColor;
			}
		}

		Player Our;
		public void ReBuild()
		{
			if(IsAPlayer) { Our = center.game.board.A; }
			else { Our = center.game.board.B; }
			Active = center.game.board.TurnA && IsAPlayer || (!center.game.board.TurnA && !IsAPlayer);

			SuspendLayout();

			Controls.Clear();

			int width = Our.HandCount * (CardPortret.StdSize.Width + DX);
			int xStart = (Width / 2 - width / 2);

			int i = 0;
			foreach(var card in Our.HandCards)
			{
				var port = new HandCardPortret(this, card);
				setLocation(port, i++, xStart);
			}

			ResumeLayout(true);
		}

		static readonly Font leftFont = new Font("Consolas", 9);
		public static readonly StringFormat leftFormat = new StringFormat() { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Far };
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			if (center.game.StartedQ)
			{
				e.Graphics.DrawString("Left:" + Our.DeckCount, leftFont, Brushes.Black, ClientRectangle, leftFormat);
			}
		}

		public const int DBoarder = 5, DX = 5;
		void setLocation(CardPortret port, int index, int xStart)
		{
			int y = DBoarder;
			int x = xStart + index * (port.Width + DX);

			port.Location = new Point(x, y);
		}
	}
	public class BoardControl : Control
	{
		public static Point StdLocation(BoardControl me) => 
			new Point(
				(me.center.parent.Width - HeroPortret.StdSize.Width - DBoarder) / 2 - me.Width / 2, 
				me.center.parent.Height / 2 - me.Height / 2
				);
		protected override Size DefaultSize => new Size(
			Field.MaxMinions * (MinionPortret.StdSize.Width + DX),
			MinionPortret.StdSize.Height * 2 + DBoarder + DX + DBoarder
			);
		void loadme()
		{
			BackColor = Color.Blue;
			DoubleBuffered = true;
		}

		public ControlCenter center;
		public GameInstance game;
		public BoardControl(ControlCenter c)
		{
			center = c;
			game = center.game;
			center.parent.Controls.Add(this);

			loadme();
		}
		
		public void ReBuild()
		{
			SuspendLayout();

			Controls.Clear();
			createLine(true);
			createLine(false);

			ResumeLayout(true);
		}
		void createLine(bool a)
		{
			IEnumerable<Minion> list = a ? game.board.A.Minions : game.board.B.Minions;

			int width = list.Count() * (MinionPortret.StdSize.Width + DX);
			int xStart = (Width / 2 - width / 2);

			int i = 0;
			foreach(var m in list)
			{
				var port = new MinionPortret(this, m);
				setLocation(port, i++, xStart);
			}
		}

		public const int DBoarder = 10, DX = 5;
		void setLocation(MinionPortret port, int index, int xStart)
		{
			int y;
			if (port.minion.Parent.IsA) { y = DBoarder; }
			else { y = Height - port.Height - DBoarder; }

			int x = xStart + index * (port.Width + DX);

			port.Location = new Point(x, y);
		}
	}
	public class EndTurnButton : Control
	{
		public static Point StdLocation(EndTurnButton butt)
		{
			return new Point(butt.center.parent.Width - butt.Width - 40, butt.center.parent.Height / 2 - butt.Height / 2);
		}

		protected override Size DefaultSize => new Size(100, 40);
		public ControlCenter center;
		public EndTurnButton(ControlCenter c)
		{
			center = c;
			c.parent.Controls.Add(this);

			MouseUp += EndTurnButton_MouseUp;
			MouseDown += EndTurnButton_MouseDown;

			initsize();

			void initsize()
			{
				var gfx = CreateGraphics();
				var textsize = gfx.MeasureString("End turn A", font);
				Size = new Size((int)textsize.Width, (int)textsize.Height);
			}
		}

		static Color colorUnpressed = Color.FromArgb(255, 255, 0), colorPressed = Color.FromArgb(150, 200, 0);
		private void EndTurnButton_MouseDown(object sender, MouseEventArgs e)
		{
			BackColor = colorPressed;
			Refresh();
		}
		private void EndTurnButton_MouseUp(object sender, MouseEventArgs e)
		{
			BackColor = colorUnpressed;
			if (center.game.StartedQ) {

				EndTurn();
			}
			else
			{
				new Thread(asyncStart).Start();
			}
			Refresh();

			void asyncStart()
			{
				center.game.Start();
			}
		}
		void EndTurn()
		{
			center.S1 = null; center.S2 = null;

            center.parent.Pause = false;
			center.TurnGoing = false;
		}

		static readonly Font font = new Font("Georgia", 14, FontStyle.Bold);
		protected override void OnPaint(PaintEventArgs e)
		{
			e.Graphics.Clear(BackColor);

			string text = "Start game";
			if (center.game.StartedQ)
			{
				text = "End turn " + (center.game.board.TurnA ? "A" : "B");
			}

			e.Graphics.DrawString(text, font, Brushes.Black, 0, 0);
		}
	}
}
