using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BurdiGames.Clases;
using System.Drawing.Drawing2D;

namespace BurdiGames.GUI
{
    public partial class FormPerfilUsuario : Form
    {
        private readonly Usuario _usuario;

        #region Colores estilo FormMain

        // Colores compartidos con FormMain
        private Color colorFondo = Color.FromArgb(8, 6, 20);
        private Color colorSidebar = Color.FromArgb(12, 8, 28);
        private Color colorNeon = Color.FromArgb(0, 255, 127);
        private Color colorNeonCian = Color.FromArgb(0, 220, 255);
        private Color colorNeonRosa = Color.FromArgb(220, 50, 180);
        private Color colorMorado = Color.FromArgb(180, 60, 255);
        private Color colorCard = Color.FromArgb(20, 12, 40);
        private Color colorCardBorde = Color.FromArgb(60, 30, 100);
        private Color colorTexto = Color.FromArgb(200, 190, 230);
        private Color colorGris = Color.FromArgb(90, 80, 120);

        #endregion

        // Logros definidos (se pueden mover a ServicioLogros después)
        private List<Logro> _logros = new();

        public FormPerfilUsuario(Usuario usuario)
        {
            InitializeComponent();
            _usuario = usuario;

            ConfigurarEstilo();
            InicializarLogros();
            CargarDatosSidebar();
            CargarLogros();
            CargarHistorial();
        }


        private void ConfigurarEstilo()
        {
            // Avatar circular
            picAvatar.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var brush = new SolidBrush(Color.FromArgb(40, 20, 70));
                e.Graphics.FillEllipse(brush, 0, 0, picAvatar.Width - 1, picAvatar.Height - 1);
                using var pen = new Pen(colorNeon, 2f);
                e.Graphics.DrawEllipse(pen, 1, 1, picAvatar.Width - 3, picAvatar.Height - 3);

                // Ícono gamepad simple en el centro
                using var penIcon = new Pen(colorNeon, 1.5f);
                var cx = picAvatar.Width / 2;
                var cy = picAvatar.Height / 2;
                e.Graphics.DrawRectangle(penIcon, cx - 16, cy - 10, 32, 20);
                e.Graphics.DrawEllipse(penIcon, cx - 4, cy - 4, 8, 8);
                e.Graphics.DrawLine(penIcon, cx - 22, cy, cx - 16, cy);
                e.Graphics.DrawLine(penIcon, cx + 16, cy, cx + 22, cy);
            };

            // Línea neón superior
            Paint += (s, e) =>
            {
                using var brush = new LinearGradientBrush(
                    new Point(0, 0), new Point(Width, 0),
                    colorNeonRosa, colorNeonCian);
                using var pen = new Pen(brush, 2f);
                e.Graphics.DrawLine(pen, 0, 1, Width, 1);
            };

            // Separador sidebar/contenido
            pnlSidebar.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(40, 30, 60), 1f);
                e.Graphics.DrawLine(pen, pnlSidebar.Width - 1, 0, pnlSidebar.Width - 1, pnlSidebar.Height);
            };
        }

        // Tabs con estilo igual al FormLogin
        private void TabPerfil_DrawItem(object sender, DrawItemEventArgs e)
        {
            bool activo = e.Index == tabPerfil.SelectedIndex;
            var bgColor = activo ? colorFondo : Color.FromArgb(14, 10, 30);
            var fgColor = activo ? colorNeon : colorGris;

            e.Graphics.FillRectangle(new SolidBrush(bgColor), e.Bounds);

            if (activo)
            {
                var line = new Rectangle(e.Bounds.X, e.Bounds.Bottom - 2, e.Bounds.Width, 2);
                e.Graphics.FillRectangle(new SolidBrush(colorNeon), line);
            }

            var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            e.Graphics.DrawString(
                tabPerfil.TabPages[e.Index].Text,
                new Font("Consolas", 8f, FontStyle.Bold),
                new SolidBrush(fgColor),
                e.Bounds, sf);
        }

        //Datos del login
        private void CargarDatosSidebar()
        {
            lblNombre.Text = _usuario.Nombre.ToUpper();
            lblEmail.Text = $"desde {_usuario.FechaRegistro:MMM yyyy}";

            int totalPartidas = _usuario.HistorialPartidas.Count;
            int logrosDesbloqueados = _logros.Count(l => l.Desbloqueado);
            int totalLogros = _logros.Count;
            int mejorPuntaje = _usuario.HistorialPartidas
                .Where(p => p.Puntaje.HasValue)
                .Select(p => p.Puntaje!.Value)
                .DefaultIfEmpty(0).Max();
            int juegosUnicos = _usuario.HistorialPartidas
                .Select(p => p.Juego.Nombre)
                .Distinct().Count();

            lblValPartidas.Text = totalPartidas.ToString();
            lblValLogros.Text = $"{logrosDesbloqueados}/{totalLogros}";
            lblValMejor.Text = mejorPuntaje > 0 ? $"{mejorPuntaje / 1000f:0.0}k" : "—";
            lblValJuegos.Text = juegosUnicos.ToString();
        }

        // ─────────────────────────────────────────────────────────────
        // LOGROS — definición y evaluación
        // ─────────────────────────────────────────────────────────────
        private void InicializarLogros()
        {
            _logros = new List<Logro>
            {
                new Logro { Nombre = "PRIMER JUEGO",   Descripcion = "Juega tu primera partida",        Icono = "🏆" },
                new Logro { Nombre = "VELOCISTA",      Descripcion = "Termina una partida en menos de 1 min", Icono = "⚡" },
                new Logro { Nombre = "CHICAGAMER",      Descripcion = "5 partidas seguidas",             Icono = "🔥" },
                new Logro { Nombre = "MAESTRO",        Descripcion = "Alcanza 50.000 pts en cualquier juego", Icono = "👑" },               
                new Logro { Nombre = "DUEÑA DEL BURDEL",        Descripcion = "Desbloquea todos los logros",     Icono = "💎" },
            };

            EvaluarLogros();
        }

        private void EvaluarLogros()
        {
            var partidas = _usuario.HistorialPartidas;

            // Primer juego
            if (partidas.Count >= 1)
                _logros.Find(l => l.Nombre == "PRIMER JUEGO")?.Desbloquear();

            // 5 partidas seguidas
            if (partidas.Count >= 5)
                _logros.Find(l => l.Nombre == "EN LLAMAS")?.Desbloquear();

            // Maestro: 50.000+ pts
            if (partidas.Any(p => p.Puntaje >= 50000))
                _logros.Find(l => l.Nombre == "MAESTRO")?.Desbloquear();

            // Leyenda: todos desbloqueados (excepto sí mismo)
            var sinLeyenda = _logros.Where(l => l.Nombre != "DUEÑA DEL BURDEL");
            if (sinLeyenda.All(l => l.Desbloqueado))
                _logros.Find(l => l.Nombre == "DUEÑA DEL BURDEL")?.Desbloquear();
        }

        // ─────────────────────────────────────────────────────────────
        // TARJETAS DE LOGROS
        // ─────────────────────────────────────────────────────────────
        private void CargarLogros()
        {
            flowLogros.Controls.Clear();
            foreach (var logro in _logros)
                flowLogros.Controls.Add(CrearTarjetaLogro(logro));
        }

        private Panel CrearTarjetaLogro(Logro logro)
        {
            bool desbloqueado = logro.Desbloqueado;

            var card = new Panel
            {
                Width = 265,
                Height = 88,
                BackColor = desbloqueado ? colorCard : Color.FromArgb(14, 10, 24),
                Margin = new Padding(8)
            };

            // Borde neón si está desbloqueado
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var borderColor = desbloqueado ? colorNeon : colorCardBorde;
                using var pen = new Pen(borderColor, 1.2f);
                e.Graphics.DrawRectangle(pen, 1, 1, card.Width - 3, card.Height - 3);

                if (desbloqueado)
                {
                    int c = 10;
                    using var penCorner = new Pen(colorNeonCian, 2f);
                    e.Graphics.DrawLine(penCorner, 1, 1, 1 + c, 1);
                    e.Graphics.DrawLine(penCorner, 1, 1, 1, 1 + c);
                    e.Graphics.DrawLine(penCorner, card.Width - 2 - c, 1, card.Width - 2, 1);
                    e.Graphics.DrawLine(penCorner, card.Width - 2, 1, card.Width - 2, 1 + c);
                }
            };

            // Ícono
            var lblIcono = new Label
            {
                Text = logro.Icono ?? "🎮",
                Font = new Font("Segoe UI Emoji", 18f),
                ForeColor = desbloqueado ? colorNeon : colorGris,
                AutoSize = false,
                Size = new Size(48, 48),
                Location = new Point(10, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblNombreLogro = new Label
            {
                Text = logro.Nombre,
                Font = new Font("Consolas", 9f, FontStyle.Bold),
                ForeColor = desbloqueado ? colorTexto : colorGris,
                AutoSize = false,
                Size = new Size(190, 18),
                Location = new Point(64, 16)
            };

            var lblDescLogro = new Label
            {
                Text = logro.Descripcion,
                Font = new Font("Consolas", 7f),
                ForeColor = desbloqueado ? Color.FromArgb(120, 110, 160) : Color.FromArgb(60, 55, 80),
                AutoSize = false,
                Size = new Size(190, 30),
                Location = new Point(64, 36)
            };

            var lblEstado = new Label
            {
                Text = desbloqueado ? "✔ DESBLOQUEADO" : "BLOQUEADO",
                Font = new Font("Consolas", 7f, FontStyle.Bold),
                ForeColor = desbloqueado ? colorNeon : Color.FromArgb(70, 60, 90),
                AutoSize = false,
                Size = new Size(190, 16),
                Location = new Point(64, 64)
            };

            card.Controls.AddRange(new Control[] { lblIcono, lblNombreLogro, lblDescLogro, lblEstado });
            return card;
        }

        // ─────────────────────────────────────────────────────────────
        // HISTORIAL
        // ─────────────────────────────────────────────────────────────
        private void CargarHistorial()
        {
            lstHistorial.Items.Clear();

            var partidas = _usuario.HistorialPartidas
                .OrderByDescending(p => p.Fecha)
                .ToList();

            if (partidas.Count == 0)
            {
                var item = new ListViewItem("—");
                item.SubItems.Add("Sin partidas registradas");
                item.SubItems.Add("—");
                item.ForeColor = colorGris;
                lstHistorial.Items.Add(item);
                return;
            }

            foreach (var partida in partidas)
            {
                var fecha = partida.Fecha?.ToString("dd/MM/yyyy HH:mm") ?? "—";
                var juego = partida.Juego?.Nombre ?? "—";
                var puntaje = partida.Puntaje.HasValue ? $"{partida.Puntaje:N0} pts" : "En progreso";

                var item = new ListViewItem(fecha);
                item.SubItems.Add(juego);
                item.SubItems.Add(puntaje);
                item.ForeColor = colorTexto;
                lstHistorial.Items.Add(item);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // EVENTOS
        // ─────────────────────────────────────────────────────────────
        private void btnEditarPerfil_Click(object sender, EventArgs e)
        {
            // Por ahora permite cambiar el nombre
            string nuevoNombre = Microsoft.VisualBasic.Interaction.InputBox(
                "Nuevo nombre de usuario:", "Editar perfil", _usuario.Nombre);

            if (!string.IsNullOrWhiteSpace(nuevoNombre) && nuevoNombre != _usuario.Nombre)
            {
                _usuario.CambiarNombre(nuevoNombre);
                lblNombre.Text = nuevoNombre.ToUpper();

                // Notifica a FormMain que el nombre cambió
                PerfilActualizado?.Invoke(this, EventArgs.Empty);
            }
        }

        // Evento que FormMain escucha para actualizar la bienvenida
        public event EventHandler? PerfilActualizado;

        // Método público para refrescar datos desde FormMain
        public void Refrescar()
        {
            CargarDatosSidebar();
            EvaluarLogros();
            CargarLogros();
            CargarHistorial();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using var brush = new LinearGradientBrush(
                new Point(0, 0), new Point(Width, 0),
                colorNeonRosa, colorNeonCian);
            using var pen = new Pen(brush, 2f);
            e.Graphics.DrawLine(pen, 0, 1, Width, 1);
        }
    }
}
