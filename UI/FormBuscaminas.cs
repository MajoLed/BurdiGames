using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MiniJuegos
{
    // Panel con doble buffer — igual que FormCohete
    public class PanelSuaveBuscaminas : Panel
    {
        public PanelSuaveBuscaminas()
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

    public class FormBuscaminas : Form
    {
        private MotorBuscaminas motor;
        private System.Windows.Forms.Timer timerHud;

        private PanelSuaveBuscaminas panelJuego;
        private Button btnPausa;
        private Button btnReiniciar;
        private Label lblPuntos;
        private Label lblMinas;
        private Label lblTiempo;

        // Tamaño de cada celda en píxeles
        private const int TAM_CELDA = 36;
        private const int FILAS = 16;
        private const int COLS = 16;
        private const int ANCHO_TABLERO = COLS * TAM_CELDA;   // 576
        private const int ALTO_TABLERO = FILAS * TAM_CELDA;  // 576

        // Paleta girlie
        private static readonly Color C_FONDO = Color.FromArgb(30, 5, 25);
        private static readonly Color C_HUD = Color.FromArgb(45, 10, 40);
        private static readonly Color C_CELDA_OCULTA = Color.FromArgb(220, 100, 160);
        private static readonly Color C_CELDA_OCULTA2 = Color.FromArgb(200, 80, 145);
        private static readonly Color C_CELDA_REV = Color.FromArgb(255, 220, 235);
        private static readonly Color C_CELDA_REV2 = Color.FromArgb(250, 200, 220);
        private static readonly Color C_BORDE_NEON = Color.FromArgb(255, 20, 180);
        private static readonly Color C_BORDE_SUAVE = Color.FromArgb(180, 60, 140);
        private static readonly Color C_MINA = Color.FromArgb(30, 0, 20);
        private static readonly Color C_TEXTO_HUD = Color.FromArgb(255, 180, 220);
        private static readonly Color C_NEON_ROSA = Color.FromArgb(255, 20, 180);
        private static readonly Color C_NEON_MORADO = Color.FromArgb(180, 0, 255);
        private static readonly Color C_ACENTO_CIAN = Color.FromArgb(255, 100, 200);

        // Colores para los números (1-8)
        private static readonly Color[] C_NUMEROS = {
            Color.FromArgb(180, 0, 255),   // 1 — morado
            Color.FromArgb(255, 20, 147),  // 2 — rosa fuerte
            Color.FromArgb(200, 0, 200),   // 3 — fucsia
            Color.FromArgb(130, 0, 200),   // 4 — violeta
            Color.FromArgb(220, 60, 100),  // 5 — rosa oscuro
            Color.FromArgb(160, 0, 160),   // 6 — morado medio
            Color.FromArgb(80, 0, 100),    // 7 — morado profundo
            Color.FromArgb(50, 0, 50),     // 8 — casi negro
        };

        public FormBuscaminas()
        {
            motor = new MotorBuscaminas();
            InicializarComponentes();

            timerHud = new System.Windows.Forms.Timer { Interval = 500 };
            timerHud.Tick += (s, e) => ActualizarHud();
            timerHud.Start();

            motor.JuegoTerminado += (s, e) => { panelJuego.Invalidate(); btnPausa.Enabled = false; };
            motor.VictoriaAlcanzada += (s, e) => { panelJuego.Invalidate(); btnPausa.Enabled = false; };
        }

        private void InicializarComponentes()
        {
            int anchoForm = ANCHO_TABLERO + 2;  // 578
            int altoHud = 70;
            int altoForm = ALTO_TABLERO + altoHud + 2;

            Text = "💣 BLOSSOM MINES";
            ClientSize = new Size(anchoForm, altoForm);
            BackColor = C_FONDO;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Font = new Font("Consolas", 9f, FontStyle.Bold);

            // ── HUD ──────────────────────────────────────────────────
            var panelHud = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(anchoForm, altoHud),
                BackColor = C_HUD
            };
            panelHud.Paint += (s, e) =>
            {
                using var pen = new Pen(C_NEON_ROSA, 2f);
                using var glow = new Pen(Color.FromArgb(60, 255, 20, 180), 6f);
                e.Graphics.DrawLine(pen, 0, altoHud - 1, anchoForm, altoHud - 1);
                e.Graphics.DrawLine(glow, 0, altoHud - 2, anchoForm, altoHud - 2);
            };

            // Puntos — izquierda
            lblPuntos = new Label
            {
                Text = "✦ 000000",
                ForeColor = C_TEXTO_HUD,
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 11f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(12, 10)
            };

            // Minas restantes — centro
            lblMinas = new Label
            {
                Text = "💣 40",
                ForeColor = C_NEON_ROSA,
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 15f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(220, 6)
            };

            // Tiempo — centro-bajo
            lblTiempo = new Label
            {
                Text = "⏱ 00:00",
                ForeColor = C_ACENTO_CIAN,
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 9f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(232, 40)
            };

            // Botón Pausa
            btnPausa = new Button
            {
                Text = "⏸ PAUSA",
                Location = new Point(348, 10),
                Size = new Size(100, 34),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(120, 0, 180),
                Cursor = Cursors.Hand,
                Font = new Font("Consolas", 8.5f, FontStyle.Bold)
            };
            btnPausa.FlatAppearance.BorderColor = C_NEON_MORADO;
            btnPausa.FlatAppearance.BorderSize = 1;
            btnPausa.MouseEnter += (s, e) => btnPausa.BackColor = Color.FromArgb(160, 20, 220);
            btnPausa.MouseLeave += (s, e) => btnPausa.BackColor = Color.FromArgb(120, 0, 180);
            btnPausa.Click += (s, e) =>
            {
                motor.TogglePausa();
                btnPausa.Text = motor.Estado == EstadoBuscaminas.Pausado ? "▶ SEGUIR" : "⏸ PAUSA";
                panelJuego.Focus();
                panelJuego.Invalidate();
            };

            // Botón Reiniciar
            btnReiniciar = new Button
            {
                Text = "↺ RESET",
                Location = new Point(460, 10),
                Size = new Size(100, 34),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(160, 0, 100),
                Cursor = Cursors.Hand,
                Font = new Font("Consolas", 8.5f, FontStyle.Bold)
            };
            btnReiniciar.FlatAppearance.BorderColor = C_NEON_ROSA;
            btnReiniciar.FlatAppearance.BorderSize = 1;
            btnReiniciar.MouseEnter += (s, e) => btnReiniciar.BackColor = Color.FromArgb(210, 20, 130);
            btnReiniciar.MouseLeave += (s, e) => btnReiniciar.BackColor = Color.FromArgb(160, 0, 100);
            btnReiniciar.Click += (s, e) =>
            {
                motor.Reiniciar();
                btnPausa.Text = "⏸ PAUSA";
                btnPausa.Enabled = true;
                panelJuego.Focus();
                panelJuego.Invalidate();
            };

            panelHud.Controls.AddRange(new Control[]
            {
                lblPuntos, lblMinas, lblTiempo, btnPausa, btnReiniciar
            });

            // ── Panel de juego ───────────────────────────────────────
            panelJuego = new PanelSuaveBuscaminas
            {
                Location = new Point(1, altoHud),
                Size = new Size(ANCHO_TABLERO, ALTO_TABLERO),
                BackColor = C_FONDO
            };
            panelJuego.Paint += PanelJuego_Paint;
            panelJuego.MouseClick += PanelJuego_MouseClick;

            Controls.Add(panelHud);
            Controls.Add(panelJuego);
        }

        // ── Actualizar HUD ───────────────────────────────────────────
        private void ActualizarHud()
        {
            lblPuntos.Text = $"✦ {motor.Puntos:D6}";
            lblMinas.Text = $"💣 {motor.MinasRestantes}";
            int seg = motor.SegundosTranscurridos;
            lblTiempo.Text = $"⏱ {seg / 60:D2}:{seg % 60:D2}";
        }

        // ── Clic en el tablero ───────────────────────────────────────
        private void PanelJuego_MouseClick(object? sender, MouseEventArgs e)
        {
            if (motor.Estado == EstadoBuscaminas.Pausado ||
                motor.Estado == EstadoBuscaminas.GameOver ||
                motor.Estado == EstadoBuscaminas.Victoria) return;

            int col = e.X / TAM_CELDA;
            int fila = e.Y / TAM_CELDA;
            if (col < 0 || col >= COLS || fila < 0 || fila >= FILAS) return;

            if (e.Button == MouseButtons.Left)
                motor.Revelar(fila, col);
            else if (e.Button == MouseButtons.Right)
                motor.ToggleBandera(fila, col);

            panelJuego.Invalidate();
        }

        // ── Pintar tablero ───────────────────────────────────────────
        private void PanelJuego_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Fondo degradado del tablero
            using var fondoGrad = new LinearGradientBrush(
                new Point(0, 0), new Point(0, ALTO_TABLERO),
                Color.FromArgb(35, 8, 30), Color.FromArgb(20, 2, 18));
            g.FillRectangle(fondoGrad, 0, 0, ANCHO_TABLERO, ALTO_TABLERO);

            // Dibujar celdas
            for (int f = 0; f < FILAS; f++)
                for (int c = 0; c < COLS; c++)
                    DibujarCelda(g, f, c);

            // Overlays
            if (motor.Estado == EstadoBuscaminas.Pausado)
                DibujarOverlay(g, "⏸  PAUSADO",
                    "Presiona PAUSA para continuar",
                    Color.FromArgb(150, 20, 0, 40),
                    Color.FromArgb(220, 150, 255));

            if (motor.Estado == EstadoBuscaminas.GameOver)
                DibujarOverlay(g, "💥 BOOM",
                    $"Pisaste una mina...\nPuntuación: {motor.Puntos:D6}\nRESET para volver a jugar",
                    Color.FromArgb(170, 60, 0, 20),
                    Color.FromArgb(255, 80, 160));

            if (motor.Estado == EstadoBuscaminas.Victoria)
                DibujarOverlay(g, "🌸 ¡GANASTE!",
                    $"¡Despejaste todas las minas!\nPuntuación: {motor.Puntos:D6}\nRESET para jugar de nuevo",
                    Color.FromArgb(160, 0, 30, 20),
                    Color.FromArgb(255, 180, 230));
        }

        private void DibujarCelda(Graphics g, int fila, int col)
        {
            var celda = motor.Tablero.Celdas[fila, col];
            int px = col * TAM_CELDA;
            int py = fila * TAM_CELDA;
            var rect = new Rectangle(px + 1, py + 1, TAM_CELDA - 2, TAM_CELDA - 2);

            if (celda.EsRevelada)
            {
                // Tablero en damero suave para reveladas
                bool par = (fila + col) % 2 == 0;
                using var br = new SolidBrush(par ? C_CELDA_REV : C_CELDA_REV2);
                g.FillRectangle(br, rect);

                if (celda.TieneMina)
                {
                    DibujarMina(g, px + TAM_CELDA / 2, py + TAM_CELDA / 2);
                }
                else if (celda.MinasVecinas > 0)
                {
                    using var font = new Font("Consolas", 13f, FontStyle.Bold);
                    using var brush = new SolidBrush(C_NUMEROS[celda.MinasVecinas - 1]);
                    var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString(celda.MinasVecinas.ToString(), font, brush,
                        new RectangleF(px, py, TAM_CELDA, TAM_CELDA), sf);
                }
            }
            else
            {
                // Celda oculta con gradiente rosa
                bool par = (fila + col) % 2 == 0;
                using var grad = new LinearGradientBrush(
                    new Point(px, py), new Point(px + TAM_CELDA, py + TAM_CELDA),
                    par ? C_CELDA_OCULTA : C_CELDA_OCULTA2,
                    par ? Color.FromArgb(190, 70, 130) : Color.FromArgb(210, 90, 150));
                g.FillRectangle(grad, rect);

                // Brillo superior izquierdo (efecto 3D suave)
                using var brillo = new SolidBrush(Color.FromArgb(60, 255, 255, 255));
                g.FillRectangle(brillo, px + 1, py + 1, TAM_CELDA - 2, 3);
                g.FillRectangle(brillo, px + 1, py + 1, 3, TAM_CELDA - 2);

                if (celda.TieneBandera)
                    DibujarBandera(g, px + TAM_CELDA / 2, py + TAM_CELDA / 2);
            }

            // Borde de la celda
            using var borde = new Pen(C_BORDE_SUAVE, 0.8f);
            g.DrawRectangle(borde, px, py, TAM_CELDA, TAM_CELDA);
        }

        private void DibujarMina(Graphics g, int cx, int cy)
        {
            int r = 9;
            // Destello de fondo rojo-rosa cuando explota
            using var destello = new SolidBrush(Color.FromArgb(80, 255, 50, 100));
            g.FillEllipse(destello, cx - r - 5, cy - r - 5, (r + 5) * 2, (r + 5) * 2);

            // Cuerpo de la mina
            using var cuerpo = new SolidBrush(C_MINA);
            g.FillEllipse(cuerpo, cx - r, cy - r, r * 2, r * 2);

            // Pinchos
            using var pincho = new Pen(C_MINA, 2.5f);
            int[] angulos = { 0, 45, 90, 135, 180, 225, 270, 315 };
            foreach (int ang in angulos)
            {
                double rad = ang * Math.PI / 180.0;
                g.DrawLine(pincho,
                    cx + (int)((r - 1) * Math.Cos(rad)),
                    cy + (int)((r - 1) * Math.Sin(rad)),
                    cx + (int)((r + 5) * Math.Cos(rad)),
                    cy + (int)((r + 5) * Math.Sin(rad)));
            }

            // Brillo
            using var brilloMina = new SolidBrush(Color.FromArgb(120, 255, 255, 255));
            g.FillEllipse(brilloMina, cx - r / 2 - 1, cy - r / 2 - 1, r / 2 + 2, r / 2 + 2);

            // Acento rosa neón encima
            using var neon = new SolidBrush(Color.FromArgb(60, 255, 20, 147));
            g.FillEllipse(neon, cx - r, cy - r, r * 2, r * 2);
        }

        private void DibujarBandera(Graphics g, int cx, int cy)
        {
            // Palo
            using var palo = new Pen(Color.FromArgb(40, 0, 60), 2.5f);
            g.DrawLine(palo, cx, cy + 9, cx, cy - 9);

            // Bandera morada con borde negro
            var puntosBandera = new PointF[]
            {
                new PointF(cx,      cy - 9),
                new PointF(cx + 12, cy - 4),
                new PointF(cx,      cy + 1)
            };
            using var rellenoBandera = new SolidBrush(Color.FromArgb(140, 0, 200));
            g.FillPolygon(rellenoBandera, puntosBandera);
            using var bordeBandera = new Pen(Color.FromArgb(30, 0, 50), 1.5f);
            g.DrawPolygon(bordeBandera, puntosBandera);

            // Brillo en la bandera
            using var brilloBandera = new SolidBrush(Color.FromArgb(60, 200, 150, 255));
            g.FillPolygon(brilloBandera, new PointF[]
            {
                new PointF(cx,      cy - 9),
                new PointF(cx + 8,  cy - 5),
                new PointF(cx,      cy - 2)
            });

            // Base del palo
            using var base1 = new SolidBrush(Color.FromArgb(50, 0, 70));
            g.FillEllipse(base1, cx - 5, cy + 7, 10, 4);
        }

        private void DibujarOverlay(Graphics g, string titulo, string subtitulo,
                                     Color colorFondo, Color colorTitulo)
        {
            using var fondo = new SolidBrush(colorFondo);
            g.FillRectangle(fondo, 0, 0, ANCHO_TABLERO, ALTO_TABLERO);

            int rw = 460, rh = 220;
            int rx = (ANCHO_TABLERO - rw) / 2;
            int ry = (ALTO_TABLERO - rh) / 2;
            var rect = new Rectangle(rx, ry, rw, rh);

            using var caja = new SolidBrush(Color.FromArgb(220, 25, 5, 30));
            g.FillRectangle(caja, rect);

            using var borde = new Pen(C_NEON_ROSA, 3f);
            g.DrawRectangle(borde, rect);
            using var glow = new Pen(Color.FromArgb(80, 255, 20, 180), 8f);
            g.DrawRectangle(glow, rect);

            // Decoración de esquinas moradas
            using var esquina = new Pen(C_NEON_MORADO, 2f);
            int k = 14;
            g.DrawLine(esquina, rx, ry, rx + k, ry);
            g.DrawLine(esquina, rx, ry, rx, ry + k);
            g.DrawLine(esquina, rx + rw, ry, rx + rw - k, ry);
            g.DrawLine(esquina, rx + rw, ry, rx + rw, ry + k);
            g.DrawLine(esquina, rx, ry + rh, rx + k, ry + rh);
            g.DrawLine(esquina, rx, ry + rh, rx, ry + rh - k);
            g.DrawLine(esquina, rx + rw, ry + rh, rx + rw - k, ry + rh);
            g.DrawLine(esquina, rx + rw, ry + rh, rx + rw, ry + rh - k);

            var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Near
            };

            using var fTitulo = new Font("Consolas", 26f, FontStyle.Bold);
            using var bTitulo = new SolidBrush(colorTitulo);
            g.DrawString(titulo, fTitulo, bTitulo,
                new RectangleF(rx, ry + 20, rw, 70), sf);

            using var fSub = new Font("Consolas", 10.5f, FontStyle.Regular);
            using var bSub = new SolidBrush(Color.FromArgb(220, 255, 200, 230));
            g.DrawString(subtitulo, fSub, bSub,
                new RectangleF(rx, ry + 100, rw, 110), sf);
        }
    }
}