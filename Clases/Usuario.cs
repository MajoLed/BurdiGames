using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BurdiGames.Clases
{
    public class Usuario
    {
        private string _contrasena;

        private static int _cantidadUsuarios = 0;
        public string Id { get; private set; }
        public string Nombre { get; set; }
        public string Contrasenia // Solo lectura para seguridad
        {
            get { return _contrasena; }
        } 
        public string Avatar { get; set; } //url
        public bool SesionActiva { get; set; }
        public DateTime FechaRegistro { get; set; }

        public List<Partida> HistorialPartidas { get; set; }

        public Usuario(string nombre, string contrasenia)
        {
            _cantidadUsuarios++;
            Id = _cantidadUsuarios.ToString("D3"); // 001, 002, 200, etc.

            _contrasena = contrasenia;

            Nombre = nombre;

            SesionActiva = true;

            FechaRegistro = DateTime.Now;

            Avatar = "Sources/Imagenes/defaultuser.png";

            HistorialPartidas = new List<Partida>();
        }

        #region Metodos

        public void CambiarNombre(string nombreNuevo)
        {
            Nombre = nombreNuevo;
        }

        public void CambiarAvatar(string urlNueva)
        {
            Avatar = urlNueva;
        }

        // Representación legible del objeto (útil para depurar)
        public override string ToString()
        {
            return $"{Nombre} — Registrado: {FechaRegistro:dd/MM/yyyy}";
        }

        #endregion

    }
}
