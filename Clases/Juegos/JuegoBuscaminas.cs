using MiniJuegos;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace MiniJuegos
{   
    public enum EstadoCelda { Oculta, Revelada, Bandera }
   
    public enum EstadoBuscaminas { Jugando, Pausado, GameOver, Victoria }

    public class Celda
    {
        public bool TieneMina { get; set; } = false;      
        public EstadoCelda Estado { get; set; } = EstadoCelda.Oculta; 
        public int MinasVecinas { get; set; } = 0;            

        public bool EsRevelada => Estado == EstadoCelda.Revelada; 
        public bool TieneBandera => Estado == EstadoCelda.Bandera; 
    }

    // ─── Tablero ─────────────────────────────────────────────────────────────
    public class Tablero
    {
        public int Filas { get; }          
        public int Columnas { get; }       
        public int TotalMinas { get; }     
        public Celda[,] Celdas { get; private set; } // Matriz bidimensional que contiene todas las celdas del tablero
        public bool Generado { get; private set; } = false; // Indica si las minas ya fueron colocadas en el tablero; por defecto false

        private static readonly Random rng = new Random(); // Instancia estática de Random para generar posiciones aleatorias de minas

        // Constructor
        public Tablero(int filas, int columnas, int minas)
        {
            Filas = filas;           
            Columnas = columnas;     
            TotalMinas = minas;      
            Celdas = new Celda[filas, columnas]; // Crea la matriz de celdas 
            for (int f = 0; f < filas; f++)      
                for (int c = 0; c < columnas; c++) 
                    Celdas[f, c] = new Celda(); // Crea e inicializa una nueva celda en la posición [f, c]
        }

        public void GenerarMinas(int filaSegura, int colSegura)
        {
            var posiciones = (from f in Enumerable.Range(0, Filas)    // Genera índices de todas las filas 
                              from c in Enumerable.Range(0, Columnas)  // Genera índices de todas las columnas 
                              where !(f == filaSegura && c == colSegura) // Excluye la celda seleccionada
                              select (f, c))                            // Proyecta cada combinación válida 
                             .OrderBy(_ => rng.Next())                  // Mezcla aleatoriamente todas las posiciones
                             .Take(TotalMinas)                          // Toma solo la cantidad de posiciones igual al total de minas
                             .ToList();                                 

            posiciones.ForEach(p => Celdas[p.f, p.c].TieneMina = true); // Marca cada posición seleccionada como mina en la matriz

            // Calcula cuántas minas vecinas tiene cada celda usando LINQ
            for (int f = 0; f < Filas; f++)          
                for (int c = 0; c < Columnas; c++)   
                    Celdas[f, c].MinasVecinas = ObtenerVecinos(f, c) // Obtiene las celdas vecinas 
                        .Count(v => v.TieneMina);    // Cuenta cuántas de esas vecinas tienen mina y lo asigna

            Generado = true; 
        }

        // Devuelve las celdas vecinas de una posición dada
        public IEnumerable<Celda> ObtenerVecinos(int fila, int col)
        {
            return from df in Enumerable.Range(-1, 3)              // Genera desplazamientos de fila
                   from dc in Enumerable.Range(-1, 3)              // Genera desplazamientos de columna
                   where !(df == 0 && dc == 0)                     // Excluye el desplazamiento (0,0) que es la celda misma
                   let nf = fila + df                              // Calcula la fila del vecino
                   let nc = col + dc                               // Calcula la columna del vecino
                   where nf >= 0 && nf < Filas && nc >= 0 && nc < Columnas // Verifica que el vecino esté dentro de los límites del tablero
                   select Celdas[nf, nc];                          // Retorna la celda vecina válida
        }

        // Devuelve las coordenadas de los vecinos de una posición dada
        public IEnumerable<(int f, int c)> ObtenerPosVecinos(int fila, int col)
        {
            return from df in Enumerable.Range(-1, 3)              // Genera desplazamientos de fila
                   from dc in Enumerable.Range(-1, 3)              // Genera desplazamientos de columna
                   where !(df == 0 && dc == 0)                     // Excluye el desplazamiento (0,0) que es la celda misma
                   let nf = fila + df                              // Calcula la fila del vecino
                   let nc = col + dc                               // Calcula la columna del vecino
                   where nf >= 0 && nf < Filas && nc >= 0 && nc < Columnas // Verifica que el vecino esté dentro de los límites del tablero
                   select (nf, nc);                                // Retorna las coordenadas del vecino válido como tupla
        }

        // Reinicia el tablero borrando todas las celdas y el estado de generación
        public void Reiniciar()
        {
            Generado = false;                        // Marca el tablero como no generado
            for (int f = 0; f < Filas; f++)          // Recorre cada fila del tablero
                for (int c = 0; c < Columnas; c++)   // Recorre cada columna de la fila actual
                    Celdas[f, c] = new Celda();      // Reemplaza la celda existente por una nueva celda limpia
        }

        // Propiedad calculada: cuenta las celdas sin mina que ya fueron reveladas (usada para verificar victoria)
        public int CeldasSinMinaReveladas =>
            (from f in Enumerable.Range(0, Filas)    // Recorre índices de filas
             from c in Enumerable.Range(0, Columnas) // Recorre índices de columnas
             where !Celdas[f, c].TieneMina && Celdas[f, c].EsRevelada // Filtra celdas sin mina y que estén reveladas
             select 1).Count();                      // Cuenta cuántas celdas cumplen la condición

        public int TotalCeldasSinMina => Filas * Columnas - TotalMinas; // Total de celdas que no tienen mina (meta de victoria)

        // Propiedad calculada: cuenta cuántas banderas hay colocadas en el tablero
        public int BanderasColocadas =>
            (from f in Enumerable.Range(0, Filas)    // Recorre índices de filas
             from c in Enumerable.Range(0, Columnas) // Recorre índices de columnas
             where Celdas[f, c].TieneBandera         // Filtra solo las celdas que tienen bandera
             select 1).Count();                      // Cuenta cuántas celdas tienen bandera
    }

    // ─── Motor del juego ─────────────────────────────────────────────────────
    public class MotorBuscaminas
    {
        public Tablero Tablero { get; private set; }                  
        public EstadoBuscaminas Estado { get; private set; } = EstadoBuscaminas.Jugando; 
        public int Puntos { get; private set; } = 0;                  
        public DateTime? TiempoInicio { get; private set; }            
        public int SegundosTranscurridos =>                             
            TiempoInicio.HasValue ? (int)(DateTime.Now - TiempoInicio.Value).TotalSeconds : 0; // Si hay tiempo de inicio, calcula la diferencia; si no, retorna 0

        private const int FILAS = 16;  
        private const int COLS = 16;   
        private const int MINAS = 40;  

        public event EventHandler? JuegoTerminado;   // Evento que se dispara cuando el jugador pierde (Game Over)
        public event EventHandler? VictoriaAlcanzada; // Evento que se dispara cuando el jugador gana


        public MotorBuscaminas()
        {
            Tablero = new Tablero(FILAS, COLS, MINAS); 
        }

        // Método para revelar una celda en la posición 
        public void Revelar(int fila, int col)
        {
            if (Estado != EstadoBuscaminas.Jugando) return; // Si el juego no está activo, no hace nada
            var celda = Tablero.Celdas[fila, col];          // Obtiene la celda en la posición indicada
            if (celda.EsRevelada || celda.TieneBandera) return; // Si ya está revelada o tiene bandera, no hace nada

            if (!Tablero.Generado)                          // Si las minas aún no han sido colocadas (primer clic)
            {
                Tablero.GenerarMinas(fila, col);            // Genera las minas evitando la celda del primer clic
                TiempoInicio = DateTime.Now;                // Registra el momento de inicio del juego
            }

            if (celda.TieneMina)                            // Si la celda revelada contiene una mina
            {
                celda.Estado = EstadoCelda.Revelada;        // Marca la celda con mina como revelada
                RevelarTodasMinas();                        // Muestra todas las minas del tablero
                Estado = EstadoBuscaminas.GameOver;         // Cambia el estado del juego a Game Over
                JuegoTerminado?.Invoke(this, EventArgs.Empty); // Dispara el evento de juego terminado
                return;                                     // Sale del método sin continuar
            }

            RevelarRecursivo(fila, col);                    // Revela la celda y sus vecinas recursivamente si corresponde
            Puntos = Tablero.CeldasSinMinaReveladas * 5;   // Actualiza los puntos: 5 puntos por cada celda sin mina revelada

            if (Tablero.CeldasSinMinaReveladas == Tablero.TotalCeldasSinMina) // Si todas las celdas sin mina fueron reveladas
            {
                Estado = EstadoBuscaminas.Victoria;                        // Cambia el estado a Victoria
                VictoriaAlcanzada?.Invoke(this, EventArgs.Empty);          // Dispara el evento de victoria
            }
        }

        // Método privado que revela una celda y se expande recursivamente si no tiene minas vecinas
        private void RevelarRecursivo(int fila, int col)
        {
            var celda = Tablero.Celdas[fila, col];                         // Obtiene la celda en la posición dada
            if (celda.EsRevelada || celda.TieneBandera || celda.TieneMina) return; // Si ya está revelada, tiene bandera o mina, detiene la recursión

            celda.Estado = EstadoCelda.Revelada;                           // Marca la celda como revelada

            if (celda.MinasVecinas == 0)                                   // Si la celda no tiene minas vecinas
                Tablero.ObtenerPosVecinos(fila, col)                       // Obtiene las coordenadas de sus vecinos
                       .ToList()                                            // Convierte a lista para poder iterar
                       .ForEach(v => RevelarRecursivo(v.f, v.c));          // Llama recursivamente para revelar cada vecino
        }

        // Método privado que revela todas las celdas con mina (se usa al perder)
        private void RevelarTodasMinas()
        {
            for (int f = 0; f < Tablero.Filas; f++)         
                for (int c = 0; c < Tablero.Columnas; c++)  
                    if (Tablero.Celdas[f, c].TieneMina)     // Si la celda actual tiene mina
                        Tablero.Celdas[f, c].Estado = EstadoCelda.Revelada; // La marca como revelada para mostrarla
        }

        // Método para colocar o quitar una bandera en la celda indicada
        public void ToggleBandera(int fila, int col)
        {
            if (Estado != EstadoBuscaminas.Jugando) return; // Si el juego no está activo, no hace nada
            var celda = Tablero.Celdas[fila, col];          // Obtiene la celda en la posición indicada
            if (celda.EsRevelada) return;                   // Si la celda ya está revelada, no se puede poner bandera

            celda.Estado = celda.TieneBandera ? EstadoCelda.Oculta : EstadoCelda.Bandera; // Alterna el estado: si tiene bandera la quita, si no la pone
        }

        // Método para pausar o reanudar el juego
        public void TogglePausa()
        {
            Estado = Estado == EstadoBuscaminas.Pausado     // Si el juego está pausado
                ? EstadoBuscaminas.Jugando                  // Lo cambia a Jugando (reanuda)
                : EstadoBuscaminas.Pausado;                 // Si no está pausado, lo pausa
        }

        // Método para reiniciar completamente el juego
        public void Reiniciar()
        {
            Tablero.Reiniciar();                            // Reinicia el tablero (borra celdas y estado)
            Estado = EstadoBuscaminas.Jugando;              // Restablece el estado del juego a Jugando
            Puntos = 0;                                     // Reinicia los puntos a cero
            TiempoInicio = null;                            // Borra el tiempo de inicio
        }

        public int MinasRestantes => Tablero.TotalMinas - Tablero.BanderasColocadas; // Minas restantes = total de minas menos banderas colocadas
    }
}

// Al final de JuegoBuscaminas.cs, fuera del namespace MiniJuegos:
namespace BurdiGames.Clases.Juegos
{
    // Clase que representa el juego Buscaminas como un Arcade dentro del sistema BurdiGames
    internal class JuegoBuscaminasArcade : Arcade
    {
        // Constructor: configura el juego con nombre, descripción, imagen y número de vidas
        public JuegoBuscaminasArcade()
            : base(                                                         // Llama al constructor de la clase base Arcade
                nombre: "Buscaminas",                                       // Nombre del juego
                descripcion: "Buscaminas. Clic izquierdo para revelar, derecho para bandera.", // Descripción visible al usuario
                rutaImagen: "Sources/Imagenes/defaultuser.png",             // Ruta de la imagen representativa del juego
                vidas: 1)                                                    // El juego otorga 1 vida al jugador
        {
        }

        // Método que lanza el formulario del juego cuando el usuario decide jugar
        public override void Jugar()
        {
            var form = new FormBuscaminas(); // Crea una instancia del formulario del Buscaminas
            form.ShowDialog();               // Muestra el formulario como diálogo modal (bloquea la ventana padre hasta cerrarlo)
        }
    }
}