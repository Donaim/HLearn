using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

using VNNAddOn;
using VNNLib;

using GameEngine;
using GameRunner;

namespace NeuralFun
{
	class nn_test_class
	{
		vnn nn;
        [vutils.Testing.TestingObject]
        public void nn_test(string nnfile, string serializerName = nameof(StdSerializers.AllOneHotSerial))
		{
			string path = File.Exists(nnfile) ? nnfile : root.StdDir + nnfile;
			nn = new vnn(File.ReadAllBytes(path));

			getPlayers(out Player A, out Player B, out var data);
            ObviousWin(A, B);

            //double[] inputs = LearningGround.serializer.ConvertPosition(A);
            double[] inputs = StdSerializers.Util.CreateFromName(serializerName, data).ConvertPosition(A);
			double output = nn.feedResult(inputs)[0];
			double formatted = Math.Round(output, 2) * 100;

			Console.WriteLine($"A will win = {formatted}%");
		}
		void getPlayers(out Player A, out Player B, out GameData gameData)
		{
			gameData = CardsLoader.LoadFromAssembly(root.StdCardset);
            GameInstance.CreateFromData(gameData).CreateOnly(out A, out B);
        }

		void ObviousWin(Player A, Player B)
		{
            //p.Hero.Health = 0;
            A.Mana = A.MaxMana = 10;
            B.Mana = B.MaxMana = 10;

            //B.Hero.Health = 1;
            A.Hero.Health = 1;
		}
	}
	class fulladopt_test_class
	{
		vnn nn;
		double err, corr;
		int i;

        [vutils.Testing.TestingObject]
        void fulladopt_test(string datafile)
		{
			learn.open(datafile, out var stream, out int len, out nn, out var nnfile, out var dir, out var file);

			ReportAsync();
			WaitStopAsync();

			err = 0.0; corr = 0.0; i = 0;
			while (learn.GetNextGame(stream, len, out var inp, out var res))
			{
				double output = nn.feedResult(inp)[0];
				double diff = Math.Abs(res[0] - output);

				err = (err * i + diff) / (double)(i + 1);
				corr = (corr * i + (diff < 0.5 ? 1 : 0)) / (double)(i + 1);

				i++;
				if (stop) { return; }
			}
		}
		void ReportAsync()
		{
			new Thread(async).Start();
			void async()
			{
				while (true)
				{
					if (stop) { return; }
					Thread.Sleep(300);
					Console.WriteLine($"After {i}: Correct= {Math.Round(corr, 2) * 100}% | Error= {err}");
				}
			}
		}
		bool stop = false;
		void WaitStopAsync()
		{
			Console.Title = "Type 'stop' to abort";
			new Thread(async).Start();
			void async()
			{
				while (true)
				{
					var line = Console.ReadLine();
					if(line == "stop") { stop = true; return; }
				}
			}
		}
	}

	class set_test_class
	{
		vnn nn;

        [vutils.Testing.TestingObject]
        void set_test(string setfile)
		{
			learn.open(setfile, out var stream, out int len, out nn, out var nnfile, out var dir, out var file);

			string line = "";
			uint skip = 0;
			uint pos = 0;
			do
			{
				pos += skip;
				for (int i = 0; i < skip; i++)
				{
					learn.GetNextGame(stream, len, out var a, out var b);
				}

				pos++;
				learn.GetNextGame(stream, len, out var inputs, out var result);
				double ourOutput = nn.feedResult(inputs)[0];

				Console.WriteLine();
				Console.WriteLine("#" + pos);
				Console.WriteLine($"Inputs : {string.Join(",", inputs)}");
				Console.WriteLine($"Correct : {result[0]}");
				Console.WriteLine($"OurOutput : {ourOutput}");

				line = Console.ReadLine();
				uint.TryParse(line, out skip);
			}
			while (line != "stop");
		}
	}
}
