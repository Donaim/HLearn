using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using VNNLib;
using VNNAddOn;

namespace LearningGround
{
    public abstract class ILogManager : IDisposable
    {
        public bool Pause { get; set; } = false;
        public bool EndFull { get; set; } = false;
        public bool WaitToEnd { get; set; } = false;

        readonly Queue<GameLog> Quenue = new Queue<GameLog>();
        protected readonly object QuenueLock = new object();
        public int Count => Quenue.Count;
        public void Add(GameLog log) { lock (QuenueLock) { Quenue.Enqueue(log); } }

        public void Dispose()
        {
            if (Disposed) { return; }
            Pause = false;
            EndFull = true;

            lock (QuenueLock)
            {
                DisposeLocal();
            }

            Disposed = true;
        }
        public bool Disposed { get; private set; } = false;
        protected abstract void DisposeLocal();

        protected abstract void OnNewGame(GameLog log);
        protected virtual void OnPauseStarted() { }
        protected virtual void OnPauseLoop() { }
        public virtual void Save() { }

        public void StartLoop() => new Thread(loop).Start();
        void loop()
        {
            while (true)
            {
                if (EndFull) { break; }

                if (Quenue.Count != 0)
                {
                    GameLog curr;
                    lock (QuenueLock) { curr = Quenue.Dequeue(); }

                    OnNewGame(curr);
                    curr.Dispose();
                }
                else
                {
                    if (WaitToEnd) { break; }
                    Thread.Sleep(1);

                    if (Pause) { OnPauseStarted(); }
                    while (Pause)
                    {
                        OnPauseLoop();
                        //if (Count == 0) { wrapper.Save(); Thread.Sleep(100); }
                        Thread.Sleep(1);
                    }
                }
            }
            
            Dispose();
        }
    }
    public class LogManagerOnline : ILogManager
    {
        public class VNNWrapper : IFeedResultNN
        {
            vnn nn;
            vnn nncopy;
            trainerNoMomentum tr;
            string savepath;
            public VNNWrapper(vnn _nn, string _savepath)
            {
                this.savepath = _savepath;
                this.nn = _nn;
                this.nncopy = nn.Copy();
                tr = new trainerNoMomentum(this.nncopy);

                slider = vutils.VSlider.RunAsync();
                manip = slider.AddWatch(setLearningRate, learningRate, 0, 0.2, 0.01);
                manip.Name = "Learning Rate";
            }
            double learningRate = 0.05;
            void setLearningRate(double to) { learningRate = Math.Max(0, Math.Min(2, to)); }
            public readonly vutils.VSlider slider;
            vutils.VSlider.IManipulator manip;

            readonly object locker = new object();

            public void feedForward(double[] pattern)
            {
                lock (locker)
                {
                    nn.feedForward(pattern);
                }
            }
            public double[] feedResult(double[] pattern)
            {
                lock (locker)
                {
                    return nn.feedResult(pattern);
                }
            }

            public void backpropagate(GameLog game)
            {
                lock (locker) { nncopy.CopyFrom(nn); }

                int[] idx = Enumerable.Range(0, game.ALog.Count + game.BLog.Count).ToArray();
                GameEngine.rng.ShuffleRandomly(idx);

                double[] desired = new double[1];
                double[] inputs;
                for (int i = 0, to = game.ALog.Count + game.BLog.Count, alen = game.ALog.Count; i < to; i++)
                {
                    int index = idx[i];
                    if (index < alen)
                    {
                        inputs = game.ALog[index];
                        desired[0] = game.AWon.Value ? 1 : 0;
                    }
                    else
                    {
                        inputs = game.BLog[index - alen];
                        desired[0] = game.AWon.Value ? 0 : 1;
                    }

                    tr.TrainOne(inputs, desired, learningRate);
                }


                lock (locker) { nn.CopyFrom(nncopy); }
            }
            /*
            public void backpropagate(GameLog _game)
            {
                return;

                int[] idx = Enumerable.Range(0, _game.ALog.Count + _game.BLog.Count).ToArray();
                idx.ShuffleRandomly();

                double[] desired = new double[1];
                double[] inputs;

                lock (locker) { backprop_locked(_game); }
                void backprop_locked(GameLog game)
                {
                    for(int i = 0, to = game.ALog.Count + game.BLog.Count, alen = game.ALog.Count; i < to; i++)
                    {
                        int index = idx[i];
                        if(index < alen)
                        {
                            inputs = game.ALog[index];
                            desired[0] = game.AWon.Value ? 1 : 0;
                        }
                        else
                        {
                            inputs = game.BLog[index - alen];
                            desired[0] = game.AWon.Value ? 0 : 1;
                        }

                        tr.TrainOne(inputs, desired, 0.05, 0.1);
                    }
                }
            }
            */
            public void Save()
            {
                lock (locker)
                {
                    File.WriteAllBytes(savepath + DateTime.Now.ToString(" MM-dd HH-mm-ss"), nn.ToBytes());
                }
            }
        }

        VNNWrapper wrapper;
        public LogManagerOnline(VNNWrapper nn)
        {
            this.wrapper = nn;
        }

        public const int MaxCount = 10_000;
        
        protected override void OnPauseStarted() => wrapper.Save();
        protected override void OnPauseLoop()
        {
            if(Count == 0)
            {
                wrapper.Save();
                Thread.Sleep(100);
            }
        }
        protected override void OnNewGame(GameLog log) => wrapper.backpropagate(log);

        public override void Save() => wrapper.Save();

        protected override void DisposeLocal()
        {
            wrapper.Save();
            wrapper.slider.Close();
        }
    }
    public class LogManagerFile : ILogManager
	{
        public LogManagerFile(string targetdir, string cardSetPath, string EndName, StdSerializers.ISerializer ser)
            : this(getOutputFile(targetdir, cardSetPath, EndName, ser))
        {
            
        }
        static string getOutputFile(string targetdir, string cardSetPath, string EndName, StdSerializers.ISerializer ser)
        {
            string dir = Path.Combine(targetdir, Path.GetFileName(cardSetPath), $"{ser.Name}-{ser.InputLength}{Path.DirectorySeparatorChar}");
            if (!Directory.Exists(dir)) { Directory.CreateDirectory(dir); }

            string file = Path.Combine(dir, $"log-{EndName}");
            return file;
        }

		private LogManagerFile(string targetfile)
		{
			var targetdir = Path.GetDirectoryName(targetfile);
			if (!Directory.Exists(targetdir)) { Directory.CreateDirectory(targetdir); }

			filepath = targetfile;
			stream = File.Open(filepath, FileMode.Append, FileAccess.Write, FileShare.Read);
		}

		public const int MaxCount = 10_000;
        
        protected override void OnPauseStarted() => stream.Flush();

        public readonly string filepath;
		FileStream stream;
		protected override void OnNewGame(GameLog log)
		{
			WriteSide(stream, log.ALog, log.AWon.Value ? 1 : 0);
			WriteSide(stream, log.BLog, log.AWon.Value ? 0 : 1);
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		static void WriteSide(Stream stream, List<double[]> Log, double win)
		{
			byte[] win_bytes = BitConverter.GetBytes((double)win);
			foreach (var pos in Log)
			{
				var bytes = GetBytesAlt(pos);
				stream.Write(bytes, 0, bytes.Length);

				stream.Write(win_bytes, 0, win_bytes.Length);
			}
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		static byte[] GetBytesAlt(double[] values)
		{
			var result = new byte[values.Length * sizeof(double)];
			Buffer.BlockCopy(values, 0, result, 0, result.Length);
			return result;
		}

        protected override void DisposeLocal()
        {
            stream.Close();
        }
    }
	public sealed class GameLog : IDisposable
	{
		public List<double[]> ALog, BLog;

		public GameLog()
		{
			ALog = new List<double[]>();
			BLog = new List<double[]>();

			AWon = null;
		}

		public void StorePosition(bool A_Move_Q, double[] board_input)
		{
			if (A_Move_Q) { ALog.Add(board_input); }
			else { BLog.Add(board_input); }
		}

		public bool? AWon;
		public void FinishGame(bool A_Won_Q, ILogManager logm)
		{
			AWon = A_Won_Q;
			logm.Add(this);
		}

		public void Dispose()
		{
			for (int i = 0; i < ALog.Count; i++) { ALog[i] = null; }
			ALog.Clear();
			ALog = null;

			for (int i = 0; i < BLog.Count; i++) { BLog[i] = null; }
			BLog.Clear();
			BLog = null;
		}
	}
}
