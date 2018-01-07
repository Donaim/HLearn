using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Windows.Forms;

using GameRunner;
using GameEngine;


namespace GameVisualizer
{
	public class GamingForm : Form
	{
		public ControlCenter center;
		public GamingForm(GameInstance g)
		{
            CheckForIllegalCrossThreadCalls = false;
			KeyPreview = true;
			StartPosition = FormStartPosition.CenterScreen;
			FormBorderStyle = FormBorderStyle.FixedToolWindow;

			Size = new Size(1300, 700);

			center = new ControlCenter(g, this);

			center.boardC.Location = BoardControl.StdLocation(center.boardC);

			center.HeroA.Location = HeroPortret.DefaultLocation(center.HeroA);
			center.HeroB.Location = HeroPortret.DefaultLocation(center.HeroB);

			center.AHand.Location = HandControl.StdLocation(center.AHand, false);
			center.BHand.Location = HandControl.StdLocation(center.BHand, true);

			center.endButt.Location = EndTurnButton.StdLocation(center.endButt);

            Load += StdForm_Load;
		}
        public bool Pause = false;

        private void StdForm_Load(object sender, EventArgs e)
        {
            RefreshTitle();
            center.ReBuild();
        }
        public void RefreshTitle()
        {
            if(center.game.board == null) { return; }
            Text = $"Move #{(center.game.board.MoveCount)} Step#{center.game.board.StepCount}";
        }

        public static void ShowAsync(Board b, string title = null)
        {
            var gi = GameInstance.CreateFakeFromBoard(b);
            var form = new GamingForm(gi);
            Application.Run(form);
            //new System.Threading.Thread(async).Start();

            //void async()
            //{
            //    try
            //    {
            //        Application.Run(form);
            //    }
            //    catch (Exception ex)
            //    {
            //        throw new Exception("RE!", ex);
            //    }
            //}
        }
	}
}
