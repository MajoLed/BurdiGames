using BurdiGames.Clases;
using System.Drawing.Drawing2D;

namespace BurdiGames.GUI                                          
{
    public partial class FormMain : Form                      
    {
        private readonly Usuario _usuario;                        
        private FormPerfilUsuario? _formPerfil;                   // Referencia nullable al formulario de perfil (permite reutilizar la misma ventana)

        private Color colorFondo = Color.FromArgb(8, 6, 20);      // Color de fondo principal: negro muy oscuro con tinte azul
        private Color colorCard = Color.FromArgb(25, 15, 45);     // Color de fondo de las tarjetas: morado muy oscuro
        private Color colorCardBorde = Color.FromArgb(80, 40, 120); // Color del borde de las tarjetas: morado medio
        private Color colorNeon = Color.FromArgb(180, 60, 255);   // Color neón principal: violeta brillante
        private Color colorNeonRosa = Color.FromArgb(220, 50, 180); // Color neón secundario: rosa fucsia
        private Color colorNeonCian = Color.FromArgb(0, 220, 255); // Color neón terciario: cian brillante
        private Color colorTexto = Color.FromArgb(220, 210, 255); // Color del texto general: blanco con tinte lavanda
        private Color colorGris = Color.FromArgb(110, 90, 150);   // Color gris para textos secundarios: morado grisáceo

        private Label lblBienvenida = null!;                      // Label del mensaje de bienvenida, guardado como campo para poder actualizarlo dinámicamente

        public FormMain(Usuario usuario)                          // Constructor que recibe el usuario autenticado
        {
            InitializeComponent();                                // Inicializa los componentes generados por el diseñador de WinForms
            _usuario = usuario;                                   // Guarda el usuario recibido en el campo privado
            this.BackColor = colorFondo;                          // Establece el color de fondo de la ventana
            ConfigurarVentana();                                  // Llama al método que construye la barra superior y los botones
            CargarTarjetasJuegos();                               // Llama al método que genera las tarjetas del catálogo
        }


        // CONFIGURACIÓN DE LA VENTANA
        private void ConfigurarVentana()                          // Método que crea y agrega los controles del encabezado de la ventana
        {
            this.Text = "BURDIGAMES — Catálogo";                  // Establece el título que aparece en la barra de título de la ventana
            this.Size = new Size(960, 620);                       // Fija el tamaño de la ventana en 960×620 píxeles
            this.StartPosition = FormStartPosition.CenterScreen;  // Hace que la ventana aparezca centrada en la pantalla al abrirse

            var lblTitulo = new Label                             // Crea el label con el nombre de la aplicación
            {
                Text = "BURDIGAMES",                              // Texto del título principal de la app
                ForeColor = colorNeon,                            // Color violeta neón para el texto
                Font = new Font("Consolas", 18f, FontStyle.Bold), // Fuente monoespaciada grande y en negrita
                AutoSize = true,                                  // El label se ajusta automáticamente al contenido
                Left = 20,                                        // Posición horizontal: 20px desde el borde izquierdo
                Top = 14                                          // Posición vertical: 14px desde el borde superior
            };

            lblBienvenida = new Label                             // Crea el label que muestra el nombre del usuario conectado
            {
                Text = $"▸  {_usuario.Nombre}",                   // Muestra el nombre del usuario con un triángulo decorativo
                ForeColor = colorNeonCian,                        // Color cian neón para el texto de bienvenida
                Font = new Font("Consolas", 9f),                  // Fuente monoespaciada pequeña
                AutoSize = true,                                  // El label se ajusta automáticamente al contenido
                Left = 24,                                        // Posición horizontal: 24px desde el borde izquierdo
                Top = 46                                          // Posición vertical: 46px desde el borde superior (debajo del título)
            };

            var btnPerfil = new Button                            // Crea el botón que abre el formulario de perfil de usuario
            {
                Text = "[ PERFIL ]",                              // Texto del botón con estilo de consola entre corchetes
                Width = 100,                                      // Ancho del botón en píxeles
                Height = 32,                                      // Alto del botón en píxeles
                Top = 18,                                         // Posición vertical: 18px desde el borde superior
                FlatStyle = FlatStyle.Flat,                       // Estilo plano sin relieve ni sombras
                BackColor = colorFondo,                           // Fondo igual al de la ventana para que parezca transparente
                ForeColor = colorNeonCian,                        // Texto en color cian neón
                Font = new Font("Consolas", 9f, FontStyle.Bold),  // Fuente monoespaciada pequeña en negrita
                Cursor = Cursors.Hand,                            // Cambia el cursor a mano al pasar por encima
                Anchor = AnchorStyles.Top | AnchorStyles.Right    // Se ancla al borde superior derecho para reposicionarse al redimensionar
            };
            btnPerfil.Left = this.ClientSize.Width - 230;         // Posiciona el botón a 230px del borde derecho de la ventana
            btnPerfil.FlatAppearance.BorderColor = colorNeonCian; // Borde del botón en color cian neón
            btnPerfil.FlatAppearance.BorderSize = 1;              // Grosor del borde: 1px
            btnPerfil.Click += BtnPerfil_Click;                   // Suscribe el manejador de evento al clic del botón de perfil

            var btnLogout = new Button                            // Crea el botón para cerrar la sesión y salir de la aplicación
            {
                Text = "[ SALIR ]",                               // Texto del botón con estilo de consola entre corchetes
                Width = 100,                                      // Ancho del botón en píxeles
                Height = 32,                                      // Alto del botón en píxeles
                Top = 18,                                         // Posición vertical: 18px desde el borde superior
                FlatStyle = FlatStyle.Flat,                       // Estilo plano sin relieve ni sombras
                BackColor = colorFondo,                           // Fondo igual al de la ventana para que parezca transparente
                ForeColor = colorNeonRosa,                        // Texto en color rosa neón para distinguirlo del botón de perfil
                Font = new Font("Consolas", 9f, FontStyle.Bold),  // Fuente monoespaciada pequeña en negrita
                Cursor = Cursors.Hand,                            // Cambia el cursor a mano al pasar por encima
                Anchor = AnchorStyles.Top | AnchorStyles.Right    // Se ancla al borde superior derecho
            };
            btnLogout.Left = this.ClientSize.Width - 115;         // Posiciona el botón a 115px del borde derecho (a la derecha del botón de perfil)
            btnLogout.FlatAppearance.BorderColor = colorNeonRosa; // Borde del botón en color rosa neón
            btnLogout.FlatAppearance.BorderSize = 1;              // Grosor del borde: 1px
            btnLogout.Click += (s, e) => this.Close();            // Al hacer clic, cierra la ventana principal (y la aplicación)

            this.Controls.AddRange(new Control[] { lblTitulo, lblBienvenida, btnPerfil, btnLogout }); // Agrega todos los controles del encabezado a la ventana de una sola vez
        }

        // ABRIR PERFIL — comunicación bidireccional
        private void BtnPerfil_Click(object? sender, EventArgs e) // Manejador del evento clic del botón de perfil
        {
            if (_formPerfil != null && !_formPerfil.IsDisposed)   // Verifica si el formulario de perfil ya está abierto y no fue cerrado
            {
                _formPerfil.Refrescar();                          // Actualiza los datos mostrados en el perfil ya abierto
                _formPerfil.BringToFront();                       // Trae la ventana de perfil al frente sin abrir una nueva
                return;                                           // Sale del método para no crear un duplicado
            }

            _formPerfil = new FormPerfilUsuario(_usuario);        // Crea una nueva instancia del formulario de perfil pasando el usuario actual

            _formPerfil.PerfilActualizado += (s, args) =>         // Suscribe al evento que el formulario de perfil lanza cuando se guardan cambios
            {
                lblBienvenida.Text = $"▸  {_usuario.Nombre}";    // Actualiza el label de bienvenida en FormMain con el nuevo nombre del usuario
            };

            _formPerfil.FormClosed += (s, args) => _formPerfil = null; // Cuando el perfil se cierra, limpia la referencia para permitir abrirlo de nuevo
            _formPerfil.Show();                                   // Muestra el formulario de perfil de forma no modal (no bloquea FormMain)
        }

        // CATÁLOGO DE JUEGOS
        private void CargarTarjetasJuegos()                       // Método que crea el panel de desplazamiento y genera una tarjeta por cada juego
        {
            var panel = new FlowLayoutPanel                       // Crea un panel que acomoda automáticamente los controles hijos en filas
            {
                Left = 0,                                         // Comienza desde el borde izquierdo de la ventana
                Top = 75,                                         // Empieza debajo del encabezado (75px desde arriba)
                Width = this.ClientSize.Width,                    // Ocupa todo el ancho disponible del área cliente
                Height = this.ClientSize.Height - 75,             // Ocupa el alto restante después del encabezado
                BackColor = colorFondo,                           // Fondo oscuro igual al de la ventana
                AutoScroll = true,                                // Habilita el scroll automático cuando hay más tarjetas que espacio disponible
                Padding = new Padding(20),                        // Relleno interior de 20px en todos los lados
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right // Se estira en todas las direcciones al redimensionar la ventana
            };

            foreach (var juego in Program.BurdiGames.CatalogoJuegos) // Itera sobre todos los juegos del catálogo global de la aplicación
                panel.Controls.Add(CrearTarjeta(juego));          // Crea y agrega la tarjeta visual de cada juego al panel

            this.Controls.Add(panel);                             // Agrega el panel de tarjetas a la ventana principal
        }

        private Panel CrearTarjeta(Juego juego)                   // Método que construye y devuelve el panel visual (tarjeta) de un juego específico
        {
            var card = new Panel                                  // Crea el contenedor principal de la tarjeta
            {
                Width = 210,                                      // Ancho de la tarjeta en píxeles
                Height = 155,                                     // Alto de la tarjeta en píxeles
                BackColor = colorCard,                            // Fondo morado oscuro de la tarjeta
                Margin = new Padding(12),                         // Margen exterior de 12px para separar las tarjetas entre sí
                Cursor = Cursors.Hand                             // Cambia el cursor a mano al pasar por encima de la tarjeta
            };

            card.Paint += (s, e) =>                               // Suscribe el evento de pintado personalizado para dibujar el borde decorativo
            {
                var g = e.Graphics;                               // Obtiene el objeto Graphics para dibujar sobre la tarjeta
                g.SmoothingMode = SmoothingMode.AntiAlias;        // Activa el suavizado de bordes para líneas más limpias
                using var pen = new Pen(colorCardBorde, 1.5f);    // Crea un lápiz morado para el borde exterior completo
                g.DrawRectangle(pen, 1, 1, card.Width - 3, card.Height - 3); // Dibuja el rectángulo de borde exterior de la tarjeta
                using var penBright = new Pen(colorNeon, 2f);     // Crea un lápiz violeta neón para los adornos de las esquinas
                int c = 12;                                       // Define la longitud en píxeles de cada trazo de esquina
                g.DrawLine(penBright, 1, 1, 1 + c, 1);           // Trazo horizontal de la esquina superior izquierda
                g.DrawLine(penBright, 1, 1, 1, 1 + c);           // Trazo vertical de la esquina superior izquierda
                g.DrawLine(penBright, card.Width - 2 - c, 1, card.Width - 2, 1); // Trazo horizontal de la esquina superior derecha
                g.DrawLine(penBright, card.Width - 2, 1, card.Width - 2, 1 + c); // Trazo vertical de la esquina superior derecha
                g.DrawLine(penBright, 1, card.Height - 2, 1 + c, card.Height - 2); // Trazo horizontal de la esquina inferior izquierda
                g.DrawLine(penBright, 1, card.Height - 2 - c, 1, card.Height - 2); // Trazo vertical de la esquina inferior izquierda
                g.DrawLine(penBright, card.Width - 2 - c, card.Height - 2, card.Width - 2, card.Height - 2); // Trazo horizontal de la esquina inferior derecha
                g.DrawLine(penBright, card.Width - 2, card.Height - 2 - c, card.Width - 2, card.Height - 2); // Trazo vertical de la esquina inferior derecha
            };

            var lblFranja = new Label                             // Crea la franja de color en la parte superior de la tarjeta para indicar el género
            {
                Width = 210,                                      // Ocupa todo el ancho de la tarjeta
                Height = 4,                                       // Solo 4px de alto: es una línea decorativa muy fina
                Top = 0,                                          // Posicionada en el tope de la tarjeta
                Left = 0,                                         // Alineada al borde izquierdo
                BackColor = ObtenerColorGenero(juego.Genero)      // Color determinado por el género del juego
            };

            var lblNombre = new Label                             // Crea el label con el nombre del juego
            {
                Text = juego.Nombre,                              // Muestra el nombre del juego
                ForeColor = colorTexto,                           // Color lavanda claro para el texto principal
                Font = new Font("Consolas", 10f, FontStyle.Bold), // Fuente monoespaciada en negrita
                AutoSize = false,                                 // Tamaño fijo para alineación consistente
                Width = 190,                                      // Ancho del label
                Height = 22,                                      // Alto del label
                Top = 14,                                         // Posición debajo de la franja de color
                Left = 10                                         // Pequeño margen izquierdo
            };

            var lblGenero = new Label                             // Crea el label que muestra el género del juego
            {
                Text = $"// {juego.Genero}",                      // Muestra el género con un prefijo de comentario de código "//" como decoración
                ForeColor = colorGris,                            // Color gris morado para texto secundario
                Font = new Font("Consolas", 8f),                  // Fuente pequeña monoespaciada
                AutoSize = false,                                 // Tamaño fijo
                Width = 190,                                      // Ancho del label
                Height = 16,                                      // Alto del label
                Top = 38,                                         // Posicionado debajo del nombre del juego
                Left = 10                                         // Pequeño margen izquierdo
            };

            var lblDesc = new Label                               // Crea el label con la descripción breve del juego
            {
                Text = juego.Descripcion,                         // Muestra la descripción del juego
                ForeColor = Color.FromArgb(140, 120, 180),        // Color morado suave para texto de descripción
                Font = new Font("Consolas", 7f),                  // Fuente muy pequeña para encajar en el espacio limitado
                AutoSize = false,                                 // Tamaño fijo
                Width = 190,                                      // Ancho del label
                Height = 32,                                      // Alto suficiente para dos líneas de texto
                Top = 58,                                         // Posicionado debajo del género
                Left = 10                                         // Pequeño margen izquierdo
            };

            var mejorPartida = _usuario.HistorialPartidas         // Busca en el historial del usuario la mejor partida para este juego
                .Where(p => p.Juego.Nombre == juego.Nombre && p.Puntaje.HasValue) // Filtra partidas de este juego que tengan puntaje registrado
                .OrderByDescending(p => p.Puntaje)               // Ordena de mayor a menor puntaje
                .FirstOrDefault();                                // Toma la primera (la de mayor puntaje) o null si no hay ninguna

            var lblPuntaje = new Label                            // Crea el label que muestra el mejor puntaje del usuario en este juego
            {
                Text = mejorPartida != null ? $"▲ {mejorPartida.Puntaje:N0} pts" : "Sin partidas", // Muestra el puntaje formateado o un mensaje si no hay partidas
                ForeColor = mejorPartida != null ? colorNeonCian : colorGris, // Cian si hay puntaje, gris si no hay partidas
                Font = new Font("Consolas", 7f),                  // Fuente muy pequeña
                AutoSize = false,                                 // Tamaño fijo
                Width = 190,                                      // Ancho del label
                Height = 14,                                      // Alto del label
                Top = 94,                                         // Posicionado debajo de la descripción
                Left = 10                                         // Pequeño margen izquierdo
            };

            var btnJugar = new Button                             // Crea el botón principal para iniciar una partida del juego
            {
                Text = "▶ JUGAR",                                 // Texto con icono de play
                Top = 112,                                        // Posicionado en la parte inferior de la tarjeta
                Left = 10,                                        // Margen izquierdo
                Width = 92,                                       // Ocupa la mitad izquierda de la tarjeta
                Height = 28,                                      // Alto del botón
                FlatStyle = FlatStyle.Flat,                       // Estilo plano
                BackColor = Color.FromArgb(50, 20, 80),           // Fondo morado oscuro
                ForeColor = colorNeon,                            // Texto violeta neón
                Font = new Font("Consolas", 8f, FontStyle.Bold),  // Fuente pequeña en negrita
                Cursor = Cursors.Hand                             // Cursor de mano
            };
            btnJugar.FlatAppearance.BorderColor = colorNeon;      // Borde violeta neón del botón jugar
            btnJugar.FlatAppearance.BorderSize = 1;               // Grosor del borde: 1px
            btnJugar.Click += (s, e) =>                           // Define lo que ocurre al hacer clic en el botón jugar
            {
                var partida = new Partida(_usuario, juego)        // Crea un objeto Partida asociando el usuario actual y el juego seleccionado
                {
                    Puntaje = 0,                                  // Inicializa el puntaje en 0 al comenzar la partida
                    Fecha = DateTime.Now                          // Registra la fecha y hora actuales como inicio de la partida
                };
                _usuario.HistorialPartidas.Add(partida);          // Agrega la nueva partida al historial del usuario
                _formPerfil?.Refrescar();                         // Si el perfil está abierto, actualiza su vista para mostrar la nueva partida
                juego.Jugar();                                    // Ejecuta la lógica del juego (método definido en la clase Juego)
            };

            var btnInfo = new Button                              // Crea el botón secundario para mostrar información del juego
            {
                Text = "INFO",                                    // Texto simple del botón
                Top = 112,                                        // Misma altura vertical que el botón jugar
                Left = 108,                                       // Posicionado a la derecha del botón jugar
                Width = 58,                                       // Más angosto que el botón jugar
                Height = 28,                                      // Misma altura que el botón jugar
                FlatStyle = FlatStyle.Flat,                       // Estilo plano
                BackColor = Color.FromArgb(20, 10, 35),           // Fondo casi negro con tinte morado
                ForeColor = colorNeonCian,                        // Texto cian neón
                Font = new Font("Consolas", 8f),                  // Fuente pequeña monoespaciada
                Cursor = Cursors.Hand                             // Cursor de mano
            };
            btnInfo.FlatAppearance.BorderColor = colorNeonCian;   // Borde cian del botón de información
            btnInfo.FlatAppearance.BorderSize = 1;                // Grosor del borde: 1px
            btnInfo.Click += (s, e) =>                            // Define lo que ocurre al hacer clic en el botón de información
            {
                MessageBox.Show(                                  // Muestra un cuadro de diálogo con la información del juego
                    $"{juego.Nombre}\n\nGénero: {juego.Genero}\n\n{juego.Descripcion}", // Contenido: nombre, género y descripción
                    "INFO", MessageBoxButtons.OK, MessageBoxIcon.None); // Título "INFO", solo botón OK, sin ícono
            };

            card.Controls.AddRange(new Control[] { lblFranja, lblNombre, lblGenero, lblDesc, lblPuntaje, btnJugar, btnInfo }); // Agrega todos los controles de la tarjeta de una sola vez
            return card;                                          // Devuelve la tarjeta completamente construida
        }

        private Color ObtenerColorGenero(string genero)           // Método que retorna el color asociado a cada género de juego
        {
            return genero?.ToLower() switch                       // Convierte el género a minúsculas y lo evalúa con pattern matching
            {
                "arcade" => Color.FromArgb(0, 220, 255),      // Arcade: cian brillante
                "rpg" => Color.FromArgb(180, 60, 255),     // RPG: violeta neón
                "puzzle" => Color.FromArgb(0, 255, 127),      // Puzzle: verde menta
                "simulación" => Color.FromArgb(255, 180, 0),      // Simulación (con tilde): naranja dorado
                "simulacion" => Color.FromArgb(255, 180, 0),      // Simulación (sin tilde): mismo naranja dorado
                "acción" => Color.FromArgb(255, 60, 100),     // Acción (con tilde): rojo rosado
                "accion" => Color.FromArgb(255, 60, 100),     // Acción (sin tilde): mismo rojo rosado
                _ => Color.FromArgb(220, 50, 180),     // Cualquier otro género: rosa fucsia como color por defecto
            };
        }

        protected override void OnPaint(PaintEventArgs e)         // Sobrescribe el método de pintado de la ventana para agregar decoración visual
        {
            base.OnPaint(e);                                      // Llama al pintado base de la clase Form
            using var brush = new LinearGradientBrush(            // Crea un pincel con gradiente lineal horizontal
                new Point(0, 0), new Point(this.Width, 0),        // De izquierda a derecha a lo largo de toda la ventana
                colorNeonRosa, colorNeonCian);                    // Gradiente de rosa a cian neón
            using var pen = new Pen(brush, 2f);                   // Crea un lápiz usando ese pincel de gradiente, de 2px de grosor
            e.Graphics.DrawLine(pen, 0, 1, this.Width, 1);        // Dibuja una línea horizontal en la parte superior de la ventana (separador decorativo con gradiente)
        }
    }
}