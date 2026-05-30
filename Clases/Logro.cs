using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BurdiGames.Clases
{
    internal class Logro
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }

        public string Icono { get; set; } //emoji
        public bool Desbloqueado { get; set; } = false;

        public void Desbloquear() => Desbloqueado = true;

    }
}
