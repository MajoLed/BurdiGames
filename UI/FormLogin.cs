using BurdiGames.Clases;
using BurdiGames.GUI;
using BurdiGames.Servicios;
using System.Drawing.Drawing2D;

namespace BurdiGames
{
    // Formulario de inicio de sesión y registro de usuarios del sistema BurdiGames
    public partial class FormLogin : Form
    {
        // Constructor: inicializa los componentes del formulario y suscribe el evento de pintura decorativa
        public FormLogin()
        {
            InitializeComponent(); // Inicializa todos los controles diseñados en el archivo .Designer.cs

            // Línea verde superior al cargar
            Paint += (s, e) => // Suscribe una lambda al evento Paint del formulario para dibujar líneas decorativas
            {
                using var pen = new Pen(Color.FromArgb(0, 255, 127), 2);          // Pluma verde neón de 2px para la línea superior
                e.Graphics.DrawLine(pen, 0, 1, Width, 1);                          // Dibuja la línea verde horizontal en la parte superior del formulario
                using var penDiv = new Pen(Color.FromArgb(35, 40, 50), 1);        // Pluma gris oscuro de 1px para la línea divisoria vertical
                e.Graphics.DrawLine(penDiv, 360, 0, 360, Height);                 // Dibuja la línea divisoria vertical que separa el panel izquierdo del derecho
            };
        }

        #region Diseño

        // Dibuja los botones con bordes redondeados usando un GraphicsPath personalizado
        private void Button_Paint(object sender, PaintEventArgs e)
        {
            Button btn = (Button)sender;   // Obtiene el botón que está siendo dibujado
            int borderRadius = 15;         // Radio de las esquinas redondeadas del botón en píxeles

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias; // Activa anti-aliasing para que los bordes redondeados sean suaves

            GraphicsPath path = new GraphicsPath(); // Crea un path gráfico para definir la forma redondeada del botón
            path.AddArc(0, 0, borderRadius, borderRadius, 180, 90);                                                // Agrega el arco de la esquina superior izquierda
            path.AddArc(btn.Width - borderRadius, 0, borderRadius, borderRadius, 270, 90);                        // Agrega el arco de la esquina superior derecha
            path.AddArc(btn.Width - borderRadius, btn.Height - borderRadius, borderRadius, borderRadius, 0, 90);  // Agrega el arco de la esquina inferior derecha
            path.AddArc(0, btn.Height - borderRadius, borderRadius, borderRadius, 90, 90);                        // Agrega el arco de la esquina inferior izquierda
            path.CloseAllFigures(); // Cierra el path conectando el último arco con el primero

            btn.Region = new Region(path); // Aplica la forma redondeada como región del botón (recorta su área visual)

            using (Pen pen = new Pen(btn.FlatAppearance.BorderColor, 2)) // Crea una pluma con el color de borde configurado en el botón y 2px de grosor
            {
                e.Graphics.DrawPath(pen, path); // Dibuja el borde redondeado del botón usando el path definido
            }
        }

        // Dibuja de forma personalizada las pestañas del TabControl de modos (Login / Registro)
        private void TabModo_DrawItem(object sender, DrawItemEventArgs e)
        {
            bool activo = e.Index == tabModo.SelectedIndex; // Determina si la pestaña actual es la seleccionada
            var bgColor = activo ? Color.FromArgb(12, 16, 28) : Color.FromArgb(20, 22, 35);        // Color de fondo: más oscuro si está activa, gris medio si no
            var fgColor = activo ? Color.FromArgb(0, 255, 127) : Color.FromArgb(100, 100, 110);    // Color del texto: verde neón si está activa, gris si no

            e.Graphics.FillRectangle(new SolidBrush(bgColor), e.Bounds); // Rellena el fondo de la pestaña con el color correspondiente

            if (activo) // Si la pestaña está activa, dibuja una línea verde en su borde inferior como indicador
            {
                var lineRect = new Rectangle(e.Bounds.X, e.Bounds.Bottom - 2, e.Bounds.Width, 2); // Rectángulo de 2px de alto en el borde inferior de la pestaña
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(0, 255, 127)), lineRect);  // Dibuja la línea indicadora verde neón
            }

            var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,     // Centra el texto horizontalmente en la pestaña
                LineAlignment = StringAlignment.Center  // Centra el texto verticalmente en la pestaña
            };
            e.Graphics.DrawString(
                tabModo.TabPages[e.Index].Text,                   // Texto de la pestaña (nombre de la página)
                new Font("Consolas", 9f, FontStyle.Bold),         // Fuente Consolas negrita 9pt para el texto de la pestaña
                new SolidBrush(fgColor),                          // Color del texto según si está activa o no
                e.Bounds, sf);                                    // Dibuja el texto centrado dentro de los límites de la pestaña
        }

        #endregion

        // Maneja el clic del botón de iniciar sesión: valida campos, autentica y abre el menú principal
        private void btnIniciarSesion_Click(object sender, EventArgs e)
        {
            string nombre = txtUsuario.Text.Trim(); // Obtiene el nombre de usuario eliminando espacios al inicio y final
            string pass = txtPassword.Text;          // Obtiene la contraseña tal como fue ingresada

            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(pass)) // Valida que ningún campo esté vacío
            {
                MessageBox.Show("Completa todos los campos.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning); // Muestra un aviso si falta algún campo
                return; // Sale del método sin continuar
            }

            var usuario = Program.BurdiGames.AutenticarUsuario(nombre, pass); // Intenta autenticar al usuario con las credenciales ingresadas

            if (usuario == null) // Si la autenticación falló (usuario no existe o contraseña incorrecta)
            {
                MessageBox.Show("Usuario o contraseña incorrectos.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error); // Muestra un mensaje de error
                return; // Sale del método sin continuar
            }

            ManejadorSesion.IniciarSesion(usuario); // Registra al usuario autenticado como sesión activa en el sistema

            var formMain = new FormMain(usuario); // Crea el formulario principal pasando el usuario autenticado
            formMain.Show();                       // Muestra el formulario principal
            this.Hide();                           // Oculta el formulario de login mientras el usuario está en el menú principal

            // Cuando FormMain se cierre, vuelve el login
            formMain.FormClosed += (s, args) => // Suscribe una lambda al evento de cierre del formulario principal
            {
                ManejadorSesion.CerrarSesion(); // Cierra la sesión activa al cerrar el formulario principal
                this.Show();                    // Vuelve a mostrar el formulario de login
            };
        }

        // Maneja el clic del botón de registro: valida campos, registra el usuario y vuelve al tab de login
        private void btnRegistrarte_Click(object sender, EventArgs e)
        {
            string nickname = txtRegNombre.Text.Trim(); // Obtiene el nickname eliminando espacios al inicio y final
            string email = txtRegEmail.Text.Trim();     // Obtiene el email eliminando espacios (no se valida ni usa actualmente)
            string pass = txtRegPass.Text;              // Obtiene la contraseña del campo de registro

            if (string.IsNullOrWhiteSpace(nickname) || string.IsNullOrWhiteSpace(pass)) // Valida que nickname y contraseña no estén vacíos
            {
                lblRegMsg.ForeColor = Color.FromArgb(220, 50, 50); // Cambia el color del mensaje a rojo para indicar error
                lblRegMsg.Text = "Completa los campos obligatorios."; // Muestra el mensaje de error en la etiqueta
                return; // Sale del método sin continuar
            }

            bool ok = Program.BurdiGames.RegistrarUsuario(nickname, pass); // Intenta registrar el nuevo usuario en el sistema

            if (!ok) // Si el registro falló (nombre de usuario ya existe)
            {
                lblRegMsg.ForeColor = Color.FromArgb(220, 50, 50); // Cambia el color del mensaje a rojo
                lblRegMsg.Text = "Ese nombre de usuario ya existe."; // Muestra el mensaje de error en la etiqueta
                return; // Sale del método sin continuar
            }

            lblRegMsg.ForeColor = Color.FromArgb(0, 200, 100);   // Cambia el color del mensaje a verde para indicar éxito
            lblRegMsg.Text = $"¡Cuenta creada, {nickname}!";     // Muestra el mensaje de éxito con el nickname del nuevo usuario

            // Limpiar y volver al tab login
            txtRegNombre.Clear(); // Limpia el campo de nickname
            txtRegEmail.Clear();  // Limpia el campo de email
            txtRegPass.Clear();   // Limpia el campo de contraseña

            Task.Delay(1500).ContinueWith(_ =>                  // Espera 1500ms de forma asíncrona antes de cambiar de pestaña
                Invoke(() => tabModo.SelectedIndex = 0));        // Vuelve al tab de login (índice 0) en el hilo de la UI usando Invoke
        }

        // Evento Paint vacío del panel de fondo de contraseña (sin implementación activa)
        private void bgTxtContrasenia_Paint(object sender, PaintEventArgs e)
        {
            // Sin implementación: el evento está declarado pero no realiza ninguna acción
        }

        // Evento Paint vacío del panel decorativo (sin implementación activa)
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            // Sin implementación: el evento está declarado pero no realiza ninguna acción
        }
    }
}