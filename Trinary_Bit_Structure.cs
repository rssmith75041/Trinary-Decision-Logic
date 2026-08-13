using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.AxHost;

namespace Trinary_Decision_Logic
{
    //001 = No, 010 = Yes, 100 = Maybe
    //111 -> Superposition
    //000 -> must collaps
    [Flags]
    public enum MFlags : byte
    {
        False = 1 << 0, // 001
        True = 1 << 1,  // 010
        Maybe = 1 << 2, // 100
        Superposed = 1 << 3
    }

    public class MBit
    {
        public MFlags Flags;

        public MBit(MFlags flags) => Flags = flags;

        public bool IsFalse => (Flags & MFlags.False) != 0;
        public bool IsTrue => (Flags & MFlags.True) != 0;
        public bool IsMaybe => (Flags & MFlags.Maybe) != 0;

        public override string ToString()
        {
            return Flags.ToString();
        }
    }

    public abstract class MBaseWord
    {
        public MBit[]? Bits;

        public MBaseWord Copy()
        {
            MBaseWord rValue = Bits!.Length switch
            {
                16 => new MWord(),
                32 => new MDWord(),
                64 => new MQWord(),
                _ => new MByte()
            };

            for (int i = 0; i < Bits.Length; i++)
            {
                rValue.Bits![i] = new MBit(this.Bits[i].Flags);
            }

            return rValue;
        }
    }

    public class MByte : MBaseWord
    {
        public MByte()
        {
            Bits = new MBit[8];

            for (int i = 0; i < Bits.Length; i++)
            {
                Bits[i] = new MBit(MFlags.Maybe);
            }
        }
    }

    public class MWord : MBaseWord
    {
        public MWord()
        {
            Bits = new MBit[16];

            for (int i = 0; i < Bits.Length; i++)
            {
                Bits[i] = new MBit(MFlags.Maybe);
            }
        }
    }

    public class MDWord : MBaseWord
    {
        public MDWord()
        {
            Bits = new MBit[32];

            for (int i = 0; i < Bits.Length; i++)
            {
                Bits[i] = new MBit(MFlags.Maybe);
            }
        }
    }

    public class MQWord : MBaseWord
    {
        public MQWord()
        {
            Bits = new MBit[64];

            for (int i = 0; i < Bits.Length; i++)
            {
                Bits[i] = new MBit(MFlags.Maybe);
            }
        }
    }
}
