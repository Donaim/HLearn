using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using vutils.Testing;

namespace NeuralFun
{
	class RandVsNNclass : IAsyncTesting
	{
        LearningGround.virtualSimulator simulator;

        [TestingObject]
		void RandVsNN(string nnfile, string cardSetPath = root.StdCardset, string serializerName = root.StdSerializerName)
		{
            root.GAll(cardSetPath, serializerName, out var data, out var gi, out var ser);
            simulator = new LearningGround.RandVsNN(gi, nnfile, ser);
            simulator.Go();
		}

        public void Command(string s, Thread workingthread) => simulator.ReadCommand(s, workingthread);
    }
    class NNVsItselfclass : IAsyncTesting
    {
        LearningGround.virtualSimulator simulator;

        [TestingObject]
        void NNVsItself(string nnfile, string cardSetPath = root.StdCardset, string serializerName = root.StdSerializerName)
        {
            root.GAll(cardSetPath, serializerName, out var data, out var gi, out var ser);
            simulator = new LearningGround.NNVsItself(gi, nnfile, ser);
            simulator.Go();
        }

        public void Command(string s, Thread workingthread) => simulator.ReadCommand(s, workingthread);
    }
}
