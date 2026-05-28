using BurdiGames.Clases;
using BurdiGames.GUI;
using BurdiGames.Servicios;
using System.Drawing.Drawing2D;

namespace BurdiGames
{
    public partial class FormLogin : Form
    {
        public FormLogin()
        {
            InitializeComponent();

            // Línea verde superior al cargar
            Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(0, 255, 127), 2);
                e.Graphics.DrawLine(pen, 0, 1, Width, 1);
                using var penDiv = new Pen(Color.FromArgb(35, 40, 50), 1);
                e.Graphics.DrawLine(penDiv, 360, 0, 360, Height);
            };
        }

        private void btnIniciarSesion_Click(object sender, EventArgs e)
        {
            string nombre = txtUsuario.Text.Trim();
            string pass = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(pass))
            {
                MessageBox.Show("Completa todos los campos.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var usuario = Program.BurdiGames.AutenticarUsuario(nombre, pass);

            if (usuario == null)
            {
                MessageBox.Show("Usuario o contraseña incorrectos.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ManejadorSesion.IniciarSesion(usuario);

            var formMain = new FormMain(usuario);
            formMain.Show();
            this.Hide();

            // Cuando FormMain se cierre, vuelve el login
            formMain.FormClosed += (s, args) =>
            {
                ManejadorSesion.CerrarSesion();
                this.Show();
            };
        }

        private void btnRegistrarte_Click(object sender, EventArgs e)
        {
            string nombre = txtUsuario.Text.Trim();
            string pass = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(pass))
            {
                MessageBox.Show("Completa todos los campos.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool ok = Program.BurdiGames.RegistrarUsuario(nombre, pass);

            if (!ok)
            {
                MessageBox.Show("Ese nombre de usuario ya existe.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show($"¡Cuenta creada! Ya puedes iniciar sesión, {nombre}.",
                "Registro exitoso", MessageBoxButtons.OK, MessageBoxIcon.Information);

            txtUsuario.Clear();
            txtPassword.Clear();
        }

        private void bgTxtContrasenia_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
