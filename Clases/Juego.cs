using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BurdiGames.Clases
{
    internal abstract class Juego
    {
        private int _id;
        public string Nombre { get; set; }
        public string Genero { get; set; }
        public string Descripcion { get; set; }
        public string Imagen { get; set; } //url

        //Método: Mostrar info (Progreso, puntaje, ult partida)

        public Juego(string nombre, string genero, string descripcion, string rutaImagen) 
        {
            Nombre = nombre;
            Genero = genero;
            Descripcion = descripcion;
            Imagen = rutaImagen;
        }
    }
}
