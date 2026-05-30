namespace BurdiGames.GUI
{
    partial class FormPerfilUsuario
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Diseño

        private void InitializeComponent()
        {
            // ── Controles principales
            pnlSidebar = new Panel();
            pnlContenido = new Panel();
            picAvatar = new PictureBox();
            lblNombre = new Label();
            lblEmail = new Label();
            lblSepStats = new Label();
            lblStatPartidas = new Label();
            lblValPartidas = new Label();
            lblStatLogros = new Label();
            lblValLogros = new Label();
            lblStatMejor = new Label();
            lblValMejor = new Label();
            lblStatJuegos = new Label();
            lblValJuegos = new Label();
            btnEditarPerfil = new Button();
            tabPerfil = new TabControl();
            tabLogros = new TabPage();
            tabHistorial = new TabPage();
            flowLogros = new FlowLayoutPanel();
            lstHistorial = new ListView();
            colFecha = new ColumnHeader();
            colJuego = new ColumnHeader();
            colPuntaje = new ColumnHeader();

            SuspendLayout();

            // ── FORM ─────────────────────────────────────────────────
            ClientSize = new Size(820, 520);
            Text = "BURDIGAMES — PERFIL";
            BackColor = Color.FromArgb(8, 6, 20);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;

            // ── SIDEBAR ──────────────────────────────────────────────
            pnlSidebar.Location = new Point(0, 0);
            pnlSidebar.Size = new Size(200, 520);
            pnlSidebar.BackColor = Color.FromArgb(12, 8, 28);

            // Avatar circular (se recorta en código)
            picAvatar.Size = new Size(72, 72);
            picAvatar.Location = new Point(64, 28);
            picAvatar.BackColor = Color.FromArgb(40, 20, 70);
            picAvatar.SizeMode = PictureBoxSizeMode.Zoom;
            picAvatar.Cursor = Cursors.Hand;

            lblNombre.Text = "PLAYERONE";
            lblNombre.Font = new Font("Consolas", 11f, FontStyle.Bold);
            lblNombre.ForeColor = Color.FromArgb(0, 255, 127);
            lblNombre.AutoSize = false;
            lblNombre.Size = new Size(180, 22);
            lblNombre.Location = new Point(10, 112);
            lblNombre.TextAlign = ContentAlignment.MiddleCenter;

            lblEmail.Text = "player@mail.com";
            lblEmail.Font = new Font("Consolas", 7f);
            lblEmail.ForeColor = Color.FromArgb(90, 80, 120);
            lblEmail.AutoSize = false;
            lblEmail.Size = new Size(180, 16);
            lblEmail.Location = new Point(10, 136);
            lblEmail.TextAlign = ContentAlignment.MiddleCenter;

            // Separador
            lblSepStats.Text = "";
            lblSepStats.AutoSize = false;
            lblSepStats.Size = new Size(160, 1);
            lblSepStats.Location = new Point(20, 168);
            lblSepStats.BackColor = Color.FromArgb(40, 30, 60);

            // Stats
            void Stat(Label lbl, Label val, string texto, string valor, int top)
            {
                lbl.Text = texto;
                lbl.Font = new Font("Consolas", 7f);
                lbl.ForeColor = Color.FromArgb(100, 90, 130);
                lbl.AutoSize = false;
                lbl.Size = new Size(100, 16);
                lbl.Location = new Point(16, top);

                val.Text = valor;
                val.Font = new Font("Consolas", 9f, FontStyle.Bold);
                val.ForeColor = Color.FromArgb(0, 220, 255);
                val.AutoSize = false;
                val.Size = new Size(70, 16);
                val.Location = new Point(118, top);
                val.TextAlign = ContentAlignment.MiddleRight;
            }

            Stat(lblStatPartidas, lblValPartidas, "PARTIDAS", "0", 184);
            Stat(lblStatLogros, lblValLogros, "LOGROS", "0/0", 210);
            Stat(lblStatMejor, lblValMejor, "MEJOR", "—", 236);
            Stat(lblStatJuegos, lblValJuegos, "JUEGOS", "0", 262);

            // Botón editar
            btnEditarPerfil.Text = "EDITAR PERFIL";
            btnEditarPerfil.Font = new Font("Consolas", 8f, FontStyle.Bold);
            btnEditarPerfil.ForeColor = Color.FromArgb(0, 255, 127);
            btnEditarPerfil.BackColor = Color.FromArgb(12, 8, 28);
            btnEditarPerfil.FlatStyle = FlatStyle.Flat;
            btnEditarPerfil.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 127);
            btnEditarPerfil.FlatAppearance.BorderSize = 1;
            btnEditarPerfil.Size = new Size(160, 34);
            btnEditarPerfil.Location = new Point(20, 440);
            btnEditarPerfil.Cursor = Cursors.Hand;
            btnEditarPerfil.Click += new EventHandler(btnEditarPerfil_Click);

            pnlSidebar.Controls.AddRange(new Control[]
            {
                picAvatar, lblNombre, lblEmail, lblSepStats,
                lblStatPartidas, lblValPartidas,
                lblStatLogros,   lblValLogros,
                lblStatMejor,    lblValMejor,
                lblStatJuegos,   lblValJuegos,
                btnEditarPerfil
            });

            // ── CONTENIDO DERECHO ─────────────────────────────────────
            pnlContenido.Location = new Point(200, 0);
            pnlContenido.Size = new Size(620, 520);
            pnlContenido.BackColor = Color.FromArgb(8, 6, 20);

            // ── TABCONTROL ────────────────────────────────────────────
            tabPerfil.Location = new Point(10, 10);
            tabPerfil.Size = new Size(600, 490);
            tabPerfil.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabPerfil.ItemSize = new Size(100, 30);
            tabPerfil.SizeMode = TabSizeMode.Fixed;
            tabPerfil.BackColor = Color.FromArgb(8, 6, 20);
            tabPerfil.DrawItem += new DrawItemEventHandler(TabPerfil_DrawItem);

            tabLogros.Text = "LOGROS";
            tabLogros.BackColor = Color.FromArgb(8, 6, 20);
            tabHistorial.Text = "HISTORIAL";
            tabHistorial.BackColor = Color.FromArgb(8, 6, 20);

            // ── FLOW LOGROS ──
            flowLogros.Dock = DockStyle.Fill;
            flowLogros.BackColor = Color.FromArgb(8, 6, 20);
            flowLogros.AutoScroll = true;
            flowLogros.Padding = new Padding(10);

            tabLogros.Controls.Add(flowLogros);

            // ── LISTVIEW HISTORIAL ────────────────────────────────────
            lstHistorial.Dock = DockStyle.Fill;
            lstHistorial.BackColor = Color.FromArgb(12, 8, 28);
            lstHistorial.ForeColor = Color.FromArgb(180, 170, 210);
            lstHistorial.Font = new Font("Consolas", 9f);
            lstHistorial.FullRowSelect = true;
            lstHistorial.GridLines = false;
            lstHistorial.BorderStyle = BorderStyle.None;
            lstHistorial.View = View.Details;
            lstHistorial.HeaderStyle = ColumnHeaderStyle.Nonclickable;

            colFecha.Text = "FECHA";
            colFecha.Width = 150;
            colJuego.Text = "JUEGO";
            colJuego.Width = 260;
            colPuntaje.Text = "PUNTAJE";
            colPuntaje.Width = 160;

            lstHistorial.Columns.AddRange(new ColumnHeader[] { colFecha, colJuego, colPuntaje });
            tabHistorial.Controls.Add(lstHistorial);

            tabPerfil.TabPages.Add(tabLogros);
            tabPerfil.TabPages.Add(tabHistorial);
            pnlContenido.Controls.Add(tabPerfil);

            Controls.AddRange(new Control[] { pnlSidebar, pnlContenido });

            ResumeLayout(false);
        }

        #endregion

        // ── Declaración de controles ─────────────────────────────────
        private Panel pnlSidebar;
        private Panel pnlContenido;
        private PictureBox picAvatar;
        private Label lblNombre;
        private Label lblEmail;
        private Label lblSepStats;
        private Label lblStatPartidas, lblValPartidas;
        private Label lblStatLogros, lblValLogros;
        private Label lblStatMejor, lblValMejor;
        private Label lblStatJuegos, lblValJuegos;
        private Button btnEditarPerfil;
        private TabControl tabPerfil;
        private TabPage tabLogros;
        private TabPage tabHistorial;
        private FlowLayoutPanel flowLogros;
        private ListView lstHistorial;
        private ColumnHeader colFecha;
        private ColumnHeader colJuego;
        private ColumnHeader colPuntaje;
    }
}