using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projectMTCG_loeffler.cards {
    enum Element {
        Fire,
        Normal,
        Water
    }

    abstract class Card : ICard {
        public ushort AttackPoints;
        public bool Defeated;
        public Element Element;

        public abstract void Attack();
    }
}
