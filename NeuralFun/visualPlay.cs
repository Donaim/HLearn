using GameEngine;
using GameRunner;
using GameVisualizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using vutils.Testing;

namespace NeuralFun
{
    class visualBots
    {
        [TestingObject]
        static void visualRandom(string cardSetPath = root.StdCardset, string serializerName = root.StdSerializerName)
        {
            new LearningGround.visualRandom(cardSetPath: cardSetPath, serializerName: serializerName).Go();
        }
        [TestingObject]
        static void visualNN(string nnfile, string cardSetPath = root.StdCardset, string serializerName = root.StdSerializerName)
        {
            new LearningGround.visualNN(nnfile: nnfile, cardSetPath: cardSetPath, serializerName: serializerName).Go();
        }
    }
    public class visualPlayclass
	{
        [TestingObject]
        public static void visualPlay(string cards_lib_path = root.StdCardset)
		{
			Console.WriteLine($"Loaded :{cards_lib_path}");

			var game = GameInstance.CreateFromCardSet(cards_lib_path);

			Application.Run(new GamingForm(game));
		}
	}
}
