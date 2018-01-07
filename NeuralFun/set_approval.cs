using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace NeuralFun
{
	class set_approval_class
	{
		bool bad = false;
		int win = 0, lose = 0;
		int longest_game = 0;
		int n = 0;

		bool finished = false;
		double lastRes;

        [vutils.Testing.TestingObject]
        public void set_approval(string setfile)
		{
			learn.open(setfile, out var stream, out int len, out var nn, out var nnfile, out var dir, out var file);

			AsyncReport();

			int current_game_length = 0;
			while(learn.GetNextGame(stream, len, out var inputs, out var result))
			{
				n++;

				if(CheckZeros(inputs, 4)) { bad = true; finished = true; return; }

				if(lastRes == result[0]) { current_game_length++; }
				else
				{
					longest_game = Math.Max(current_game_length, longest_game);
					current_game_length = 0;
				}

				lastRes = result[0];
				switch (lastRes)
				{
					case 0: lose++; continue;
					case 1: win++; continue;
					default:
						bad = true;
						finished = true;
						return;
				}
			}

			finished = true;
		}
		static bool CheckZeros(double[] inputs, int len)
		{
			int zerocount = 0;
			for (int i = 0; i < len; i++) { if (inputs[i] == 0) { zerocount++; } }
			if (zerocount >= len)
			{ return true; }
			else { return false; }
		}

		void AsyncReport()
		{
			new Thread(async).Start();
			void async()
			{
				while (!finished)
				{
					Thread.Sleep(100);

					Console.WriteLine($"n:{n} | A: {win} x B:{lose} {getWinrate()}");
					if (bad) { Console.WriteLine($"BAD OUTPUT ON {n} GAME = {lastRes}"); }
				}
				Console.WriteLine($"Longest game: {longest_game}");
			}
			string getWinrate() => $"{Math.Round(win / (double)(lose + win) * 100)}%";
		}
	}
}
