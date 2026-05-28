using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BurdiGames.Clases.Juegos
{
    internal class Tetris : Juego
    {
        public int Nivel { get; set; }
        public int Vidas { get; set; }

        public Tetris(int vidas = 3) : base("Tetris", "Arcade", "El clásico tetris", "")
        {
            Vidas = vidas;
        }

        public override void Jugar()
        {
            throw new NotImplementedException();
        }
    }
}
