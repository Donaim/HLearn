using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using vutils.Testing;

namespace NeuralFun
{
	class randomPlayers
	{
		public class RandVsRandclass : IAsyncTesting
        {
            LearningGround.virtualSimulator simulator;

            [TestingObject]
			void RandVsRand(string cardSetPath = root.StdCardset, string targetDir = root.StdDir, string serializerName = root.StdSerializerName)
			{
                root.GAll(cardSetPath, serializerName, out var data, out var gi, out var ser);
                simulator = new LearningGround.RandVsRand(gi, cardSetPath: cardSetPath, targetdir: targetDir, ser: ser);
                simulator.Go();
			}

            public void Command(string s, Thread workingthread) => simulator.ReadCommand(s, workingthread);
        }
    }
}
