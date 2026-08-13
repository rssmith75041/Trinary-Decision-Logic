using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trinary_Decision_Logic
{
    public class MRegister
    {
        private MBaseWord? rValue = null;
        public MBaseWord Value
        {
            get => rValue!;
            set => rValue = value;
        }

        public bool Locked { get; set; }
        public string Name { get; }

        public MRegister(string name)
        {
            Locked = false;
            Name = name;
        }

        public void Lock() { Locked = true; }
        public void Unlock() { Locked = false; }
    }
}
