using System;
using System.Collections.Generic;
using projectMTCG_loeffler.cards;

namespace projectMTCG_loeffler.Battle {
    class BattleHandler {
        public void StartBattle(List<Card>playerOneCards, List<Card>playerTwoCards) {
            int indexPlayerOne = 0;
            int indexPlayerTwo = 0;
            var rng = new Random();

            for (int rounds = 0; rounds < 100; rounds++) {
                if ((playerOneCards[indexPlayerOne] is Monster) && (playerTwoCards[indexPlayerTwo] is Monster)) {
                    //pure monster fights are unaffected by elemental weaknesses
                    if (playerOneCards[indexPlayerOne].AttackPoints > playerTwoCards[indexPlayerTwo].AttackPoints) {
                        playerTwoCards[indexPlayerTwo].Defeated = true;
                    }
                    else if (playerOneCards[indexPlayerOne].AttackPoints < playerTwoCards[indexPlayerTwo].AttackPoints) {
                        playerOneCards[indexPlayerOne].Defeated = true;
                    }
                    else {
                        indexPlayerOne = rng.Next(0, 4);
                        indexPlayerTwo = rng.Next(0, 4);
                    }
                }
                else {
                    if (playerOneCards[indexPlayerOne].AttackPoints *
                        ElementalMultiplier(playerOneCards[indexPlayerOne].Element,
                            playerTwoCards[indexPlayerTwo].Element) < (playerTwoCards[indexPlayerTwo].AttackPoints *
                                                                       ElementalMultiplier(
                                                                           playerTwoCards[indexPlayerTwo].Element,
                                                                           playerOneCards[indexPlayerOne].Element))) {

                    }
                }
                
            }
        }

        public int ElementalMultiplier(Element attackerOne, Element attackerTwo) {
            switch (attackerOne) {
                case Element.Water:
                    return 1;
                default:
                    return 1;
            }
        }
    }
}
