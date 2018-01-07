using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using System.Windows.Forms;

using GameRunner;
using GameEngine;
using GameVisualizer;

namespace LearningGround
{
    public class visualRandom : visualPlayerc
    {
        public visualRandom(string cardSetPath, string serializerName) 
            : this(CardsLoader.LoadFromAssembly(cardSetPath), serializerName)
        {
        }
        public visualRandom(GameData data, string serializerName)
            : base(data, new boardCopyPlayer.extendedRandomPlayer(StdSerializers.Util.CreateFromName(serializerName, data)))
        {

        }
    }
    public class visualNN : visualPlayerc
    {
        public visualNN(string cardSetPath, string serializerName, string nnfile)
            : this(CardsLoader.LoadFromAssembly(cardSetPath), serializerName, nnfile)
        {
        }
        public visualNN(GameData data, string serializerName, string nnfile)
            : base(data, new boardCopyPlayer.neuralPlayer(nnfile, StdSerializers.Util.CreateFromName(serializerName, data)))
        {

        }
    }
    public abstract class visualPlayerc
	{
		GamingForm form;
		GameInstance game;
        IPlayer ai;
		public visualPlayerc(GameData data, IPlayer pl)
		{
            //var props = new GameStartingProps(cardSetPath);
            game = GameInstance.CreateFromData(data);
			form = new GamingForm(game);

			form.center.Input = slowrandMoves;
			form.KeyUp += Form_KeyUp;

            ai = pl;
		}
        public void Go()
        {
            Application.Run(form);
        }

		bool slowrandMoves(Player p)
		{
            form.center.TurnGoing = true;

            form.center.InvokeRebuild();
            form.Pause = true;
            while (form.Pause) { Thread.Sleep(1); }
            if (!form.center.TurnGoing) { return false; }

            var re = ai.DoStep(ref p, game);
            form.center.InvokeRebuild();
            //if (!re)
            //{
            //    form.Pause = true;
            //    while (form.Pause) { Thread.Sleep(1); }
            //    if (!form.center.TurnGoing) { return false; }
            //}

            form.center.TurnGoing = re;
            return re;
		}

		void Form_KeyUp(object sender, KeyEventArgs e)
		{
            if(e.KeyCode != Keys.Space) { return; }

			if (form.center.game.StartedQ)
			{
				form.Pause = false;
			}
			else
			{
				new Thread(() =>
				{
					form.center.game.Start();
				}).Start();
			}
		}
	}
}
