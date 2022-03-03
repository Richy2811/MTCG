using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projectMTCG_loeffler.cards {
    public class Dragon : Monster {
        public Dragon(string id, string name, ushort attack, Element element) {
            Id = id;
            Name = name;
            AttackPoints = attack;
            Element = element;
        }
    }
}
