using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BurdiGames.Clases
{
    internal class Usuario
    {
        #region propiedades

        private static int cantUsuarios;

        private string _contrasena;
        public string Nombre { get; set; }
        public string Contrasenia { get { return _contrasena; } } //Solo lectura (Seguridad)
        public string Avatar { get; set; } //url
        public bool SesionActiva { get; set; }
        public DateTime? FechaRegistro { get; set; }

        public List<Partida> HistorialPartidas { get; set; } //Lista de partidas

        #endregion

        //Constructor 

        public Usuario(string nombre, string contrasenia, int id = 1)
        {
            id++;
            _contrasena = contrasenia;
            Nombre = nombre;
            SesionActiva = true;
            FechaRegistro = DateTime.Now;
            Avatar = "../Sources/Imagenes/defaultuser.png";
        }

        #region Metodos
        public bool ValidarContrasenia(string contrasenia)
        {
            bool validacion;

            if (this.Contrasenia == contrasenia)
                validacion = true;

            return validacion = false;
        }

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
