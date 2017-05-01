using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Battleships
{
    class AIShipInitialiser
    {
        #region Variables
        //The random number generator used when selecting a ship's location
        Random randomNumberGenerator;
        //The gameboard that will be initialised randomly
        GameBoard boardToInitialise;
        #endregion

        //The constructor seeds the random number generator and takes the gameboard to initialise as an argument.
        public AIShipInitialiser(GameBoard boardThatNeedsInitialisation)
        {
            boardToInitialise = boardThatNeedsInitialisation;
            randomNumberGenerator = new Random(DateTime.Now.Millisecond);
        }

        //Returns a random location for a ship as a string
        private string GetAIShipVector()
        {
            int x = 65 + randomNumberGenerator.Next(0, 10);
            int y = 49 + randomNumberGenerator.Next(0, 10);
            int direction = randomNumberGenerator.Next(0, 4);
            switch (direction)
            {
                case 0:
                    direction = 78;
                    break;
                case 1:
                    direction = 69;
                    break;
                case 2:
                    direction = 83;
                    break;
                case 3:
                    direction = 87;
                    break;
            }

            string toReturn = "";
            toReturn = toReturn + (char)x;
            toReturn = toReturn + (char)y;
            toReturn = toReturn + (char)direction;
            return toReturn;
        }

        //The following region contains a modified version of the methods used to intialise a gameboard
        //They have been modified to remove any error messages that would arise due to invalid input.
        #region FillAIGameboard
        //Fills the board to initialise with randomly placed ships
        public void InitialiseGameBoard(string nameOfBoardUser)
        {
            Console.WriteLine(nameOfBoardUser + " will now place their ships on the grid.");
            AddShips();
            boardToInitialise.HideShips();
            Console.Clear();
        }

        //Add ships one by one to the board randomly
        private void AddShips()
        {
            int index = 0;
            int size = 0;
            char ship = 'x';
            int[] vectorInt = new int[3];
            while (index < 5)
            {
                switch (index)
                {
                    case 0:
                        size = 5;
                        ship = 'a';
                        break;

                    case 1:
                        size = 4;
                        ship = 'b';
                        break;

                    case 2:
                        size = 3;
                        ship = 's';
                        break;

                    case 3:
                        size = 3;
                        ship = 'd';
                        break;

                    case 4:
                        size = 2;
                        ship = 'p';
                        break;
                }
                vectorInt = GetShipVector(size);

                if (ValidateVectorInt(vectorInt, size))
                {
                    boardToInitialise.AddShip(vectorInt, size, ship);
                    index++;
                }
            }
        }

        //Returns a valid yet random coordinate for a ship of given size.
        private int[] GetShipVector(int size)
        {
            int[] toReturn = new int[3];
            char[] vector = new char[3];
            string temporary;
            do
            {
                temporary = GetAIShipVector();

                for (int i = 0; i < 3; i++)
                {
                    vector[i] = temporary[i];
                }

                if (ValidateVectorChar(vector))
                {
                    toReturn = boardToInitialise.CharVectToIntVect(vector);
                }
            }
            while (!ValidateVectorChar(vector));
            return toReturn;
        }

        //If the input char array is correctly formatted to place a ship return true
        private bool ValidateVectorChar(char[] toValidate)
        {
            if (toValidate[0] < 'A' || toValidate[0] > 'J')
            {
                return false;
            }
            if (toValidate[1] < '1' || toValidate[1] > ':')
            {
                return false;
            }
            if (toValidate[2] != 'N' && toValidate[2] != 'E' && toValidate[2] != 'S' && toValidate[2] != 'W')
            {
                return false;
            }

            return true;
        }

        //Return's true if the input coordinate is on the board and doesn't result in ships being placed on top of one another.
        private bool ValidateVectorInt(int[] toValidate, int size)
        {
            switch (toValidate[2])
            {
                case 78:
                    if (!CheckUp(toValidate, size))
                    {
                        return false;
                    }
                    break;
                case 69:
                    if (!CheckRight(toValidate, size))
                    {
                        return false;
                    }
                    break;
                case 83:
                    if (!CheckDown(toValidate, size))
                    {
                        return false;
                    }
                    break;
                case 87:
                    if (!CheckLeft(toValidate, size))
                    {
                        return false;
                    }
                    break;
            }
            return true;
        }

        #region ValidateVectorInt Direction Specific Methods
        //Each of the four following methods checks to make sure a ship being placed at a given coord, facing a given direction, does not end up placed on top of any other ships.
        //There is one method for each cardinal direction.
        private bool CheckUp(int[] toValidate, int size)
        {
            if (toValidate[1] - size < -1)
            {
                return false;
            }
            for (int i = 0; i < size; i++)
            {
                if (boardToInitialise.ShipsAndSea[toValidate[0], toValidate[1] - i] != 'x')
                {
                    return false;
                }
            }
            return true;
        }

        private bool CheckRight(int[] toValidate, int size)
        {
            if (toValidate[0] + size > 10)
            {
                return false;
            }
            for (int i = 0; i < size; i++)
            {
                if (boardToInitialise.ShipsAndSea[toValidate[0] + i, toValidate[1]] != 'x')
                {
                    return false;
                }
            }
            return true;
        }

        private bool CheckDown(int[] toValidate, int size)
        {
            if (toValidate[1] + size > 10)
            {
                return false;
            }
            for (int i = 0; i < size; i++)
            {
                if (boardToInitialise.ShipsAndSea[toValidate[0], toValidate[1] + i] != 'x')
                {
                    return false;
                }
            }
            return true;
        }

        private bool CheckLeft(int[] toValidate, int size)
        {
            if (toValidate[0] - size < -1)
            {
                return false;
            }
            for (int i = 0; i < size; i++)
            {
                if (boardToInitialise.ShipsAndSea[toValidate[0] - i, toValidate[1]] != 'x')
                {
                    return false;
                }
            }
            return true;
        }
        #endregion
        #endregion
    }
}
