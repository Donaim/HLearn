using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using System.IO;
using VNNAddOn;
using VNNLib;
using vutils.Testing;

namespace NeuralFun
{
	public class learn : IAsyncTesting
	{
		public string dir, file;

	 	vnn nn;
		public vnn GetNNCopy() => new vnn(nn.ToBytes());

		trainerNoMomentum tr;
		FileStream stream;
		public long StreamLen => stream.Length;
		public long StreamPos => stream.Position;

		public int n = 0;
		progressing prog;

        [TestingObject]
		public void learnStart(string data_file)
		{
			open(data_file, out stream, out len, out nn, out nnfile, out dir, out file);
			tr = new trainerNoMomentum(nn);
			prog = new progressing(this);

			reportTh =  prog.ReportAsync();
            TrainingLoop();
            Save();
            End = true;
        }
        void Save()
        {
            lock (prog.TestingLock)
            {
                File.WriteAllBytes(nnfile, nn.ToBytes());
            }
        }
        Thread reportTh;
        public void Command(string s, Thread workingthread)
        {
            lock (prog.TestingLock) { underlock(); }
            void underlock()
            {
                switch (s)
                {
                    case "save":
                        Save();
                        break;
                    case "stop":
                        Save();
                        goto case "abort";
                    case "abort":
                        End = true;
                        Pause = false;
                        reportTh.Abort();
                        break;

                    case "pause":
                        Pause = !Pause;
                        return;
                }
            }
        }
        public bool Pause = false, End = false;
		void TrainingLoop()
		{
			while (true)
			{
                while (Pause) { Thread.Sleep(1); }
                if (End) { return; }
				lock (prog.TestingLock)
				{
					if (GetNextGame(stream, len, out var inp, out var res))
					{
						tr.TrainOne(inp, res, 0.05);
						n++;
					}
					else { return; }
				}
			}
		}

		public string nnfile;
		public int len;
		public static void open(string data_file, out FileStream stream, out int len, out vnn net, out string nnfile, out string dir, out string file)
		{
            data_file = data_file.Replace("\"", string.Empty);

			if (File.Exists(data_file)) { dir = Path.GetDirectoryName(data_file) + Path.DirectorySeparatorChar; file = Path.GetFileName(data_file); }
			else { dir = root.StdDir; file = data_file; }

            //var split = file.Split('-');
            //len = int.Parse(split[0]);
            string lenS = dir.Substring(dir.LastIndexOf('-') + 1).TrimEnd(Path.DirectorySeparatorChar);
            len = int.Parse(lenS);

			stream = File.Open(Path.Combine(dir, file), FileMode.Open, FileAccess.Read, FileShare.Read);

			nnfile = Path.Combine(dir, "nn");
			if (File.Exists(nnfile)) { net = new vnn(File.ReadAllBytes(nnfile)); }
			else { net = new vnn(len, 1000, 1, (nn) => addon.RandomizeUniform(nn, 5.0, 3.5)); }
		}

		public static bool GetNextGame(Stream s, int len, out double[] inputs, out double[] result)
		{
			var bts = new byte[len * sizeof(double)];
			s.Read(bts, 0, bts.Length);

			inputs = ConvertToDoubles(bts);

			var res_bts = new byte[sizeof(double)];
			s.Read(res_bts, 0, sizeof(double));
			result = ConvertToDoubles(res_bts);

			return s.Position != s.Length;
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		static double[] ConvertToDoubles(byte[] arr)
		{
			var re = new double[arr.Length / sizeof(double)];
			Buffer.BlockCopy(arr, 0, re, 0, arr.Length);
			return re;
		}
    }
}
