using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MiniJuegos                                                                           
{
    // ─── Panel doble buffer ───────────────────────────────────────────────────
    public class PanelSuaveTamagotchi : Panel                                                  
    {
        public PanelSuaveTamagotchi()                                                          // Constructor del panel suavizado
        {
            DoubleBuffered = true;                                                             // Activa el doble buffer: dibuja en memoria antes de mostrar en pantalla
            SetStyle(ControlStyles.AllPaintingInWmPaint |                                      // Evita que Windows limpie el fondo antes de pintar (reduce parpadeo)
                     ControlStyles.UserPaint |                                                 // Indica que el control se pinta a sí mismo (no delega al sistema)
                     ControlStyles.OptimizedDoubleBuffer, true);                               // Activa el doble buffer optimizado del sistema de estilos de WinForms
            UpdateStyles();                                                                    // Aplica los cambios de estilos recién configurados
        }
    }

    // ─── Pantalla de configuración inicial ───────────────────────────────────
    public class FormConfigTamagotchi : Form                                                   
    {
        public string NombreMascota { get; private set; } = "Buddy";                          
        public TipoMascota Tipo { get; private set; } = TipoMascota.Perro;                    
        public GeneroMascota Genero { get; private set; } = GeneroMascota.Nino;               

        private TextBox txtNombre;                                                             // Campo de texto donde el usuario escribe el nombre de su mascota
        private Panel panelMascota;                                                            // Panel que contiene los botones de selección de tipo de mascota
        private Panel panelGenero;                                                             // Panel que contiene los botones de selección de género
        private Button btnJugar;                                                               // Botón que confirma la configuración e inicia el juego
        private TipoMascota tipoSelec = TipoMascota.Perro;                                    // Variable temporal que almacena el tipo de mascota actualmente seleccionado
        private GeneroMascota genSelec = GeneroMascota.Nino;                                  // Variable temporal que almacena el género actualmente seleccionado

        private static readonly Color C_FONDO = Color.FromArgb(245, 240, 255);               // Color de fondo del formulario: lavanda muy claro
        private static readonly Color C_ROSA = Color.FromArgb(255, 150, 190);                // Color rosa pastel para acentos y título
        private static readonly Color C_VERDE = Color.FromArgb(160, 230, 180);               // Color verde menta para el botón de mascota seleccionada
        private static readonly Color C_AZUL = Color.FromArgb(150, 200, 255);                // Color azul cielo para el botón de género masculino
        private static readonly Color C_MORADO = Color.FromArgb(180, 130, 255);              // Color morado pastel para bordes de botones
        private static readonly Color C_TEXTO = Color.FromArgb(80, 50, 100);                 // Color morado oscuro para el texto general

        public FormConfigTamagotchi()                                                          // Constructor del formulario de configuración
        {
            Text = "🐾 ¡Crea tu mascota!";                                                    // Título de la ventana de configuración
            ClientSize = new Size(420, 520);                                                  // Tamaño del área cliente de la ventana
            BackColor = C_FONDO;                                                              // Color de fondo lavanda muy claro
            FormBorderStyle = FormBorderStyle.FixedSingle;                                    // Borde fijo, no redimensionable
            MaximizeBox = false;                                                               // Desactiva el botón de maximizar
            StartPosition = FormStartPosition.CenterScreen;                                   // Centra la ventana en la pantalla al abrirse
            Font = new Font("Consolas", 10f);                                                 // Fuente monoespaciada como base para todos los controles

            // Título
            var lblTitulo = new Label                                                          // Crea el label del título principal del formulario
            {
                Text = "🌸 TAMAGOTCHI 🌸",                                                    // Texto decorativo con emojis de flores
                Font = new Font("Consolas", 16f, FontStyle.Bold),                             // Fuente grande y en negrita
                ForeColor = C_ROSA,                                                           // Color rosa pastel para el título
                AutoSize = true,                                                               // Se ajusta automáticamente al contenido
                Location = new Point(75, 18)                                                  // Posición centrada visualmente en la parte superior
            };

            // Nombre
            var lblNombre = new Label                                                          // Crea el label indicador del campo de nombre
            {
                Text = "Nombre de tu mascota:",                                               // Texto descriptivo para el campo de nombre
                ForeColor = C_TEXTO,                                                          // Color morado oscuro
                AutoSize = true,                                                               // Se ajusta al contenido
                Font = new Font("Consolas", 10f, FontStyle.Bold),                             // Fuente monoespaciada en negrita
                Location = new Point(30, 65)                                                  // Posicionado en la parte superior izquierda
            };
            txtNombre = new TextBox                                                            // Crea el campo de texto para escribir el nombre de la mascota
            {
                Location = new Point(30, 88),                                                 // Posicionado debajo del label de nombre
                Size = new Size(360, 30),                                                     // Ancho completo del formulario con márgenes
                Font = new Font("Consolas", 12f),                                             // Fuente más grande para facilitar la lectura
                BackColor = Color.White,                                                       // Fondo blanco del campo de texto
                ForeColor = C_TEXTO,                                                          // Texto en morado oscuro
                MaxLength = 18,                                                               // Limita el nombre a 18 caracteres máximo
                Text = "Buddy"                                                                // Valor inicial por defecto
            };
            txtNombre.BorderStyle = BorderStyle.FixedSingle;                                  // Borde simple sin efecto 3D

            // Mascota
            var lblMascota = new Label                                                         // Crea el label indicador de la sección de selección de mascota
            {
                Text = "Elige tu mascota:",                                                   // Texto descriptivo de la sección
                ForeColor = C_TEXTO,                                                          // Color morado oscuro
                AutoSize = true,                                                               // Se ajusta al contenido
                Font = new Font("Consolas", 10f, FontStyle.Bold),                             // Fuente monoespaciada en negrita
                Location = new Point(30, 130)                                                 // Posicionado debajo del campo de nombre
            };

            panelMascota = new Panel                                                           // Crea el panel contenedor de los botones de selección de mascota
            {
                Location = new Point(30, 155),                                                // Posicionado debajo del label de mascota
                Size = new Size(360, 100),                                                    // Tamaño suficiente para los 4 botones
                BackColor = Color.Transparent                                                  // Fondo transparente para mostrar el fondo del formulario
            };

            var mascotas = new (string emoji, string nombre, TipoMascota tipo)[]              // Define un arreglo de tuplas con los datos de cada mascota disponible
            {
                ("🐶", "Perro", TipoMascota.Perro),                                           // Opción 1: Perro con emoji y tipo correspondiente
                ("🐱", "Gato",  TipoMascota.Gato),                                            // Opción 2: Gato con emoji y tipo correspondiente
                ("🐥", "Pato",  TipoMascota.Pato),                                            // Opción 3: Pato con emoji y tipo correspondiente
                ("🦊", "Zorro", TipoMascota.Zorro)                                            // Opción 4: Zorro con emoji y tipo correspondiente
            };

            Button[] btnsMascota = new Button[4];                                             // Arreglo que almacena los 4 botones de mascota para poder referenciarlos después
            for (int i = 0; i < 4; i++)                                                       // Itera para crear los 4 botones de selección de mascota
            {
                int idx = i;                                                                   // Captura el índice en una variable local para usar en el closure del evento Click
                var btn = new Button                                                           // Crea cada botón de selección de mascota
                {
                    Text = mascotas[i].emoji + "\n" + mascotas[i].nombre,                     // Texto con emoji arriba y nombre abajo separados por salto de línea
                    Size = new Size(82, 80),                                                  // Tamaño cuadrado para mostrar emoji y nombre
                    Location = new Point(i * 90, 0),                                          // Cada botón se desplaza 90px a la derecha del anterior
                    FlatStyle = FlatStyle.Flat,                                               // Estilo plano sin relieve
                    BackColor = i == 0 ? C_VERDE : Color.White,                               // El primer botón (Perro) empieza seleccionado en verde; los demás en blanco
                    ForeColor = C_TEXTO,                                                      // Texto en morado oscuro
                    Font = new Font("Segoe UI Emoji", 9f, FontStyle.Bold),                    // Fuente con soporte de emojis y negrita
                    Cursor = Cursors.Hand,                                                     // Cursor de mano al pasar por encima
                    Tag = mascotas[i].tipo                                                     // Almacena el tipo de mascota en el Tag para referencia
                };
                btn.FlatAppearance.BorderColor = C_MORADO;                                    // Borde morado pastel para todos los botones
                btn.FlatAppearance.BorderSize = 2;                                             // Grosor del borde de 2px
                btn.Click += (s, e) =>                                                        // Define lo que ocurre al hacer clic en un botón de mascota
                {
                    tipoSelec = mascotas[idx].tipo;                                           // Actualiza el tipo de mascota seleccionada con el índice capturado
                    foreach (Button b in btnsMascota)                                          // Recorre todos los botones de mascota
                        b.BackColor = Color.White;                                            // Resetea el fondo de todos los botones a blanco
                    btn.BackColor = C_VERDE;                                                  // Resalta el botón seleccionado en verde
                };
                btnsMascota[i] = btn;                                                         // Guarda el botón en el arreglo para referenciar más adelante
                panelMascota.Controls.Add(btn);                                               // Agrega el botón al panel de mascotas
            }

            // Género
            var lblGenero = new Label                                                          // Crea el label indicador de la sección de selección de género
            {
                Text = "Género:",                                                             // Texto descriptivo de la sección
                ForeColor = C_TEXTO,                                                          // Color morado oscuro
                AutoSize = true,                                                               // Se ajusta al contenido
                Font = new Font("Consolas", 10f, FontStyle.Bold),                             // Fuente monoespaciada en negrita
                Location = new Point(30, 270)                                                 // Posicionado debajo del panel de mascotas
            };
            panelGenero = new Panel                                                            // Crea el panel contenedor de los botones de género
            {
                Location = new Point(30, 295),                                                // Posicionado debajo del label de género
                Size = new Size(360, 55),                                                     // Tamaño para los dos botones de género
                BackColor = Color.Transparent                                                  // Fondo transparente
            };

            var btnNino = new Button                                                           // Crea el botón para seleccionar género masculino
            {
                Text = "♂  Niño",                                                             // Texto con símbolo de masculino
                Size = new Size(170, 46),                                                     // Ocupa casi la mitad del panel
                Location = new Point(0, 0),                                                   // Alineado a la izquierda del panel
                FlatStyle = FlatStyle.Flat,                                                   // Estilo plano
                BackColor = C_AZUL,                                                           // Fondo azul porque empieza seleccionado por defecto
                ForeColor = C_TEXTO,                                                          // Texto morado oscuro
                Font = new Font("Consolas", 11f, FontStyle.Bold),                             // Fuente monoespaciada en negrita
                Cursor = Cursors.Hand                                                          // Cursor de mano
            };
            btnNino.FlatAppearance.BorderColor = C_MORADO;                                    // Borde morado pastel
            btnNino.FlatAppearance.BorderSize = 2;                                             // Grosor del borde de 2px

            var btnNina = new Button                                                           // Crea el botón para seleccionar género femenino
            {
                Text = "♀  Niña",                                                             // Texto con símbolo de femenino
                Size = new Size(170, 46),                                                     // Mismo tamaño que el botón de niño
                Location = new Point(185, 0),                                                 // Posicionado a la derecha del botón de niño con pequeño espacio
                FlatStyle = FlatStyle.Flat,                                                   // Estilo plano
                BackColor = Color.White,                                                       // Fondo blanco porque no está seleccionado por defecto
                ForeColor = C_TEXTO,                                                          // Texto morado oscuro
                Font = new Font("Consolas", 11f, FontStyle.Bold),                             // Fuente monoespaciada en negrita
                Cursor = Cursors.Hand                                                          // Cursor de mano
            };
            btnNina.FlatAppearance.BorderColor = C_MORADO;                                    // Borde morado pastel
            btnNina.FlatAppearance.BorderSize = 2;                                             // Grosor del borde de 2px

            btnNino.Click += (s, e) =>                                                        // Define lo que ocurre al seleccionar género masculino
            {
                genSelec = GeneroMascota.Nino;                                                // Actualiza la selección de género a masculino
                btnNino.BackColor = C_AZUL;                                                   // Resalta el botón de niño en azul
                btnNina.BackColor = Color.White;                                               // Resetea el botón de niña a blanco
            };
            btnNina.Click += (s, e) =>                                                        // Define lo que ocurre al seleccionar género femenino
            {
                genSelec = GeneroMascota.Nina;                                                // Actualiza la selección de género a femenino
                btnNina.BackColor = C_ROSA;                                                   // Resalta el botón de niña en rosa
                btnNino.BackColor = Color.White;                                              // Resetea el botón de niño a blanco
            };

            panelGenero.Controls.AddRange(new Control[] { btnNino, btnNina });                // Agrega ambos botones de género al panel de género

            // Vista previa dibujada
            var panelPreview = new PanelSuaveTamagotchi                                       // Crea un panel con doble buffer para mostrar la vista previa de la mascota
            {
                Location = new Point(145, 365),                                               // Centrado horizontalmente en el formulario, debajo de los controles de género
                Size = new Size(130, 100),                                                    // Tamaño suficiente para mostrar la mascota dibujada
                BackColor = Color.Transparent                                                  // Fondo transparente para mostrar el fondo del formulario
            };
            panelPreview.Paint += (s, e) =>                                                   // Suscribe el evento de pintado personalizado del panel de vista previa
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;                          // Activa el suavizado de bordes para el dibujo
                var tmpMascota = new Mascota(                                                  // Crea una mascota temporal con las opciones actualmente seleccionadas
                    txtNombre.Text.Length > 0 ? txtNombre.Text : "?",                         // Usa el nombre escrito o "?" si el campo está vacío
                    tipoSelec, genSelec);                                                     // Pasa el tipo y género actualmente seleccionados
                DibujadorMascota.Dibujar(e.Graphics, tmpMascota, 65, 85);                     // Delega el dibujo de la mascota temporal al dibujador en el centro del panel
            };

            // Refrescar preview al cambiar nombre o mascota
            txtNombre.TextChanged += (s, e) => panelPreview.Invalidate();                     // Cuando cambia el nombre, fuerza el redibujado del preview para actualizar el nombre mostrado
            foreach (Button b in btnsMascota)                                                  // Recorre todos los botones de mascota
                b.Click += (s, e) => panelPreview.Invalidate();                               // Al hacer clic en cualquier botón de mascota, actualiza el preview con el nuevo tipo
            btnNino.Click += (s, e) => panelPreview.Invalidate();                             // Al seleccionar género masculino, actualiza el preview
            btnNina.Click += (s, e) => panelPreview.Invalidate();                             // Al seleccionar género femenino, actualiza el preview

            // Botón jugar
            btnJugar = new Button                                                              // Crea el botón que confirma la configuración e inicia el juego
            {
                Text = "▶  ¡JUGAR!",                                                          // Texto con icono de play
                Location = new Point(110, 475),                                               // Centrado en la parte inferior del formulario
                Size = new Size(200, 42),                                                     // Tamaño prominente para resaltar como acción principal
                FlatStyle = FlatStyle.Flat,                                                   // Estilo plano
                BackColor = C_ROSA,                                                           // Fondo rosa para destacarlo como botón principal
                ForeColor = Color.White,                                                       // Texto blanco para contraste sobre el rosa
                Font = new Font("Consolas", 13f, FontStyle.Bold),                             // Fuente grande y en negrita
                Cursor = Cursors.Hand                                                          // Cursor de mano
            };
            btnJugar.FlatAppearance.BorderColor = C_MORADO;                                   // Borde morado pastel
            btnJugar.FlatAppearance.BorderSize = 2;                                            // Grosor del borde de 2px
            btnJugar.Click += (s, e) =>                                                       // Define lo que ocurre al hacer clic en el botón Jugar
            {
                NombreMascota = txtNombre.Text.Trim().Length > 0 ? txtNombre.Text.Trim() : "Buddy"; // Guarda el nombre escrito sin espacios; si está vacío usa "Buddy"
                Tipo = tipoSelec;                                                             // Guarda el tipo de mascota seleccionado en la propiedad pública
                Genero = genSelec;                                                            // Guarda el género seleccionado en la propiedad pública
                DialogResult = DialogResult.OK;                                               // Establece el resultado del diálogo como OK para que el código que lo llamó pueda detectarlo
                Close();                                                                       // Cierra el formulario de configuración
            };

            Controls.AddRange(new Control[]                                                    // Agrega todos los controles al formulario de una sola vez
            {
                lblTitulo, lblNombre, txtNombre,                                               // Título, label de nombre y campo de texto
                lblMascota, panelMascota,                                                      // Label de mascota y su panel de botones
                lblGenero, panelGenero,                                                        // Label de género y su panel de botones
                panelPreview, btnJugar                                                         // Panel de vista previa y botón de inicio
            });
        }
    }

    // ─── Form principal del Tamagotchi ────────────────────────────────────────
    public class FormTamagotchi : Form                                                         // Formulario principal del juego Tamagotchi donde se desarrolla la partida
    {
        private Mascota mascota;                                                               // Referencia al objeto mascota con su estado y lógica de juego
        private System.Windows.Forms.Timer timerJuego;                                        // Temporizador que ejecuta el bucle de juego a ~30 FPS

        private PanelSuaveTamagotchi panelJuego;                                              // Panel con doble buffer donde se dibuja visualmente la mascota y el escenario
        private Button btnPausa;                                                               // Botón para pausar y reanudar el juego
        private Button btnReiniciar;                                                           // Botón para abrir la configuración y crear una nueva mascota
        private Label lblNombre;                                                               // Label que muestra el nombre de la mascota en el HUD superior
        private Label lblMensaje;                                                              // Label que muestra el mensaje de estado actual de la mascota

        // Barras de estado
        private Label lblBarraHambre;                                                          // Label con el emoji e icono de la barra de hambre
        private Label lblBarraEnergia;                                                         // Label con el emoji e icono de la barra de energía
        private Label lblBarraFelicidad;                                                       // Label con el emoji e icono de la barra de felicidad
        private ProgressBar barHambre;                                                         // Barra de progreso que muestra el nivel de hambre de la mascota
        private ProgressBar barEnergia;                                                        // Barra de progreso que muestra el nivel de energía de la mascota
        private ProgressBar barFelicidad;                                                      // Barra de progreso que muestra el nivel de felicidad de la mascota

        // Botones de acción
        private Button btnComer;                                                               // Botón para darle comida a la mascota
        private Button btnDormir;                                                              // Botón para poner a dormir a la mascota
        private Button btnJugar;                                                               // Botón para jugar con la mascota

        // Paleta pastel
        private static readonly Color C_FONDO = Color.FromArgb(235, 248, 255);               // Color de fondo principal: azul cielo muy claro
        private static readonly Color C_HUD = Color.FromArgb(210, 235, 255);                 // Color del HUD superior e inferior: azul pastel suave
        private static readonly Color C_PANEL = Color.FromArgb(225, 255, 235);               // Color del panel de juego: verde menta muy claro
        private static readonly Color C_ROSA = Color.FromArgb(255, 150, 190);                // Color rosa para la barra de hambre y acentos
        private static readonly Color C_VERDE = Color.FromArgb(130, 210, 160);               // Color verde para la barra de felicidad y botón de jugar
        private static readonly Color C_AZUL = Color.FromArgb(100, 180, 255);                // Color azul para la barra de energía y botón de dormir
        private static readonly Color C_MORADO = Color.FromArgb(180, 130, 255);              // Color morado para bordes y subtítulos
        private static readonly Color C_TEXTO = Color.FromArgb(70, 50, 100);                 // Color morado oscuro para el texto general
        private static readonly Color C_NEON_ROSA = Color.FromArgb(255, 100, 160);           // Color rosa neón para líneas decorativas y bordes de overlays

        private float timerParticulas = 0f;                                                    // Acumulador de tiempo para controlar la frecuencia de creación de burbujas decorativas
        private readonly System.Collections.Generic.List<(float x, float y, float vy, Color c, float vida)> burbujas = new(); // Lista de burbujas decorativas activas con su posición, velocidad, color y tiempo de vida
        private static readonly Random rng = new Random();                                    // Generador de números aleatorios compartido para posiciones y colores de burbujas

        // ── Constructor ───────────────────────────────────────────────────────
        public FormTamagotchi()                                                                // Constructor principal del juego: muestra configuración y arranca el juego
        {
            // Mostrar configuración primero
            using var cfg = new FormConfigTamagotchi();                                       // Crea el formulario de configuración como recurso desechable
            if (cfg.ShowDialog() != DialogResult.OK)                                          // Muestra la configuración como diálogo modal; si el usuario cancela
            {
                Close();                                                                       // Cierra la ventana del juego sin iniciarlo
                return;                                                                        // Sale del constructor sin continuar la inicialización
            }

            mascota = new Mascota(cfg.NombreMascota, cfg.Tipo, cfg.Genero);                   // Crea la mascota con los datos recogidos en la configuración
            mascota.MascotaEscapo += (s, e) => MostrarEscapo();                              // Suscribe el evento de escape de la mascota al método que detiene el juego

            InicializarComponentes();                                                          // Construye y configura toda la interfaz gráfica del juego

            timerJuego = new System.Windows.Forms.Timer { Interval = 33 };                   // Crea el timer del bucle de juego con intervalo de 33ms (~30 FPS)
            timerJuego.Tick += GameLoop;                                                      // Suscribe el bucle de juego al evento Tick del timer
            timerJuego.Start();                                                               // Arranca el timer para iniciar el bucle de juego

            panelJuego.Focus();                                                               // Da foco al panel de juego para capturar eventos de teclado si los hay
        }

        private void InicializarComponentes()                                                  // Método que construye y configura todos los controles visuales del juego
        {
            Text = $"🐾 {mascota.Nombre}";                                                    // Título de la ventana con el nombre de la mascota
            ClientSize = new Size(480, 620);                                                  // Tamaño del área cliente de la ventana principal
            BackColor = C_FONDO;                                                              // Color de fondo azul cielo muy claro
            FormBorderStyle = FormBorderStyle.FixedSingle;                                    // Borde fijo, no redimensionable
            MaximizeBox = false;                                                               // Desactiva el botón de maximizar
            StartPosition = FormStartPosition.CenterScreen;                                   // Centra la ventana en la pantalla
            Font = new Font("Consolas", 9f, FontStyle.Bold);                                  // Fuente base monoespaciada en negrita para todos los controles

            // ── HUD superior ─────────────────────────────────────────────────
            var panelHud = new Panel                                                           // Crea el panel del HUD en la parte superior de la ventana
            {
                Location = new Point(0, 0),                                                   // Posicionado en la esquina superior izquierda
                Size = new Size(480, 70),                                                     // Ocupa todo el ancho con 70px de alto
                BackColor = C_HUD                                                             // Fondo azul pastel del HUD
            };
            panelHud.Paint += (s, e) =>                                                       // Suscribe el pintado personalizado del HUD para dibujar una línea inferior decorativa
            {
                using var pen = new Pen(C_NEON_ROSA, 2f);                                     // Lápiz rosa neón de 2px para la línea principal
                using var glow = new Pen(Color.FromArgb(60, 255, 100, 160), 6f);              // Lápiz rosa semitransparente de 6px para el efecto de resplandor debajo
                e.Graphics.DrawLine(pen, 0, 69, 480, 69);                                    // Dibuja la línea rosa neón en el borde inferior del HUD
                e.Graphics.DrawLine(glow, 0, 68, 480, 68);                                   // Dibuja el resplandor difuso justo encima de la línea principal
            };

            lblNombre = new Label                                                              // Crea el label del nombre de la mascota en el HUD
            {
                Text = mascota.Nombre.ToUpper(),                                              // Nombre de la mascota en mayúsculas
                ForeColor = C_TEXTO,                                                          // Color morado oscuro
                BackColor = Color.Transparent,                                                 // Fondo transparente para mostrar el HUD
                Font = new Font("Consolas", 13f, FontStyle.Bold),                             // Fuente grande y en negrita
                AutoSize = true,                                                               // Se ajusta al contenido
                Location = new Point(14, 10)                                                  // Posicionado en la parte superior izquierda del HUD
            };

            var lblEdad = new Label                                                            // Crea el label decorativo con el subtítulo "TAMAGOTCHI"
            {
                Text = "🐾 TAMAGOTCHI",                                                       // Texto con emoji de huella de pata
                ForeColor = C_MORADO,                                                         // Color morado pastel
                BackColor = Color.Transparent,                                                 // Fondo transparente
                Font = new Font("Consolas", 9f),                                              // Fuente pequeña monoespaciada
                AutoSize = true,                                                               // Se ajusta al contenido
                Location = new Point(14, 40)                                                  // Posicionado debajo del nombre en el HUD
            };

            btnPausa = new Button                                                              // Crea el botón de pausa en el HUD
            {
                Text = "⏸ PAUSA",                                                             // Texto inicial con icono de pausa
                Location = new Point(260, 10),                                                // Posicionado en la parte derecha del HUD
                Size = new Size(100, 34),                                                     // Tamaño del botón
                FlatStyle = FlatStyle.Flat,                                                   // Estilo plano
                ForeColor = Color.White,                                                       // Texto blanco para contraste
                BackColor = C_MORADO,                                                         // Fondo morado pastel
                Cursor = Cursors.Hand,                                                         // Cursor de mano
                Font = new Font("Consolas", 8.5f, FontStyle.Bold)                             // Fuente pequeña en negrita
            };
            btnPausa.FlatAppearance.BorderColor = Color.FromArgb(150, 80, 230);               // Borde morado más oscuro para contraste
            btnPausa.FlatAppearance.BorderSize = 1;                                            // Grosor del borde de 1px
            btnPausa.MouseEnter += (s, e) => btnPausa.BackColor = Color.FromArgb(210, 160, 255); // Al pasar el mouse, aclara el fondo del botón
            btnPausa.MouseLeave += (s, e) => btnPausa.BackColor = C_MORADO;                   // Al salir el mouse, restaura el color original del botón
            btnPausa.Click += (s, e) =>                                                       // Define lo que ocurre al hacer clic en el botón de pausa
            {
                if (timerJuego.Enabled) { timerJuego.Stop(); btnPausa.Text = "▶ SEGUIR"; }   // Si el juego está corriendo, lo pausa y cambia el texto a "SEGUIR"
                else { timerJuego.Start(); btnPausa.Text = "⏸ PAUSA"; }                      // Si está pausado, lo reanuda y restaura el texto a "PAUSA"
                panelJuego.Focus();                                                            // Devuelve el foco al panel de juego
            };

            btnReiniciar = new Button                                                          // Crea el botón de reset para crear una nueva mascota
            {
                Text = "↺ RESET",                                                             // Texto con símbolo de reinicio
                Location = new Point(370, 10),                                                // Posicionado a la derecha del botón de pausa
                Size = new Size(100, 34),                                                     // Mismo tamaño que el botón de pausa
                FlatStyle = FlatStyle.Flat,                                                   // Estilo plano
                ForeColor = Color.White,                                                       // Texto blanco para contraste
                BackColor = C_ROSA,                                                           // Fondo rosa para distinguirlo del botón de pausa
                Cursor = Cursors.Hand,                                                         // Cursor de mano
                Font = new Font("Consolas", 8.5f, FontStyle.Bold)                             // Fuente pequeña en negrita
            };
            btnReiniciar.FlatAppearance.BorderColor = C_NEON_ROSA;                            // Borde rosa neón para destacarlo
            btnReiniciar.FlatAppearance.BorderSize = 1;                                        // Grosor del borde de 1px
            btnReiniciar.MouseEnter += (s, e) => btnReiniciar.BackColor = Color.FromArgb(255, 180, 210); // Al pasar el mouse, aclara el fondo del botón
            btnReiniciar.MouseLeave += (s, e) => btnReiniciar.BackColor = C_ROSA;             // Al salir el mouse, restaura el color rosa original
            btnReiniciar.Click += (s, e) =>                                                   // Define lo que ocurre al hacer clic en el botón de reset
            {
                using var cfg = new FormConfigTamagotchi();                                   // Abre el formulario de configuración para crear una nueva mascota
                if (cfg.ShowDialog() == DialogResult.OK)                                      // Solo continúa si el usuario confirmó la nueva configuración
                {
                    mascota = new Mascota(cfg.NombreMascota, cfg.Tipo, cfg.Genero);           // Crea la nueva mascota con los datos de configuración
                    mascota.MascotaEscapo += (s2, e2) => MostrarEscapo();                    // Vuelve a suscribir el evento de escape para la nueva mascota
                    lblNombre.Text = mascota.Nombre.ToUpper();                                // Actualiza el label del nombre con el nombre de la nueva mascota
                    btnPausa.Text = "⏸ PAUSA";                                               // Restaura el texto del botón de pausa a su estado inicial
                    btnPausa.Enabled = true;                                                  // Reactiva el botón de pausa por si estaba deshabilitado tras un escape
                    timerJuego.Start();                                                       // Reinicia el timer del bucle de juego
                    burbujas.Clear();                                                          // Limpia todas las burbujas decorativas de la partida anterior
                }
                panelJuego.Focus();                                                            // Devuelve el foco al panel de juego
            };

            panelHud.Controls.AddRange(new Control[] { lblNombre, lblEdad, btnPausa, btnReiniciar }); // Agrega todos los controles del HUD al panel de una sola vez

            // ── Panel de juego ────────────────────────────────────────────────
            panelJuego = new PanelSuaveTamagotchi                                             // Crea el panel con doble buffer donde se dibuja el escenario del juego
            {
                Location = new Point(0, 70),                                                  // Posicionado inmediatamente debajo del HUD
                Size = new Size(480, 280),                                                    // Ocupa todo el ancho con 280px de alto para el escenario
                BackColor = C_PANEL                                                           // Fondo verde menta muy claro como color base del escenario
            };
            panelJuego.Paint += PanelJuego_Paint;                                             // Suscribe el método de dibujo personalizado del escenario al evento Paint

            // ── Mensaje de estado ─────────────────────────────────────────────
            lblMensaje = new Label                                                             // Crea el label que muestra los mensajes de estado de la mascota
            {
                Text = "",                                                                     // Sin texto inicial, se rellena en el bucle de juego
                ForeColor = C_TEXTO,                                                          // Color morado oscuro para el texto
                BackColor = Color.FromArgb(200, 255, 255, 255),                               // Fondo blanco semitransparente (alpha 200)
                Font = new Font("Consolas", 10f, FontStyle.Bold),                             // Fuente monoespaciada en negrita
                AutoSize = false,                                                              // Tamaño fijo para que siempre ocupe el mismo espacio
                Size = new Size(480, 30),                                                     // Ocupa todo el ancho de la ventana con 30px de alto
                Location = new Point(0, 350),                                                 // Posicionado justo debajo del panel de juego
                TextAlign = ContentAlignment.MiddleCenter                                      // Texto centrado vertical y horizontalmente
            };

            // ── Barras de estadísticas ────────────────────────────────────────
            var panelStats = new Panel                                                         // Crea el panel contenedor de las barras de estadísticas
            {
                Location = new Point(0, 385),                                                 // Posicionado debajo del label de mensaje
                Size = new Size(480, 110),                                                    // Altura suficiente para las 3 barras
                BackColor = Color.FromArgb(240, 250, 255)                                     // Fondo azul muy claro para la sección de estadísticas
            };

            (string emoji, string label, Color color)[] stats =                               // Define un arreglo de tuplas con los datos de cada estadística
            {
                ("🍎", "HAMBRE",    C_ROSA),                                                  // Estadística de hambre: emoji de manzana y color rosa
                ("⚡", "ENERGÍA",   C_AZUL),                                                  // Estadística de energía: emoji de rayo y color azul
                ("🌸", "FELICIDAD", C_VERDE)                                                  // Estadística de felicidad: emoji de flor y color verde
            };

            var bars = new ProgressBar[3];                                                    // Arreglo temporal para las 3 barras de progreso
            var barLabels = new Label[3];                                                     // Arreglo temporal para los 3 labels de estadísticas

            for (int i = 0; i < 3; i++)                                                       // Itera para crear los 3 pares de label + barra de estadísticas
            {
                int idx = i;                                                                   // Captura el índice para el closure (aunque aquí no se usa en eventos)
                barLabels[i] = new Label                                                      // Crea el label con emoji y nombre de la estadística
                {
                    Text = stats[i].emoji + " " + stats[i].label,                            // Combina emoji y nombre de la estadística
                    ForeColor = C_TEXTO,                                                      // Color morado oscuro
                    BackColor = Color.Transparent,                                             // Fondo transparente
                    Font = new Font("Consolas", 9f, FontStyle.Bold),                          // Fuente pequeña en negrita
                    AutoSize = true,                                                           // Se ajusta al contenido
                    Location = new Point(16, 8 + i * 32)                                     // Cada label se desplaza 32px hacia abajo del anterior
                };
                bars[i] = new ProgressBar                                                     // Crea la barra de progreso para la estadística
                {
                    Location = new Point(130, 8 + i * 32),                                   // A la derecha del label, misma altura
                    Size = new Size(300, 20),                                                 // Barra larga y delgada
                    Minimum = 0,                                                               // Valor mínimo de la barra
                    Maximum = 100,                                                             // Valor máximo de la barra (porcentaje)
                    Value = 80,                                                                // Valor inicial: 80% de la estadística
                    Style = ProgressBarStyle.Continuous                                        // Estilo continuo sin segmentos para una visualización suave
                };
                bars[i].ForeColor = stats[i].color;                                          // Asigna el color de la barra según la estadística (rosa/azul/verde)
                panelStats.Controls.Add(barLabels[i]);                                        // Agrega el label al panel de estadísticas
                panelStats.Controls.Add(bars[i]);                                             // Agrega la barra al panel de estadísticas
            }

            barHambre = bars[0];                                                              // Asigna la primera barra al campo de hambre para acceso directo
            barEnergia = bars[1];                                                             // Asigna la segunda barra al campo de energía para acceso directo
            barFelicidad = bars[2];                                                           // Asigna la tercera barra al campo de felicidad para acceso directo

            // ── Botones de acción ─────────────────────────────────────────────
            var panelAcciones = new Panel                                                      // Crea el panel inferior con los botones de interacción con la mascota
            {
                Location = new Point(0, 500),                                                 // Posicionado en la parte inferior de la ventana
                Size = new Size(480, 118),                                                    // Ocupa todo el ancho con altura suficiente para los botones grandes
                BackColor = C_HUD                                                             // Fondo azul pastel igual al HUD superior para simetría visual
            };
            panelAcciones.Paint += (s, e) =>                                                  // Suscribe el pintado personalizado para dibujar una línea superior decorativa
            {
                using var pen = new Pen(C_NEON_ROSA, 2f);                                     // Lápiz rosa neón de 2px
                e.Graphics.DrawLine(pen, 0, 0, 480, 0);                                      // Dibuja la línea rosa en el borde superior del panel de acciones
            };

            (string texto, Color color, Action accion)[] acciones =                           // Define un arreglo de tuplas con los datos de cada botón de acción
            {
                ("🍎\nCOMER",    C_ROSA,   () => mascota.AccionComer()),                      // Acción de comer: emoji de manzana, color rosa, llama a AccionComer
                ("💤\nDORMIR",  C_AZUL,   () => mascota.AccionDormir()),                     // Acción de dormir: emoji de sueño, color azul, llama a AccionDormir
                ("🎮\nJUGAR",   C_VERDE,  () => mascota.AccionJugar())                       // Acción de jugar: emoji de gamepad, color verde, llama a AccionJugar
            };

            for (int i = 0; i < 3; i++)                                                       // Itera para crear los 3 botones de acción
            {
                int idx = i;                                                                   // Captura el índice para el closure del evento Click
                Color colorBtn = acciones[i].color;                                           // Captura el color del botón para usarlo en los eventos de hover
                var btn = new Button                                                           // Crea cada botón de acción
                {
                    Text = acciones[i].texto,                                                 // Texto con emoji y nombre separados por salto de línea
                    Location = new Point(20 + i * 150, 12),                                  // Cada botón se desplaza 150px a la derecha del anterior
                    Size = new Size(130, 88),                                                 // Botones grandes y cuadrados para fácil interacción
                    FlatStyle = FlatStyle.Flat,                                               // Estilo plano
                    BackColor = colorBtn,                                                      // Color propio de cada acción
                    ForeColor = Color.White,                                                   // Texto blanco para contraste
                    Font = new Font("Segoe UI Emoji", 13f, FontStyle.Bold),                   // Fuente grande con soporte de emojis
                    Cursor = Cursors.Hand                                                      // Cursor de mano
                };
                btn.FlatAppearance.BorderColor = C_MORADO;                                    // Borde morado pastel para todos los botones de acción
                btn.FlatAppearance.BorderSize = 2;                                             // Grosor del borde de 2px
                btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Light(colorBtn, 0.3f); // Al pasar el mouse, aclara el color del botón un 30%
                btn.MouseLeave += (s, e) => btn.BackColor = colorBtn;                         // Al salir el mouse, restaura el color original del botón
                btn.Click += (s, e) =>                                                        // Define lo que ocurre al hacer clic en el botón de acción
                {
                    acciones[idx].accion();                                                   // Ejecuta la acción correspondiente de la mascota usando el índice capturado
                    panelJuego.Focus();                                                        // Devuelve el foco al panel de juego tras la acción
                };
                panelAcciones.Controls.Add(btn);                                              // Agrega el botón al panel de acciones
                if (i == 0) btnComer = btn;                                                   // Guarda referencia al botón de comer para acceso externo si fuera necesario
                if (i == 1) btnDormir = btn;                                                  // Guarda referencia al botón de dormir
                if (i == 2) btnJugar = btn;                                                   // Guarda referencia al botón de jugar
            }

            Controls.AddRange(new Control[]                                                    // Agrega todos los paneles principales a la ventana de una sola vez
            {
                panelHud, panelJuego, lblMensaje, panelStats, panelAcciones                   // Paneles en orden visual de arriba hacia abajo
            });
        }

        // ── Game loop ─────────────────────────────────────────────────────────
        private DateTime ultimoFrame = DateTime.Now;                                           // Almacena el momento del último frame para calcular el delta de tiempo

        private void GameLoop(object? sender, EventArgs e)                                    // Método ejecutado cada ~33ms por el timer como bucle principal del juego
        {
            var ahora = DateTime.Now;                                                          // Captura el tiempo actual del frame
            float delta = (float)(ahora - ultimoFrame).TotalSeconds;                          // Calcula el tiempo transcurrido en segundos desde el último frame
            ultimoFrame = ahora;                                                               // Actualiza el timestamp del último frame para el siguiente cálculo
            delta = Math.Min(delta, 0.05f);                                                   // Limita el delta a 50ms máximo para evitar saltos grandes si el juego se congela

            mascota.Actualizar(delta);                                                         // Actualiza el estado interno de la mascota (hambre, energía, felicidad) basándose en el tiempo transcurrido

            // Actualizar barras
            barHambre.Value = (int)Math.Clamp(mascota.Stats.Hambre, 0, 100);                 // Actualiza la barra de hambre con el valor actual, limitado entre 0 y 100
            barEnergia.Value = (int)Math.Clamp(mascota.Stats.Energia, 0, 100);               // Actualiza la barra de energía con el valor actual, limitado entre 0 y 100
            barFelicidad.Value = (int)Math.Clamp(mascota.Stats.Felicidad, 0, 100);           // Actualiza la barra de felicidad con el valor actual, limitado entre 0 y 100

            // Mensaje
            lblMensaje.Text = mascota.ObtenerMensaje();                                       // Actualiza el mensaje de estado con lo que la mascota "expresa" según su estado actual

            // Burbujas decorativas
            timerParticulas += delta;                                                          // Acumula el tiempo transcurrido para el temporizador de burbujas
            if (timerParticulas >= 0.4f)                                                      // Si han pasado 0.4 segundos desde la última burbuja
            {
                timerParticulas = 0f;                                                          // Resetea el acumulador de partículas
                Color[] coloresBurbuja = { C_ROSA, C_AZUL, C_VERDE, C_MORADO };              // Define la paleta de colores disponibles para las burbujas
                burbujas.Add((                                                                 // Agrega una nueva burbuja a la lista con propiedades aleatorias
                    rng.Next(20, 460),                                                         // Posición X aleatoria dentro del panel de juego
                    290f,                                                                      // Posición Y inicial: fondo del panel de juego
                    rng.NextSingle() * 30f + 20f,                                             // Velocidad vertical aleatoria entre 20 y 50 píxeles por segundo
                    coloresBurbuja[rng.Next(coloresBurbuja.Length)],                          // Color aleatorio de la paleta
                    1.5f));                                                                    // Tiempo de vida de 1.5 segundos
            }

            for (int i = burbujas.Count - 1; i >= 0; i--)                                    // Itera las burbujas de atrás hacia adelante para poder eliminar de forma segura
            {
                var b = burbujas[i];                                                           // Obtiene la burbuja actual
                float vida = b.vida - delta;                                                   // Reduce el tiempo de vida de la burbuja
                if (vida <= 0 || b.y < 70) { burbujas.RemoveAt(i); continue; }               // Si la burbuja murió o salió por arriba del panel, la elimina y pasa a la siguiente
                burbujas[i] = (b.x, b.y - b.vy * delta, b.vy, b.c, vida);                   // Actualiza la burbuja: sube en Y según su velocidad y el delta, reduce su vida
            }

            panelJuego.Invalidate();                                                           // Fuerza el redibujado del panel de juego en cada frame
        }

        // ── Pintar panel de juego ─────────────────────────────────────────────
        private void PanelJuego_Paint(object? sender, PaintEventArgs e)                       // Método de dibujo llamado automáticamente cuando el panel de juego se invalida
        {
            var g = e.Graphics;                                                                // Obtiene el objeto Graphics para dibujar
            g.SmoothingMode = SmoothingMode.AntiAlias;                                        // Activa el suavizado de bordes para dibujos más limpios

            // Fondo degradado pastel
            using var fondo = new LinearGradientBrush(                                        // Crea un pincel de gradiente vertical para el fondo del escenario
                new Point(0, 0), new Point(0, panelJuego.Height),                             // De arriba hacia abajo
                Color.FromArgb(220, 245, 255),                                                // Color superior: azul cielo pastel
                Color.FromArgb(235, 255, 235));                                               // Color inferior: verde menta muy claro
            g.FillRectangle(fondo, 0, 0, panelJuego.Width, panelJuego.Height);               // Dibuja el fondo degradado en todo el panel

            // Suelo
            using var brSuelo = new SolidBrush(Color.FromArgb(180, 230, 200));               // Pincel verde suave para el rectángulo del suelo
            g.FillRectangle(brSuelo, 0, panelJuego.Height - 30, panelJuego.Width, 30);       // Dibuja la franja verde del suelo en los últimos 30px del panel
            using var penSuelo = new Pen(Color.FromArgb(140, 200, 160), 2f);                 // Lápiz verde más oscuro para la línea del borde del suelo
            g.DrawLine(penSuelo, 0, panelJuego.Height - 30, panelJuego.Width, panelJuego.Height - 30); // Dibuja la línea que separa el suelo del cielo

            // Nubes decorativas
            DibujarNube(g, 60, 35, 0.8f);                                                    // Dibuja una nube pequeña en la parte izquierda del cielo
            DibujarNube(g, 340, 20, 1.0f);                                                   // Dibuja una nube mediana en la parte derecha del cielo
            DibujarNube(g, 190, 50, 0.6f);                                                   // Dibuja una nube aún más pequeña en el centro

            // Burbujas
            foreach (var (bx, by, _, bc, bvida) in burbujas)                                  // Itera todas las burbujas activas (ignora la velocidad con _)
            {
                int alpha = (int)(bvida / 1.5f * 130);                                       // Calcula la opacidad proporcional al tiempo de vida restante (máximo 130 de 255)
                using var brB = new SolidBrush(Color.FromArgb(Math.Clamp(alpha, 0, 130), bc)); // Pincel del color de la burbuja con opacidad calculada
                using var penB = new Pen(Color.FromArgb(Math.Clamp(alpha + 40, 0, 200), bc), 1.5f); // Lápiz del borde de la burbuja con opacidad ligeramente mayor
                g.FillEllipse(brB, bx - 8, by - 8, 16, 16);                                 // Dibuja el relleno circular de la burbuja
                g.DrawEllipse(penB, bx - 8, by - 8, 16, 16);                                // Dibuja el borde circular de la burbuja
            }

            // Mascota
            int mascX = mascota.Animo == EstadoAnimo.Escapando                               // Calcula la posición X de la mascota: si está escapando usa su posición de escape
                ? (int)mascota.EscapeX                                                        // Posición X dinámica durante la animación de escape
                : panelJuego.Width / 2;                                                       // Centro del panel si no está escapando
            int mascY = panelJuego.Height - 50;                                               // Posición Y: parada sobre el suelo (50px desde abajo del panel)

            if (mascota.EscapeVisible)                                                         // Solo dibuja la mascota si aún es visible (no terminó de salir de pantalla)
                DibujadorMascota.Dibujar(e.Graphics, mascota, mascX, mascY);                 // Delega el dibujo de la mascota al dibujador estático

            // Overlay pausa
            if (!timerJuego.Enabled)                                                          // Si el timer está detenido, el juego está pausado
                DibujarOverlay(g, "⏸  PAUSADO", "Presiona PAUSA para continuar",             // Muestra el overlay de pausa con su título y subtítulo
                    Color.FromArgb(130, 200, 220, 255),                                       // Fondo azul semitransparente del overlay de pausa
                    Color.FromArgb(100, 60, 200));                                            // Color morado oscuro para el título del overlay de pausa

            // Overlay escapado
            if (mascota.Animo == EstadoAnimo.Escapando && !mascota.EscapeVisible)            // Si la mascota escapó y ya no es visible en pantalla
                DibujarOverlay(g, "🚨 ¡SE ESCAPÓ!",                                          // Muestra el overlay de escape con mensaje de alerta
                    $"¡{mascota.Nombre} se fue de aburrimiento!\nPresiona RESET para empezar de nuevo", // Subtítulo con nombre dinámico y instrucción
                    Color.FromArgb(150, 255, 200, 220),                                       // Fondo rosado semitransparente del overlay de escape
                    C_NEON_ROSA);                                                             // Título en rosa neón llamativo
        }

        private static void DibujarNube(Graphics g, int cx, int cy, float escala)            // Método auxiliar estático que dibuja una nube simple usando 3 elipses superpuestas
        {
            using var br = new SolidBrush(Color.FromArgb(200, 255, 255, 255));               // Pincel blanco semitransparente para las nubes (alpha 200)
            int r = (int)(30 * escala);                                                       // Radio base de la nube escalado según el parámetro de escala
            g.FillEllipse(br, cx - r, cy - r / 2, r * 2, r);                                // Dibuja la elipse central y más larga de la nube (el cuerpo principal)
            g.FillEllipse(br, cx - r / 2, cy - r, (int)(r * 1.4f), r);                      // Dibuja la elipse superior izquierda más alta (la "panza" de la nube)
            g.FillEllipse(br, cx + r / 2 - 5, cy - r / 2, r, r);                            // Dibuja la elipse superior derecha para completar la forma de nube
        }

        private void DibujarOverlay(Graphics g, string titulo, string subtitulo,             // Método que dibuja un overlay semitransparente con título y subtítulo centrados
                                     Color colorFondo, Color colorTitulo)                     // Recibe el color de fondo y el color del título como parámetros
        {
            using var fondo = new SolidBrush(colorFondo);                                    // Pincel del color de fondo semitransparente del overlay
            g.FillRectangle(fondo, 0, 0, panelJuego.Width, panelJuego.Height);               // Cubre todo el panel con el color de fondo semitransparente

            int rw = 420, rh = 160;                                                           // Define el ancho y alto del rectángulo de la caja de mensaje
            int rx = (panelJuego.Width - rw) / 2;                                            // Calcula la posición X para centrar la caja horizontalmente
            int ry = (panelJuego.Height - rh) / 2;                                           // Calcula la posición Y para centrar la caja verticalmente

            using var caja = new SolidBrush(Color.FromArgb(220, 255, 250, 255));             // Pincel blanco lavanda casi opaco para el relleno de la caja
            using var borde = new Pen(C_NEON_ROSA, 2.5f);                                    // Lápiz rosa neón de 2.5px para el borde de la caja
            using var glow = new Pen(Color.FromArgb(80, 255, 100, 180), 7f);                 // Lápiz rosa muy semitransparente de 7px para el efecto de resplandor exterior

            g.FillRectangle(caja, rx, ry, rw, rh);                                           // Dibuja el relleno de la caja de mensaje
            g.DrawRectangle(borde, rx, ry, rw, rh);                                          // Dibuja el borde rosa neón de la caja
            g.DrawRectangle(glow, rx, ry, rw, rh);                                           // Dibuja el resplandor difuso alrededor de la caja

            var sf = new StringFormat                                                          // Crea el formato de texto para el centrado dentro de la caja
            {
                Alignment = StringAlignment.Center,                                            // Alineación horizontal centrada
                LineAlignment = StringAlignment.Near                                           // Alineación vertical desde arriba (para controlar manualmente la posición)
            };

            using var fT = new Font("Consolas", 20f, FontStyle.Bold);                        // Fuente grande y en negrita para el título del overlay
            using var bT = new SolidBrush(colorTitulo);                                      // Pincel del color del título recibido como parámetro
            g.DrawString(titulo, fT, bT,                                                      // Dibuja el texto del título
                new RectangleF(rx, ry + 14, rw, 55), sf);                                    // Dentro del área superior de la caja, con margen de 14px desde arriba

            using var fS = new Font("Consolas", 9.5f);                                       // Fuente más pequeña para el subtítulo del overlay
            using var bS = new SolidBrush(C_TEXTO);                                          // Pincel morado oscuro para el subtítulo
            g.DrawString(subtitulo, fS, bS,                                                   // Dibuja el texto del subtítulo
                new RectangleF(rx, ry + 75, rw, 80), sf);                                    // Debajo del título, con margen de 75px desde arriba de la caja
        }

        private void MostrarEscapo()                                                           // Método llamado cuando la mascota activa el evento de escape
        {
            timerJuego.Stop();                                                                 // Detiene el bucle de juego al escapar la mascota
            btnPausa.Enabled = false;                                                          // Deshabilita el botón de pausa porque ya no hay juego activo
            panelJuego.Invalidate();                                                           // Fuerza un redibujado para mostrar el overlay de escape
        }
    }
}