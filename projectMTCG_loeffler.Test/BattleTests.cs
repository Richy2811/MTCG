using System;
using System.Collections.Generic;
using NUnit.Framework;
using projectMTCG_loeffler.cards;
using projectMTCG_loeffler.Battle;

namespace projectMTCG_loeffler.Test {
    public class BattleTests {
        [Test]
        public void Monster_Fight() {
            //arrange
            List<Card> p_One_Cards = new List<Card>() {
                new Dragon("1234-a0b1-c2d3-e4f5", "Dragoon", 40, Element.Fire),
                new Knight("5678-a0b1-c2d3-e4f5", "Stollott", 40, Element.Water),
                new FireElve("0101-a0b1-c2d3-e4f5", "Roji", 40, Element.Water),
                new Goblin("0101-g6h7-i8j9-k1l0", "Gul", 40, Element.Normal)
            };
            List<Card> p_Two_Cards = new List<Card>() {
                new Ork("0111-a1b2-c3d4-e5f", "Bruter", 19, Element.Fire),
                new Kraken("0111-g6h7-i8j9-k1l0", "Kask", 19, Element.Water),
                new Wizzard("0101-m1n1-o1p2-q1r3", "Waterpulse", 19, Element.Water),
                new Ork("0101-s1t4-u1v5-w1x6", "Jorg", 19, Element.Normal)
            };
            var battle = new BattleHandler("Anton", "Lucas", p_One_Cards, p_Two_Cards);

            //act
            battle.StartBattle();
            int winner = battle.GetWinner();

            //assert
            Assert.AreEqual(1, winner);
        }

        [Test]
        public void Spell_Fight() {
            //arrange
            List<Card> p_One_Cards = new List<Card>() {
                new Spell("1234-a0b1-c2d3-e4f5", "Flame", 40, Element.Fire),
                new Spell("5678-a0b1-c2d3-e4f5", "Bubble", 40, Element.Water),
                new Spell("0101-a0b1-c2d3-e4f5", "Wind", 40, Element.Normal),
                new Spell("0101-g6h7-i8j9-k1l0", "Earthquake", 40, Element.Normal)
            };
            List<Card> p_Two_Cards = new List<Card>() {
                new Spell("0111-a1b2-c3d4-e5f", "Incinerate", 10, Element.Fire),
                new Spell("0111-g6h7-i8j9-k1l0", "Torrential Rain", 10, Element.Water),
                new Spell("0101-m1n1-o1p2-q1r3", "Meteor", 10, Element.Fire),
                new Spell("0101-s1t4-u1v5-w1x6", "Hurricane", 10, Element.Normal)
            };
            var battle = new BattleHandler("Victor", "Gloria", p_One_Cards, p_Two_Cards);

            //act
            battle.StartBattle();
            int winner = battle.GetWinner();

            //assert
            Assert.AreEqual(1, winner);
        }

        [Test]
        public void Element_Fight() {
            //arrange
            List<Card> p_One_Cards = new List<Card>() {
                new Spell("1234-a0b1-c2d3-e4f5", "Flame", 40, Element.Fire),
                new Spell("5678-a0b1-c2d3-e4f5", "Heatstorm", 40, Element.Fire),
                new Spell("0101-a0b1-c2d3-e4f5", "Fireblast", 40, Element.Fire),
                new Spell("0101-g6h7-i8j9-k1l0", "Incinerate", 40, Element.Fire)
            };
            List<Card> p_Two_Cards = new List<Card>() {
                new Spell("0111-a1b2-c3d4-e5f", "Flood", 21, Element.Water),
                new Spell("0111-g6h7-i8j9-k1l0", "Torrential Rain", 21, Element.Water),
                new Spell("0101-m1n1-o1p2-q1r3", "Seastorm", 21, Element.Water),
                new Spell("0101-s1t4-u1v5-w1x6", "Bubble", 21, Element.Water)
            };
            var battle = new BattleHandler("Yumi", "Sonja", p_One_Cards, p_Two_Cards);

            //act
            battle.StartBattle();
            int winner = battle.GetWinner();

            //assert
            Assert.AreEqual(2, winner);
        }

        [Test]
        public void Elemental_Advantage_Fire() {
            //arrange
            List<Card> p_One_Cards = new List<Card>() {
                new Dragon("1234-a0b1-c2d3-e4f5", "Dragoon", 11, Element.Fire),
                new Dragon("5678-a0b1-c2d3-e4f5", "Ahrgohr", 11, Element.Fire),
                new Dragon("0101-a0b1-c2d3-e4f5", "Rosha", 11, Element.Fire),
                new Dragon("0101-g6h7-i8j9-k1l0", "Nyra", 11, Element.Fire)
            };
            List<Card> p_Two_Cards = new List<Card>() {
                new Spell("0111-a1b2-c3d4-e5f", "Wind", 40, Element.Normal),
                new Spell("0111-g6h7-i8j9-k1l0", "Earthquake", 40, Element.Normal),
                new Spell("0101-m1n1-o1p2-q1r3", "Ultima", 40, Element.Normal),
                new Spell("0101-s1t4-u1v5-w1x6", "Lightning", 40, Element.Normal)
            };
            var battle = new BattleHandler("Adam", "Perla", p_One_Cards, p_Two_Cards);

            //act
            battle.StartBattle();
            int winner = battle.GetWinner();

            //assert
            Assert.AreEqual(1, winner);
        }

        [Test]
        public void Elemental_Advantage_Normal() {
            //arrange
            List<Card> p_One_Cards = new List<Card>() {
                new Dragon("1234-a0b1-c2d3-e4f5", "Dragoon", 11, Element.Normal),
                new Dragon("5678-a0b1-c2d3-e4f5", "Ahrgohr", 11, Element.Normal),
                new Dragon("0101-a0b1-c2d3-e4f5", "Rosha", 11, Element.Normal),
                new Dragon("0101-g6h7-i8j9-k1l0", "Nyra", 11, Element.Normal)
            };
            List<Card> p_Two_Cards = new List<Card>() {
                new Spell("0111-a1b2-c3d4-e5f", "Ocean Flood", 40, Element.Water),
                new Spell("0111-g6h7-i8j9-k1l0", "Water Beam", 40, Element.Water),
                new Spell("0101-m1n1-o1p2-q1r3", "Water Pulse", 40, Element.Water),
                new Spell("0101-s1t4-u1v5-w1x6", "Waterfall", 40, Element.Water)
            };
            var battle = new BattleHandler("Adam", "Perla", p_One_Cards, p_Two_Cards);

            //act
            battle.StartBattle();
            int winner = battle.GetWinner();

            //assert
            Assert.AreEqual(1, winner);
        }

        [Test]
        public void Elemental_Advantage_Water() {
            //arrange
            List<Card> p_One_Cards = new List<Card>() {
                new Dragon("1234-a0b1-c2d3-e4f5", "Dragoon", 11, Element.Water),
                new Dragon("5678-a0b1-c2d3-e4f5", "Ahrgohr", 11, Element.Water),
                new Dragon("0101-a0b1-c2d3-e4f5", "Rosha", 11, Element.Water),
                new Dragon("0101-g6h7-i8j9-k1l0", "Nyra", 11, Element.Water)
            };
            List<Card> p_Two_Cards = new List<Card>() {
                new Spell("0111-a1b2-c3d4-e5f", "Flame", 40, Element.Fire),
                new Spell("0111-g6h7-i8j9-k1l0", "Sizzle", 40, Element.Fire),
                new Spell("0101-m1n1-o1p2-q1r3", "Fireball", 40, Element.Fire),
                new Spell("0101-s1t4-u1v5-w1x6", "Nova", 40, Element.Fire)
            };
            var battle = new BattleHandler("Adam", "Perla", p_One_Cards, p_Two_Cards);

            //act
            battle.StartBattle();
            int winner = battle.GetWinner();

            //assert
            Assert.AreEqual(1, winner);
        }

        [Test]
        public void Mixed_Fight() {
            //arrange
            List<Card> p_One_Cards = new List<Card>() {
                new Dragon("1234-a0b1-c2d3-e4f5", "Dragoon", 40, Element.Fire),
                new Kraken("5678-a0b1-c2d3-e4f5", "Stollott", 40, Element.Water),
                new FireElve("0101-a0b1-c2d3-e4f5", "Roji", 40, Element.Water),
                new Spell("0101-g6h7-i8j9-k1l0", "Ether", 40, Element.Normal)
            };
            List<Card> p_Two_Cards = new List<Card>() {
                new Spell("0111-a1b2-c3d4-e5f", "Flame", 19, Element.Fire),
                new Spell("0111-g6h7-i8j9-k1l0", "Waterstream", 19, Element.Water),
                new Spell("0101-m1n1-o1p2-q1r3", "Waterpulse", 19, Element.Water),
                new Spell("0101-s1t4-u1v5-w1x6", "Avalange", 19, Element.Normal)
            };
            var battle = new BattleHandler("Adam", "Perla", p_One_Cards, p_Two_Cards);

            //act
            battle.StartBattle();
            int winner = battle.GetWinner();

            //assert
            Assert.AreEqual(1, winner);
        }

        [Test]
        public void Fight_Specialty_Dragon_Goblin() {
            //arrange
            List<Card> p_One_Cards = new List<Card>() {
                new Goblin("0111-a1b2-c3d4-e5f", "Gob", 40, Element.Fire),
                new Goblin("0111-g6h7-i8j9-k1l0", "Gubul", 40, Element.Normal),
                new Goblin("0101-m1n1-o1p2-q1r3", "Giba", 40, Element.Water),
                new Goblin("0101-s1t4-u1v5-w1x6", "Globba", 40, Element.Normal)
            };
            List<Card> p_Two_Cards = new List<Card>() {
                new Dragon("1234-a0b1-c2d3-e4f5", "Dragoon", 10, Element.Fire),
                new Dragon("5678-a0b1-c2d3-e4f5", "Ohgnum", 10, Element.Water),
                new Dragon("0101-a0b1-c2d3-e4f5", "Ighnar", 10, Element.Fire),
                new Dragon("0101-g6h7-i8j9-k1l0", "Lypthra", 10, Element.Normal)
            };
            var battle = new BattleHandler("Robert", "Daniel", p_One_Cards, p_Two_Cards);

            //act
            battle.StartBattle();
            int winner = battle.GetWinner();

            //assert
            Assert.AreEqual(2, winner);
        }

        [Test]
        public void Fight_Specialty_Wizzard_Ork() {
            //arrange
            List<Card> p_One_Cards = new List<Card>() {
                new Ork("0111-a1b2-c3d4-e5f", "Smacra", 40, Element.Fire),
                new Ork("0111-g6h7-i8j9-k1l0", "Ompfa", 40, Element.Normal),
                new Ork("0101-m1n1-o1p2-q1r3", "Sombrul", 40, Element.Water),
                new Ork("0101-s1t4-u1v5-w1x6", "Irgatan", 40, Element.Normal)
            };
            List<Card> p_Two_Cards = new List<Card>() {
                new Wizzard("1234-a0b1-c2d3-e4f5", "Ihsrev", 10, Element.Fire),
                new Wizzard("5678-a0b1-c2d3-e4f5", "Ovas", 10, Element.Water),
                new Wizzard("0101-a0b1-c2d3-e4f5", "Lorth", 10, Element.Fire),
                new Wizzard("0101-g6h7-i8j9-k1l0", "Andra", 10, Element.Normal)
            };
            var battle = new BattleHandler("Nina", "Bob", p_One_Cards, p_Two_Cards);

            //act
            battle.StartBattle();
            int winner = battle.GetWinner();

            //assert
            Assert.AreEqual(2, winner);
        }

        [Test]
        public void Fight_Specialty_WaterSpell_Knight() {
            //arrange
            List<Card> p_One_Cards = new List<Card>() {
                new Knight("0111-a1b2-c3d4-e5f", "Don Quihot", 40, Element.Fire),
                new Knight("0111-g6h7-i8j9-k1l0", "Sir Shiner", 40, Element.Normal),
                new Knight("0101-m1n1-o1p2-q1r3", "Zottra", 40, Element.Water),
                new Knight("0101-s1t4-u1v5-w1x6", "Lord Imklir", 40, Element.Normal)
            };
            List<Card> p_Two_Cards = new List<Card>() {
                new Spell("1234-a0b1-c2d3-e4f5", "Whirlpool", 10, Element.Water),
                new Spell("5678-a0b1-c2d3-e4f5", "Bubble", 10, Element.Water),
                new Spell("0101-a0b1-c2d3-e4f5", "Flood", 10, Element.Water),
                new Spell("0101-g6h7-i8j9-k1l0", "River Rain", 10, Element.Water)
            };
            var battle = new BattleHandler("Paul", "Austin", p_One_Cards, p_Two_Cards);

            //act
            battle.StartBattle();
            int winner = battle.GetWinner();

            //assert
            Assert.AreEqual(2, winner);
        }

        [Test]
        public void Fight_Specialty_Kraken_Spell() {
            //arrange
            List<Card> p_One_Cards = new List<Card>() {
                new Spell("0111-a1b2-c3d4-e5f", "Sun Inferno", 40, Element.Fire),
                new Spell("0111-g6h7-i8j9-k1l0", "Ultima", 999, Element.Normal),
                new Spell("0101-m1n1-o1p2-q1r3", "Ocean Wave", 40, Element.Water),
                new Spell("0101-s1t4-u1v5-w1x6", "Tornado", 40, Element.Normal)
            };
            List<Card> p_Two_Cards = new List<Card>() {
                new Kraken("1234-a0b1-c2d3-e4f5", "Urjno", 10, Element.Normal),
                new Kraken("5678-a0b1-c2d3-e4f5", "Pouhju", 10, Element.Water),
                new Kraken("0101-a0b1-c2d3-e4f5", "Yffdra", 10, Element.Fire),
                new Kraken("0101-g6h7-i8j9-k1l0", "Olsmurah", 10, Element.Fire)
            };
            var battle = new BattleHandler("George", "Albert", p_One_Cards, p_Two_Cards);

            //act
            battle.StartBattle();
            int winner = battle.GetWinner();

            //assert
            Assert.AreEqual(2, winner);
        }

        [Test]
        public void Fight_Specialty_FireElve_Dragon() {
            //arrange
            List<Card> p_One_Cards = new List<Card>() {
                new Dragon("1234-a0b1-c2d3-e4f5", "Dragoon", 40, Element.Fire),
                new Dragon("5678-a0b1-c2d3-e4f5", "Ohgnum", 40, Element.Water),
                new Dragon("0101-a0b1-c2d3-e4f5", "Ighnar", 40, Element.Fire),
                new Dragon("0101-g6h7-i8j9-k1l0", "Lypthra", 40, Element.Normal)
            };
            List<Card> p_Two_Cards = new List<Card>() {
                new FireElve("0111-a1b2-c3d4-e5f", "Inzi", 10, Element.Fire),
                new FireElve("0111-g6h7-i8j9-k1l0", "Binzi", 10, Element.Normal),
                new FireElve("0101-m1n1-o1p2-q1r3", "Lyjra", 10, Element.Water),
                new FireElve("0101-s1t4-u1v5-w1x6", "Iunise", 10, Element.Normal)
            };
            var battle = new BattleHandler("Cassandra", "Rex", p_One_Cards, p_Two_Cards);

            //act
            battle.StartBattle();
            int winner = battle.GetWinner();

            //assert
            Assert.AreEqual(2, winner);
        }

        [Test]
        public void Multiplier_Water_Water() {
            //arrange
            Element attacker = Element.Water;
            Element attacked = Element.Water;

            //act
            float mult = BattleHandler.ElementalMultiplier(attacker, attacked);
            
            //assert
            Assert.AreEqual(1, mult);
        }

        [Test]
        public void Multiplier_Water_Fire() {
            //arrange
            Element attacker = Element.Water;
            Element attacked = Element.Fire;

            //act
            float mult = BattleHandler.ElementalMultiplier(attacker, attacked);

            //assert
            Assert.AreEqual(2, mult);
        }

        [Test]
        public void Multiplier_Water_Normal() {
            //arrange
            Element attacker = Element.Water;
            Element attacked = Element.Normal;

            //act
            float mult = BattleHandler.ElementalMultiplier(attacker, attacked);

            //assert
            Assert.AreEqual(0.5, mult);
        }

        [Test]
        public void Multiplier_Fire_Water() {
            //arrange
            Element attacker = Element.Fire;
            Element attacked = Element.Water;

            //act
            float mult = BattleHandler.ElementalMultiplier(attacker, attacked);

            //assert
            Assert.AreEqual(0.5, mult);
        }

        [Test]
        public void Multiplier_Fire_Fire() {
            //arrange
            Element attacker = Element.Fire;
            Element attacked = Element.Fire;

            //act
            float mult = BattleHandler.ElementalMultiplier(attacker, attacked);

            //assert
            Assert.AreEqual(1, mult);
        }

        [Test]
        public void Multiplier_Fire_Normal() {
            //arrange
            Element attacker = Element.Fire;
            Element attacked = Element.Normal;

            //act
            float mult = BattleHandler.ElementalMultiplier(attacker, attacked);

            //assert
            Assert.AreEqual(2, mult);
        }

        [Test]
        public void Multiplier_Normal_Water() {
            //arrange
            Element attacker = Element.Normal;
            Element attacked = Element.Water;

            //act
            float mult = BattleHandler.ElementalMultiplier(attacker, attacked);

            //assert
            Assert.AreEqual(2, mult);
        }

        [Test]
        public void Multiplier_Normal_Fire() {
            //arrange
            Element attacker = Element.Normal;
            Element attacked = Element.Fire;

            //act
            float mult = BattleHandler.ElementalMultiplier(attacker, attacked);

            //assert
            Assert.AreEqual(0.5, mult);
        }

        [Test]
        public void Multiplier_Normal_Normal() {
            //arrange
            Element attacker = Element.Normal;
            Element attacked = Element.Normal;

            //act
            float mult = BattleHandler.ElementalMultiplier(attacker, attacked);

            //assert
            Assert.AreEqual(1, mult);
        }

        [Test]
        public void Attempt_Battle_No_Cards() {
            //arrange
            List<Card> p_One_Cards = new List<Card>();
            List<Card> p_Two_Cards = new List<Card>();
            var battle = new BattleHandler("Cassandra", "Rex", p_One_Cards, p_Two_Cards);

            //act
            battle.StartBattle();
            int winner = battle.GetWinner();

            //assert
            Assert.AreEqual(0, winner);
        }

        [Test]
        public void Attempt_Battle_Not_Enough_Cards() {
            //arrange
            List<Card> p_One_Cards = new List<Card>(){
                new Dragon("1234-a0b1-c2d3-e4f5", "Dragoon", 40, Element.Fire),
                new Dragon("5678-a0b1-c2d3-e4f5", "Dragoon", 40, Element.Fire),
                new Dragon("0101-a0b1-c2d3-e4f5", "Dragoon", 40, Element.Fire)
            };
            List<Card> p_Two_Cards = new List<Card>(){
                new Dragon("0101-m1n1-o1p2-q1r3", "Dragoon", 40, Element.Fire),
                new Dragon("0101-s1t4-u1v5-w1x6", "Dragoon", 40, Element.Fire)
            };
            var battle = new BattleHandler("Cassandra", "Rex", p_One_Cards, p_Two_Cards);

            //act
            try {
                battle.StartBattle();
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
            int winner = battle.GetWinner();

            //assert
            Assert.AreEqual(-1, winner);
        }
    }
}