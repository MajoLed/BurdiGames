namespace BurdiGames
{
    partial class FormLogin
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
            pnlIzquierdo = new Panel();
            pnlDerecho = new Panel();
            lblTitulo = new Label();
            tabModo = new TabControl();
            tabLogin = new TabPage();
            tabRegistro = new TabPage();

            // -- Tab Login controls
            lblUsuario = new Label();
            txtUsuario = new TextBox();
            lblPassword = new Label();
            txtPassword = new TextBox();
            btnIniciarSesion = new Button();
            lblError = new Label();

            // -- Tab Registro controls
            lblRegNombre = new Label();
            txtRegNombre = new TextBox();
            lblRegEmail = new Label();
            txtRegEmail = new TextBox();
            lblRegPass = new Label();
            txtRegPass = new TextBox();
            btnRegistrarte = new Button();
            lblRegMsg = new Label();

            SuspendLayout();

            // ── FORM ──────────────────────────────────────────────────
            ClientSize = new Size(760, 440);
            Text = "BURDIGAMES.EXE";
            BackColor = Color.FromArgb(12, 16, 28);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;

            // ── PANEL IZQUIERDO ───────────────────────────────────────
            pnlIzquierdo.BackColor = Color.FromArgb(12, 16, 28);
            pnlIzquierdo.Location = new Point(0, 0);
            pnlIzquierdo.Size = new Size(360, 440);

            lblTitulo.Text = "BURDIGAMES";
            lblTitulo.Font = new Font("Consolas", 22f, FontStyle.Bold);
            lblTitulo.ForeColor = Color.FromArgb(180, 0, 0);
            lblTitulo.AutoSize = false;
            lblTitulo.Size = new Size(340, 50);
            lblTitulo.Location = new Point(10, 195);
            lblTitulo.TextAlign = ContentAlignment.MiddleCenter;

            pnlIzquierdo.Controls.Add(lblTitulo);

            // ── PANEL DERECHO ─────────────────────────────────────────
            pnlDerecho.BackColor = Color.FromArgb(12, 16, 28);
            pnlDerecho.Location = new Point(380, 0);
            pnlDerecho.Size = new Size(380, 440);

            // ── TABCONTROL ────────────────────────────────────────────
            tabModo.Location = new Point(20, 60);
            tabModo.Size = new Size(340, 340);
            tabModo.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabModo.ItemSize = new Size(167, 40);
            tabModo.SizeMode = TabSizeMode.Fixed;
            tabModo.DrawItem += TabModo_DrawItem;

            tabLogin.Text = "LOGIN";
            tabLogin.BackColor = Color.FromArgb(12, 16, 28);
            tabRegistro.Text = "REGISTRO";
            tabRegistro.BackColor = Color.FromArgb(12, 16, 28);

            tabModo.TabPages.Add(tabLogin);
            tabModo.TabPages.Add(tabRegistro);

            // ── TAB LOGIN ─────────────────────────────────────────────
            lblUsuario.Text = "USUARIO";
            lblUsuario.Font = new Font("Consolas", 8f);
            lblUsuario.ForeColor = Color.FromArgb(120, 120, 125);
            lblUsuario.AutoSize = false;
            lblUsuario.Size = new Size(300, 16);
            lblUsuario.Location = new Point(16, 20);

            txtUsuario.BackColor = Color.FromArgb(40, 42, 54);
            txtUsuario.ForeColor = Color.White;
            txtUsuario.BorderStyle = BorderStyle.None;
            txtUsuario.Font = new Font("Consolas", 12f);
            txtUsuario.Size = new Size(300, 26);
            txtUsuario.Location = new Point(16, 40);

            lblPassword.Text = "CONTRASEÑA";
            lblPassword.Font = new Font("Consolas", 8f);
            lblPassword.ForeColor = Color.FromArgb(120, 120, 125);
            lblPassword.AutoSize = false;
            lblPassword.Size = new Size(300, 16);
            lblPassword.Location = new Point(16, 90);

            txtPassword.BackColor = Color.FromArgb(40, 42, 54);
            txtPassword.ForeColor = Color.White;
            txtPassword.BorderStyle = BorderStyle.None;
            txtPassword.Font = new Font("Consolas", 12f);
            txtPassword.Size = new Size(300, 26);
            txtPassword.Location = new Point(16, 110);
            txtPassword.PasswordChar = '•';

            lblError.Text = "";
            lblError.Font = new Font("Consolas", 8f);
            lblError.ForeColor = Color.FromArgb(220, 50, 50);
            lblError.AutoSize = false;
            lblError.Size = new Size(300, 16);
            lblError.Location = new Point(16, 152);

            btnIniciarSesion.Text = "▶  INICIAR SESIÓN";
            btnIniciarSesion.Font = new Font("Consolas", 10f, FontStyle.Bold);
            btnIniciarSesion.ForeColor = Color.White;
            btnIniciarSesion.BackColor = Color.FromArgb(12, 16, 28);
            btnIniciarSesion.FlatStyle = FlatStyle.Flat;
            btnIniciarSesion.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 127);
            btnIniciarSesion.FlatAppearance.BorderSize = 1;
            btnIniciarSesion.Size = new Size(300, 42);
            btnIniciarSesion.Location = new Point(16, 175);
            btnIniciarSesion.Cursor = Cursors.Hand;
            btnIniciarSesion.Click += new EventHandler(btnIniciarSesion_Click);

            tabLogin.Controls.AddRange(new Control[]
            {
                lblUsuario, txtUsuario,
                lblPassword, txtPassword,
                lblError, btnIniciarSesion
            });

            // ── TAB REGISTRO ──────────────────────────────────────────
            lblRegNombre.Text = "NOMBRE DE USUARIO";
            lblRegNombre.Font = new Font("Consolas", 8f);
            lblRegNombre.ForeColor = Color.FromArgb(120, 120, 125);
            lblRegNombre.AutoSize = false;
            lblRegNombre.Size = new Size(300, 16);
            lblRegNombre.Location = new Point(16, 16);

            txtRegNombre.BackColor = Color.FromArgb(40, 42, 54);
            txtRegNombre.ForeColor = Color.White;
            txtRegNombre.BorderStyle = BorderStyle.None;
            txtRegNombre.Font = new Font("Consolas", 12f);
            txtRegNombre.Size = new Size(300, 26);
            txtRegNombre.Location = new Point(16, 34);

            lblRegEmail.Text = "EMAIL";
            lblRegEmail.Font = new Font("Consolas", 8f);
            lblRegEmail.ForeColor = Color.FromArgb(120, 120, 125);
            lblRegEmail.AutoSize = false;
            lblRegEmail.Size = new Size(300, 16);
            lblRegEmail.Location = new Point(16, 74);

            txtRegEmail.BackColor = Color.FromArgb(40, 42, 54);
            txtRegEmail.ForeColor = Color.White;
            txtRegEmail.BorderStyle = BorderStyle.None;
            txtRegEmail.Font = new Font("Consolas", 12f);
            txtRegEmail.Size = new Size(300, 26);
            txtRegEmail.Location = new Point(16, 92);

            lblRegPass.Text = "CONTRASEÑA";
            lblRegPass.Font = new Font("Consolas", 8f);
            lblRegPass.ForeColor = Color.FromArgb(120, 120, 125);
            lblRegPass.AutoSize = false;
            lblRegPass.Size = new Size(300, 16);
            lblRegPass.Location = new Point(16, 132);

            txtRegPass.BackColor = Color.FromArgb(40, 42, 54);
            txtRegPass.ForeColor = Color.White;
            txtRegPass.BorderStyle = BorderStyle.None;
            txtRegPass.Font = new Font("Consolas", 12f);
            txtRegPass.Size = new Size(300, 26);
            txtRegPass.Location = new Point(16, 150);
            txtRegPass.PasswordChar = '•';

            lblRegMsg.Text = "";
            lblRegMsg.Font = new Font("Consolas", 8f);
            lblRegMsg.ForeColor = Color.FromArgb(0, 200, 100);
            lblRegMsg.AutoSize = false;
            lblRegMsg.Size = new Size(300, 16);
            lblRegMsg.Location = new Point(16, 190);

            btnRegistrarte.Text = "✚  REGISTRARSE";
            btnRegistrarte.Font = new Font("Consolas", 10f, FontStyle.Bold);
            btnRegistrarte.ForeColor = Color.White;
            btnRegistrarte.BackColor = Color.FromArgb(12, 16, 28);
            btnRegistrarte.FlatStyle = FlatStyle.Flat;
            btnRegistrarte.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 127);
            btnRegistrarte.FlatAppearance.BorderSize = 1;
            btnRegistrarte.Size = new Size(300, 42);
            btnRegistrarte.Location = new Point(16, 210);
            btnRegistrarte.Cursor = Cursors.Hand;
            btnRegistrarte.Click += new EventHandler(btnRegistrarte_Click);

            tabRegistro.Controls.AddRange(new Control[]
            {
                lblRegNombre, txtRegNombre,
                lblRegEmail,  txtRegEmail,
                lblRegPass,   txtRegPass,
                lblRegMsg,    btnRegistrarte
            });

            pnlDerecho.Controls.Add(tabModo);

            Controls.AddRange(new Control[] { pnlIzquierdo, pnlDerecho });

            ResumeLayout(false);
        }

        #endregion

        // ── Declaración de controles ────
        private Panel pnlIzquierdo;
        private Panel pnlDerecho;
        private Label lblTitulo;
        private TabControl tabModo;
        private TabPage tabLogin;
        private TabPage tabRegistro;
        private Label lblUsuario;
        private TextBox txtUsuario;
        private Label lblPassword;
        private TextBox txtPassword;
        private Button btnIniciarSesion;
        private Label lblError;
        private Label lblRegNombre;
        private TextBox txtRegNombre;
        private Label lblRegEmail;
        private TextBox txtRegEmail;
        private Label lblRegPass;
        private TextBox txtRegPass;
        private Button btnRegistrarte;
        private Label lblRegMsg;
    }

}



