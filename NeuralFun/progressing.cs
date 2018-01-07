using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

using System.IO;
using VNNAddOn;
using VNNLib;

namespace NeuralFun
{
	class progressing
	{
		learn Parent;
		public progressing(learn parent)
		{
			Parent = parent;

			logStream = File.Open(Path.Combine(Parent.dir, Parent.file), FileMode.Open, FileAccess.Read, FileShare.Read);
		}

		public readonly object TestingLock = new object();

		public Thread ReportAsync()
		{
			var th = new Thread(loop);
			th.Start();
			return th;
		}

		Stopwatch sw = new Stopwatch();
		void loop()
		{
			sw.Start();
			while (true)
			{
				Thread.Sleep(5000);
                while (Parent.Pause) { Thread.Sleep(1); }
                if (Parent.End) { return; }

				lock (TestingLock) { logStream.Position = Parent.StreamPos; nnCopy = Parent.GetNNCopy(); }

				GetError(logStream, Parent.len, nnCopy, out double err, out double corr, out double avg_out);

				SaveProgress(Parent.n, err, corr);
                Console.WriteLine($"After {Parent.n}: corr = {corr.ToString("N2")} & err = {err.ToString("N4")} | speed = {getSpeed()} & ETA = {getETA()} | avg_out = {avg_out.ToString("N2")}");
				Console.Title = $"{sw.Elapsed.ToString()} | Type 'stop' to end loop";
			}

			string getSpeed() => (int)(Parent.n / sw.Elapsed.TotalSeconds) + " moves/sec";
			string getETA()
			{
				double speed = Parent.n / sw.Elapsed.TotalSeconds;

				long bytesLeft = (logStream.Length - logStream.Position);
				long size = bytesLeft / Parent.len;

				double time = size / speed;

				return new TimeSpan(0, 0, (int)time).ToString();
			} 
		}
		vnn nnCopy;

		FileStream logStream;
        static string getLogSpecification(string data_file) => data_file.Split('-').Last();
		void SaveProgress(int n, double err, double corr)
		{
			string target = Path.Combine(Parent.dir, $"rep-{getLogSpecification(Parent.file)}.csv");

			Stream stream;
			if (File.Exists(target))
			{
				stream = File.Open(target, FileMode.Append, FileAccess.Write, FileShare.Read);
			}
			else
			{
				stream = File.Create(target);
				string title = "n,err,corr,time\n";
				byte[] b = Encoding.ASCII.GetBytes(title);
				stream.Write(b, 0, b.Length);
			}

			SaveProgress(stream, n, err, corr);
		}
		void SaveProgress(Stream stream, int n, double err, double corr)
		{
			string text = $"{n},{err},{corr},{sw.Elapsed.TotalSeconds}\n";
			var bts = Encoding.ASCII.GetBytes(text);
			stream.Write(bts, 0, bts.Length);
			stream.Dispose();
		}

		static void GetError(FileStream logStream, int len, vnn nn, out double err, out double corr, out double avg_out)
		{
			const int n = 3000;
			err = 0.0; corr = 0.0; avg_out = 0.0;
			for (int i = 0; i < n; i++)
			{
				if(learn.GetNextGame(logStream, len, out var inp, out var res))
				{
					double output = nn.feedResult(inp)[0];
					double diff = Math.Abs(res[0] - output);

					err = (err * i + diff) / (double)(i + 1);
                    avg_out = (avg_out * i + output) / (double)(i + 1);

					if (diff < 0.5) { corr++; }
				}
				else { break; }
			}

			corr = corr / n;
		}
	}
}
