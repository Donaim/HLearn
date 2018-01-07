using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static System.Console;
using GameRunner;
using GameEngine;

namespace NeuralFun
{
	class root
	{
        public const string StdDir = @"/home/d0naim/tmp/hlearning/";
        public const string StdCardset
            //= @"E:\OneDrive\Documents\Programing\C#\Projects\HearthstoneLearning\card_sets\primitive\bin\Release\primitive.dll";
            = @"/home/d0naim/dev/HearthstoneLearning/card_sets/realset/bin/Release/realset.dll";
        public const string StdSerializerName =
            //nameof(StdSerializers.AllOneHotSerial);
            nameof(StdSerializers.SingleIndexingSerial);

        public static void Main(string[] args)
		{
            vutils.Testing.TestingModule.ChooseMethodsLoop();
		}

        public static void GAll(string cardSetPath, string serializerName, out GameData data, out GameInstance gi, out StdSerializers.ISerializer ser)
        {
            data = CardsLoader.LoadFromAssembly(cardSetPath);
            gi = GameInstance.CreateFromData(data);
            ser = StdSerializers.Util.CreateFromName(serializerName, data);
        }
	}
}
