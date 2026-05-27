using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BurdiGames.Clases
{
    internal class Partida
    {
        public Usuario Usuario { get; set; }
        public Juego Juego   {get; set; }
        public int? Puntaje { get; set; }
        public DateTime? Fecha { get; set; }
    }

}
