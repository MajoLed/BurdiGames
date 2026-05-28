using BurdiGames.Clases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace BurdiGames.Core
{
    public class PlataformaJuegos
    {

        public List<Usuario> Usuarios = new List<Usuario>();

        public List<Juego> CatalogoJuegos = new List<Juego>(); //Juegos Disponibles

        //Métodos
        public Usuario AutenticarUsuario(string nickname, string contra)
        {
            return Usuarios.FirstOrDefault(u => u.Nombre == nickname && u.Contrasenia == contra);
        }

        public bool RegistrarUsuario(string nickname, string contra)
        {
            //Revisar Si ya hay usuarios registrados con ese nombre
            if (Usuarios.Any(u => u.Nombre == nickname)) 
                return false;

            Usuarios.Add(new Usuario(nickname, contra));
            return true;
        }

        public void MostrarUsuariosRegistrados()
        {
            foreach (var usuario in Usuarios)
            {
                Console.WriteLine();
            }
        }
    }

}
