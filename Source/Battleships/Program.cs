using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Battleships
{
    class Program
    {
        static void Main(string[] args)
        {
            //setup the game (make the console bigger and recieve the player names)
            Console.SetWindowSize(80, 55);

            string input;
            
            Console.WriteLine("Player 1 will go first.");

            //Initialise Player1
            Console.WriteLine("What is player 1's name?");
            input = Console.ReadLine().ToUpper();
            Player player1;
            if (input == "")
            {
                input = "CPU";
            }
            player1 = new Player(input);            
         
            //Initialise Player2
            Console.WriteLine("What is player 2's name? (Leave blank to play against the computer)");
            input = Console.ReadLine().ToUpper();
            Player player2;
            if (input == "")
            {
                input = "CPU";         
            }
            player2 = new Player(input);

            Console.Clear();

            //Initialise a game of battleship between the two players and then begin the game.
            Battleship game = new Battleship(player1, player2);
            game.Start();
        }
    }
}
