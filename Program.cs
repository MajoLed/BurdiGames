using BurdiGames.Clases;
using BurdiGames.Clases.Juegos;
using BurdiGames.Core;

namespace BurdiGames
{
    internal static class Program
    {

        public static PlataformaJuegos BurdiGames = new PlataformaJuegos();

        [STAThread]
        static void Main()
        {
            //Datos de prueba
            BurdiGames.RegistrarUsuario("PlayerOne", "1234");
            BurdiGames.RegistrarUsuario("Majo", "abcd");

            BurdiGames.CatalogoJuegos.Add(new Arcade("Galaxian", "Juego de naves espaciales","Sources/Imagenes/logo_galaxian.png"));
            BurdiGames.CatalogoJuegos.Add(new RPG("Bomberman", "Aventura RPG pixel art", "Sources/Imagenes/logo_bomberman.png") );
            BurdiGames.CatalogoJuegos.Add(new Arcade("Puzzle Block", "Bloques y lógica", "Sources/Imagenes/defaultuser.png"));
            /*BurdiGames.CatalogoJuegos.Add(new JuegoTamagotchi());*/
            BurdiGames.CatalogoJuegos.Add(new Tamagotchi("Tamagotchi", "Cuida tu mascota virtual", "Sources/Imagenes/Tamagotchi/logo.png"));
            BurdiGames.CatalogoJuegos.Add(new JuegoPacman());


            var playerOne = BurdiGames.AutenticarUsuario("PlayerOne", "1234");

            if (playerOne != null)
            {

                BurdiGames.CatalogoJuegos.Add(BurdiGames.CatalogoJuegos[0]);

                // Partida de prueba en el historial
                playerOne.HistorialPartidas.Add(new Partida(playerOne, BurdiGames.CatalogoJuegos[0])
                {
                    Puntaje = 12000,
                    Fecha = DateTime.Now.AddHours(-2)
                });
            }


            //Iniciar aplicacion
            ApplicationConfiguration.Initialize();
            Application.Run(new FormLogin());
        }
    }
}