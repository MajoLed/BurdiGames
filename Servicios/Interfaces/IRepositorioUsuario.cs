using BurdiGames.Clases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BurdiGames.Servicios.Interfaces
{
    internal interface IRepositorioUsuario
    {
        void Agregar(Usuario usuario);

        Usuario BuscarNombre(string nombre);
    }
}
