using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

using GameRunner;
using GameEngine;

using VNNLib;

namespace LearningGround
{
    public class RandVsNN : virtualSimulator
    {
        boardCopyPlayer.neuralPlayer np;
        boardCopyPlayer.extendedRandomPlayer rp;
        //public RandVsNN(string cardsetPath, string nnfile, string serializerName)
        //    : this(CardsLoader.LoadFromAssembly(cardsetPath), nnfile: nnfile, serializerName: serializerName) { }
        //public RandVsNN(GameData gameData, string nnfile, string serializerName)
        //    : this(GameInstance.CreateFromData(gameData), getWrapper(nnfile), StdSerializers.Util.CreateFromName(serializerName, gameData)) { }
        public RandVsNN(GameInstance gi, string nnfile, StdSerializers.ISerializer ser)
            : this(gi, getWrapper(nnfile), ser) { }
        public RandVsNN(GameInstance gi, LogManagerOnline.VNNWrapper vnnw, StdSerializers.ISerializer ser) 
            : base(gi, new LogManagerOnline(vnnw), ser)
        {
            np = new boardCopyPlayer.neuralPlayer(vnnw, ser);
            rp = new boardCopyPlayer.extendedRandomPlayer(ser);
        }

        static LogManagerOnline.VNNWrapper getWrapper(string nnfile)
        {
            var nn = new vnn(File.ReadAllBytes(nnfile));
            return new LogManagerOnline.VNNWrapper(nn, nnfile);
        }

		protected override bool MoveA(ref Player p, GameInstance g)
		{
            return np.DoStep(ref p, g);
        }

		protected override bool MoveB(ref Player p, GameInstance g)
		{
            return rp.DoStep(ref p, g);
        }
	}
    public class NNVsItself : virtualSimulator
    {
        boardCopyPlayer.neuralPlayer np, rp;

        //public NNVsItself(string cardsetPath, string nnfile, string serializerName)
        //    : this(CardsLoader.LoadFromAssembly(cardsetPath), nnfile: nnfile, serializerName: serializerName) { }
        //public NNVsItself(GameData gameData, string nnfile, string serializerName)
        //    : this(GameInstance.CreateFromData(gameData), getWrapper(nnfile), StdSerializers.Util.CreateFromName(serializerName, gameData)) { }
        public NNVsItself(GameInstance gi, string nnfile, StdSerializers.ISerializer ser)
            : this(gi, getWrapper(nnfile), ser) { } 
        public NNVsItself(GameInstance gi, LogManagerOnline.VNNWrapper vnnw, StdSerializers.ISerializer ser) 
            : base(gi, new LogManagerOnline(vnnw), ser)
        {
            np = new boardCopyPlayer.neuralPlayer(vnnw, ser);
            rp = new boardCopyPlayer.neuralPlayer(vnnw, ser);
        }

        static LogManagerOnline.VNNWrapper getWrapper(string nnfile)
        {
            var nn = new vnn(File.ReadAllBytes(nnfile));
            return new LogManagerOnline.VNNWrapper(nn, nnfile);
        }

        protected override bool MoveA(ref Player p, GameInstance g)
        {
            return np.DoStep(ref p, g);
        }

        protected override bool MoveB(ref Player p, GameInstance g)
        {
            return rp.DoStep(ref p, g);
        }
    }
}
