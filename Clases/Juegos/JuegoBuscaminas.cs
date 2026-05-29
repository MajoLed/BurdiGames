using MiniJuegos;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace MiniJuegos
{
    // ─── Enums ───────────────────────────────────────────────────────────────

    public enum EstadoCelda { Oculta, Revelada, Bandera }
    public enum EstadoBuscaminas { Jugando, Pausado, GameOver, Victoria }

    // ─── Celda individual ────────────────────────────────────────────────────

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
        public Celda[,] Celdas { get; private set; }
        public bool Generado { get; private set; } = false;

        private static readonly Random rng = new Random();

        public Tablero(int filas, int columnas, int minas)
        {
            Filas = filas;
            Columnas = columnas;
            TotalMinas = minas;
            Celdas = new Celda[filas, columnas];
            for (int f = 0; f < filas; f++)
                for (int c = 0; c < columnas; c++)
                    Celdas[f, c] = new Celda();
        }

        // Genera el tablero asegurando que el primer clic no sea mina
        public void GenerarMinas(int filaSegura, int colSegura)
        {
            var posiciones = (from f in Enumerable.Range(0, Filas)
                              from c in Enumerable.Range(0, Columnas)
                              where !(f == filaSegura && c == colSegura)
                              select (f, c))
                             .OrderBy(_ => rng.Next())
                             .Take(TotalMinas)
                             .ToList();

            posiciones.ForEach(p => Celdas[p.f, p.c].TieneMina = true);

            // Calcular vecinos con LINQ
            for (int f = 0; f < Filas; f++)
                for (int c = 0; c < Columnas; c++)
                    Celdas[f, c].MinasVecinas = ObtenerVecinos(f, c)
                        .Count(v => v.TieneMina);

            Generado = true;
        }

        public IEnumerable<Celda> ObtenerVecinos(int fila, int col)
        {
            return from df in Enumerable.Range(-1, 3)
                   from dc in Enumerable.Range(-1, 3)
                   where !(df == 0 && dc == 0)
                   let nf = fila + df
                   let nc = col + dc
                   where nf >= 0 && nf < Filas && nc >= 0 && nc < Columnas
                   select Celdas[nf, nc];
        }

        public IEnumerable<(int f, int c)> ObtenerPosVecinos(int fila, int col)
        {
            return from df in Enumerable.Range(-1, 3)
                   from dc in Enumerable.Range(-1, 3)
                   where !(df == 0 && dc == 0)
                   let nf = fila + df
                   let nc = col + dc
                   where nf >= 0 && nf < Filas && nc >= 0 && nc < Columnas
                   select (nf, nc);
        }

        public void Reiniciar()
        {
            Generado = false;
            for (int f = 0; f < Filas; f++)
                for (int c = 0; c < Columnas; c++)
                    Celdas[f, c] = new Celda();
        }

        // Celdas sin mina reveladas — para verificar victoria
        public int CeldasSinMinaReveladas =>
            (from f in Enumerable.Range(0, Filas)
             from c in Enumerable.Range(0, Columnas)
             where !Celdas[f, c].TieneMina && Celdas[f, c].EsRevelada
             select 1).Count();

        public int TotalCeldasSinMina => Filas * Columnas - TotalMinas;

        public int BanderasColocadas =>
            (from f in Enumerable.Range(0, Filas)
             from c in Enumerable.Range(0, Columnas)
             where Celdas[f, c].TieneBandera
             select 1).Count();
    }

    // ─── Motor del juego ─────────────────────────────────────────────────────

    public class MotorBuscaminas
    {
        public Tablero Tablero { get; private set; }
        public EstadoBuscaminas Estado { get; private set; } = EstadoBuscaminas.Jugando;
        public int Puntos { get; private set; } = 0;
        public DateTime? TiempoInicio { get; private set; }
        public int SegundosTranscurridos =>
            TiempoInicio.HasValue ? (int)(DateTime.Now - TiempoInicio.Value).TotalSeconds : 0;

        private const int FILAS = 16;
        private const int COLS = 16;
        private const int MINAS = 40;

        public event EventHandler? JuegoTerminado;
        public event EventHandler? VictoriaAlcanzada;

        public MotorBuscaminas()
        {
            Tablero = new Tablero(FILAS, COLS, MINAS);
        }

        public void Revelar(int fila, int col)
        {
            if (Estado != EstadoBuscaminas.Jugando) return;
            var celda = Tablero.Celdas[fila, col];
            if (celda.EsRevelada || celda.TieneBandera) return;

            if (!Tablero.Generado)
            {
                Tablero.GenerarMinas(fila, col);
                TiempoInicio = DateTime.Now;
            }

            if (celda.TieneMina)
            {
                celda.Estado = EstadoCelda.Revelada;
                RevelarTodasMinas();
                Estado = EstadoBuscaminas.GameOver;
                JuegoTerminado?.Invoke(this, EventArgs.Empty);
                return;
            }

            RevelarRecursivo(fila, col);
            Puntos = Tablero.CeldasSinMinaReveladas * 5;

            if (Tablero.CeldasSinMinaReveladas == Tablero.TotalCeldasSinMina)
            {
                Estado = EstadoBuscaminas.Victoria;
                VictoriaAlcanzada?.Invoke(this, EventArgs.Empty);
            }
        }

        private void RevelarRecursivo(int fila, int col)
        {
            var celda = Tablero.Celdas[fila, col];
            if (celda.EsRevelada || celda.TieneBandera || celda.TieneMina) return;

            celda.Estado = EstadoCelda.Revelada;

            if (celda.MinasVecinas == 0)
                Tablero.ObtenerPosVecinos(fila, col)
                       .ToList()
                       .ForEach(v => RevelarRecursivo(v.f, v.c));
        }

        private void RevelarTodasMinas()
        {
            for (int f = 0; f < Tablero.Filas; f++)
                for (int c = 0; c < Tablero.Columnas; c++)
                    if (Tablero.Celdas[f, c].TieneMina)
                        Tablero.Celdas[f, c].Estado = EstadoCelda.Revelada;
        }

        public void ToggleBandera(int fila, int col)
        {
            if (Estado != EstadoBuscaminas.Jugando) return;
            var celda = Tablero.Celdas[fila, col];
            if (celda.EsRevelada) return;

            celda.Estado = celda.TieneBandera ? EstadoCelda.Oculta : EstadoCelda.Bandera;
        }

        public void TogglePausa()
        {
            Estado = Estado == EstadoBuscaminas.Pausado
                ? EstadoBuscaminas.Jugando
                : EstadoBuscaminas.Pausado;
        }

        public void Reiniciar()
        {
            Tablero.Reiniciar();
            Estado = EstadoBuscaminas.Jugando;
            Puntos = 0;
            TiempoInicio = null;
        }

        public int MinasRestantes => Tablero.TotalMinas - Tablero.BanderasColocadas;
    }
}
// Al final de JuegoBuscaminas.cs, fuera del namespace MiniJuegos:
namespace BurdiGames.Clases.Juegos
{
    internal class JuegoBuscaminasArcade : Arcade
    {
        public JuegoBuscaminasArcade()
            : base(
                nombre: "Blossom Mines",
                descripcion: "Buscaminas con estética girlie. Clic izquierdo para revelar, derecho para bandera.",
                rutaImagen: "Sources/Imagenes/defaultuser.png",
                vidas: 1)
        {
        }

        public override void Jugar()
        {
            var form = new FormBuscaminas();
            form.ShowDialog();
        }
    }
}