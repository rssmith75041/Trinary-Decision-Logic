using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trinary_Decision_Logic
{
    public static class MAlu
    {
        public static MFlags Xor(MFlags a, MFlags b)
        {
            if ((a & MFlags.Maybe) != 0 || (b & MFlags.Maybe) != 0) { return MFlags.Maybe; }
            if (a == MFlags.False && b == MFlags.False) { return MFlags.False; }
            if (a == MFlags.True && b == MFlags.True) { return MFlags.True; }

            return MFlags.Maybe;
        }

        public static MFlags And(MFlags a, MFlags b)
        {
            // If either is Maybe, result is Maybe
            if ((a & MFlags.Maybe) != 0 || (b & MFlags.Maybe) != 0) { return MFlags.Maybe; }
            // True * True = True
            if (a == MFlags.True && b == MFlags.True) { return MFlags.True; }
            // Mixed True/False = False
            return MFlags.False;
        }

        public static MFlags Or(MFlags a, MFlags b)
        {
            // If either is True → result is True
            if (a == MFlags.True || b == MFlags.True) { return MFlags.True; }
            // If either is Maybe → result is Maybe
            if ((a & MFlags.Maybe) != 0 || (b & MFlags.Maybe) != 0) { return MFlags.Maybe; }
            // Otherwise both are False
            return MFlags.False;
        }

        public static MFlags Not(MFlags a)
        {
            switch(a)
            {
                case MFlags.False: { return MFlags.True; }
                case MFlags.True: { return MFlags.False; }
                default: { return MFlags.Maybe; }
            }
        }

        public static MBit Xor(MBit a, MBit b) => new MBit(Xor(a.Flags, b.Flags));
        public static MBit And(MBit a, MBit b) => new MBit(And(a.Flags, b.Flags));
        public static MBit Or(MBit a, MBit b) => new MBit(Or(a.Flags, b.Flags));
        public static MBit Not(MBit a) => new MBit(Not(a.Flags));

        public static MBaseWord Xor(MBaseWord a, MBaseWord b)
        {
            MBaseWord r = (MBaseWord)a.Copy();
            int len = a.Bits!.Length;

            for (int i = 0; i < len; i++)
            {
                r.Bits![i] = Xor(a.Bits[i], b.Bits![i]);
            }

            return r;
        }

        public static MBaseWord And(MBaseWord a, MBaseWord b)
        {
            MBaseWord r = (MBaseWord)a.Copy();
            int len = a.Bits!.Length;

            for (int i = 0; i < len; i++)
            {
                r.Bits![i] = And(a.Bits[i], b.Bits![i]);
            }

            return r;
        }

        public static MBaseWord Or(MBaseWord a, MBaseWord b)
        {
            MBaseWord r = (MBaseWord)a.Copy();
            int len = a.Bits!.Length;

            for (int i = 0; i < len; i++)
            {
                r.Bits![i] = Or(a.Bits[i], b.Bits![i]);
            }

            return r;
        }

        public static MBaseWord Not(MBaseWord a)
        {
            MBaseWord r = (MBaseWord)a.Copy();
            int len = a.Bits!.Length;

            for (int i = 0; i < len; i++)
            {
                r.Bits![i] = Not(a.Bits[i]);
            }

            return r;
        }

        public static MBaseWord BAnd(MBaseWord a, MBaseWord b, int bit)
        {
            MBaseWord r = (MBaseWord)a.Copy();
            r.Bits![bit] = And(a.Bits![bit], b.Bits![bit]);
            return r;
        }

        public static MBaseWord BNot(MBaseWord a, int bit)
        {
            MBaseWord r = (MBaseWord)a.Copy();
            r.Bits![bit] = Not(a.Bits![bit]);
            return r;
        }

        public static MBaseWord BOr(MBaseWord a, MBaseWord b, int bit)
        {
            MBaseWord r = (MBaseWord)a.Copy();
            r.Bits![bit] = Or(a.Bits![bit], b.Bits![bit]);
            return r;
        }

        public static MBaseWord BXor(MBaseWord a, MBaseWord b, int bit)
        {
            MBaseWord r = (MBaseWord)a.Copy();
            r.Bits![bit] = Xor(a.Bits![bit], b.Bits![bit]);
            return r;
        }
    }
}
