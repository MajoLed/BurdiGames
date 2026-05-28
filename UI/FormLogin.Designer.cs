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
            txtUsuario = new TextBox();
            txtPassword = new TextBox();
            btnIniciarSesion = new Button();
            btnRegistrarte = new Button();
            lblTitulo = new Label();
            lblUsuario = new Label();
            lblPassword = new Label();
            lblError = new Label();
            pnlIzquierdo = new Panel();
            pnlDerecho = new Panel();

            SuspendLayout();

            // ── Form ──────────────────────────────────────────
            ClientSize = new Size(760, 440);
            Text = "BURDIGAMES.EXE";
            BackColor = Color.FromArgb(12, 16, 28);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;
            Name = "FormLogin";

            // ── Panel izquierdo (logo) ────────────────────────
            pnlIzquierdo.BackColor = Color.FromArgb(12, 16, 28);
            pnlIzquierdo.Location = new Point(0, 0);
            pnlIzquierdo.Size = new Size(360, 440);

            lblTitulo.Text = "BURDIGAMES";
            lblTitulo.Font = new Font("Consolas", 22f, FontStyle.Bold);
            lblTitulo.ForeColor = Color.FromArgb(180, 0, 0);
            lblTitulo.AutoSize = false;
            lblTitulo.Size = new Size(340, 50);
            lblTitulo.Location = new Point(10, 180);
            lblTitulo.TextAlign = ContentAlignment.MiddleCenter;

            pnlIzquierdo.Controls.Add(lblTitulo);

            // ── Panel derecho (formulario) ────────────────────
            pnlDerecho.BackColor = Color.FromArgb(13, 13, 31);
            pnlDerecho.Location = new Point(380, 0);
            pnlDerecho.Size = new Size(361, 440);

            // Label USUARIO
            lblUsuario.Text = "USUARIO";
            lblUsuario.Font = new Font("Consolas", 9f);
            lblUsuario.ForeColor = Color.FromArgb(120, 120, 125);
            lblUsuario.AutoSize = false;
            lblUsuario.Size = new Size(300, 18);
            lblUsuario.Location = new Point(40, 100);

            // TextBox usuario
            txtUsuario.BackColor = Color.FromArgb(40, 42, 44);
            txtUsuario.ForeColor = Color.White;
            txtUsuario.BorderStyle = BorderStyle.None;
            txtUsuario.Font = new Font("Consolas", 13f);
            txtUsuario.Size = new Size(300, 28);
            txtUsuario.Location = new Point(40, 122);

            // Label CONTRASEÑA
            lblPassword.Text = "CONTRASEÑA";
            lblPassword.Font = new Font("Consolas", 9f);
            lblPassword.ForeColor = Color.FromArgb(120, 120, 125);
            lblPassword.AutoSize = false;
            lblPassword.Size = new Size(300, 18);
            lblPassword.Location = new Point(40, 175);

            // TextBox password
            txtPassword.BackColor = Color.FromArgb(40, 42, 44);
            txtPassword.ForeColor = Color.White;
            txtPassword.BorderStyle = BorderStyle.None;
            txtPassword.Font = new Font("Consolas", 13f);
            txtPassword.Size = new Size(300, 28);
            txtPassword.Location = new Point(40, 197);
            txtPassword.PasswordChar = '•';

            // Label error
            lblError.Text = "";
            lblError.Font = new Font("Consolas", 8f);
            lblError.ForeColor = Color.FromArgb(220, 50, 50);
            lblError.AutoSize = false;
            lblError.Size = new Size(300, 18);
            lblError.Location = new Point(40, 240);

            // Botón iniciar sesión
            btnIniciarSesion.Text = "▶  INICIAR SESIÓN";
            btnIniciarSesion.Font = new Font("Consolas", 11f, FontStyle.Bold);
            btnIniciarSesion.ForeColor = Color.White;
            btnIniciarSesion.BackColor = Color.FromArgb(12, 16, 28);
            btnIniciarSesion.FlatStyle = FlatStyle.Flat;
            btnIniciarSesion.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 127);
            btnIniciarSesion.FlatAppearance.BorderSize = 1;
            btnIniciarSesion.Size = new Size(300, 45);
            btnIniciarSesion.Location = new Point(40, 270);
            btnIniciarSesion.Cursor = Cursors.Hand;
            btnIniciarSesion.Click += new EventHandler(btnIniciarSesion_Click);

            // Botón registrarse
            btnRegistrarte.Text = " ¿No tienes cuenta? REGISTRATE  ";
            btnRegistrarte.Font = new Font("Consolas", 11f, FontStyle.Bold);
            btnRegistrarte.ForeColor = Color.FromArgb(120, 120, 125);
            btnRegistrarte.BackColor = Color.FromArgb(12, 16, 28);
            btnRegistrarte.FlatStyle = FlatStyle.Flat;
            btnRegistrarte.FlatAppearance.BorderColor = Color.WhiteSmoke;
            btnRegistrarte.FlatAppearance.BorderSize = 1;
            btnRegistrarte.Size = new Size(300, 55);
            btnRegistrarte.Location = new Point(40, 328);
            btnRegistrarte.Cursor = Cursors.Hand;
            btnRegistrarte.Click += new EventHandler(btnRegistrarte_Click);

            pnlDerecho.Controls.AddRange(new Control[]
            {
                lblUsuario, txtUsuario,
                lblPassword, txtPassword,
                lblError,
                btnIniciarSesion, btnRegistrarte
            });

            // ── Agregar al form ───────────────────────────────
            Controls.AddRange(new Control[] { pnlIzquierdo, pnlDerecho });

            ResumeLayout(false);
        }

        #endregion

        // ── Declaración de controles ──────────────────────────
        private Panel pnlIzquierdo;
        private Panel pnlDerecho;
        private Label lblTitulo;
        private Label lblUsuario;
        private Label lblPassword;
        private Label lblError;
        private TextBox txtUsuario;
        private TextBox txtPassword;
        private Button btnIniciarSesion;
        private Button btnRegistrarte;
    }

}



