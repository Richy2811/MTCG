using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projectMTCG_loeffler.cards {
    public enum Element {
        Fire,
        Normal,
        Water
    }

    public abstract class Card {
        public string Id { get; protected set; }
        public string Name { get; protected set; }
        public ushort AttackPoints { get; protected set; }
        public Element Element { get; protected set; }
    }
}
