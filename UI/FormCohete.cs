using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace MiniJuegos
{
    // Panel con doble buffer para eliminar el flickering
    public class PanelSuave : Panel
    {
        public PanelSuave()
        {
            this.DoubleBuffered = true;
            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer,
                true);
            this.UpdateStyles();
        }
    }

    public class FormCohete : Form
    {
        private JuegoCohete juego;
        private System.Windows.Forms.Timer timerJuego;
        private System.Windows.Forms.Timer timerEstrellas;
        private DateTime ultimoFrame;

        private readonly HashSet<Keys> teclasPresionadas = new();

        private PanelSuave panelJuego;
        private Button btnPausa;
        private Button btnReiniciar;
        private Label lblPuntos;
        private Label lblVidas;
        private Label lblNivel;

        private readonly List<(float x, float y, float brillo, float vel)> estrellas = new();
        private static readonly Random rng = new Random();
        private readonly List<(float x, float y, float vx, float vy, float vida, Color color)> particulas = new();

        public FormCohete()
        {
            InicializarComponentes();
            GenerarEstrellas();

            juego = new JuegoCohete(panelJuego.Width, panelJuego.Height);
            juego.JuegoTerminado += (s, e) => MostrarGameOver();

            timerJuego = new System.Windows.Forms.Timer { Interval = 16 };
            timerJuego.Tick += GameLoop;

            timerEstrellas = new System.Windows.Forms.Timer { Interval = 50 };
            timerEstrellas.Tick += (s, e) => ActualizarEstrellas();

            ultimoFrame = DateTime.Now;
            timerJuego.Start();
            timerEstrellas.Start();

            panelJuego.Focus();
        }

        private void InicializarComponentes()
        {
            Text = "🚀 COSMIC FURY";
            ClientSize = new Size(800, 700);
            BackColor = Color.FromArgb(5, 0, 20);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Font = new Font("Consolas", 9f, FontStyle.Bold);
            KeyPreview = true;
            KeyDown += Form_KeyDown;
            KeyUp += Form_KeyUp;

            // ── Panel HUD superior ──────────────────────────────────
            var panelHud = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(800, 70),
                BackColor = Color.FromArgb(15, 0, 40)
            };
            panelHud.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(180, 0, 255), 2f);
                e.Graphics.DrawLine(pen, 0, 69, 800, 69);
                using var glow = new Pen(Color.FromArgb(60, 180, 0, 255), 6f);
                e.Graphics.DrawLine(glow, 0, 68, 800, 68);
            };

            // Columna izquierda: puntos
            lblPuntos = new Label
            {
                Text = "PUNTOS: 000000",
                ForeColor = Color.FromArgb(220, 180, 255),
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 11f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(16, 10)
            };

            // Columna centro: vidas
            lblVidas = new Label
            {
                Text = "♥ ♥ ♥",
                ForeColor = Color.FromArgb(255, 80, 180),
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 15f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(310, 6)
            };

            // Columna centro-bajo: nivel
            lblNivel = new Label
            {
                Text = "NIVEL: 1",
                ForeColor = Color.FromArgb(0, 220, 255),
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 9f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(326, 40)
            };

            // Botón Pausa — esquina derecha arriba
            btnPausa = new Button
            {
                Text = "⏸ PAUSA",
                Location = new Point(570, 10),
                Size = new Size(100, 34),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(100, 0, 200),
                Cursor = Cursors.Hand,
                Font = new Font("Consolas", 8.5f, FontStyle.Bold)
            };
            btnPausa.FlatAppearance.BorderColor = Color.FromArgb(220, 150, 255);
            btnPausa.FlatAppearance.BorderSize = 1;
            btnPausa.MouseEnter += (s, e) => btnPausa.BackColor = Color.FromArgb(140, 30, 240);
            btnPausa.MouseLeave += (s, e) => btnPausa.BackColor = Color.FromArgb(100, 0, 200);
            btnPausa.Click += (s, e) =>
            {
                juego.TogglePausa();
                btnPausa.Text = juego.Estado.Estado == EstadoJuego.Pausado ? "▶ SEGUIR" : "⏸ PAUSA";
                panelJuego.Focus();
            };

            // Botón Reiniciar — a la derecha del de pausa
            btnReiniciar = new Button
            {
                Text = "↺ RESET",
                Location = new Point(682, 10),
                Size = new Size(100, 34),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(180, 0, 80),
                Cursor = Cursors.Hand,
                Font = new Font("Consolas", 8.5f, FontStyle.Bold)
            };
            btnReiniciar.FlatAppearance.BorderColor = Color.FromArgb(255, 100, 180);
            btnReiniciar.FlatAppearance.BorderSize = 1;
            btnReiniciar.MouseEnter += (s, e) => btnReiniciar.BackColor = Color.FromArgb(220, 20, 100);
            btnReiniciar.MouseLeave += (s, e) => btnReiniciar.BackColor = Color.FromArgb(180, 0, 80);
            btnReiniciar.Click += (s, e) =>
            {
                juego.Reiniciar();
                btnPausa.Text = "⏸ PAUSA";
                btnPausa.Enabled = true;
                particulas.Clear();
                panelJuego.Focus();
            };

            panelHud.Controls.AddRange(new Control[]
            {
                lblPuntos, lblVidas, lblNivel, btnPausa, btnReiniciar
            });

            // ── Panel de juego con doble buffer ─────────────────────
            panelJuego = new PanelSuave
            {
                Location = new Point(0, 70),
                Size = new Size(800, 630),
                BackColor = Color.FromArgb(5, 0, 30)
            };
            panelJuego.Paint += PanelJuego_Paint;

            Controls.Add(panelHud);
            Controls.Add(panelJuego);
        }

        private void GenerarEstrellas()
        {
            for (int i = 0; i < 140; i++)
            {
                estrellas.Add((
                    rng.Next(0, 800),
                    rng.Next(0, 700),
                    rng.NextSingle(),
                    rng.NextSingle() * 0.6f + 0.1f
                ));
            }
        }

        private void ActualizarEstrellas()
        {
            for (int i = 0; i < estrellas.Count; i++)
            {
                var (x, y, br, vel) = estrellas[i];
                float ny = y + vel;
                if (ny > 700) ny = 0;
                estrellas[i] = (x, ny, br, vel);
            }
        }

        private void GameLoop(object? sender, EventArgs e)
        {
            var ahora = DateTime.Now;
            float delta = (float)(ahora - ultimoFrame).TotalSeconds;
            ultimoFrame = ahora;
            delta = Math.Min(delta, 0.05f);

            if (juego.Estado.Estado == EstadoJuego.Jugando)
            {
                float vel = 220f * delta;
                if (teclasPresionadas.Contains(Keys.Left) || teclasPresionadas.Contains(Keys.A))
                    juego.MoverJugador(-vel, 0);
                if (teclasPresionadas.Contains(Keys.Right) || teclasPresionadas.Contains(Keys.D))
                    juego.MoverJugador(vel, 0);
                if (teclasPresionadas.Contains(Keys.Up) || teclasPresionadas.Contains(Keys.W))
                    juego.MoverJugador(0, -vel);
                if (teclasPresionadas.Contains(Keys.Down) || teclasPresionadas.Contains(Keys.S))
                    juego.MoverJugador(0, vel);
                if (teclasPresionadas.Contains(Keys.Space))
                    juego.Disparar();

                juego.Actualizar(delta);
                ActualizarParticulas(delta);

                // Actualizar HUD
                lblPuntos.Text = $"PUNTOS: {juego.Estado.Puntos:D6}";

                int nivelVisual = juego.Estado.Puntos / 150 + 1;
                lblNivel.Text = $"NIVEL: {nivelVisual}";

                lblVidas.Text = juego.Jugador.Vidas switch
                {
                    3 => "♥ ♥ ♥",
                    2 => "♥ ♥ ♡",
                    1 => "♥ ♡ ♡",
                    _ => "♡ ♡ ♡"
                };
            }

            panelJuego.Invalidate();
        }

        private void ActualizarParticulas(float delta)
        {
            for (int i = particulas.Count - 1; i >= 0; i--)
            {
                var p = particulas[i];
                float vida = p.vida - delta;
                if (vida <= 0) { particulas.RemoveAt(i); continue; }
                particulas[i] = (p.x + p.vx * delta, p.y + p.vy * delta,
                                 p.vx, p.vy * 0.95f + 20 * delta, vida, p.color);
            }
        }

        private void PanelJuego_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Fondo degradado
            using var fondo = new LinearGradientBrush(
                new Point(0, 0), new Point(0, panelJuego.Height),
                Color.FromArgb(5, 0, 30), Color.FromArgb(10, 0, 55));
            g.FillRectangle(fondo, 0, 0, panelJuego.Width, panelJuego.Height);

            // Nebulosas decorativas
            using var neb1 = new SolidBrush(Color.FromArgb(15, 100, 0, 180));
            g.FillEllipse(neb1, -80, 80, 380, 200);
            using var neb2 = new SolidBrush(Color.FromArgb(10, 180, 0, 100));
            g.FillEllipse(neb2, 480, 280, 320, 200);

            // Estrellas
            foreach (var (sx, sy, br, _) in estrellas)
            {
                int alpha = (int)(br * 255);
                using var sb = new SolidBrush(Color.FromArgb(alpha, 220, 200, 255));
                float sz = br < 0.3f ? 1f : br < 0.7f ? 1.5f : 2.5f;
                g.FillEllipse(sb, sx, sy, sz, sz);
            }

            // Entidades del juego
            juego.Meteoritos.ForEach(m => m.Dibujar(e));
            juego.Corazones.ForEach(c => c.Dibujar(e));
            juego.Balas.ForEach(b => b.Dibujar(e));
            juego.Aliens.ForEach(a => a.Dibujar(e));
            if (juego.Jugador.Activo) juego.Jugador.Dibujar(e);

            // Partículas
            foreach (var (px, py, _, _, vida, color) in particulas)
            {
                int alpha = Math.Clamp((int)(vida / 0.8f * 200), 0, 255);
                using var pb = new SolidBrush(Color.FromArgb(alpha, color));
                g.FillEllipse(pb, px - 3, py - 3, 6, 6);
            }

            // Overlays
            if (juego.Estado.Estado == EstadoJuego.Pausado)
                DibujarOverlay(g,
                    "⏸  PAUSADO",
                    "Presiona PAUSA para continuar",
                    Color.FromArgb(150, 0, 0, 60),
                    Color.FromArgb(200, 150, 255));

            if (juego.Estado.Estado == EstadoJuego.GameOver)
                DibujarOverlay(g,
                    "💀  GAME OVER",
                    $"Puntuación: {juego.Estado.Puntos:D6}\nPresiona RESET para volver a jugar",
                    Color.FromArgb(170, 60, 0, 0),
                    Color.FromArgb(255, 80, 120));
        }

        private void DibujarOverlay(Graphics g, string titulo, string subtitulo,
                                     Color colorFondo, Color colorTitulo)
        {
            using var fondo = new SolidBrush(colorFondo);
            g.FillRectangle(fondo, 0, 0, panelJuego.Width, panelJuego.Height);

            var rect = new Rectangle(130, 210, 540, 210);
            using var caja = new SolidBrush(Color.FromArgb(210, 10, 0, 40));
            g.FillRectangle(caja, rect);
            using var borde = new Pen(Color.FromArgb(255, 180, 0, 255), 3f);
            g.DrawRectangle(borde, rect);
            using var glow = new Pen(Color.FromArgb(80, 180, 0, 255), 8f);
            g.DrawRectangle(glow, rect);

            using var fTitulo = new Font("Consolas", 28f, FontStyle.Bold);
            using var bTitulo = new SolidBrush(colorTitulo);
            var sf = new StringFormat { Alignment = StringAlignment.Center };
            g.DrawString(titulo, fTitulo, bTitulo, new RectangleF(130, 230, 540, 80), sf);

            using var fSub = new Font("Consolas", 11f, FontStyle.Regular);
            using var bSub = new SolidBrush(Color.FromArgb(210, 200, 180, 255));
            g.DrawString(subtitulo, fSub, bSub, new RectangleF(130, 320, 540, 90), sf);
        }

        private void MostrarGameOver()
        {
            btnPausa.Enabled = false;
        }

        private void Form_KeyDown(object? sender, KeyEventArgs e)
        {
            teclasPresionadas.Add(e.KeyCode);
            if (e.KeyCode == Keys.Escape)
            {
                juego.TogglePausa();
                btnPausa.Text = juego.Estado.Estado == EstadoJuego.Pausado ? "▶ SEGUIR" : "⏸ PAUSA";
            }
            if (e.KeyCode == Keys.R)
            {
                juego.Reiniciar();
                btnPausa.Text = "⏸ PAUSA";
                btnPausa.Enabled = true;
                particulas.Clear();
            }
        }

        private void Form_KeyUp(object? sender, KeyEventArgs e)
        {
            teclasPresionadas.Remove(e.KeyCode);
        }
    }
}