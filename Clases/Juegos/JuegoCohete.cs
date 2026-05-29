using MiniJuegos;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace MiniJuegos
{
    // ─── Jerarquía de entidades ───────────────────────────────────────────────

    public abstract class EntidadJuego
    {
        public float X { get; set; }
        public float Y { get; set; }
        public int Ancho { get; protected set; }
        public int Alto { get; protected set; }
        public bool Activo { get; set; } = true;

        public RectangleF Hitbox => new RectangleF(X, Y, Ancho, Alto);

        public abstract void Actualizar(float delta);
        public abstract void Dibujar(System.Windows.Forms.PaintEventArgs e);

        public bool ColisionaCon(EntidadJuego otra)
        {
            return Activo && otra.Activo && Hitbox.IntersectsWith(otra.Hitbox);
        }
    }

    public class Cohete : EntidadJuego
    {
        public int Vidas { get; private set; } = 3;
        public bool Invulnerable { get; private set; } = false;
        private float tiempoInvulnerable = 0f;
        private const float DURACION_INVULNERABILIDAD = 2f;
        private float frameParpadeo = 0f;
        public bool Visible { get; private set; } = true;

        public Cohete(float x, float y)
        {
            X = x; Y = y;
            Ancho = 40; Alto = 60;
        }

        public void RecibirDanio()
        {
            if (Invulnerable) return;
            Vidas--;
            Invulnerable = true;
            tiempoInvulnerable = 0f;
            if (Vidas <= 0) Activo = false;
        }

        public override void Actualizar(float delta)
        {
            if (Invulnerable)
            {
                tiempoInvulnerable += delta;
                frameParpadeo += delta;
                Visible = (frameParpadeo % 0.2f) < 0.1f;
                if (tiempoInvulnerable >= DURACION_INVULNERABILIDAD)
                {
                    Invulnerable = false;
                    Visible = true;
                }
            }
        }

        public override void Dibujar(System.Windows.Forms.PaintEventArgs e)
        {
            if (!Visible) return;
            var g = e.Graphics;

            // Llama del cohete
            using var pincelLlama = new System.Drawing.Drawing2D.LinearGradientBrush(
                new PointF(X + 10, Y + Alto),
                new PointF(X + 30, Y + Alto + 20),
                Color.FromArgb(255, 100, 0),
                Color.FromArgb(255, 220, 0));
            g.FillEllipse(pincelLlama, X + 12, Y + Alto - 5, 16, 20);

            // Cuerpo del cohete
            using var pincelCuerpo = new System.Drawing.Drawing2D.LinearGradientBrush(
                new PointF(X, Y),
                new PointF(X + Ancho, Y),
                Color.FromArgb(180, 0, 255),
                Color.FromArgb(255, 0, 200));
            var puntosRocket = new PointF[]
            {
                new PointF(X + Ancho / 2, Y),
                new PointF(X + Ancho, Y + Alto * 0.6f),
                new PointF(X + Ancho, Y + Alto),
                new PointF(X, Y + Alto),
                new PointF(X, Y + Alto * 0.6f)
            };
            g.FillPolygon(pincelCuerpo, puntosRocket);

            // Ventana neón
            using var pincelVentana = new SolidBrush(Color.FromArgb(0, 255, 255));
            g.FillEllipse(pincelVentana, X + 13, Y + 18, 14, 14);
            using var bordeVentana = new Pen(Color.White, 1.5f);
            g.DrawEllipse(bordeVentana, X + 13, Y + 18, 14, 14);

            // Alas
            using var pincelAlas = new SolidBrush(Color.FromArgb(200, 0, 180));
            var alaIzq = new PointF[] {
                new PointF(X, Y + Alto * 0.6f),
                new PointF(X - 12, Y + Alto),
                new PointF(X, Y + Alto)
            };
            var alaDer = new PointF[] {
                new PointF(X + Ancho, Y + Alto * 0.6f),
                new PointF(X + Ancho + 12, Y + Alto),
                new PointF(X + Ancho, Y + Alto)
            };
            g.FillPolygon(pincelAlas, alaIzq);
            g.FillPolygon(pincelAlas, alaDer);
        }
    }

    public class Meteorito : EntidadJuego
    {
        private float velocidad;
        private float rotacion = 0f;
        private static readonly Random rng = new Random();
        private int lados;

        public Meteorito(float x, float y, float vel)
        {
            X = x; Y = y;
            velocidad = vel;
            Ancho = rng.Next(20, 45);
            Alto = Ancho;
            lados = rng.Next(5, 9);
        }

        public override void Actualizar(float delta)
        {
            Y += velocidad * delta;
            rotacion += 60f * delta;
            if (Y > 700) Activo = false;
        }

        public override void Dibujar(System.Windows.Forms.PaintEventArgs e)
        {
            var g = e.Graphics;
            var estado = g.Save();
            g.TranslateTransform(X + Ancho / 2, Y + Alto / 2);
            g.RotateTransform(rotacion);

            var puntos = new PointF[lados];
            for (int i = 0; i < lados; i++)
            {
                double angulo = (Math.PI * 2 / lados) * i;
                float r = Ancho / 2 * (0.7f + (float)(new Random(i * 7 + lados).NextDouble() * 0.3f));
                puntos[i] = new PointF((float)Math.Cos(angulo) * r, (float)Math.Sin(angulo) * r);
            }

            using var pincel = new SolidBrush(Color.FromArgb(255, 105, 180));
            using var borde = new Pen(Color.FromArgb(255, 20, 147), 2f);
            g.FillPolygon(pincel, puntos);
            g.DrawPolygon(borde, puntos);

            // Brillo
            using var brillo = new SolidBrush(Color.FromArgb(80, 255, 255, 255));
            g.FillEllipse(brillo, -Ancho / 6, -Alto / 6, Ancho / 4, Alto / 4);

            g.Restore(estado);
        }
    }

    public class Alien : EntidadJuego
    {
        private float velocidadX;
        private float tiempoDisparo = 0f;
        private float intervaloDisparo;
        public event EventHandler<PointF>? DisparoCorazon;
        private static readonly Random rng = new Random();
        private float animFrame = 0f;

        public Alien(float x, float y)
        {
            X = x; Y = y;
            Ancho = 50; Alto = 50;
            velocidadX = rng.Next(0, 2) == 0 ? 80f : -80f;
            intervaloDisparo = rng.Next(2, 5);
        }

        public override void Actualizar(float delta)
        {
            X += velocidadX * delta;
            animFrame += delta;
            if (X < 0 || X > 750) velocidadX *= -1;

            tiempoDisparo += delta;
            if (tiempoDisparo >= intervaloDisparo)
            {
                tiempoDisparo = 0f;
                DisparoCorazon?.Invoke(this, new PointF(X + Ancho / 2, Y + Alto));
            }
        }

        public override void Dibujar(System.Windows.Forms.PaintEventArgs e)
        {
            var g = e.Graphics;
            float bounce = (float)Math.Sin(animFrame * 3) * 3f;

            // Cuerpo verde alienígena
            using var pincelCuerpo = new SolidBrush(Color.FromArgb(50, 205, 50));
            g.FillEllipse(pincelCuerpo, X + 5, Y + 20 + bounce, 40, 28);

            // Cabeza
            using var pincelCabeza = new SolidBrush(Color.FromArgb(80, 220, 80));
            g.FillEllipse(pincelCabeza, X + 8, Y + bounce, 34, 30);

            // Peluca rosa
            using var pincelPeluca = new SolidBrush(Color.FromArgb(255, 105, 180));
            g.FillEllipse(pincelPeluca, X, Y - 8 + bounce, 50, 20);
            g.FillRectangle(pincelPeluca, X, Y - 2 + bounce, 6, 18);
            g.FillRectangle(pincelPeluca, X + 44, Y - 2 + bounce, 6, 18);

            // Lazo en la peluca
            using var pincelLazo = new SolidBrush(Color.FromArgb(255, 20, 147));
            var puntosLazo = new PointF[] {
                new PointF(X + 20, Y - 10 + bounce),
                new PointF(X + 25, Y - 5 + bounce),
                new PointF(X + 30, Y - 10 + bounce),
                new PointF(X + 25, Y - 15 + bounce)
            };
            g.FillPolygon(pincelLazo, puntosLazo);

            // Ojos grandes con pestañas
            using var pincelOjo = new SolidBrush(Color.White);
            g.FillEllipse(pincelOjo, X + 11, Y + 7 + bounce, 12, 12);
            g.FillEllipse(pincelOjo, X + 27, Y + 7 + bounce, 12, 12);
            using var pincelPupila = new SolidBrush(Color.FromArgb(180, 0, 180));
            g.FillEllipse(pincelPupila, X + 15, Y + 10 + bounce, 6, 6);
            g.FillEllipse(pincelPupila, X + 31, Y + 10 + bounce, 6, 6);

            // Pestañas
            using var pincelPest = new Pen(Color.Black, 1.5f);
            for (int i = 0; i < 3; i++)
            {
                g.DrawLine(pincelPest, X + 13 + i * 2, Y + 6 + bounce, X + 11 + i * 2, Y + 2 + bounce);
                g.DrawLine(pincelPest, X + 29 + i * 2, Y + 6 + bounce, X + 27 + i * 2, Y + 2 + bounce);
            }

            // Boca con labial
            using var pincelLabio = new SolidBrush(Color.FromArgb(220, 20, 60));
            g.FillEllipse(pincelLabio, X + 16, Y + 22 + bounce, 18, 8);
            using var pincelLabioSup = new SolidBrush(Color.FromArgb(180, 0, 60));
            g.FillEllipse(pincelLabioSup, X + 16, Y + 20 + bounce, 18, 5);

            // Antenas
            using var pincelAntena = new Pen(Color.FromArgb(50, 205, 50), 2f);
            g.DrawLine(pincelAntena, X + 17, Y + bounce, X + 12, Y - 12 + bounce);
            g.DrawLine(pincelAntena, X + 33, Y + bounce, X + 38, Y - 12 + bounce);
            using var pincelBolita = new SolidBrush(Color.FromArgb(255, 105, 180));
            g.FillEllipse(pincelBolita, X + 8, Y - 16 + bounce, 8, 8);
            g.FillEllipse(pincelBolita, X + 34, Y - 16 + bounce, 8, 8);

            // Brazos con bolso
            using var pincelBrazo = new Pen(Color.FromArgb(50, 205, 50), 3f);
            g.DrawLine(pincelBrazo, X + 5, Y + 30 + bounce, X - 8, Y + 38 + bounce);
            g.DrawLine(pincelBrazo, X + 45, Y + 30 + bounce, X + 55, Y + 38 + bounce);
            using var pincelBolso = new SolidBrush(Color.FromArgb(255, 20, 147));
            g.FillRectangle(pincelBolso, X + 53, Y + 34 + bounce, 10, 8);
        }
    }

    public class Corazon : EntidadJuego
    {
        private float velocidad = 180f;

        public Corazon(float x, float y)
        {
            X = x; Y = y;
            Ancho = 18; Alto = 16;
        }

        public override void Actualizar(float delta)
        {
            Y += velocidad * delta;
            if (Y > 700) Activo = false;
        }

        public override void Dibujar(System.Windows.Forms.PaintEventArgs e)
        {
            var g = e.Graphics;
            // Corazón verde tóxico
            using var pincel = new SolidBrush(Color.FromArgb(0, 255, 80));
            using var brillo = new Pen(Color.FromArgb(150, 200, 255, 100), 1.5f);

            // Forma de corazón simplificada con dos círculos + triángulo
            g.FillEllipse(pincel, X, Y, 10, 10);
            g.FillEllipse(pincel, X + 8, Y, 10, 10);
            var puntosCorazon = new PointF[] {
                new PointF(X, Y + 6),
                new PointF(X + 9, Y + 16),
                new PointF(X + 18, Y + 6)
            };
            g.FillPolygon(pincel, puntosCorazon);

            // Brillo neón
            g.DrawEllipse(brillo, X, Y, 10, 10);
            g.DrawEllipse(brillo, X + 8, Y, 10, 10);
        }
    }

    public class Bala : EntidadJuego
    {
        private float velocidad = 400f;

        public Bala(float x, float y)
        {
            X = x; Y = y;
            Ancho = 6; Alto = 14;
        }

        public override void Actualizar(float delta)
        {
            Y -= velocidad * delta;
            if (Y < -20) Activo = false;
        }

        public override void Dibujar(System.Windows.Forms.PaintEventArgs e)
        {
            var g = e.Graphics;
            using var pincelBala = new System.Drawing.Drawing2D.LinearGradientBrush(
                new PointF(X, Y), new PointF(X, Y + Alto),
                Color.FromArgb(0, 255, 255),
                Color.FromArgb(180, 0, 255));
            g.FillRectangle(pincelBala, X, Y, Ancho, Alto);

            using var halo = new Pen(Color.FromArgb(80, 0, 255, 255), 2f);
            g.DrawRectangle(halo, X - 1, Y - 1, Ancho + 2, Alto + 2);
        }
    }

    // ─── Puntuación y lógica con LINQ ────────────────────────────────────────

    public class RegistroPuntuacion
    {
        private List<int> historial = new List<int>();

        public void Agregar(int puntos) => historial.Add(puntos);

        public int MejorPuntaje() => historial.Any() ? historial.Max() : 0;

        public double PromedioPuntajes() => historial.Any() ? historial.Average() : 0;

        public IEnumerable<int> PuntajesMayoresQue(int umbral) =>
            historial.Where(p => p > umbral).OrderByDescending(p => p);

        public int TotalPartidas() => historial.Count;
    }

    // ─── Estado del juego ────────────────────────────────────────────────────

    public enum EstadoJuego { Jugando, Pausado, GameOver, Victoria }

    public class EstadoCohete
    {
        public EstadoJuego Estado { get; set; } = EstadoJuego.Jugando;
        public int Puntos { get; set; } = 0;
        public int Nivel { get; private set; } = 1;
        private float tiempoNivel = 0f;

        public void ActualizarNivel(float delta)
        {
            tiempoNivel += delta;
            if (tiempoNivel >= 20f)
            {
                tiempoNivel = 0f;
                Nivel++;
            }
        }

        public float VelocidadMeteoritos => 150f + (Nivel - 1) * 30f;
        public float IntervaloSpawnMeteoritos => Math.Max(0.5f, 1.5f - (Nivel - 1) * 0.15f);
        public float IntervaloSpawnAliens => Math.Max(3f, 8f - (Nivel - 1) * 0.5f);
    }

    // ─── Motor principal del juego ────────────────────────────────────────────

    public class JuegoCohete
    {
        public Cohete Jugador { get; private set; }
        public List<Meteorito> Meteoritos { get; private set; } = new();
        public List<Alien> Aliens { get; private set; } = new();
        public List<Corazon> Corazones { get; private set; } = new();
        public List<Bala> Balas { get; private set; } = new();
        public EstadoCohete Estado { get; private set; } = new();
        public RegistroPuntuacion Puntuaciones { get; private set; } = new();

        private static readonly Random rng = new Random();
        private float timerMeteoritos = 0f;
        private float timerAliens = 0f;
        private float timerDisparo = 0f;
        private const float COOLDOWN_DISPARO = 0.3f;

        private readonly int anchoPanel;
        private readonly int altoPanel;

        // Eventos para la UI
        public event EventHandler? JugadorMurio;
        public event EventHandler? JuegoTerminado;

        public JuegoCohete(int ancho, int alto)
        {
            anchoPanel = ancho;
            altoPanel = alto;
            Jugador = new Cohete(ancho / 2 - 20, alto - 100);
        }

        public void Actualizar(float delta)
        {
            if (Estado.Estado != EstadoJuego.Jugando) return;

            Estado.ActualizarNivel(delta);
            Jugador.Actualizar(delta);

            // Spawn meteoritos
            timerMeteoritos += delta;
            if (timerMeteoritos >= Estado.IntervaloSpawnMeteoritos)
            {
                timerMeteoritos = 0f;
                float xSpawn = rng.Next(10, anchoPanel - 50);
                Meteoritos.Add(new Meteorito(xSpawn, -50, Estado.VelocidadMeteoritos));
            }

            // Spawn aliens
            timerAliens += delta;
            if (timerAliens >= Estado.IntervaloSpawnAliens)
            {
                timerAliens = 0f;
                float xAlien = rng.Next(10, anchoPanel - 60);
                var alien = new Alien(xAlien, rng.Next(30, 150));
                alien.DisparoCorazon += (s, pos) =>
                    Corazones.Add(new Corazon(pos.X - 9, pos.Y));
                Aliens.Add(alien);
            }

            timerDisparo += delta;

            // Actualizar entidades con LINQ para limpiar inactivos
            Meteoritos.ForEach(m => m.Actualizar(delta));
            Aliens.ForEach(a => a.Actualizar(delta));
            Corazones.ForEach(c => c.Actualizar(delta));
            Balas.ForEach(b => b.Actualizar(delta));

            VerificarColisiones();
            LimpiarEntidadesInactivas();

            if (!Jugador.Activo)
            {
                Estado.Estado = EstadoJuego.GameOver;
                Puntuaciones.Agregar(Estado.Puntos);
                JuegoTerminado?.Invoke(this, EventArgs.Empty);
            }
        }

        private void VerificarColisiones()
        {
            // Balas vs Meteoritos — LINQ: obtener pares que colisionan
            var impactosMeteoritos = (from bala in Balas
                                      from met in Meteoritos
                                      where bala.ColisionaCon(met)
                                      select (bala, met)).ToList();

            impactosMeteoritos.ForEach(par =>
            {
                par.bala.Activo = false;
                par.met.Activo = false;
                Estado.Puntos += 10;
            });

            // Balas vs Aliens
            var impactosAliens = (from bala in Balas
                                  from alien in Aliens
                                  where bala.ColisionaCon(alien)
                                  select (bala, alien)).ToList();

            impactosAliens.ForEach(par =>
            {
                par.bala.Activo = false;
                par.alien.Activo = false;
                Estado.Puntos += 25;
            });

            // Cohete vs Meteoritos
            Meteoritos.Where(m => m.ColisionaCon(Jugador)).ToList()
                      .ForEach(m => { m.Activo = false; Jugador.RecibirDanio(); });

            // Cohete vs Corazones
            Corazones.Where(c => c.ColisionaCon(Jugador)).ToList()
                     .ForEach(c => { c.Activo = false; Jugador.RecibirDanio(); });
        }

        private void LimpiarEntidadesInactivas()
        {
            Meteoritos.RemoveAll(m => !m.Activo);
            Aliens.RemoveAll(a => !a.Activo);
            Corazones.RemoveAll(c => !c.Activo);
            Balas.RemoveAll(b => !b.Activo);
        }

        public void MoverJugador(float dx, float dy)
        {
            if (Estado.Estado != EstadoJuego.Jugando) return;
            float nx = Jugador.X + dx;
            float ny = Jugador.Y + dy;
            Jugador.X = Math.Clamp(nx, 0, anchoPanel - Jugador.Ancho);
            Jugador.Y = Math.Clamp(ny, 0, altoPanel - Jugador.Alto);
        }

        public void Disparar()
        {
            if (Estado.Estado != EstadoJuego.Jugando) return;
            if (timerDisparo < COOLDOWN_DISPARO) return;
            timerDisparo = 0f;
            Balas.Add(new Bala(Jugador.X + Jugador.Ancho / 2 - 3, Jugador.Y - 14));
        }

        public void TogglePausa()
        {
            Estado.Estado = Estado.Estado == EstadoJuego.Pausado
                ? EstadoJuego.Jugando
                : EstadoJuego.Pausado;
        }

        public void Reiniciar()
        {
            Jugador = new Cohete(anchoPanel / 2 - 20, altoPanel - 100);
            Meteoritos.Clear();
            Aliens.Clear();
            Corazones.Clear();
            Balas.Clear();
            Estado = new EstadoCohete();
            timerMeteoritos = 0f;
            timerAliens = 0f;
            timerDisparo = 0f;
        }
    }
}
// Al inicio del archivo agrega estos usings si no los tienes:
// using BurdiGames.Clases;
// using BurdiGames.Clases.Juegos;

namespace BurdiGames.Clases.Juegos
{
    internal class JuegoCoheteArcade : Arcade
    {
        public JuegoCoheteArcade()
            : base(
                nombre: "Cohete Espacial",
                descripcion: "Esquiva meteoritos rosas y elimina aliens maquillados en el espacio. utiliza a,d para ir derecha e izquierda" +
                  "y w, s para ir arriba y abajo. y con Enter Disparas",
                rutaImagen: "Sources/Imagenes/logo_galaxian.png",
                vidas: 3)
        {
        }

        public override void Jugar()
        {
            var form = new FormCohete();
            form.ShowDialog();
        }
    }
}