using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.IO;

using GameEngine;

namespace GameRunner
{
	public interface IGameData { }
    public sealed class GameData
    {
        public readonly IReadOnlyList<Type>
            Spells, Minions, Heroes,
            RandomEvents,
            Modificators,
            Players;

        public readonly CardsLoader.Registry Reg;

        internal GameData(GameDataCollector c)
        {
            Spells = c.Spells.ToArray();
            Minions = c.Minions.ToArray();
            Heroes = c.Heroes.ToArray();

            Modificators = c.Modificators.ToArray();
            RandomEvents = c.RandomEvents.ToArray();

            Players = c.Players.ToArray();

            Reg = c.Registry;
        }
    }
    internal sealed class GameDataCollector
	{

		public readonly List<Type> 
			Spells = new List<Type>(), 
			Minions = new List<Type>(), 
			Heroes = new List<Type>();

		public readonly List<Type> 
            RandomEvents = new List<Type>(),
            Modificators = new List<Type>();

		public readonly List<Type> Players = new List<Type>();

        public readonly CardsLoader.Registry Registry = new CardsLoader.Registry();

        public void Add(Type type)
        {
            Type pretype = type;
            Type parent = type;
            while(parent.BaseType != typeof(object))
            {
                pretype = parent;
                parent = parent.BaseType;
            }

            if (parent == typeof(Card))
            {
                if(pretype == typeof(Spell))
                {
                    Spells.Add(type);
                    Registry.Spells.Reg(type);
                }
                else { throw new NotImplementedException(); }
            }
            else if(parent == typeof(Creature))
            {
                if (pretype == typeof(Minion))
                {
                    Minions.Add(type);
                    Registry.Minions.Reg(type);
                }
                else if (pretype == typeof(Hero))
                {
                    Heroes.Add(type);
                }
                else { throw new NotImplementedException(); }
            }
            else if(parent == typeof(RandomEvent))
            {
                RandomEvents.Add(type);
                Registry.RandomEvents.Reg(type);
            }
            else if (parent == typeof(Player))
            {
                Players.Add(type);
            }
            else if(parent == typeof(Hero.HeroPower)) { }
            else { throw new NotImplementedException(); }
        }
	}

	public static class CardsLoader
	{
        static readonly string ROOTDIR;
        static CardsLoader()
        {
            ROOTDIR = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            while (ROOTDIR.Contains(Path.DirectorySeparatorChar) && Path.GetFileName(ROOTDIR).ToLower().Trim(Path.DirectorySeparatorChar, ' ') != "hearthstonelearning")
            { ROOTDIR = ROOTDIR.Remove(ROOTDIR.LastIndexOf(Path.DirectorySeparatorChar)); }
        }
        static void print_ids(GameDataCollector re)
		{
			Console.WriteLine("Modificators:");
			foreach (var m in re.Modificators) { line(m); }
			Console.WriteLine();

			Console.WriteLine("Spells:");
			foreach (var m in re.Spells) { line(m); }
			Console.WriteLine();

			Console.WriteLine("Minions:");
			foreach (var m in re.Minions) { line(m); }
			Console.WriteLine();

			void line(Type m)
			{
				Console.WriteLine($"[{(int)m.GetField("id", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null)}] {m.Name}");
			}
		}

		public static GameData LoadFromAssembly(string filepath)
		{   
            if (!File.Exists(filepath))
            {
                filepath = Path.Combine(ROOTDIR, filepath);
                if (!File.Exists(filepath))
                {
                    throw new Exception($"File {filepath} does not exist!");
                }
            }
            Console.WriteLine($"Loading assembly: \"{filepath}\"");
            // System.Threading.Thread.Sleep(-1);

			var re = new GameDataCollector();

			var ass = Assembly.UnsafeLoadFrom(filepath);
			foreach(var target in ass.GetTypes())
			{
                if (target.IsAbstract || target.IsSealed) { continue; }
                re.Add(target);
			}

#if sdebug
			print_ids(re);
#endif

            //Register.Loaded = true;
            return new GameData(re);
		}

		public sealed class Registry
		{
            public readonly RegType
                Spells = new Registry.RegType(),
                Minions = new Registry.RegType(),
                RandomEvents = new Registry.RegType(1);
			public class RegType
			{
                public RegType(int starting_index = 0)
                {
                    index = (ushort)starting_index;
                }

                int index;
				HashSet<Type> set = new HashSet<Type>();
				public int Count => index;

				public bool Reg(SingleSpell mod) { return Reg(mod.GetType()); }
				public bool Reg(Type mod) { return PrivateReg(mod, ref index, ref set); }
			}

            static bool PrivateReg(Type mod, ref int index, ref HashSet<Type> set)
			{
				if (set.Contains(mod)) { return false; }

				var field = mod.GetField("id", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if (field == null) { throw new NotImplementedException($"Minion '{mod.Name}' has no static field 'id' !"); }

				field.SetValue(null, index);
				index++;

				set.Add(mod);
				return true;
			}
		}
	}
}
