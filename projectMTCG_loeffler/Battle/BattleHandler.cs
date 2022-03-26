using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using projectMTCG_loeffler.cards;
using projectMTCG_loeffler.Database;

namespace projectMTCG_loeffler.Battle {
    public class BattleHandler {
        private string _challengerName;
        private string _opponentName;

        private List<Card> _playerOneCards;
        private List<Card> _playerTwoCards;

        private int _indexP1;
        private int _indexP2;

        private Random _rng = new Random();
        private DbHandler _dbHandler;
        private StringBuilder _battleLog = new StringBuilder();

        private void JarrayExtractCards(JArray playerOneCards, JArray playerTwoCards) {
            if ((playerOneCards == null) || (playerTwoCards == null)) {
                return;
            }

            Element currentElement = Element.Water;
            foreach (JToken card in playerOneCards) {
                switch (card["Element"].ToString()) {
                    case "Water":
                        currentElement = Element.Water;
                        break;

                    case "Fire":
                        currentElement = Element.Fire;
                        break;

                    case "Normal":
                        currentElement = Element.Normal;
                        break;
                }
                switch (card["Type"].ToString()) {
                    case "Goblin":
                        _playerOneCards.Add(new Goblin(card["Id"].ToString(), card["Name"].ToString(), card["Damage"].ToObject<ushort>(), currentElement));
                        break;

                    case "Dragon":
                        _playerOneCards.Add(new Dragon(card["Id"].ToString(), card["Name"].ToString(), card["Damage"].ToObject<ushort>(), currentElement));
                        break;

                    case "Kraken":
                        _playerOneCards.Add(new Kraken(card["Id"].ToString(), card["Name"].ToString(), card["Damage"].ToObject<ushort>(), currentElement));
                        break;

                    case "Wizzard":
                        _playerOneCards.Add(new Wizzard(card["Id"].ToString(), card["Name"].ToString(), card["Damage"].ToObject<ushort>(), currentElement));
                        break;

                    case "Knight":
                        _playerOneCards.Add(new Knight(card["Id"].ToString(), card["Name"].ToString(), card["Damage"].ToObject<ushort>(), currentElement));
                        break;

                    case "FireElve":
                        _playerOneCards.Add(new FireElve(card["Id"].ToString(), card["Name"].ToString(), card["Damage"].ToObject<ushort>(), currentElement));
                        break;

                    case "Ork":
                        _playerOneCards.Add(new Ork(card["Id"].ToString(), card["Name"].ToString(), card["Damage"].ToObject<ushort>(), currentElement));
                        break;

                    case "Spell":
                        _playerOneCards.Add(new Spell(card["Id"].ToString(), card["Name"].ToString(), card["Damage"].ToObject<ushort>(), currentElement));
                        break;
                }
            }

            foreach (JToken card in playerTwoCards) {
                switch (card["Element"].ToString()) {
                    case "Water":
                        currentElement = Element.Water;
                        break;

                    case "Fire":
                        currentElement = Element.Fire;
                        break;

                    case "Normal":
                        currentElement = Element.Normal;
                        break;
                }
                switch (card["Type"].ToString()) {
                    case "Goblin":
                        _playerTwoCards.Add(new Goblin(card["Id"].ToString(), card["Name"].ToString(), card["Damage"].ToObject<ushort>(), currentElement));
                        break;

                    case "Dragon":
                        _playerTwoCards.Add(new Dragon(card["Id"].ToString(), card["Name"].ToString(), card["Damage"].ToObject<ushort>(), currentElement));
                        break;

                    case "Kraken":
                        _playerTwoCards.Add(new Kraken(card["Id"].ToString(), card["Name"].ToString(), card["Damage"].ToObject<ushort>(), currentElement));
                        break;

                    case "Wizzard":
                        _playerTwoCards.Add(new Wizzard(card["Id"].ToString(), card["Name"].ToString(), card["Damage"].ToObject<ushort>(), currentElement));
                        break;

                    case "Knight":
                        _playerTwoCards.Add(new Knight(card["Id"].ToString(), card["Name"].ToString(), card["Damage"].ToObject<ushort>(), currentElement));
                        break;

                    case "FireElve":
                        _playerTwoCards.Add(new FireElve(card["Id"].ToString(), card["Name"].ToString(), card["Damage"].ToObject<ushort>(), currentElement));
                        break;

                    case "Ork":
                        _playerTwoCards.Add(new Ork(card["Id"].ToString(), card["Name"].ToString(), card["Damage"].ToObject<ushort>(), currentElement));
                        break;

                    case "Spell":
                        _playerTwoCards.Add(new Spell(card["Id"].ToString(), card["Name"].ToString(), card["Damage"].ToObject<ushort>(), currentElement));
                        break;
                }
            }
        }
        

        public BattleHandler(string opponentJsonString, Dictionary<string, string> headerParts, DbHandler dbHandler) {
            _dbHandler = dbHandler;

            //get challenger and opponent name
            JObject opponent = JObject.Parse(opponentJsonString);
            _challengerName = dbHandler.BasicAuthGetUsername(headerParts["Authorization"]);
            _opponentName = opponent["Username"].ToString();

            //get carddeck from both users
            JArray playerOneCards = dbHandler.GetCards(_challengerName, false);
            JArray playerTwoCards = dbHandler.GetCards(_opponentName, false);

            _playerOneCards = new List<Card>();
            _playerTwoCards = new List<Card>();
            JarrayExtractCards(playerOneCards, playerTwoCards);

            _indexP1 = _rng.Next(0, _playerOneCards.Count);
            _indexP2 = _rng.Next(0, _playerTwoCards.Count);
        }


        public BattleHandler(string challengerName, string opponentName, List<Card> playerOneCards, List<Card> playerTwoCards) {
            _challengerName = challengerName;
            _opponentName = opponentName;

            _playerOneCards = playerOneCards;
            _playerTwoCards = playerTwoCards;

            _indexP1 = _rng.Next(0, _playerOneCards.Count);
            _indexP2 = _rng.Next(0, _playerTwoCards.Count);
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
                _battleLog.AppendLine($"{_playerTwoCards[_indexP2].Name} is to afraid to attack {_playerOneCards[_indexP1].Name}. {_playerTwoCards[_indexP2].Name} got defeated");
                return 2;
            }
            if (_playerTwoCards[_indexP2] is Dragon && _playerOneCards[_indexP1] is Goblin) {
                _battleLog.AppendLine($"{_playerOneCards[_indexP1].Name} is to afraid to attack {_playerTwoCards[_indexP2].Name}. {_playerOneCards[_indexP1].Name} got defeated");
                return 1;
            }

            //Wizzard > Ork
            if (_playerOneCards[_indexP1] is Wizzard && _playerTwoCards[_indexP2] is Ork) {
                _battleLog.AppendLine($"{_playerOneCards[_indexP1].Name} put {_playerTwoCards[_indexP2].Name} under its control. {_playerTwoCards[_indexP2].Name} got defeated");
                return 2;
            }
            if (_playerTwoCards[_indexP2] is Wizzard && _playerOneCards[_indexP1] is Ork) {
                _battleLog.AppendLine($"{_playerTwoCards[_indexP2].Name} put {_playerOneCards[_indexP1].Name} under its control. {_playerOneCards[_indexP1].Name} got defeated");
                return 1;
            }

            //Water Spell > Knight
            if ((_playerOneCards[_indexP1] is Spell && _playerOneCards[_indexP1].Element == Element.Water) && _playerTwoCards[_indexP2] is Knight) {
                _battleLog.AppendLine($"{_playerOneCards[_indexP1].Name} drowned {_playerTwoCards[_indexP2].Name}. {_playerTwoCards[_indexP2].Name} got defeated");
                return 1;
            }
            if ((_playerTwoCards[_indexP2] is Spell && _playerTwoCards[_indexP2].Element == Element.Water) && _playerOneCards[_indexP1] is Knight) {
                _battleLog.AppendLine($"{_playerTwoCards[_indexP2].Name} drowned {_playerOneCards[_indexP1].Name}. {_playerOneCards[_indexP1].Name} got defeated");
                return 1;
            }

            //Kraken > Spell
            if (_playerOneCards[_indexP1] is Kraken && _playerTwoCards[_indexP2] is Spell) {
                _battleLog.AppendLine($"{_playerTwoCards[_indexP2].Name} has no effect on {_playerOneCards[_indexP1].Name}. {_playerTwoCards[_indexP2].Name} got defeated");
                return 2;
            }
            if (_playerTwoCards[_indexP2] is Kraken && _playerOneCards[_indexP1] is Spell) {
                _battleLog.AppendLine($"{_playerOneCards[_indexP1].Name} has no effect on {_playerTwoCards[_indexP2].Name}. {_playerOneCards[_indexP1].Name} got defeated");
                return 1;
            }

            //FireElve > Dragon
            if (_playerOneCards[_indexP1] is FireElve && _playerTwoCards[_indexP2] is Dragon) {
                _battleLog.AppendLine($"{_playerTwoCards[_indexP2].Name} can not hit {_playerOneCards[_indexP1].Name} due to its size. {_playerTwoCards[_indexP2].Name} got defeated");
                return 2;
            }
            if (_playerTwoCards[_indexP2] is FireElve && _playerOneCards[_indexP1] is Dragon) {
                _battleLog.AppendLine($"{_playerOneCards[_indexP1].Name} can not hit {_playerTwoCards[_indexP2].Name} due to its size. {_playerOneCards[_indexP1].Name} got defeated");
                return 1;
            }

            //if no special condition applies 0 gets returned and the round starts as usual
            return 0;
        }

        public int GetWinner() {
            //return winner (1 => player one, 2 => player two, 0 => draw, -1 => error)
            if (_playerTwoCards.Count < _playerOneCards.Count) {
                return 1;
            }
            else if (_playerOneCards.Count < _playerTwoCards.Count) {
                return 2;
            }
            else if (_playerOneCards.Count == _playerTwoCards.Count) {
                return 0;
            }
            else {
                return -1;
            }
        }

        public void PrintP1CardLost(int _indexP1, int _indexP2) {
            _battleLog.AppendLine($"{_playerOneCards[_indexP1].Name} ({_playerOneCards[_indexP1].AttackPoints} atk) lost to {_playerTwoCards[_indexP2].Name} ({_playerTwoCards[_indexP2].AttackPoints} atk)");
            _battleLog.AppendLine($"{_playerOneCards[_indexP1].Name} is now under the control of {_opponentName}");
            return;
        }

        public void PrintP2CardLost(int _indexP1, int _indexP2) {
            _battleLog.AppendLine($"{_playerTwoCards[_indexP2].Name} ({_playerTwoCards[_indexP2].AttackPoints} atk) lost to {_playerOneCards[_indexP1].Name} ({_playerOneCards[_indexP1].AttackPoints} atk)");
            _battleLog.AppendLine($"{_playerTwoCards[_indexP2].Name} is now under the control of {_challengerName}");
            return;
        }

        public string StartBattle() {
            if ((_playerOneCards.Count == 0) || (_playerTwoCards.Count == 0)) {
                Console.Error.WriteLine("Error: empty card deck given");
                return "The battle could not be carried out. Every player has to have cards in their deck";
            }
            else if ((_playerOneCards.Count != 4) || (_playerTwoCards.Count != 4)) {
                throw new Exception("Number of cards in player cards list does not match expected number of cards");
            }

            int specialityTmp = -1;
            //start battle; each round the cards are pit against each other; the card with less attack power gets removed from the list of of cards that are able to battle
            //in case of equivalent attack on each side, a random card from each deck gets chosen to battle; if the battle is not over after a certain amount of rounds the battle is decided as a draw
            for (int round = 1; round <= 100; round++) {
                if ((_playerOneCards.Count == 0) || (_playerTwoCards.Count == 0)) {
                    break;
                }
                if (round != 1) {
                    _battleLog.AppendLine();
                }
                _battleLog.AppendLine($"Player {_challengerName} remaining cards: {_playerOneCards.Count} | Player {_opponentName} remaining cards: {_playerTwoCards.Count}");

                _indexP1 = _rng.Next(0, _playerOneCards.Count);
                _indexP2 = _rng.Next(0, _playerTwoCards.Count);
                
                //add an additional space after every round for better visibility
                _battleLog.AppendLine("----------------------------------------------------------------");
                
                _battleLog.AppendLine($"Round {round}:");
                _battleLog.AppendLine($"The {_playerOneCards[_indexP1].Element} {_playerOneCards[_indexP1].GetType().Name} {_playerOneCards[_indexP1].Name} ({_challengerName}) " +
                                  $"vs The {_playerTwoCards[_indexP2].Element} {_playerTwoCards[_indexP2].GetType().Name} {_playerTwoCards[_indexP2].Name} ({_opponentName})");

                specialityTmp = CheckSpecialty();
                //check for special condition (method returns losing player number)
                if (specialityTmp == 1) {
                    _battleLog.AppendLine($"{_playerOneCards[_indexP1].Name} is now under the control of {_opponentName}");

                    //move card from player one to player two
                    _playerTwoCards.Add(_playerOneCards[_indexP1]);
                    _playerOneCards.RemoveAt(_indexP1);
                    continue;
                }
                else if (specialityTmp == 2) {
                    _battleLog.AppendLine($"{_playerTwoCards[_indexP2].Name} is now under the control of {_challengerName}");

                    //move card from player two to player one
                    _playerOneCards.Add(_playerTwoCards[_indexP2]);
                    _playerTwoCards.RemoveAt(_indexP2);
                    continue;
                }

                //no special condition -> continue round
                if ((_playerOneCards[_indexP1] is Monster) && (_playerTwoCards[_indexP2] is Monster)) {
                    //pure monster fights are unaffected by elemental weaknesses
                    if (_playerOneCards[_indexP1].AttackPoints > _playerTwoCards[_indexP2].AttackPoints) {
                        PrintP2CardLost(_indexP1, _indexP2);

                        //move card from player two to player one
                        _playerOneCards.Add(_playerTwoCards[_indexP2]);
                        _playerTwoCards.RemoveAt(_indexP2);
                    }
                    else if (_playerOneCards[_indexP1].AttackPoints < _playerTwoCards[_indexP2].AttackPoints) {
                        PrintP1CardLost(_indexP1, _indexP2);

                        //move card from player one to player two
                        _playerTwoCards.Add(_playerOneCards[_indexP1]);
                        _playerOneCards.RemoveAt(_indexP1);
                    }
                    else {
                        _battleLog.AppendLine("Both opposing cards are equal in power. There is no winner this round");
                    }
                }
                else {
                    if (_playerOneCards[_indexP1].AttackPoints * ElementalMultiplier(_playerOneCards[_indexP1].Element, _playerTwoCards[_indexP2].Element)
                        > (_playerTwoCards[_indexP2].AttackPoints * ElementalMultiplier(_playerTwoCards[_indexP2].Element, _playerOneCards[_indexP1].Element))) {
                        PrintP2CardLost(_indexP1, _indexP2);

                        //move card from player two to player one
                        _playerOneCards.Add(_playerTwoCards[_indexP2]);
                        _playerTwoCards.RemoveAt(_indexP2);
                    }
                    else if (_playerOneCards[_indexP1].AttackPoints * ElementalMultiplier(_playerOneCards[_indexP1].Element, _playerTwoCards[_indexP2].Element)
                        < (_playerTwoCards[_indexP2].AttackPoints * ElementalMultiplier(_playerTwoCards[_indexP2].Element, _playerOneCards[_indexP1].Element))) {
                        PrintP1CardLost(_indexP1, _indexP2);

                        //move card from player one to player two
                        _playerTwoCards.Add(_playerOneCards[_indexP1]);
                        _playerOneCards.RemoveAt(_indexP1);
                    }
                    else {
                        _battleLog.AppendLine("Both opposing cards are equal in power. There is no winner this round");
                    }
                }
            }

            _battleLog.AppendLine();

            string winnerTmpStr;
            string loserTmpStr;
            

            switch (GetWinner()) {
                case 1:
                    _battleLog.AppendLine($"{_opponentName} has no more cards to battle with. {_challengerName} wins");
                    _battleLog.AppendLine();
                    _battleLog.AppendLine("The rankings have changed...");
                    try {
                        winnerTmpStr = $"Player {_challengerName} ELO: {_dbHandler.GetEloRating(_challengerName)} -> ";
                        loserTmpStr = $"Player {_opponentName} ELO: {_dbHandler.GetEloRating(_opponentName)} -> ";

                        _dbHandler.UpdateElo(_challengerName, _opponentName);

                        _battleLog.AppendLine(winnerTmpStr + _dbHandler.GetEloRating(_challengerName));
                        _battleLog.AppendLine(loserTmpStr + _dbHandler.GetEloRating(_opponentName));
                    }
                    catch (Exception e) {
                        Console.Error.WriteLine(e.Message);
                        _battleLog.AppendLine($"Error, {e.Message}");
                    }
                    break;

                case 2:
                    _battleLog.AppendLine($"{_challengerName} has no more cards to battle with. {_opponentName} wins");
                    _battleLog.AppendLine();
                    _battleLog.AppendLine("The rankings have changed...");
                    try {
                        winnerTmpStr = $"Player {_opponentName} ELO: {_dbHandler.GetEloRating(_opponentName)} -> ";
                        loserTmpStr = $"Player {_challengerName} ELO: {_dbHandler.GetEloRating(_challengerName)} -> ";

                        _dbHandler.UpdateElo(_opponentName, _challengerName);

                        _battleLog.AppendLine(winnerTmpStr + _dbHandler.GetEloRating(_opponentName));
                        _battleLog.AppendLine(loserTmpStr + _dbHandler.GetEloRating(_challengerName));
                    }
                    catch (Exception e) {
                        Console.Error.WriteLine(e.Message);
                        _battleLog.AppendLine($"Error, {e.Message}");
                    }
                    break;

                case 0:
                    _battleLog.AppendLine("It is a draw. Ranking stays unchanged");
                    break;

                case -1:
                    //can not occur
                    Console.Error.WriteLine("An error occurred. GetWinner() returned unexpected value");
                    break;
            }

            return _battleLog.ToString();
        }
    }
}
