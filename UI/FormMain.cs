using BurdiGames.Clases;
using System.Drawing.Drawing2D;

namespace BurdiGames.GUI
{
    public partial class FormMain : Form
    {
        private readonly Usuario _usuario;

        private Color colorFondo = Color.FromArgb(8, 6, 20);
        private Color colorCard = Color.FromArgb(25, 15, 45);
        private Color colorCardBorde = Color.FromArgb(80, 40, 120);
        private Color colorNeon = Color.FromArgb(180, 60, 255);
        private Color colorNeonRosa = Color.FromArgb(220, 50, 180);
        private Color colorNeonCian = Color.FromArgb(0, 220, 255);
        private Color colorTexto = Color.FromArgb(220, 210, 255);
        private Color colorGris = Color.FromArgb(110, 90, 150);

        public FormMain(Usuario usuario)
        {
            InitializeComponent();
            _usuario = usuario;
            this.BackColor = colorFondo;
            ConfigurarVentana();
            CargarTarjetasJuegos();
        }

        private void ConfigurarVentana()
        {
            this.Text = "BURDIGAMES — Catálogo";
            this.Size = new Size(960, 620);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = colorFondo;

            // Título principal
            var lblTitulo = new Label
            {
                Text = "BURDIGAMES",
                ForeColor = colorNeon,
                Font = new Font("Consolas", 18f, FontStyle.Bold),
                AutoSize = true,
                Left = 20,
                Top = 14
            };

            // Bienvenida al usuario
            var lblBienvenida = new Label
            {
                Text = $"▸  {_usuario.Nombre}",
                ForeColor = colorNeonCian,
                Font = new Font("Consolas", 9f),
                AutoSize = true,
                Left = 24,
                Top = 46
            };

            // Botón logout
            var btnLogout = new Button
            {
                Text = "[ SALIR ]",
                Width = 100,
                Height = 32,
                Top = 18,
                FlatStyle = FlatStyle.Flat,
                BackColor = colorFondo,
                ForeColor = colorNeonRosa,
                Font = new Font("Consolas", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnLogout.Left = this.ClientSize.Width - 115;
            btnLogout.FlatAppearance.BorderColor = colorNeonRosa;
            btnLogout.FlatAppearance.BorderSize = 1;
            btnLogout.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { lblTitulo, lblBienvenida, btnLogout });
        }

        private void CargarTarjetasJuegos()
        {
            var panel = new FlowLayoutPanel
            {
                Left = 0,
                Top = 75,
                Width = this.ClientSize.Width,
                Height = this.ClientSize.Height - 75,
                BackColor = colorFondo,
                AutoScroll = true,
                Padding = new Padding(20),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            foreach (var juego in Program.BurdiGames.CatalogoJuegos)
                panel.Controls.Add(CrearTarjeta(juego));

            this.Controls.Add(panel);
        }

        private Panel CrearTarjeta(Juego juego)
        {
            var card = new Panel
            {
                Width = 210,
                Height = 155,
                BackColor = colorCard,
                Margin = new Padding(12),
                Cursor = Cursors.Hand
            };

            // Borde neón pintado a mano
            card.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var pen = new Pen(colorCardBorde, 1.5f);
                g.DrawRectangle(pen, 1, 1, card.Width - 3, card.Height - 3);

                // Esquinas brillantes
                using var penBright = new Pen(colorNeon, 2f);
                int c = 12; // largo de la esquina
                // top-left
                g.DrawLine(penBright, 1, 1, 1 + c, 1);
                g.DrawLine(penBright, 1, 1, 1, 1 + c);
                // top-right
                g.DrawLine(penBright, card.Width - 2 - c, 1, card.Width - 2, 1);
                g.DrawLine(penBright, card.Width - 2, 1, card.Width - 2, 1 + c);
                // bottom-left
                g.DrawLine(penBright, 1, card.Height - 2, 1 + c, card.Height - 2);
                g.DrawLine(penBright, 1, card.Height - 2 - c, 1, card.Height - 2);
                // bottom-right
                g.DrawLine(penBright, card.Width - 2 - c, card.Height - 2, card.Width - 2, card.Height - 2);
                g.DrawLine(penBright, card.Width - 2, card.Height - 2 - c, card.Width - 2, card.Height - 2);
            };

            // Franja superior de color por género
            var lblFranja = new Label
            {
                Width = 210,
                Height = 4,
                Top = 0,
                Left = 0,
                BackColor = ObtenerColorGenero(juego.Genero)
            };

            var lblNombre = new Label
            {
                Text = juego.Nombre,
                ForeColor = colorTexto,
                Font = new Font("Consolas", 10f, FontStyle.Bold),
                AutoSize = false,
                Width = 190,
                Height = 22,
                Top = 14,
                Left = 10
            };

            var lblGenero = new Label
            {
                Text = $"// {juego.Genero}",
                ForeColor = colorGris,
                Font = new Font("Consolas", 8f),
                AutoSize = false,
                Width = 190,
                Height = 16,
                Top = 38,
                Left = 10
            };

            var lblDesc = new Label
            {
                Text = juego.Descripcion,
                ForeColor = Color.FromArgb(140, 120, 180),
                Font = new Font("Consolas", 7f),
                AutoSize = false,
                Width = 190,
                Height = 32,
                Top = 58,
                Left = 10
            };

            var btnJugar = new Button
            {
                Text = "▶ JUGAR",
                Top = 112,
                Left = 10,
                Width = 92,
                Height = 28,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(50, 20, 80),
                ForeColor = colorNeon,
                Font = new Font("Consolas", 8f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnJugar.FlatAppearance.BorderColor = colorNeon;
            btnJugar.FlatAppearance.BorderSize = 1;
            btnJugar.Click += (s, e) =>
            {
                var partida = new Partida(_usuario, juego)
                {
                    Puntaje = 0,
                    Fecha = DateTime.Now
                };
                _usuario.HistorialPartidas.Add(partida);
                juego.Jugar();
            };

            var btnInfo = new Button
            {
                Text = "INFO",
                Top = 112,
                Left = 108,
                Width = 58,
                Height = 28,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(20, 10, 35),
                ForeColor = colorNeonCian,
                Font = new Font("Consolas", 8f),
                Cursor = Cursors.Hand
            };
            btnInfo.FlatAppearance.BorderColor = colorNeonCian;
            btnInfo.FlatAppearance.BorderSize = 1;
            btnInfo.Click += (s, e) =>
            {
                MessageBox.Show(
                    $"{juego.Nombre}\n\nGénero: {juego.Genero}\n\n{juego.Descripcion}",
                    "INFO", MessageBoxButtons.OK, MessageBoxIcon.None);
            };

            card.Controls.AddRange(new Control[] { lblFranja, lblNombre, lblGenero, lblDesc, btnJugar, btnInfo });
            return card;
        }

        // Cada género tiene su propio color de acento
        private Color ObtenerColorGenero(string genero)
        {
            return genero?.ToLower() switch
            {
                "arcade" => Color.FromArgb(0, 220, 255),   // cian
                "rpg" => Color.FromArgb(180, 60, 255),  // morado
                "simulación" => Color.FromArgb(255, 180, 0),   // ámbar
                "simulacion" => Color.FromArgb(255, 180, 0),
                "acción" => Color.FromArgb(255, 60, 100),  // rojo neón
                "accion" => Color.FromArgb(255, 60, 100),
                _ => Color.FromArgb(220, 50, 180),  // rosa por defecto
            };
        }

        // Línea de degradado superior
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using var brush = new LinearGradientBrush(
                new Point(0, 0), new Point(this.Width, 0),
                colorNeonRosa, colorNeonCian);
            using var pen = new Pen(brush, 2f);
            e.Graphics.DrawLine(pen, 0, 1, this.Width, 1);
        }
    }
}