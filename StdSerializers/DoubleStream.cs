using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.CompilerServices;

namespace StdSerializers
{
    public sealed class DoubleStream
    {
        readonly int inputSize;
        double[] buffer;
        int pos = 0;

        public DoubleStream(int inputsize)
        {
            this.inputSize = inputsize;
            buffer = new double[inputSize];
        }

        public double this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => buffer[pos + index];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => buffer[pos + index] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(double d) { buffer[pos++] = d; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteEmpty(int len) { pos += len; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Skip(int len) { pos += len; }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteOneHot(int len, int hotIndex)
        {
            buffer[pos + hotIndex] = 1;
            pos += len;
        }

        public double[] ToArray()
        {
            if (pos != inputSize) { throw new Exception("Bad stream size! Probably isn't finished writing"); }
            return buffer;
        }
    }
}
