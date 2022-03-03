using System;
using System.Collections.Generic;
using projectMTCG_loeffler.cards;

namespace projectMTCG_loeffler.Battle {
    public class BattleHandler {
        private List<Card> _playerOneCards;
        private List<Card> _playerTwoCards;

        private int _indexP1;
        private int _indexP2;

        public BattleHandler(List<Card> playerOneCards, List<Card> playerTwoCards) {
            _playerOneCards = playerOneCards;
            _playerTwoCards = playerTwoCards;

            _indexP1 = 0;
            _indexP2 = 0;
        }

        public static float ElementalMultiplier(Element attackerCardElement, Element attackedCardElement) {
            switch (attackerCardElement) {
                case Element.Water:
                    switch (attackedCardElement) {
                        case Element.Water:
                            return 1;

                        case Element.Fire:
                            return 2;

                        case Element.Normal:
                            return (float)0.5;

                        default:
                            return 1;
                    }

                case Element.Fire:
                    switch (attackedCardElement) {
                        case Element.Water:
                            return (float)0.5;

                        case Element.Fire:
                            return 1;

                        case Element.Normal:
                            return 2;

                        default:
                            return 1;
                    }

                case Element.Normal:
                    switch (attackedCardElement) {
                        case Element.Water:
                            return 2;

                        case Element.Fire:
                            return (float)0.5;

                        case Element.Normal:
                            return 1;

                        default:
                            return 1;
                    }

                default:
                    return 1;
            }
        }

        public int CheckSpecialty() {
            //check if a special condition applies
            //if a special condition applies where one card is unable to attack the other one or if one card instantly defeats the other card then the player number of the losing card gets returned
            
            //Dragon > Goblin
            if (_playerOneCards[_indexP1] is Dragon && _playerTwoCards[_indexP2] is Goblin) {
                Console.WriteLine($"{_playerTwoCards[_indexP2].Name} is to afraid to attack {_playerOneCards[_indexP1].Name}. {_playerTwoCards[_indexP2].Name} got defeated");
                return 2;
            }
            if (_playerTwoCards[_indexP2] is Dragon && _playerOneCards[_indexP1] is Goblin) {
                Console.WriteLine($"{_playerOneCards[_indexP1].Name} is to afraid to attack {_playerTwoCards[_indexP2].Name}. {_playerOneCards[_indexP1].Name} got defeated");
                return 1;
            }

            //Wizzard > Ork
            if (_playerOneCards[_indexP1] is Wizzard && _playerTwoCards[_indexP2] is Ork) {
                Console.WriteLine($"{_playerOneCards[_indexP1].Name} put {_playerTwoCards[_indexP2].Name} under its control. {_playerTwoCards[_indexP2].Name} got defeated");
                return 2;
            }
            if (_playerTwoCards[_indexP2] is Wizzard && _playerOneCards[_indexP1] is Ork) {
                Console.WriteLine($"{_playerTwoCards[_indexP2].Name} put {_playerOneCards[_indexP1].Name} under its control. {_playerOneCards[_indexP1].Name} got defeated");
                return 1;
            }

            //Water Spell > Knight
            if ((_playerOneCards[_indexP1] is Spell && _playerOneCards[_indexP1].Element == Element.Water) && _playerTwoCards[_indexP2] is Knight) {
                Console.WriteLine($"{_playerOneCards[_indexP1].Name} drowned {_playerTwoCards[_indexP2].Name}. {_playerTwoCards[_indexP2].Name} got defeated");
                return 1;
            }
            if ((_playerTwoCards[_indexP2] is Spell && _playerTwoCards[_indexP2].Element == Element.Water) && _playerOneCards[_indexP1] is Knight) {
                Console.WriteLine($"{_playerTwoCards[_indexP2].Name} drowned {_playerOneCards[_indexP1].Name}. {_playerOneCards[_indexP1].Name} got defeated");
                return 1;
            }

            //Kraken > Spell
            if (_playerOneCards[_indexP1] is Kraken && _playerTwoCards[_indexP2] is Spell) {
                Console.WriteLine($"{_playerTwoCards[_indexP2].Name} has no effect on {_playerOneCards[_indexP1].Name}. {_playerTwoCards[_indexP2].Name} got defeated");
                return 2;
            }
            if (_playerTwoCards[_indexP2] is Kraken && _playerOneCards[_indexP1] is Spell) {
                Console.WriteLine($"{_playerOneCards[_indexP1].Name} has no effect on {_playerTwoCards[_indexP2].Name}. {_playerOneCards[_indexP1].Name} got defeated");
                return 1;
            }

            //FireElve > Dragon
            if (_playerOneCards[_indexP1] is FireElve && _playerTwoCards[_indexP2] is Dragon) {
                Console.WriteLine($"{_playerTwoCards[_indexP2].Name} can not hit {_playerOneCards[_indexP1].Name} due to its size. {_playerTwoCards[_indexP2].Name} got defeated");
                return 2;
            }
            if (_playerTwoCards[_indexP2] is FireElve && _playerOneCards[_indexP1] is Dragon) {
                Console.WriteLine($"{_playerOneCards[_indexP1].Name} can not hit {_playerTwoCards[_indexP2].Name} due to its size. {_playerOneCards[_indexP1].Name} got defeated");
                return 1;
            }

            //if no special condition applies 0 gets returned and the round starts as usual
            return 0;
        }

        public int GetWinner() {
            //return winner (1 => player one, 2 => player two, 0 => draw, -1 => error)
            if (_playerTwoCards.Count == 0) {
                return 1;
            }
            else if (_playerOneCards.Count == 0) {
                return 2;
            }
            else if ((_playerOneCards.Count == 0) && (_playerTwoCards.Count == 0)) {
                return 0;
            }
            else {
                return -1;
            }
        }

        public void PrintP1CardLost(int _indexP1, int _indexP2) {
            Console.WriteLine($"{_playerOneCards[_indexP1].Name} ({_playerOneCards[_indexP1].AttackPoints} atk) lost to {_playerTwoCards[_indexP2].Name} ({_playerTwoCards[_indexP2].AttackPoints} atk)");
            return;
        }

        public void PrintP2CardLost(int _indexP1, int _indexP2) {
            Console.WriteLine($"{_playerTwoCards[_indexP2].Name} ({_playerTwoCards[_indexP2].AttackPoints} atk) lost to {_playerOneCards[_indexP1].Name} ({_playerOneCards[_indexP1].AttackPoints} atk)");
            return;
        }

        public void StartBattle() {
            if ((_playerOneCards.Count == 0) || (_playerTwoCards.Count == 0)) {
                Console.Error.WriteLine("Error: empty card package given");
                return;
            }
            
            var rng = new Random();

            //start battle; each round the cards are pit against each other; the card with less attack power gets removed from the list of of cards that are able to battle
            //in case of equivalent attack on each side, a random card from each deck gets chosen to battle; if the battle is not over after a certain amount of rounds the battle is decided as a draw
            for (int round = 0; round < 100; round++) {
                if ((_playerOneCards.Count == 0) || (_playerTwoCards.Count == 0)) {
                    break;
                }

                Console.WriteLine($"Round {round}:");
                Console.WriteLine($"The {_playerOneCards[_indexP1].Element} {_playerOneCards[_indexP1].GetType()} {_playerOneCards[_indexP1].Name} " +
                                  $"vs The {_playerTwoCards[_indexP2].Element} {_playerTwoCards[_indexP2].GetType()} {_playerTwoCards[_indexP2].Name}");

                //check for special condition
                if (CheckSpecialty() == 1) {
                    _playerOneCards.RemoveAt(_indexP1);
                    _indexP1 = rng.Next(0, _playerOneCards.Count);
                    continue;
                }
                else if (CheckSpecialty() == 2) {
                    _playerTwoCards.RemoveAt(_indexP2);
                    _indexP2 = rng.Next(0, _playerTwoCards.Count);
                    continue;
                }

                //no special condition -> continue round
                if ((_playerOneCards[_indexP1] is Monster) && (_playerTwoCards[_indexP2] is Monster)) {
                    //pure monster fights are unaffected by elemental weaknesses
                    if (_playerOneCards[_indexP1].AttackPoints > _playerTwoCards[_indexP2].AttackPoints) {
                        PrintP2CardLost(_indexP1, _indexP2);
                        _playerTwoCards.RemoveAt(_indexP2);
                        _indexP2 = rng.Next(0, _playerTwoCards.Count);
                    }
                    else if (_playerOneCards[_indexP1].AttackPoints < _playerTwoCards[_indexP2].AttackPoints) {
                        PrintP1CardLost(_indexP1, _indexP2);
                        _playerOneCards.RemoveAt(_indexP1);
                        _indexP1 = rng.Next(0, _playerOneCards.Count);
                    }
                    else {
                        Console.WriteLine("Both opposing cards are equal in power. There is no winner this round");
                        _indexP1 = rng.Next(0, _playerOneCards.Count);
                        _indexP2 = rng.Next(0, _playerTwoCards.Count);
                    }
                }
                else {
                    if (_playerOneCards[_indexP1].AttackPoints * ElementalMultiplier(_playerOneCards[_indexP1].Element, _playerTwoCards[_indexP2].Element)
                        > (_playerTwoCards[_indexP2].AttackPoints * ElementalMultiplier(_playerTwoCards[_indexP2].Element, _playerOneCards[_indexP1].Element))) {
                        PrintP2CardLost(_indexP1, _indexP2);
                        _playerTwoCards.RemoveAt(_indexP2);
                        _indexP2 = rng.Next(0, _playerTwoCards.Count);
                    }
                    else if (_playerOneCards[_indexP1].AttackPoints * ElementalMultiplier(_playerOneCards[_indexP1].Element, _playerTwoCards[_indexP2].Element)
                        < (_playerTwoCards[_indexP2].AttackPoints * ElementalMultiplier(_playerTwoCards[_indexP2].Element, _playerOneCards[_indexP1].Element))) {
                        PrintP1CardLost(_indexP1, _indexP2);
                        _playerOneCards.RemoveAt(_indexP1);
                        _indexP1 = rng.Next(0, _playerOneCards.Count);
                    }
                    else {
                        Console.WriteLine("Both opposing cards are equal in power. There is no winner this round");
                        _indexP1 = rng.Next(0, _playerOneCards.Count);
                        _indexP2 = rng.Next(0, _playerTwoCards.Count);
                    }
                }
            }

            switch (GetWinner()) {
                case 1:
                    Console.WriteLine("Player one wins");
                    break;

                case 2:
                    Console.WriteLine("Player two wins");
                    break;

                case 0:
                    Console.WriteLine("It is a draw");
                    break;

                case -1:
                    //should not occur
                    Console.Error.WriteLine("An error occurred");
                    break;
            }
        }
    }
}
