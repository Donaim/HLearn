using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GameRunner;
using GameEngine;
using GameVisualizer;

using VNNAddOn;
using VNNLib;

namespace LearningGround
{
    public interface IPlayer
    {
        bool DoStep(ref Player p, GameInstance gi);
    }
	public static class randomPlayer
	{
        /*
		public class Fast
		{
			public static bool DoMove(Player p)
			{
                var handCards = p.HandCards.ToArray();
                handCards.ShuffleRandomly();

                var myMinions = p.Minions.ToArray();
                myMinions.ShuffleRandomly();

                //var enemyMinions = p.Enemy.Minions.ToArray();
                //enemyMinions.ShuffleRandomly();

                var enemyCreatures = p.Enemy.Minions.ToList<Creature>();
                enemyCreatures.Add(p.Enemy.Hero);
                enemyCreatures.ShuffleRandomly();

				foreach(var c in handCards)
				{
                    if (rng.Bool())
                    {
                        if(c is ISinglePlayCard spc)
                        {
					        if (spc.CanPlay(out var play)) { play(); } 
                        }
                        else if(c is ITargetPlayCard tpc)
                        {
                            switch (tpc.TargetType)
                            {
                                case TargetSpell.TargetTypes.EnemyCreatures:
                                    foreach(var ec in enemyCreatures)
                                    {
                                        if(tpc.CanPlay(ec, out var onplay)) { onplay(); }
                                    }
                                    break;

                                default:
                                    throw new NotImplementedException();
                            }
                        }
                    }
				}

				foreach (var att in myMinions)
				{
					if (rng.Bool())
					{
                        foreach(var ec in enemyCreatures)
                        {
                            if (att.CanAttack(ec)) { att.Hit(ec); }
                        }
					}
				}

				//var getterHeroTarget = new RandomTargetGetter<Creature>(p.Enemy.Minions.Union(new Creature[] { p.Enemy.Hero }), (c) => p.Hero.CanAttack(c));
				//if (rng.Bool() && getterHeroTarget.GetNext(out var target)) { p.Hero.Hit(target); }

				if (rng.Bool() && p.TryHeroPower(out var pow)) { pow(); }

				return false;
			}
		}
		public class vinstance
		{
			public vinstance()
			{
			}

			HashSet<Card> cards = new HashSet<Card>();
			HashSet<Minion> minions = new HashSet<Minion>();
            
            public bool DoMove(ref Player p)
            {
                var handCards = p.HandCards.ToArray();
                handCards.ShuffleRandomly();

                var myMinions = p.Minions.ToArray();
                myMinions.ShuffleRandomly();

                //var enemyMinions = p.Enemy.Minions.ToArray();
                //enemyMinions.ShuffleRandomly();

                var enemyCreatures = p.Enemy.Minions.ToList<Creature>();
                enemyCreatures.Add(p.Enemy.Hero);
                enemyCreatures.ShuffleRandomly();

                foreach (var c in handCards)
				{
					if (cards.Contains(c)) { continue; }
					cards.Add(c);

                    if (rng.Bool())
                    {
                        if(c is ISinglePlayCard spc)
                        {
                            if(spc.CanPlay(out var play)) { play(); return true; }
                        }
                        else if (c is ITargetPlayCard tpc)
                        {
                            switch (tpc.TargetType)
                            {
                                case TargetSpell.TargetTypes.EnemyCreatures:
                                    foreach (var ec in enemyCreatures)
                                    {
                                        if (tpc.CanPlay(ec, out var onplay)) { onplay(); }
                                    }
                                    break;

                                default:
                                    throw new NotImplementedException();
                            }
                        }
                    }
				}

				foreach (var att in myMinions)
				{
					if (minions.Contains(att)) { continue; }
					minions.Add(att);

					if (rng.Bool())
					{
                        foreach(var ec in enemyCreatures)
                        {
                            if (att.CanAttack(ec)) { att.Hit(ec); return true; }
                        }
					}
				}

				if (rng.Bool() && p.TryHeroPower(out var pow)) { pow(); return true; }

				return false;
			}
		}

        */
        public struct RandomTargetGetter<T>
		{
			List<T> targets;
			Func<T, bool> tester;
			public RandomTargetGetter(IEnumerable<T> _targets, Func<T, bool> _tester)
			{
				tester = _tester;
				targets = new List<T>(_targets);
			}

			public bool GetNext(out T member)
			{
				while (targets.Count > 0)
				{
					int index = rng.Int(targets.Count);
					member = targets[index];
					targets.RemoveAt(index);

					if (tester(member)) { return true; }
				}
				member = default(T);
				return false;
			}
		}
	}

	public class boardCopyPlayer : IPlayer
	{
		public readonly Func<double[], double> eval;
        public readonly StdSerializers.ISerializer serializer;
		public boardCopyPlayer(Func<double[], double> evalFunc, StdSerializers.ISerializer ser)
		{
			eval = evalFunc;
            serializer = ser;
		}

		public void DoMove(ref Player p, GameInstance gi)
		{
			while (DoStep(ref p, gi)) { }
		}
        public bool DoStep(ref Player p, GameInstance gi)
        {
            double endTurnValue = eval(serializer.ConvertPosition(p));

            var boards = GetBoards(p);
            //foreach (var b in boards) { GamingForm.ShowAsync(b, "LOL"); }

            GetMaxBoard(boards, serializer, p.IsA, eval, out var maxBoard, out var maxValue);

            if (maxValue > endTurnValue)
            {
                p = p.IsA ? maxBoard.A : maxBoard.B;
                gi.board = p.Board;
                return true;
            }
            else
            {
                gi.board = p.Board;
                new RandomEvent.EndTurnREvent(p);
                return false;
            }
        }

        //static int max_boards_count = -1;
		static void GetMaxBoard(List<Board> boards, StdSerializers.ISerializer serializer, bool isA, Func<double[], double> eval, out Board maxBoard, out double maxValue)
		{
			maxValue = double.MinValue;
			maxBoard = null;
            //max_boards_count = Math.Max(boards.Count, max_boards_count);

            for(int i = 0; i < boards.Count; i++)
			{
                var b = boards[i];

				var conv = isA ? serializer.ConvertPosition(b.A) : serializer.ConvertPosition(b.B);
				var value = eval(conv);

				if (value > maxValue) { maxValue = value; maxBoard = b; }
			}
		}

		static List<Board> GetBoards(Player p)
		{
			List<Board> results = new List<Board>();
			renewCopies(p, out var copyBoard, out var copyPlayer);

            for(int i = 0, to = copyPlayer.HandCards.Count; i < to; i++)
			{
                var card = (copyPlayer.HandCards)[i];

                if(card is ISinglePlayCard spc)
                {
                    if (getPlaySingle(copyPlayer, spc)) { results.Add(copyBoard); renewCopies(p, out copyBoard, out copyPlayer); }
                }
                else if (card is ITargetPlayCard tpc)
                {
                    handleTargetSpells(tpc.TargetType, p, tpc, results, ref copyBoard, ref copyPlayer);
                }
                else { throw new Exception("WTF?"); }
			}

			for (int attI = 0; attI < copyPlayer.Count; attI++) //attacker index
			{
				for (int tgI = 0; tgI < copyPlayer.Enemy.Count; tgI++) //target index
				{
					if (getHit(copyPlayer, attI, tgI)) { results.Add(copyBoard); renewCopies(p, out copyBoard, out copyPlayer); }
				}
				if (getEnemyHeroHit(copyPlayer, attI)) { results.Add(copyBoard); renewCopies(p, out copyBoard, out copyPlayer); }
			}

			if (copyPlayer.TryHeroPower(out var pow)) { pow(); results.Add(copyBoard); renewCopies(p, out copyBoard, out copyPlayer); }

			return results;
		}
		static void renewCopies(Player p, out Board copyBoard, out Player copyPlayer)
		{
			copyBoard = p.Board.Copy();
			copyPlayer = p.IsA ? copyBoard.A : copyBoard.B;
		}
		static bool getPlaySingle(Player pcopy, ISinglePlayCard spc)
		{
            if (spc.CanPlay(out var play)) { play(); return true; }
            else { return false; }
		}
        static void handleTargetSpells(TargetSpell.TargetTypes type, Player p, ITargetPlayCard tpc, List<Board> results, ref Board copyBoard, ref Player copyPlayer)
        {
            TargetSpell.TargetTypes type1, type2;
            switch (type)
            {
                case TargetSpell.TargetTypes.All:
                    type1 = TargetSpell.TargetTypes.EnemyCreatures;
                    type2 = TargetSpell.TargetTypes.FriendCreatures;
                    break;
                case TargetSpell.TargetTypes.AllMinions:
                    type1 = TargetSpell.TargetTypes.EnemyMinions;
                    type2 = TargetSpell.TargetTypes.FriendMinions;
                    break;

                case TargetSpell.TargetTypes.FriendCreatures:
                    type1 = TargetSpell.TargetTypes.FriendHero;
                    type2 = TargetSpell.TargetTypes.FriendMinions;
                    break;
                case TargetSpell.TargetTypes.EnemyCreatures:
                    type1 = TargetSpell.TargetTypes.EnemyHero;
                    type2 = TargetSpell.TargetTypes.EnemyMinions;
                    break;

                case TargetSpell.TargetTypes.FriendHero:
                    if (getPlayTargeted(copyPlayer, tpc, copyPlayer.Hero)) { results.Add(copyBoard); renewCopies(p, out copyBoard, out copyPlayer); }
                    return;
                case TargetSpell.TargetTypes.EnemyHero:
                    if (getPlayTargeted(copyPlayer, tpc, copyPlayer.Enemy.Hero)) { results.Add(copyBoard); renewCopies(p, out copyBoard, out copyPlayer); }
                    return;

                case TargetSpell.TargetTypes.FriendMinions:
                    for (int j = 0, toem = copyPlayer.Minions.Count; j < toem; j++)
                    {
                        var em = copyPlayer.Minions[j];
                        if (getPlayTargeted(copyPlayer, tpc, em)) { results.Add(copyBoard); renewCopies(p, out copyBoard, out copyPlayer); }
                    }
                    return;
                case TargetSpell.TargetTypes.EnemyMinions:
                    for (int j = 0, toem = copyPlayer.Enemy.Minions.Count; j < toem; j++)
                    {
                        var em = copyPlayer.Enemy.Minions[j];
                        if (getPlayTargeted(copyPlayer, tpc, em)) { results.Add(copyBoard); renewCopies(p, out copyBoard, out copyPlayer); }
                    }
                    return;

                default: throw new NotImplementedException();
            }

            handleTargetSpells(type1, p, tpc, results, ref copyBoard, ref copyPlayer);
            handleTargetSpells(type2, p, tpc, results, ref copyBoard, ref copyPlayer);
        }
        static bool getPlayTargeted(Player pcopy, ITargetPlayCard tpc, Creature c)
        {
            if (tpc.CanPlay(c, out var play)) { play(); return true; }
            else { return false; }
        }
		static bool getHit(Player pcopy, int attI, int tgI)
		{
			var att = (pcopy.Minions)[attI];
			var tg = (pcopy.Enemy.Minions)[tgI];

			if (att.CanAttack(tg)) { att.Hit(tg); return true; }
			else { return false; }
		}
		static bool getEnemyHeroHit(Player pcopy, int attI)
		{
			var att = (pcopy.Minions)[attI];
			var tg = pcopy.Enemy.Hero;

			if (att.CanAttack(tg)) { att.Hit(tg); return true; }
			else { return false; }
		}

		public class neuralPlayer : boardCopyPlayer
		{
			public readonly IFeedResultNN nn;
			public neuralPlayer(string nnfile, StdSerializers.ISerializer ser) : this(new vnn(System.IO.File.ReadAllBytes(nnfile)), ser) { }
			public neuralPlayer(IFeedResultNN net, StdSerializers.ISerializer ser) : base((arr) => net.feedResult(arr)[0], ser)
			{
				nn = net;
			}
		}

		public class extendedRandomPlayer : boardCopyPlayer
		{
			static double Eval(double[] arr) => rng.Double();
			public extendedRandomPlayer(StdSerializers.ISerializer ser) : base(Eval, ser)
			{

			}
		}
	}
}
