using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using System.IO;

using GameRunner;
using GameEngine;

namespace LearningGround
{
	public abstract class virtualSimulator
	{
		protected readonly GameInstance game;
		protected readonly ILogManager logmanager;
        protected readonly StdSerializers.ISerializer serializer;

        //protected virtualSimulator(string cardsetPath, ILogManager lm, string serializerName)
        //    : this(CardsLoader.LoadFromAssembly(cardsetPath), lm, serializerName) { }
        //protected virtualSimulator(GameData gameData, ILogManager lm, string serializerName) 
        //    : this(GameInstance.CreateFromData(gameData), lm, StdSerializers.Util.CreateFromName(serializerName, gameData)) { }
		public virtualSimulator(GameInstance gi, ILogManager lm, StdSerializers.ISerializer ser)
		{
            //var props = new GameStartingProps(cardSetPath);
            //game = new GameInstance(props, 0, 0);
            serializer = ser;
            game = gi;

			game.Input = move;

            logmanager = lm;
			//logmanager = new LogManagerInstance(targetdir + '\\' + Path.GetFileName(cardSetPath) + '\\' + serializer.DoubleStream.GetInputSize() + "-DATA-" + EndName);
		}

		public void Go()
		{
			logmanager.StartLoop();
			ReportAsync();
			gamingLoop();
		}
		int awon = 0, bwon = 0;
		System.Diagnostics.Stopwatch sw;

        public void ReadCommand(string str, Thread thread)
        {
            switch (str)
            {
                case "save":
                    logmanager.Save();
                    break;
                case "stop":
                    logmanager.Pause = false;
                    logmanager.WaitToEnd = true;
                    Console.Title = "Waiting till logmanager ends writing...";
                    break;
                case "abort":
                    logmanager.Dispose();
                    Console.Title = "All stopped";
                    break;

                case "pause":
                    logmanager.Pause = !logmanager.Pause;
                    Console.Title = logmanager.Pause ? "--PAUSED--" : "--UNPAUSED--";
                    break;
            }
        }
		void gamingLoop()
		{
			sw = System.Diagnostics.Stopwatch.StartNew();
            while (true)
			{
                if (logmanager.EndFull || logmanager.WaitToEnd) { break; }
				if (logmanager.Pause || logmanager.Count > LogManagerFile.MaxCount)
				{
					sw.Stop();
					while (logmanager.Pause || logmanager.Count > LogManagerFile.MaxCount) { Thread.Sleep(1); }
					sw.Start();
				}

				log = new GameLog();
				game.Start();
				totalMoves += game.board.MoveCount;
				log.FinishGame(game.board.Won, logmanager);

				if (game.board.Won) { awon++; }
				else { bwon++; }

                OnGameEnded(game);
            }

            while (!logmanager.EndFull) { Thread.Sleep(1); }
            logmanager.Dispose();

            Console.WriteLine("End of gaming loop");
        }
        protected virtual void OnGameEnded(GameInstance game) { }

		protected static GameLog log;
		protected int totalMoves = 0;
		protected bool move(Player p)
		{
            bool re;
			if (p.IsA)
            {
                re = MoveA(ref p, game);
            }
			else
            {
                re = MoveB(ref p, game);
            }

            to_log(p);

            return re;
        }
        void to_log(Player p)
        {
            var binary = serializer.ConvertPosition(p);
            log.StorePosition(p.IsA, binary);
        }
        protected abstract bool MoveA(ref Player p, GameInstance g);
		protected abstract bool MoveB(ref Player p, GameInstance g);

        public const int AUTOSAVE_MS = 5 * 60 * 1000; //every 5 min
		protected void ReportAsync()
		{
			new Thread(async).Start();
			void async()
			{
				int lastCount = 0;
                var autosave_sw = System.Diagnostics.Stopwatch.StartNew();
                while (true)
				{
					while (logmanager.Pause && logmanager.Count == lastCount) { Thread.Sleep(1); }
                    if (logmanager.EndFull) { return; }
                    if(autosave_sw.ElapsedMilliseconds > AUTOSAVE_MS)
                    {
                        autosave_sw.Restart();
                        logmanager.Save();
                    }
					lastCount = logmanager.Count;

					string text = "";
					Thread.Sleep(100);

					text += getState() + ' ';
					text += "Winrate:" + getWinrate() + ' ';
					text += "Speed:" + getMovesSpeed() + ' ';
					text += "TotalMoves:" + getTotalMoves() + ' ';
					text += "Writing:" + getCurrWriting() + ' ';

					Console.WriteLine(text);
					//Console.Title = "MoveSpeed:" + getMovesSpeed() + ' ' + "TotalMoves:" + getTotalMoves();
				}
			}
			string getState() => $"A:{awon} x B:{bwon}";
			string getWinrate() => $"{Math.Round(getWinRate() * 100)}%";
			string getMovesSpeed() => $"{(int)(totalMoves / (sw.ElapsedMilliseconds / 1000.0))} moves/sec";
			string getTotalMoves() => totalMoves.ToString("N0");
			string getCurrWriting() => logmanager.Count.ToString();
		}

        protected virtual double getWinRate() => awon / (double)(bwon + awon);
	}
	public class RandVsRand : virtualSimulator
	{
        boardCopyPlayer.extendedRandomPlayer player;
        //public RandVsRand(string cardSetPath, string targetdir, string serializerName)
        //    : this(GameInstance.CreateFromCardSet(cardSetPath), cardSetPath: cardSetPath, targetdir: targetdir, ser: StdSerializers.Util.CreateFromName(serializerName))
        //{ }
        public RandVsRand(GameInstance gi, string cardSetPath, string targetdir, StdSerializers.ISerializer ser)
            : base(gi, new LogManagerFile(targetdir, cardSetPath, nameof(RandVsRand), ser), ser)
        {
            player = new boardCopyPlayer.extendedRandomPlayer(ser);
        }

        //protected override void MoveA(Player p, GameInstance g) => randomPlayer.Fast.DoMove(p);
        //protected override void MoveB(Player p, GameInstance g) => randomPlayer.Fast.DoMove(p);

        protected override bool MoveA(ref Player p, GameInstance g) => player.DoStep(ref p, g);
        protected override bool MoveB(ref Player p, GameInstance g) => player.DoStep(ref p, g);
    }
}
