using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using GameEngine;
using GameRunner;

namespace StdSerializers
{
    public interface ISerializer
    {
        string Name { get; }
        GameData Game { get; }
        int InputLength { get; }
        double[] ConvertPosition(Player p);
    }
    public static class Util
    {
        public class TestSerializer : ISerializer
        {
            public GameData Game => throw new NotImplementedException();

            public string Name => throw new NotImplementedException();

            public int InputLength => throw new NotImplementedException();

            public double[] ConvertPosition(Player p)
            {
                throw new NotImplementedException();
            }
        }

        static readonly Assembly currAss;
        public static readonly IReadOnlyList<Type> SerializerTypes;
        static Util()
        {
            currAss = Assembly.GetAssembly(typeof(Util));

            var types = currAss.GetTypes();
            SerializerTypes = types.Where(o => o.GetInterface(nameof(ISerializer)) != null).ToArray();
        }

        public static ISerializer CreateFromType(Type t, GameRunner.GameData data)
        {
            return (ISerializer)Activator.CreateInstance(t, new object[] { data });
        }
        public static ISerializer CreateFromName(string name, GameRunner.GameData data)
        {
            return CreateFromType(SerializerTypes.First(o => o.Name == name), data);
            //return (ISerializer)Activator.CreateInstance(SerializerTypes.First(o => o.Name == name), new object[] { });
            //return (ISerializer)Activator.CreateInstance(currAss.FullName, name, new object[] { });
        }
    }
}
