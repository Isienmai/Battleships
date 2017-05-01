using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Battleships
{
    class AITargeter
    {
        #region Variables
        //The random number generator used to pick a random grid coordinate.
        Random randomNumberGenerator;
        //This variable is used to let the AI know wther to hit randomly or to try and hit the same ship again.
        bool locatedUnsunkShip;
        //This variable is used to let the AI know if it knows whether the ship it is trying to hit is placed vertically or horizontally
        char shipDirection;
        //The following array stores one boolean value per ship representing wether or not the ship has been sunk.
        bool[] shipsDown = new bool[5];
        //ThelastTarget
        int[] previousTarget;
        //Stores the gameboard being targeted
        GameBoard boardToTarget;
        //Stores a bool which determines wether or not the AI is trying to sink a hit ship
        bool gettingShipDirection;
        //This bool determines wether or not a Targeter has reached the end of a ship and needs to start hitting it's other end
        bool turning;
        #endregion

        //The constructor seeds the random number generator and sets all values to default
        public AITargeter()
        {
            SetValuesToDefault();
        }

        //Returns a random valid target in the form of a string.
        public string GetAITarget(GameBoard boardToTarget)
        {
            this.boardToTarget = boardToTarget;
            if (locatedUnsunkShip)
            {
                return GetShipBasedTarget();
            }
            else
            {
                shipDirection = '?';
                return GetRandomTarget();
            }
        }

        #region Get Coords

        #region Get Specific Coords
        //Returns a target coordinate that is chosen to try and hit the previously hit ship again
        private string GetShipBasedTarget()
        {
            if (gettingShipDirection)
            {
                return DetermineShipDirection();
            }
            else
            {
                return UseShipDirection();
            }
        }

        //Selects the next target based on the current target and the direction the hit ship is facing
        private string UseShipDirection()
        {
            string toReturn;
            toReturn = "";
            if (!turning && DirectionStillValid())
            {
                toReturn = toReturn + (char)(previousTarget[0] + 65);
                toReturn = toReturn + (char)(previousTarget[1] + 49);
                return toReturn;
            }

            ReverseDirection();
            while (!DirectionStillValid())
            {
                MoveTargetBasedOnShipDirection();
            }
            turning = false;
            toReturn = toReturn + (char)(previousTarget[0] + 65);
            toReturn = toReturn + (char)(previousTarget[1] + 49);
            return toReturn;
        }

        //Changes previousTarget to target the next square in the current direction
        private void MoveTargetBasedOnShipDirection()
        {
            switch (shipDirection)
            {
                case 'n':
                    previousTarget[1]--;
                    break;
                case 'e':
                    previousTarget[0]++;
                    break;
                case 's':
                    previousTarget[1]++;
                    break;
                case 'w':
                    previousTarget[0]--;
                    break;
            }
        }

        //Reverses shipDirection
        private void ReverseDirection()
        {
            switch (shipDirection)
            {
                case 'n':
                    shipDirection = 's';
                    break;
                case 'e':
                    shipDirection = 'w';
                    break;
                case 's':
                    shipDirection = 'n';
                    break;
                case 'w':
                    shipDirection = 'e';
                    break;
            }
        }

        //Returns true if the current direction won't put the target off the board or onto a previously bombed square
        private bool DirectionStillValid()
        {
            switch (shipDirection)
            {
                case 'n':
                    if (previousTarget[1] == 0)
                    {
                        turning = true;
                        return false;
                    }
                    if (boardToTarget.Bombed[previousTarget[0], previousTarget[1] - 1])
                    {
                        turning = true;
                        return false;
                    }
                    if (boardToTarget.ShipsAndSea[previousTarget[0], previousTarget[1] - 1] != boardToTarget.ShipsAndSea[previousTarget[0], previousTarget[1]])
                    {
                        turning = true;
                    }
                    previousTarget[1]--;
                    return true;
                case 'e':
                    if (previousTarget[0] == 9)
                    {
                        turning = true;
                        return false;
                    }
                    if (boardToTarget.Bombed[previousTarget[0] + 1, previousTarget[1]])
                    {
                        turning = true;
                        return false;
                    }
                    if (boardToTarget.ShipsAndSea[previousTarget[0] + 1, previousTarget[1]] != boardToTarget.ShipsAndSea[previousTarget[0], previousTarget[1]])
                    {
                        turning = true;
                    }
                    previousTarget[0]++;
                    return true;
                case 's':
                    if (previousTarget[1] == 9)
                    {
                        turning = true;
                        return false;
                    }
                    if (boardToTarget.Bombed[previousTarget[0], previousTarget[1] + 1])
                    {
                        turning = true;
                        return false;
                    }
                    if (boardToTarget.ShipsAndSea[previousTarget[0], previousTarget[1] + 1] != boardToTarget.ShipsAndSea[previousTarget[0], previousTarget[1]])
                    {
                        turning = true;
                    }
                    previousTarget[1]++;
                    return true;
                case 'w':
                    if (previousTarget[0] == 0)
                    {
                        turning = true;
                        return false;
                    }
                    if (boardToTarget.Bombed[previousTarget[0] - 1, previousTarget[1]])
                    {
                        turning = true;
                        return false;
                    }
                    if (boardToTarget.ShipsAndSea[previousTarget[0] - 1, previousTarget[1]] != boardToTarget.ShipsAndSea[previousTarget[0], previousTarget[1]])
                    {
                        turning = true;
                    }
                    previousTarget[0]--;
                    return true;
                default:
                    return false;
            }
        }

        //Checks each square bordering the current square to determine the direction the targeted ship is facing, returning a coordinate of the first unbombed bordering square
        private string DetermineShipDirection()
        {
            string toReturn;
            toReturn = "";
            if (!BombedNorth(previousTarget) && previousTarget[1] != 0 && shipDirection == '?')
            {
                toReturn = toReturn + (char)(previousTarget[0] + 65);
                toReturn = toReturn + (char)(previousTarget[1] + 48);
                if (boardToTarget.ShipsAndSea[previousTarget[0], previousTarget[1] - 1] == boardToTarget.ShipsAndSea[previousTarget[0], previousTarget[1]])
                {
                    previousTarget[1]--;
                    gettingShipDirection = false;
                }
                shipDirection = 'n';
                return toReturn;
            }
            shipDirection = 'n';
            if (!BombedEast(previousTarget) && previousTarget[0] != 9 && shipDirection == 'n')
            {
                toReturn = toReturn + (char)(previousTarget[0] + 66);
                toReturn = toReturn + (char)(previousTarget[1] + 49);
                if (boardToTarget.ShipsAndSea[previousTarget[0] + 1, previousTarget[1]] == boardToTarget.ShipsAndSea[previousTarget[0], previousTarget[1]])
                {
                    previousTarget[0]++;
                    gettingShipDirection = false;
                }
                shipDirection = 'e';
                return toReturn;
            }
            shipDirection = 'e';
            if (!BombedSouth(previousTarget) && previousTarget[1] != 9 && shipDirection == 'e')
            {
                toReturn = toReturn + (char)(previousTarget[0] + 65);
                toReturn = toReturn + (char)(previousTarget[1] + 50);
                if (boardToTarget.ShipsAndSea[previousTarget[0], previousTarget[1] + 1] == boardToTarget.ShipsAndSea[previousTarget[0], previousTarget[1]])
                {
                    previousTarget[1]++;
                    gettingShipDirection = false;
                }
                shipDirection = 's';
                return toReturn;
            }
            shipDirection = 's';
            if (!BombedWest(previousTarget) && previousTarget[0] != 0 && shipDirection == 's')
            {
                toReturn = toReturn + (char)(previousTarget[0] + 64);
                toReturn = toReturn + (char)(previousTarget[1] + 49);
                if (boardToTarget.ShipsAndSea[previousTarget[0] - 1, previousTarget[1]] == boardToTarget.ShipsAndSea[previousTarget[0], previousTarget[1]])
                {
                    previousTarget[0]--;
                    gettingShipDirection = false;
                }
                shipDirection = 'w';
                return toReturn;
            }
            return "Error";
        }

        //The following four methods each check one direction. They will return true if the next square in their direction has been bombed previously
        #region Directional Bomb Check
        private bool BombedNorth(int[] target)
        {
            if (target[1] != 0)
            {
                return boardToTarget.Bombed[target[0], target[1] - 1];
            }
            return false;
        }

        private bool BombedEast(int[] target)
        {
            if (target[0] != 9)
            {
                return boardToTarget.Bombed[target[0] + 1, target[1]];
            }
            return false;
        }

        private bool BombedSouth(int[] target)
        {
            if (target[1] != 9)
            {
                return boardToTarget.Bombed[target[0], target[1] + 1];
            }
            return false;
        }

        private bool BombedWest(int[] target)
        {
            if (target[0] != 0)
            {
                return boardToTarget.Bombed[target[0] - 1, target[1]];
            }
            return false;
        }
        #endregion
        #endregion

        //Returns a random target coord as a string
        private string GetRandomTarget()
        {
            int x;
            int y;
            string toReturn;

            do
            {
                x = randomNumberGenerator.Next(0, 10);
                y = randomNumberGenerator.Next(0, 10);
                previousTarget[0] = x;
                previousTarget[1] = y;
                toReturn = "";
                toReturn = toReturn + (char)(x + 65);
                toReturn = toReturn + (char)(y + 49);
            }
            while (boardToTarget.Bombed[x, y]);

            return toReturn;
        }
        #endregion

        //Sets all variables to their default value and seeds the random number generator
        private void SetValuesToDefault()
        {
            randomNumberGenerator = new Random(DateTime.Now.Millisecond);
            locatedUnsunkShip = false;
            shipDirection = '?';
            for (int i = 0; i < 5; i++)
            {
                shipsDown[i] = false;
            }
            previousTarget = new int[2];
            gettingShipDirection = true;
            turning = false;
        }        

        //The following properties all allow modification to various variables
        #region Properties
        public bool LocatedUnsunkShip
        {
            set
            {
                locatedUnsunkShip = value;
            }
        }

        public bool GettingShipDirection
        {
            set
            {
                gettingShipDirection = value;
            }
        }

        public bool[] ShipsDown
        {
            get
            {
                return shipsDown;
            }
            set
            {
                shipsDown = value;
            }
        }
        #endregion
    }
}
