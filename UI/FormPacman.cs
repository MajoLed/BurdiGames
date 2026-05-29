
﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using BurdiGames.Clases.Juegos;

namespace BurdiGames.UI
{
    // ============================================================
    //  FORMULARIO PACMAN — solo UI, sin lógica de juego
    //  La lógica vive en MotorPacman (JuegoPacman.cs)
    // ============================================================
    public class FormPacman : Form
    {
        private MotorPacman _motor;
        private System.Windows.Forms.Timer _timerJuego;
        private Panel _canvas;
        private Label _lblPuntuacion;
        private Label _lblVidas;
        private Label _lblNivel;
        private Label _lblOverlay;
        private Panel _panelHud;
        private Button _btnVolver;
        private Button _btnReiniciar;

        // Interpolación suave de posición
        private float _pacLerpX, _pacLerpY;

        public FormPacman()
        {
            _motor = new MotorPacman();
            _motor.AlCambiarPuntuacion += _ => ActualizarHUD();
            _motor.AlCambiarEstado += MostrarOverlay;

            InicializarUI();

            _pacLerpX = _motor.Jugador.Posicion.X;
            _pacLerpY = _motor.Jugador.Posicion.Y;

            IniciarTimer();

            this.Focus();
        }

        private void InicializarUI()
        {
            int mapaW = MapaPacman.Cols * MapaPacman.Celda;
            int mapaH = MapaPacman.Filas * MapaPacman.Celda;

            this.Text = "🌸 Pac-Girl Rosa 🌸";
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(255, 230, 245);
            this.ClientSize = new Size(mapaW, mapaH + 100);
            this.KeyDown += Form_KeyDown;
            this.KeyPreview = true;

            // ── HUD superior ──────────────────────────────────────
            _panelHud = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(mapaW, 60),
                BackColor = PaletaPacman.HudFondo
            };

            _lblPuntuacion = CrearLabelHud("⭐ 0", 10, 15);
            _lblNivel = CrearLabelHud("🌸 Nivel 1", mapaW / 2 - 40, 15);
            _lblVidas = CrearLabelHud("🩷 🩷 🩷", mapaW - 130, 15);
            _panelHud.Controls.AddRange(new Control[] { _lblPuntuacion, _lblNivel, _lblVidas });
            this.Controls.Add(_panelHud);

            // ── Canvas de juego ───────────────────────────────────
            _canvas = new Panel
            {
                Location = new Point(0, 60),
                Size = new Size(mapaW, mapaH),
                BackColor = PaletaPacman.FondoColor,
                TabStop = false,
            };

            // Activar doble buffer sin parpadeo
            typeof(Panel).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(_canvas, true);

            _canvas.Paint += Canvas_Paint;
            this.Controls.Add(_canvas);

            // ── Overlay de mensajes ───────────────────────────────
            _lblOverlay = new Label
            {
                AutoSize = false,
                Size = new Size(mapaW, mapaH),
                Location = new Point(0, 60),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = PaletaPacman.TextoColor,
                BackColor = Color.Transparent,
                Visible = false
            };

            this.Controls.Add(_lblOverlay);
            _lblOverlay.BringToFront();

            // ── Botón volver al menú ──────────────────────────────
            _btnVolver = new Button
            {
                Text = "⬅ Volver al Menú",
                Location = new Point(10, mapaH + 68),
                Size = new Size(160, 26),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(255, 182, 213),
                ForeColor = Color.FromArgb(180, 60, 100),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            _btnVolver.FlatAppearance.BorderSize = 0;
            _btnVolver.Click += (s, e) =>
            {
                _timerJuego?.Stop();
                this.Close();
            };

            this.Controls.Add(_btnVolver);

            _btnReiniciar = new Button
            {
                Text = "🔄 Reiniciar",
                Location = new Point(185, mapaH + 68),
                Size = new Size(120, 26),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(255, 182, 213),
                ForeColor = Color.FromArgb(180, 60, 100),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Visible = false
            };

            _btnReiniciar.FlatAppearance.BorderSize = 0;
            _btnReiniciar.Click += (s, e) => ReiniciarJuego();
            this.Controls.Add(_btnReiniciar);
        }

        private Label CrearLabelHud(string texto, int x, int y) => new Label
        {
            Text = texto,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = PaletaPacman.TextoColor,
            AutoSize = true,
            Location = new Point(x, y),
            BackColor = Color.Transparent
        };

        private void ActualizarHUD()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(ActualizarHUD);
                return;
            }

            _lblPuntuacion.Text = $"⭐ {_motor.Puntuacion}";
            _lblNivel.Text = $"🌸 Nivel {_motor.Nivel}";
            int vidas = _motor.Jugador.Vidas;

            _lblVidas.Text = string.Join(" ", Enumerable.Repeat("🩷", Math.Max(0, vidas)));
        }

        private void MostrarOverlay(string estado)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(() => MostrarOverlay(estado));
                return;
            }

            switch (estado)
            {
                case "ganaste":
                    _lblOverlay.Text = "🌸 ¡Ganaste! 🌸\n\nPresiona ENTER para continuar";
                    _lblOverlay.Visible = true;
                    this.Focus();
                    break;

                case "gameover":
                    _lblOverlay.Text = $"💔 Game Over 💔\n\n⭐ Puntaje: {_motor.Puntuacion}";
                    _lblOverlay.Visible = true;
                    _btnReiniciar.Visible = true;
                    _btnReiniciar.BringToFront();
                    break;

                case "muerto":
                    _lblOverlay.Text = "💫 ¡Ouch! 💫";
                    _lblOverlay.Visible = true;

                    var t = new System.Windows.Forms.Timer { Interval = 1500 };
                    t.Tick += (s, e) =>
                    {
                        _lblOverlay.Visible = false;
                        t.Stop();
                        t.Dispose();
                    };
                    t.Start();
                    break;

                default:
                    _lblOverlay.Visible = false;
                    _btnReiniciar.Visible = false;
                    break;
            }
        }

        private void IniciarTimer()
        {
            _timerJuego = new System.Windows.Forms.Timer { Interval = 100 };
            _timerJuego.Tick += BucleJuego;
            _timerJuego.Start();
        }

        private void BucleJuego(object? s, EventArgs e)
        {
            _motor.Tick();

            // Interpolación suave de posición
            _pacLerpX += (_motor.Jugador.Posicion.X - _pacLerpX) * 0.5f;
            _pacLerpY += (_motor.Jugador.Posicion.Y - _pacLerpY) * 0.5f;

            _canvas.Invalidate();
        }

        private void Canvas_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.CompositingQuality = CompositingQuality.HighSpeed;

            // Fondo degradado suave
            using var fondo = new LinearGradientBrush(
                _canvas.ClientRectangle,
                PaletaPacman.FondoColor,
                Color.FromArgb(255, 250, 240),
                LinearGradientMode.Vertical);

            g.FillRectangle(fondo, _canvas.ClientRectangle);

            // Dibujar mapa
            _motor.Mapa.Dibujar(g);

            // Dibujar fantasmas
            foreach (var fantasma in _motor.Fantasmas)
                fantasma.Dibujar(g, fantasma.Posicion.X, fantasma.Posicion.Y);

            // Dibujar jugador
            _motor.Jugador.Dibujar(g, _pacLerpX, _pacLerpY);

            DibujarDestellos(g);
        }

        private void DibujarDestellos(Graphics g)
        {
            var rng = new Random(Environment.TickCount / 200);

            var destellos = Enumerable.Range(0, 3)
                .Select(_ => new { X = rng.Next(MapaPacman.Cols), Y = rng.Next(MapaPacman.Filas) })
                .Where(p => _motor.Mapa.Obtener(p.Y, p.X) == TipoCelda.Vacio)
                .Take(2);

            foreach (var d in destellos)
            {
                int px = d.X * MapaPacman.Celda + MapaPacman.Celda / 2;
                int py = d.Y * MapaPacman.Celda + MapaPacman.Celda / 2;

                using var sb = new SolidBrush(Color.FromArgb(80, 255, 215, 0));
                g.FillEllipse(sb, px - 3, py - 3, 6, 6);
            }
        }

        private void Form_KeyDown(object? sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                    _motor.EncolarDireccion(DireccionPac.Arriba);
                    break;

                case Keys.Down:
                    _motor.EncolarDireccion(DireccionPac.Abajo);
                    break;

                case Keys.Left:
                    _motor.EncolarDireccion(DireccionPac.Izquierda);
                    break;

                case Keys.Right:
                    _motor.EncolarDireccion(DireccionPac.Derecha);
                    break;

                case Keys.Enter:
                    if (_motor.Estado == "ganaste")
                    {
                        _motor.SiguienteNivel();
                        ActualizarHUD();
                        _lblOverlay.Visible = false;
                    }
                    break;

                case Keys.R:
                    if (_motor.Estado == "gameover")
                    {
                        ReiniciarJuego();
                    }
                    break;
            }

            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Up:
                    _motor.EncolarDireccion(DireccionPac.Arriba);
                    return true;

                case Keys.Down:
                    _motor.EncolarDireccion(DireccionPac.Abajo);
                    return true;

                case Keys.Left:
                    _motor.EncolarDireccion(DireccionPac.Izquierda);
                    return true;

                case Keys.Right:
                    _motor.EncolarDireccion(DireccionPac.Derecha);
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void ReiniciarJuego()
        {
            _timerJuego?.Stop();

            _motor = new MotorPacman();
            _motor.AlCambiarPuntuacion += _ => ActualizarHUD();
            _motor.AlCambiarEstado += MostrarOverlay;

            _pacLerpX = _motor.Jugador.Posicion.X;
            _pacLerpY = _motor.Jugador.Posicion.Y;

            _lblOverlay.Visible = false;
            _btnReiniciar.Visible = false;

            ActualizarHUD();
            _timerJuego?.Start();

            this.Focus();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _timerJuego?.Stop();
            _timerJuego?.Dispose();
            base.OnFormClosed(e);
        }
    }
}



