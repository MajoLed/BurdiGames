using BurdiGames.Servicios.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BurdiGames.Core;
using System.Threading.Tasks;

namespace BurdiGames.Clases
{
    public class Partida : IGuardable
    {
        #region
            public Usuario Usuario { get; set; }
            public Juego Juego { get; set; }
            public int? Puntaje { get; set; }
            public DateTime? Fecha { get; set; }

        #endregion
        public Partida(Usuario usuario, Juego juego)
        {
            Usuario = usuario;
            Juego = juego;
            Fecha = DateTime.Now;
        }
        public void Guardar() =>
            Console.WriteLine($"Partida de {Usuario.Nombre} en {Juego.Nombre} guardada.");

        public void Cargar() =>
            Console.WriteLine($"Partida de {Usuario.Nombre} cargada.");

    }

}
    

