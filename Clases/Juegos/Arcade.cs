using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BurdiGames.Clases.Juegos
{
    internal class Arcade : Juego
    {
        public int Vidas { get; set; }
        public int HighScore { get; set; }

        public Arcade(string nombre, string descripcion, string rutaImagen, int vidas = 3)
            : base(nombre, "Arcade", descripcion, rutaImagen)
        {
            Vidas = vidas;
            HighScore = 0;
        }

        public override void Jugar()
        {
            Console.WriteLine($"Iniciando arcade {Nombre} con {Vidas} vidas.");
        }
    }
}
