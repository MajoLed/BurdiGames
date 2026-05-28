using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BurdiGames.Clases.Juegos
{
    internal class RPG : Juego
    {
        public int Nivel { get; set; }
        public int Progreso { get; set; } // 0-100%

        public RPG(string nombre, string descripcion, string rutaImagen)
            : base(nombre, "RPG", descripcion, rutaImagen)
        {
            Nivel = 1;
            Progreso = 0;
        }

        public override void Jugar()
        {
            Console.WriteLine($"Continuando RPG {Nombre}, nivel {Nivel}.");
        }
    }
}
