using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GameEngine;

namespace GameRunner
{
	public class GameInstance
	{
		public readonly GameData Data;
		private GameInstance(GameData data) { Data = data; }

        public GameInstance(GameData data, Type player_a_type, Type player_b_type) : this(data)
		{
			pAType = player_a_type;
			pBType = player_b_type;
		}
		public GameInstance(GameData data, int player_a_index, int player_b_index) : this(data)
		{
			pAType = data.Players[player_a_index];
			pBType = data.Players[player_b_index];
		}
        public static GameInstance CreateFromData(GameData data) => new GameInstance(data, 0, 0);
        public static GameInstance CreateFromCardSet(string cardset_file)
        {
            //var props = new GameStartingProps(cardset_file);
            var data = CardsLoader.LoadFromAssembly(cardset_file);
            return new GameInstance(data, 0, 0);
        }
        private GameInstance(Board b)
        {
            board = b;
            StartedQ = board.GameGoing;
        }
        public static GameInstance CreateFakeFromBoard(Board b) => new GameInstance(b);

        public bool StartedQ { get; private set; } = false;
		Type pAType, pBType;
		public Board board;
		public Func<Player, bool> Input = (p) => { throw new Exception("No input handling!"); };
		public void Start()
		{
			CreateOnly(out var A, out var B);

			StartedQ = true;
			OnStart(this);

			while (board.GameGoing)
			{
                if (board.TurnA)
                {
                    board.A.StartTurn();
                    board.InvokeRandomEvents();
                    while (Input(board.A)) { board.InvokeRandomEvents(); board.StepCount++; }
                    board.A.EndTurn();
                    board.InvokeRandomEvents();
                }
                else
                {
                    board.B.StartTurn();
                    board.InvokeRandomEvents();
                    while (Input(board.B)) { board.InvokeRandomEvents(); board.StepCount++; }
                    board.B.EndTurn();
                    board.InvokeRandomEvents();
                }

                board.TurnA = !board.TurnA;
                board.MoveCount++;
                board.StepCount++;
			}

			StartedQ = false;
			OnEnd(this);
		}
		public void CreateOnly(out Player A, out Player B)
		{
			A = (Player)Activator.CreateInstance(pAType);
			B = (Player)Activator.CreateInstance(pBType);
			board = new Board(A, B);
		}

		public Action<GameInstance> OnStart = (gd) => { };
		public Action<GameInstance> OnEnd = (gd) => { };
	}
}
