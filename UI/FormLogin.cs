using System.Drawing.Drawing2D;

namespace BurdiGames
{
    public partial class FormLogin : Form
    {
        // Colores de la paleta cyberpunk extraídos de image_fa5fe0.png
        private Color colorFondo = Color.FromArgb(12, 16, 28);
        private Color colorFondoCampos = Color.FromArgb(40, 42, 44);
        private Color colorVerdeNeon = Color.FromArgb(0, 255, 127); // SpringGreen
        private Color colorTextoGris = Color.FromArgb(120, 120, 125);

        public FormLogin()
        {
            InitializeComponent();
            ConfigurarInterfaz();
        }

        private void ConfigurarInterfaz()
        {
            // Configuración del Formulario Base
            this.BackColor = colorFondo;
            this.Size = new Size(820, 520);
            this.Text = "BURDIGAMES.EXE";
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.StartPosition = FormStartPosition.CenterScreen;

            // --- CONFIGURACIÓN DE CAJAS DE TEXTO ---
            // Recuerda poner estos mismos nombres en la propiedad (Name) en el diseñador
            txtUsuario.BackColor = colorFondoCampos;
            txtUsuario.ForeColor = Color.White;
            txtUsuario.BorderStyle = BorderStyle.None;
            txtUsuario.Font = new Font("Consolas", 14F, FontStyle.Regular);
            txtUsuario.Text = "";

            txtPassword.BackColor = colorFondoCampos;
            txtPassword.ForeColor = Color.White;
            txtPassword.BorderStyle = BorderStyle.None;
            txtPassword.Font = new Font("Consolas", 14F, FontStyle.Regular);
            txtPassword.PasswordChar = '•';
            txtPassword.Text = "";

            // --- CONFIGURACIÓN DE BOTONES ---
            ConfigurarBotonPersonalizado(btnIniciarSesion, "▶  INICIAR\n    SESIÓN");
            ConfigurarBotonPersonalizado(btnRegistrarte, "¿ No tienes cuenta ?\n? Regístrate");
        }

        private void ConfigurarBotonPersonalizado(Button btn, string texto)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = colorFondo;
            btn.ForeColor = Color.White;
            btn.Font = new Font("Consolas", 11F, FontStyle.Bold);
            btn.Text = texto;
            btn.Cursor = Cursors.Hand;

            // Evento para dibujar el borde redondeado estilizado
            btn.Paint += Boton_Paint;
        }

        // Dibuja los bordes redondeados finos como se ve en la interfaz de image_fa5fe0.png
        private void Boton_Paint(object sender, PaintEventArgs e)
        {
            Button btn = (Button)sender;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            int borderRadius = 15;
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(0, 0, borderRadius, borderRadius, 180, 90);
                path.AddArc(btn.Width - borderRadius - 1, 0, borderRadius, borderRadius, 270, 90);
                path.AddArc(btn.Width - borderRadius - 1, btn.Height - borderRadius - 1, borderRadius, borderRadius, 0, 90);
                path.AddArc(0, btn.Height - borderRadius - 1, borderRadius, borderRadius, 90, 90);
                path.CloseAllFigures();

                // Borde gris sutil
                using (Pen pen = new Pen(Color.FromArgb(60, 65, 75), 1.5f))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }

            // Centrado correcto del texto
            TextRenderer.DrawText(e.Graphics, btn.Text, btn.Font, btn.ClientRectangle, btn.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        // Dibuja la línea decorativa verde superior y el divisor central
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Línea verde superior
            using (Pen penVerde = new Pen(colorVerdeNeon, 2))
            {
                e.Graphics.DrawLine(penVerde, 0, 1, this.Width, 1);
            }

            // Línea divisoria vertical
            int xDivision = 520;
            using (Pen penDivisoria = new Pen(Color.FromArgb(35, 40, 50), 1))
            {
                e.Graphics.DrawLine(penDivisoria, xDivision, 0, xDivision, this.Height);
            }

            // Indicador verde debajo del texto LOGIN
            using (Pen penLogin = new Pen(colorVerdeNeon, 3))
            {
                e.Graphics.DrawLine(penLogin, 550, 115, 660, 115);
            }
        }

        private void btnRegistrate_Click(object sender, EventArgs e)
        {
            // Validar primero que el usuario no haya dejado los campos en blanco
            if (string.IsNullOrWhiteSpace(txtUsuario.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Por favor, rellena todos los campos antes de registrarte.", "Campos vacíos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Aquí el sistema "coge" de la pantalla las credenciales REALES escritas por el usuario
            string nuevoUsuario = txtUsuario.Text;
            string nuevaPassword = txtPassword.Text;

            // A partir de aquí, guardas estas variables en tu base de datos, archivo de texto, etc.
            // Por ejemplo, un mensaje de confirmación mostrando lo que "cogió" el programa:
            MessageBox.Show($"¡Cuenta creada con éxito!\nUsuario registrado: {nuevoUsuario}", "Registro Completado", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Opcional: Limpiar las cajas después de registrarse exitosamente
            txtUsuario.Clear();
            txtPassword.Clear();
        }
    }
}
