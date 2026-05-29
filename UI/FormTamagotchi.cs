using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MiniJuegos
{
    // ─── Panel doble buffer ───────────────────────────────────────────────────
    public class PanelSuaveTamagotchi : Panel
    {
        public PanelSuaveTamagotchi()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();
        }
    }

    // ─── Pantalla de configuración inicial ───────────────────────────────────
    public class FormConfigTamagotchi : Form
    {
        public string NombreMascota { get; private set; } = "Buddy";
        public TipoMascota Tipo { get; private set; } = TipoMascota.Perro;
        public GeneroMascota Genero { get; private set; } = GeneroMascota.Nino;

        private TextBox txtNombre;
        private Panel panelMascota;
        private Panel panelGenero;
        private Button btnJugar;
        private TipoMascota tipoSelec = TipoMascota.Perro;
        private GeneroMascota genSelec = GeneroMascota.Nino;

        private static readonly Color C_FONDO = Color.FromArgb(245, 240, 255);
        private static readonly Color C_ROSA = Color.FromArgb(255, 150, 190);
        private static readonly Color C_VERDE = Color.FromArgb(160, 230, 180);
        private static readonly Color C_AZUL = Color.FromArgb(150, 200, 255);
        private static readonly Color C_MORADO = Color.FromArgb(180, 130, 255);
        private static readonly Color C_TEXTO = Color.FromArgb(80, 50, 100);

        public FormConfigTamagotchi()
        {
            Text = "🐾 ¡Crea tu mascota!";
            ClientSize = new Size(420, 520);
            BackColor = C_FONDO;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Consolas", 10f);

            // Título
            var lblTitulo = new Label
            {
                Text = "🌸 TAMAGOTCHI 🌸",
                Font = new Font("Consolas", 16f, FontStyle.Bold),
                ForeColor = C_ROSA,
                AutoSize = true,
                Location = new Point(75, 18)
            };

            // Nombre
            var lblNombre = new Label
            {
                Text = "Nombre de tu mascota:",
                ForeColor = C_TEXTO,
                AutoSize = true,
                Font = new Font("Consolas", 10f, FontStyle.Bold),
                Location = new Point(30, 65)
            };
            txtNombre = new TextBox
            {
                Location = new Point(30, 88),
                Size = new Size(360, 30),
                Font = new Font("Consolas", 12f),
                BackColor = Color.White,
                ForeColor = C_TEXTO,
                MaxLength = 18,
                Text = "Buddy"
            };
            txtNombre.BorderStyle = BorderStyle.FixedSingle;

            // Mascota
            var lblMascota = new Label
            {
                Text = "Elige tu mascota:",
                ForeColor = C_TEXTO,
                AutoSize = true,
                Font = new Font("Consolas", 10f, FontStyle.Bold),
                Location = new Point(30, 130)
            };

            panelMascota = new Panel
            {
                Location = new Point(30, 155),
                Size = new Size(360, 100),
                BackColor = Color.Transparent
            };

            var mascotas = new (string emoji, string nombre, TipoMascota tipo)[]
            {
                ("🐶", "Perro", TipoMascota.Perro),
                ("🐱", "Gato",  TipoMascota.Gato),
                ("🐥", "Pato",  TipoMascota.Pato),
                ("🦊", "Zorro", TipoMascota.Zorro)
            };

            Button[] btnsMascota = new Button[4];
            for (int i = 0; i < 4; i++)
            {
                int idx = i;
                var btn = new Button
                {
                    Text = mascotas[i].emoji + "\n" + mascotas[i].nombre,
                    Size = new Size(82, 80),
                    Location = new Point(i * 90, 0),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = i == 0 ? C_VERDE : Color.White,
                    ForeColor = C_TEXTO,
                    Font = new Font("Segoe UI Emoji", 9f, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    Tag = mascotas[i].tipo
                };
                btn.FlatAppearance.BorderColor = C_MORADO;
                btn.FlatAppearance.BorderSize = 2;
                btn.Click += (s, e) =>
                {
                    tipoSelec = mascotas[idx].tipo;
                    foreach (Button b in btnsMascota)
                        b.BackColor = Color.White;
                    btn.BackColor = C_VERDE;
                };
                btnsMascota[i] = btn;
                panelMascota.Controls.Add(btn);
            }

            // Género
            var lblGenero = new Label
            {
                Text = "Género:",
                ForeColor = C_TEXTO,
                AutoSize = true,
                Font = new Font("Consolas", 10f, FontStyle.Bold),
                Location = new Point(30, 270)
            };
            panelGenero = new Panel
            {
                Location = new Point(30, 295),
                Size = new Size(360, 55),
                BackColor = Color.Transparent
            };

            var btnNino = new Button
            {
                Text = "♂  Niño",
                Size = new Size(170, 46),
                Location = new Point(0, 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = C_AZUL,
                ForeColor = C_TEXTO,
                Font = new Font("Consolas", 11f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnNino.FlatAppearance.BorderColor = C_MORADO;
            btnNino.FlatAppearance.BorderSize = 2;

            var btnNina = new Button
            {
                Text = "♀  Niña",
                Size = new Size(170, 46),
                Location = new Point(185, 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = C_TEXTO,
                Font = new Font("Consolas", 11f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnNina.FlatAppearance.BorderColor = C_MORADO;
            btnNina.FlatAppearance.BorderSize = 2;

            btnNino.Click += (s, e) =>
            {
                genSelec = GeneroMascota.Nino;
                btnNino.BackColor = C_AZUL;
                btnNina.BackColor = Color.White;
            };
            btnNina.Click += (s, e) =>
            {
                genSelec = GeneroMascota.Nina;
                btnNina.BackColor = C_ROSA;
                btnNino.BackColor = Color.White;
            };

            panelGenero.Controls.AddRange(new Control[] { btnNino, btnNina });

            // Vista previa dibujada
            var panelPreview = new PanelSuaveTamagotchi
            {
                Location = new Point(145, 365),
                Size = new Size(130, 100),
                BackColor = Color.Transparent
            };
            panelPreview.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var tmpMascota = new Mascota(
                    txtNombre.Text.Length > 0 ? txtNombre.Text : "?",
                    tipoSelec, genSelec);
                DibujadorMascota.Dibujar(e.Graphics, tmpMascota, 65, 85);
            };

            // Refrescar preview al cambiar nombre o mascota
            txtNombre.TextChanged += (s, e) => panelPreview.Invalidate();
            foreach (Button b in btnsMascota)
                b.Click += (s, e) => panelPreview.Invalidate();
            btnNino.Click += (s, e) => panelPreview.Invalidate();
            btnNina.Click += (s, e) => panelPreview.Invalidate();

            // Botón jugar
            btnJugar = new Button
            {
                Text = "▶  ¡JUGAR!",
                Location = new Point(110, 475),
                Size = new Size(200, 42),
                FlatStyle = FlatStyle.Flat,
                BackColor = C_ROSA,
                ForeColor = Color.White,
                Font = new Font("Consolas", 13f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnJugar.FlatAppearance.BorderColor = C_MORADO;
            btnJugar.FlatAppearance.BorderSize = 2;
            btnJugar.Click += (s, e) =>
            {
                NombreMascota = txtNombre.Text.Trim().Length > 0 ? txtNombre.Text.Trim() : "Buddy";
                Tipo = tipoSelec;
                Genero = genSelec;
                DialogResult = DialogResult.OK;
                Close();
            };

            Controls.AddRange(new Control[]
            {
                lblTitulo, lblNombre, txtNombre,
                lblMascota, panelMascota,
                lblGenero, panelGenero,
                panelPreview, btnJugar
            });
        }
    }

    // ─── Form principal del Tamagotchi ────────────────────────────────────────
    public class FormTamagotchi : Form
    {
        private Mascota mascota;
        private System.Windows.Forms.Timer timerJuego;

        private PanelSuaveTamagotchi panelJuego;
        private Button btnPausa;
        private Button btnReiniciar;
        private Label lblNombre;
        private Label lblMensaje;

        // Barras de estado
        private Label lblBarraHambre;
        private Label lblBarraEnergia;
        private Label lblBarraFelicidad;
        private ProgressBar barHambre;
        private ProgressBar barEnergia;
        private ProgressBar barFelicidad;

        // Botones de acción
        private Button btnComer;
        private Button btnDormir;
        private Button btnJugar;

        // Paleta pastel
        private static readonly Color C_FONDO = Color.FromArgb(235, 248, 255);
        private static readonly Color C_HUD = Color.FromArgb(210, 235, 255);
        private static readonly Color C_PANEL = Color.FromArgb(225, 255, 235);
        private static readonly Color C_ROSA = Color.FromArgb(255, 150, 190);
        private static readonly Color C_VERDE = Color.FromArgb(130, 210, 160);
        private static readonly Color C_AZUL = Color.FromArgb(100, 180, 255);
        private static readonly Color C_MORADO = Color.FromArgb(180, 130, 255);
        private static readonly Color C_TEXTO = Color.FromArgb(70, 50, 100);
        private static readonly Color C_NEON_ROSA = Color.FromArgb(255, 100, 160);

        private float timerParticulas = 0f;
        private readonly System.Collections.Generic.List<(float x, float y, float vy, Color c, float vida)> burbujas = new();
        private static readonly Random rng = new Random();

        // ── Constructor ───────────────────────────────────────────────────────
        public FormTamagotchi()
        {
            // Mostrar configuración primero
            using var cfg = new FormConfigTamagotchi();
            if (cfg.ShowDialog() != DialogResult.OK)
            {
                Close();
                return;
            }

            mascota = new Mascota(cfg.NombreMascota, cfg.Tipo, cfg.Genero);
            mascota.MascotaEscapo += (s, e) => MostrarEscapo();

            InicializarComponentes();

            timerJuego = new System.Windows.Forms.Timer { Interval = 33 };
            timerJuego.Tick += GameLoop;
            timerJuego.Start();

            panelJuego.Focus();
        }

        private void InicializarComponentes()
        {
            Text = $"🐾 {mascota.Nombre}";
            ClientSize = new Size(480, 620);
            BackColor = C_FONDO;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Consolas", 9f, FontStyle.Bold);

            // ── HUD superior ─────────────────────────────────────────────────
            var panelHud = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(480, 70),
                BackColor = C_HUD
            };
            panelHud.Paint += (s, e) =>
            {
                using var pen = new Pen(C_NEON_ROSA, 2f);
                using var glow = new Pen(Color.FromArgb(60, 255, 100, 160), 6f);
                e.Graphics.DrawLine(pen, 0, 69, 480, 69);
                e.Graphics.DrawLine(glow, 0, 68, 480, 68);
            };

            lblNombre = new Label
            {
                Text = mascota.Nombre.ToUpper(),
                ForeColor = C_TEXTO,
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 13f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(14, 10)
            };

            var lblEdad = new Label
            {
                Text = "🐾 TAMAGOTCHI",
                ForeColor = C_MORADO,
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 9f),
                AutoSize = true,
                Location = new Point(14, 40)
            };

            btnPausa = new Button
            {
                Text = "⏸ PAUSA",
                Location = new Point(260, 10),
                Size = new Size(100, 34),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = C_MORADO,
                Cursor = Cursors.Hand,
                Font = new Font("Consolas", 8.5f, FontStyle.Bold)
            };
            btnPausa.FlatAppearance.BorderColor = Color.FromArgb(150, 80, 230);
            btnPausa.FlatAppearance.BorderSize = 1;
            btnPausa.MouseEnter += (s, e) => btnPausa.BackColor = Color.FromArgb(210, 160, 255);
            btnPausa.MouseLeave += (s, e) => btnPausa.BackColor = C_MORADO;
            btnPausa.Click += (s, e) =>
            {
                if (timerJuego.Enabled) { timerJuego.Stop(); btnPausa.Text = "▶ SEGUIR"; }
                else { timerJuego.Start(); btnPausa.Text = "⏸ PAUSA"; }
                panelJuego.Focus();
            };

            btnReiniciar = new Button
            {
                Text = "↺ RESET",
                Location = new Point(370, 10),
                Size = new Size(100, 34),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = C_ROSA,
                Cursor = Cursors.Hand,
                Font = new Font("Consolas", 8.5f, FontStyle.Bold)
            };
            btnReiniciar.FlatAppearance.BorderColor = C_NEON_ROSA;
            btnReiniciar.FlatAppearance.BorderSize = 1;
            btnReiniciar.MouseEnter += (s, e) => btnReiniciar.BackColor = Color.FromArgb(255, 180, 210);
            btnReiniciar.MouseLeave += (s, e) => btnReiniciar.BackColor = C_ROSA;
            btnReiniciar.Click += (s, e) =>
            {
                using var cfg = new FormConfigTamagotchi();
                if (cfg.ShowDialog() == DialogResult.OK)
                {
                    mascota = new Mascota(cfg.NombreMascota, cfg.Tipo, cfg.Genero);
                    mascota.MascotaEscapo += (s2, e2) => MostrarEscapo();
                    lblNombre.Text = mascota.Nombre.ToUpper();
                    btnPausa.Text = "⏸ PAUSA";
                    btnPausa.Enabled = true;
                    timerJuego.Start();
                    burbujas.Clear();
                }
                panelJuego.Focus();
            };

            panelHud.Controls.AddRange(new Control[] { lblNombre, lblEdad, btnPausa, btnReiniciar });

            // ── Panel de juego ────────────────────────────────────────────────
            panelJuego = new PanelSuaveTamagotchi
            {
                Location = new Point(0, 70),
                Size = new Size(480, 280),
                BackColor = C_PANEL
            };
            panelJuego.Paint += PanelJuego_Paint;

            // ── Mensaje de estado ─────────────────────────────────────────────
            lblMensaje = new Label
            {
                Text = "",
                ForeColor = C_TEXTO,
                BackColor = Color.FromArgb(200, 255, 255, 255),
                Font = new Font("Consolas", 10f, FontStyle.Bold),
                AutoSize = false,
                Size = new Size(480, 30),
                Location = new Point(0, 350),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // ── Barras de estadísticas ────────────────────────────────────────
            var panelStats = new Panel
            {
                Location = new Point(0, 385),
                Size = new Size(480, 110),
                BackColor = Color.FromArgb(240, 250, 255)
            };

            (string emoji, string label, Color color)[] stats =
            {
                ("🍎", "HAMBRE",    C_ROSA),
                ("⚡", "ENERGÍA",   C_AZUL),
                ("🌸", "FELICIDAD", C_VERDE)
            };

            var bars = new ProgressBar[3];
            var barLabels = new Label[3];

            for (int i = 0; i < 3; i++)
            {
                int idx = i;
                barLabels[i] = new Label
                {
                    Text = stats[i].emoji + " " + stats[i].label,
                    ForeColor = C_TEXTO,
                    BackColor = Color.Transparent,
                    Font = new Font("Consolas", 9f, FontStyle.Bold),
                    AutoSize = true,
                    Location = new Point(16, 8 + i * 32)
                };
                bars[i] = new ProgressBar
                {
                    Location = new Point(130, 8 + i * 32),
                    Size = new Size(300, 20),
                    Minimum = 0,
                    Maximum = 100,
                    Value = 80,
                    Style = ProgressBarStyle.Continuous
                };
                // Color de la barra con la paleta
                bars[i].ForeColor = stats[i].color;
                panelStats.Controls.Add(barLabels[i]);
                panelStats.Controls.Add(bars[i]);
            }

            barHambre = bars[0];
            barEnergia = bars[1];
            barFelicidad = bars[2];

            // ── Botones de acción ─────────────────────────────────────────────
            var panelAcciones = new Panel
            {
                Location = new Point(0, 500),
                Size = new Size(480, 118),
                BackColor = C_HUD
            };
            panelAcciones.Paint += (s, e) =>
            {
                using var pen = new Pen(C_NEON_ROSA, 2f);
                e.Graphics.DrawLine(pen, 0, 0, 480, 0);
            };

            (string texto, Color color, Action accion)[] acciones =
            {
                ("🍎\nCOMER",    C_ROSA,   () => mascota.AccionComer()),
                ("💤\nDORMIR",  C_AZUL,   () => mascota.AccionDormir()),
                ("🎮\nJUGAR",   C_VERDE,  () => mascota.AccionJugar())
            };

            for (int i = 0; i < 3; i++)
            {
                int idx = i;
                Color colorBtn = acciones[i].color;
                var btn = new Button
                {
                    Text = acciones[i].texto,
                    Location = new Point(20 + i * 150, 12),
                    Size = new Size(130, 88),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = colorBtn,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI Emoji", 13f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderColor = C_MORADO;
                btn.FlatAppearance.BorderSize = 2;
                btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Light(colorBtn, 0.3f);
                btn.MouseLeave += (s, e) => btn.BackColor = colorBtn;
                btn.Click += (s, e) =>
                {
                    acciones[idx].accion();
                    panelJuego.Focus();
                };
                panelAcciones.Controls.Add(btn);
                if (i == 0) btnComer = btn;
                if (i == 1) btnDormir = btn;
                if (i == 2) btnJugar = btn;
            }

            Controls.AddRange(new Control[]
            {
                panelHud, panelJuego, lblMensaje, panelStats, panelAcciones
            });
        }

        // ── Game loop ─────────────────────────────────────────────────────────
        private DateTime ultimoFrame = DateTime.Now;

        private void GameLoop(object? sender, EventArgs e)
        {
            var ahora = DateTime.Now;
            float delta = (float)(ahora - ultimoFrame).TotalSeconds;
            ultimoFrame = ahora;
            delta = Math.Min(delta, 0.05f);

            mascota.Actualizar(delta);

            // Actualizar barras
            barHambre.Value = (int)Math.Clamp(mascota.Stats.Hambre, 0, 100);
            barEnergia.Value = (int)Math.Clamp(mascota.Stats.Energia, 0, 100);
            barFelicidad.Value = (int)Math.Clamp(mascota.Stats.Felicidad, 0, 100);

            // Mensaje
            lblMensaje.Text = mascota.ObtenerMensaje();

            // Burbujas decorativas
            timerParticulas += delta;
            if (timerParticulas >= 0.4f)
            {
                timerParticulas = 0f;
                Color[] coloresBurbuja = { C_ROSA, C_AZUL, C_VERDE, C_MORADO };
                burbujas.Add((
                    rng.Next(20, 460),
                    290f,
                    rng.NextSingle() * 30f + 20f,
                    coloresBurbuja[rng.Next(coloresBurbuja.Length)],
                    1.5f));
            }

            for (int i = burbujas.Count - 1; i >= 0; i--)
            {
                var b = burbujas[i];
                float vida = b.vida - delta;
                if (vida <= 0 || b.y < 70) { burbujas.RemoveAt(i); continue; }
                burbujas[i] = (b.x, b.y - b.vy * delta, b.vy, b.c, vida);
            }

            panelJuego.Invalidate();
        }

        // ── Pintar panel de juego ─────────────────────────────────────────────
        private void PanelJuego_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Fondo degradado pastel
            using var fondo = new LinearGradientBrush(
                new Point(0, 0), new Point(0, panelJuego.Height),
                Color.FromArgb(220, 245, 255),
                Color.FromArgb(235, 255, 235));
            g.FillRectangle(fondo, 0, 0, panelJuego.Width, panelJuego.Height);

            // Suelo
            using var brSuelo = new SolidBrush(Color.FromArgb(180, 230, 200));
            g.FillRectangle(brSuelo, 0, panelJuego.Height - 30, panelJuego.Width, 30);
            using var penSuelo = new Pen(Color.FromArgb(140, 200, 160), 2f);
            g.DrawLine(penSuelo, 0, panelJuego.Height - 30, panelJuego.Width, panelJuego.Height - 30);

            // Nubes decorativas
            DibujarNube(g, 60, 35, 0.8f);
            DibujarNube(g, 340, 20, 1.0f);
            DibujarNube(g, 190, 50, 0.6f);

            // Burbujas
            foreach (var (bx, by, _, bc, bvida) in burbujas)
            {
                int alpha = (int)(bvida / 1.5f * 130);
                using var brB = new SolidBrush(Color.FromArgb(Math.Clamp(alpha, 0, 130), bc));
                using var penB = new Pen(Color.FromArgb(Math.Clamp(alpha + 40, 0, 200), bc), 1.5f);
                g.FillEllipse(brB, bx - 8, by - 8, 16, 16);
                g.DrawEllipse(penB, bx - 8, by - 8, 16, 16);
            }

            // Mascota
            int mascX = mascota.Animo == EstadoAnimo.Escapando
                ? (int)mascota.EscapeX
                : panelJuego.Width / 2;
            int mascY = panelJuego.Height - 50;

            if (mascota.EscapeVisible)
                DibujadorMascota.Dibujar(e.Graphics, mascota, mascX, mascY);

            // Overlay pausa
            if (!timerJuego.Enabled)
                DibujarOverlay(g, "⏸  PAUSADO", "Presiona PAUSA para continuar",
                    Color.FromArgb(130, 200, 220, 255),
                    Color.FromArgb(100, 60, 200));

            // Overlay escapado
            if (mascota.Animo == EstadoAnimo.Escapando && !mascota.EscapeVisible)
                DibujarOverlay(g, "🚨 ¡SE ESCAPÓ!",
                    $"¡{mascota.Nombre} se fue de aburrimiento!\nPresiona RESET para empezar de nuevo",
                    Color.FromArgb(150, 255, 200, 220),
                    C_NEON_ROSA);
        }

        private static void DibujarNube(Graphics g, int cx, int cy, float escala)
        {
            using var br = new SolidBrush(Color.FromArgb(200, 255, 255, 255));
            int r = (int)(30 * escala);
            g.FillEllipse(br, cx - r, cy - r / 2, r * 2, r);
            g.FillEllipse(br, cx - r / 2, cy - r, (int)(r * 1.4f), r);
            g.FillEllipse(br, cx + r / 2 - 5, cy - r / 2, r, r);
        }

        private void DibujarOverlay(Graphics g, string titulo, string subtitulo,
                                     Color colorFondo, Color colorTitulo)
        {
            using var fondo = new SolidBrush(colorFondo);
            g.FillRectangle(fondo, 0, 0, panelJuego.Width, panelJuego.Height);

            int rw = 420, rh = 160;
            int rx = (panelJuego.Width - rw) / 2;
            int ry = (panelJuego.Height - rh) / 2;

            using var caja = new SolidBrush(Color.FromArgb(220, 255, 250, 255));
            using var borde = new Pen(C_NEON_ROSA, 2.5f);
            using var glow = new Pen(Color.FromArgb(80, 255, 100, 180), 7f);

            g.FillRectangle(caja, rx, ry, rw, rh);
            g.DrawRectangle(borde, rx, ry, rw, rh);
            g.DrawRectangle(glow, rx, ry, rw, rh);

            var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Near
            };

            using var fT = new Font("Consolas", 20f, FontStyle.Bold);
            using var bT = new SolidBrush(colorTitulo);
            g.DrawString(titulo, fT, bT,
                new RectangleF(rx, ry + 14, rw, 55), sf);

            using var fS = new Font("Consolas", 9.5f);
            using var bS = new SolidBrush(C_TEXTO);
            g.DrawString(subtitulo, fS, bS,
                new RectangleF(rx, ry + 75, rw, 80), sf);
        }

        private void MostrarEscapo()
        {
            timerJuego.Stop();
            btnPausa.Enabled = false;
            panelJuego.Invalidate();
        }
    }
}