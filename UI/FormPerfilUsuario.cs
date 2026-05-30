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
    public partial class FormPerfilUsuario : Form                                               // Clase parcial que representa la ventana del perfil de usuario, hereda de Form
    {
        private readonly Usuario _usuario;                                                     

        #region Colores estilo FormMain

        // Colores compartidos con FormMain
        private Color colorFondo = Color.FromArgb(8, 6, 20);                                  // Color de fondo principal: negro muy oscuro con tinte azul (igual a FormMain)
        private Color colorSidebar = Color.FromArgb(12, 8, 28);                               // Color del panel lateral izquierdo: negro morado muy oscuro
        private Color colorNeon = Color.FromArgb(0, 255, 127);                                // Color neón principal: verde menta brillante (distinto al violeta de FormMain)
        private Color colorNeonCian = Color.FromArgb(0, 220, 255);                            // Color neón cian brillante para detalles decorativos
        private Color colorNeonRosa = Color.FromArgb(220, 50, 180);                           // Color neón rosa para la línea superior y gradientes
        private Color colorMorado = Color.FromArgb(180, 60, 255);                             // Color morado violeta para acentos adicionales
        private Color colorCard = Color.FromArgb(20, 12, 40);                                 // Color de fondo de las tarjetas de logros desbloqueados
        private Color colorCardBorde = Color.FromArgb(60, 30, 100);                           // Color del borde de tarjetas bloqueadas: morado muy oscuro
        private Color colorTexto = Color.FromArgb(200, 190, 230);                             // Color del texto general: blanco lavanda suave
        private Color colorGris = Color.FromArgb(90, 80, 120);                                // Color gris morado para elementos inactivos o bloqueados

        #endregion

        private List<Logro> _logros = new();                                                   // Lista de logros del juego, se inicializa vacía y se llena en InicializarLogros

        public FormPerfilUsuario(Usuario usuario)                                               // Constructor que recibe el usuario cuyo perfil se va a mostrar
        {
            InitializeComponent();                                                             // Inicializa los componentes generados por el diseñador de WinForms
            _usuario = usuario;                                                                // Guarda el usuario recibido en el campo privado

            ConfigurarEstilo();                                                                // Aplica los estilos visuales personalizados (avatar, líneas, separadores)
            InicializarLogros();                                                               // Crea la lista de logros y evalúa cuáles están desbloqueados
            CargarDatosSidebar();                                                              // Rellena el panel lateral con las estadísticas del usuario
            CargarLogros();                                                                    // Genera y muestra las tarjetas de logros en el FlowLayoutPanel
            CargarHistorial();                                                                 // Rellena el ListView con el historial de partidas del usuario
        }


        private void ConfigurarEstilo()                                                        // Método que aplica los estilos visuales personalizados a los controles del formulario
        {
            // Avatar circular
            picAvatar.Paint += (s, e) =>                                                      // Suscribe el pintado personalizado del PictureBox del avatar
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;                          // Activa el suavizado de bordes para el círculo y el icono
                using var brush = new SolidBrush(Color.FromArgb(40, 20, 70));                // Pincel morado oscuro para el relleno del círculo del avatar
                e.Graphics.FillEllipse(brush, 0, 0, picAvatar.Width - 1, picAvatar.Height - 1); // Dibuja el círculo de fondo del avatar
                using var pen = new Pen(colorNeon, 2f);                                      // Lápiz verde neón de 2px para el borde circular del avatar
                e.Graphics.DrawEllipse(pen, 1, 1, picAvatar.Width - 3, picAvatar.Height - 3); // Dibuja el borde circular verde neón del avatar

                // Ícono gamepad simple en el centro
                using var penIcon = new Pen(colorNeon, 1.5f);                                // Lápiz verde neón de 1.5px para dibujar el ícono de gamepad
                var cx = picAvatar.Width / 2;                                                 // Calcula el centro horizontal del avatar
                var cy = picAvatar.Height / 2;                                                // Calcula el centro vertical del avatar
                e.Graphics.DrawRectangle(penIcon, cx - 16, cy - 10, 32, 20);                // Dibuja el cuerpo rectangular del gamepad centrado en el avatar
                e.Graphics.DrawEllipse(penIcon, cx - 4, cy - 4, 8, 8);                      // Dibuja el círculo analógico central del gamepad
                e.Graphics.DrawLine(penIcon, cx - 22, cy, cx - 16, cy);                     // Dibuja el stick analógico izquierdo del gamepad
                e.Graphics.DrawLine(penIcon, cx + 16, cy, cx + 22, cy);                     // Dibuja el stick analógico derecho del gamepad
            };

            // Línea neón superior
            Paint += (s, e) =>                                                                // Suscribe el pintado personalizado del formulario para la línea decorativa superior
            {
                using var brush = new LinearGradientBrush(                                   // Crea un pincel con gradiente horizontal de rosa a cian
                    new Point(0, 0), new Point(Width, 0),                                    // De izquierda a derecha a lo largo de todo el ancho del formulario
                    colorNeonRosa, colorNeonCian);                                            // Gradiente de rosa neón a cian neón
                using var pen = new Pen(brush, 2f);                                          // Lápiz que usa el pincel degradado, de 2px de grosor
                e.Graphics.DrawLine(pen, 0, 1, Width, 1);                                   // Dibuja la línea decorativa en el borde superior del formulario
            };

            // Separador sidebar/contenido
            pnlSidebar.Paint += (s, e) =>                                                    // Suscribe el pintado personalizado del panel lateral para dibujar su borde derecho
            {
                using var pen = new Pen(Color.FromArgb(40, 30, 60), 1f);                    // Lápiz morado muy oscuro y semitransparente de 1px
                e.Graphics.DrawLine(pen, pnlSidebar.Width - 1, 0, pnlSidebar.Width - 1, pnlSidebar.Height); // Dibuja una línea vertical en el borde derecho del sidebar para separarlo del contenido
            };
        }

        // Tabs con estilo igual al FormLogin
        private void TabPerfil_DrawItem(object sender, DrawItemEventArgs e)                   // Manejador de dibujo personalizado para las pestañas del TabControl
        {
            bool activo = e.Index == tabPerfil.SelectedIndex;                                 // Determina si la pestaña que se está dibujando es la seleccionada actualmente
            var bgColor = activo ? colorFondo : Color.FromArgb(14, 10, 30);                  // Fondo negro oscuro para activa, negro más claro para inactivas
            var fgColor = activo ? colorNeon : colorGris;                                     // Texto verde neón para activa, gris para inactivas

            e.Graphics.FillRectangle(new SolidBrush(bgColor), e.Bounds);                    // Rellena el fondo de la pestaña con el color calculado

            if (activo)                                                                       // Solo dibuja el indicador inferior si la pestaña está activa
            {
                var line = new Rectangle(e.Bounds.X, e.Bounds.Bottom - 2, e.Bounds.Width, 2); // Rectángulo de 2px de alto en el borde inferior de la pestaña activa
                e.Graphics.FillRectangle(new SolidBrush(colorNeon), line);                   // Dibuja la barra verde neón indicadora de pestaña activa
            }

            var sf = new StringFormat                                                          // Define el formato de alineación del texto de la pestaña
            {
                Alignment = StringAlignment.Center,                                            // Centrado horizontal del texto
                LineAlignment = StringAlignment.Center                                         // Centrado vertical del texto
            };
            e.Graphics.DrawString(                                                            // Dibuja el texto de la pestaña
                tabPerfil.TabPages[e.Index].Text,                                             // Texto tomado directamente del título de la página de la pestaña
                new Font("Consolas", 8f, FontStyle.Bold),                                    // Fuente monoespaciada pequeña en negrita
                new SolidBrush(fgColor),                                                      // Color del texto: verde neón si activa, gris si inactiva
                e.Bounds, sf);                                                                // Dibujado dentro de los límites de la pestaña con el formato definido
        }

        //Datos del login
        private void CargarDatosSidebar()                                                      // Método que calcula y muestra las estadísticas del usuario en el panel lateral
        {
            lblNombre.Text = _usuario.Nombre.ToUpper();                                       // Muestra el nombre del usuario en mayúsculas
            lblEmail.Text = $"desde {_usuario.FechaRegistro:MMM yyyy}";                      // Muestra el mes y año de registro del usuario (ej: "desde Ene 2024")

            int totalPartidas = _usuario.HistorialPartidas.Count;                             // Cuenta el total de partidas jugadas por el usuario
            int logrosDesbloqueados = _logros.Count(l => l.Desbloqueado);                    // Cuenta cuántos logros tiene desbloqueados el usuario
            int totalLogros = _logros.Count;                                                  // Cuenta el total de logros existentes en el juego
            int mejorPuntaje = _usuario.HistorialPartidas                                     // Calcula el mejor puntaje entre todas las partidas del usuario
                .Where(p => p.Puntaje.HasValue)                                               // Filtra solo partidas que tienen puntaje registrado
                .Select(p => p.Puntaje!.Value)                                                // Extrae los valores de puntaje
                .DefaultIfEmpty(0).Max();                                                     // Devuelve 0 si no hay partidas, o el máximo puntaje si las hay
            int juegosUnicos = _usuario.HistorialPartidas                                     // Calcula cuántos juegos distintos ha jugado el usuario
                .Select(p => p.Juego.Nombre)                                                  // Extrae el nombre del juego de cada partida
                .Distinct().Count();                                                          // Elimina duplicados y cuenta los juegos únicos

            lblValPartidas.Text = totalPartidas.ToString();                                   // Muestra el total de partidas en su label correspondiente
            lblValLogros.Text = $"{logrosDesbloqueados}/{totalLogros}";                      // Muestra el progreso de logros como "X/Y"
            lblValMejor.Text = mejorPuntaje > 0 ? $"{mejorPuntaje / 1000f:0.0}k" : "—";    // Muestra el mejor puntaje en miles (ej: "12.5k") o "—" si no hay puntaje
            lblValJuegos.Text = juegosUnicos.ToString();                                      // Muestra la cantidad de juegos únicos jugados
        }

        // LOGROS — definición y evaluación
        private void InicializarLogros()                                                       // Método que crea la lista de logros disponibles y los evalúa
        {
            _logros = new List<Logro>                                                          // Inicializa la lista con todos los logros del juego
            {
                new Logro { Nombre = "PRIMER JUEGO",   Descripcion = "Juega tu primera partida",        Icono = "🏆" }, // Logro por jugar la primera partida
                new Logro { Nombre = "VELOCISTA",      Descripcion = "Termina una partida en menos de 1 min", Icono = "⚡" }, // Logro por terminar una partida rápido
                new Logro { Nombre = "CHICAGAMER",      Descripcion = "5 partidas seguidas",             Icono = "🔥" }, // Logro por jugar 5 partidas consecutivas
                new Logro { Nombre = "MAESTRO",        Descripcion = "Alcanza 50.000 pts en cualquier juego", Icono = "👑" }, // Logro por alcanzar 50.000 puntos               
                new Logro { Nombre = "DUEÑA DEL BURDEL",        Descripcion = "Desbloquea todos los logros",     Icono = "💎" }, // Logro maestro por desbloquear todos los demás
            };

            EvaluarLogros();                                                                  // Evalúa inmediatamente cuáles logros ya están desbloqueados para el usuario
        }

        private void EvaluarLogros()                                                           // Método que revisa las condiciones de cada logro y los desbloquea si se cumplen
        {
            var partidas = _usuario.HistorialPartidas;                                        // Referencia local al historial de partidas para simplificar el acceso

            // Primer juego
            if (partidas.Count >= 1)                                                          // Condición: el usuario ha jugado al menos 1 partida
                _logros.Find(l => l.Nombre == "PRIMER JUEGO")?.Desbloquear();                // Desbloquea el logro "PRIMER JUEGO" si existe en la lista

            // 5 partidas seguidas
            if (partidas.Count >= 5)                                                          // Condición: el usuario ha jugado al menos 5 partidas
                _logros.Find(l => l.Nombre == "EN LLAMAS")?.Desbloquear();                   // Busca el logro "EN LLAMAS" (nota: el nombre no coincide con "CHICAGAMER" en la lista)

            // Maestro: 50.000+ pts
            if (partidas.Any(p => p.Puntaje >= 50000))                                       // Condición: alguna partida tiene 50.000 puntos o más
                _logros.Find(l => l.Nombre == "MAESTRO")?.Desbloquear();                     // Desbloquea el logro "MAESTRO"

            // Leyenda: todos desbloqueados (excepto sí mismo)
            var sinLeyenda = _logros.Where(l => l.Nombre != "DUEÑA DEL BURDEL");            // Filtra todos los logros excepto el logro maestro
            if (sinLeyenda.All(l => l.Desbloqueado))                                         // Condición: todos los logros (excepto el maestro) están desbloqueados
                _logros.Find(l => l.Nombre == "DUEÑA DEL BURDEL")?.Desbloquear();            // Desbloquea el logro maestro "DUEÑA DEL BURDEL"
        }

        // TARJETAS DE LOGROS
        private void CargarLogros()                                                            // Método que limpia y regenera todas las tarjetas de logros en el FlowLayoutPanel
        {
            flowLogros.Controls.Clear();                                                      // Elimina todas las tarjetas de logros previamente mostradas
            foreach (var logro in _logros)                                                    // Itera sobre todos los logros definidos
                flowLogros.Controls.Add(CrearTarjetaLogro(logro));                           // Crea y agrega la tarjeta visual de cada logro
        }

        private Panel CrearTarjetaLogro(Logro logro)                                          // Método que construye y devuelve el panel visual (tarjeta) de un logro específico
        {
            bool desbloqueado = logro.Desbloqueado;                                          // Captura el estado de desbloqueo para usarlo en múltiples sitios de este método

            var card = new Panel                                                               // Crea el contenedor principal de la tarjeta del logro
            {
                Width = 265,                                                                  // Ancho de la tarjeta en píxeles
                Height = 88,                                                                  // Alto de la tarjeta en píxeles
                BackColor = desbloqueado ? colorCard : Color.FromArgb(14, 10, 24),           // Fondo más claro si desbloqueado, más oscuro si bloqueado
                Margin = new Padding(8)                                                       // Margen exterior de 8px para separar las tarjetas entre sí
            };

            // Borde neón si está desbloqueado
            card.Paint += (s, e) =>                                                           // Suscribe el pintado personalizado para los bordes decorativos de la tarjeta
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;                          // Activa el suavizado de bordes para líneas más limpias
                var borderColor = desbloqueado ? colorNeon : colorCardBorde;                 // Color verde neón si desbloqueado, morado oscuro si bloqueado
                using var pen = new Pen(borderColor, 1.2f);                                  // Lápiz del color calculado con 1.2px de grosor
                e.Graphics.DrawRectangle(pen, 1, 1, card.Width - 3, card.Height - 3);       // Dibuja el borde exterior de la tarjeta

                if (desbloqueado)                                                             // Solo dibuja las esquinas decorativas si el logro está desbloqueado
                {
                    int c = 10;                                                               // Longitud de los trazos de esquina en píxeles
                    using var penCorner = new Pen(colorNeonCian, 2f);                        // Lápiz cian neón de 2px para los adornos de esquinas
                    e.Graphics.DrawLine(penCorner, 1, 1, 1 + c, 1);                         // Trazo horizontal de la esquina superior izquierda
                    e.Graphics.DrawLine(penCorner, 1, 1, 1, 1 + c);                         // Trazo vertical de la esquina superior izquierda
                    e.Graphics.DrawLine(penCorner, card.Width - 2 - c, 1, card.Width - 2, 1); // Trazo horizontal de la esquina superior derecha
                    e.Graphics.DrawLine(penCorner, card.Width - 2, 1, card.Width - 2, 1 + c); // Trazo vertical de la esquina superior derecha
                }
            };

            // Ícono
            var lblIcono = new Label                                                           // Crea el label que muestra el emoji del logro
            {
                Text = logro.Icono ?? "🎮",                                                   // Emoji del logro, o gamepad por defecto si no tiene uno asignado
                Font = new Font("Segoe UI Emoji", 18f),                                      // Fuente grande con soporte de emojis
                ForeColor = desbloqueado ? colorNeon : colorGris,                            // Verde neón si desbloqueado, gris si bloqueado
                AutoSize = false,                                                              // Tamaño fijo para alineación consistente
                Size = new Size(48, 48),                                                     // Tamaño cuadrado para el emoji
                Location = new Point(10, 20),                                                // Posicionado en el lado izquierdo de la tarjeta
                TextAlign = ContentAlignment.MiddleCenter                                     // Emoji centrado dentro del label
            };

            var lblNombreLogro = new Label                                                     // Crea el label con el nombre del logro
            {
                Text = logro.Nombre,                                                          // Nombre del logro en mayúsculas
                Font = new Font("Consolas", 9f, FontStyle.Bold),                             // Fuente monoespaciada pequeña en negrita
                ForeColor = desbloqueado ? colorTexto : colorGris,                           // Lavanda claro si desbloqueado, gris si bloqueado
                AutoSize = false,                                                              // Tamaño fijo
                Size = new Size(190, 18),                                                    // Ancho para el texto del nombre
                Location = new Point(64, 16)                                                 // Posicionado a la derecha del emoji
            };

            var lblDescLogro = new Label                                                       // Crea el label con la descripción del logro
            {
                Text = logro.Descripcion,                                                     // Texto descriptivo de la condición del logro
                Font = new Font("Consolas", 7f),                                             // Fuente muy pequeña monoespaciada
                ForeColor = desbloqueado ? Color.FromArgb(120, 110, 160) : Color.FromArgb(60, 55, 80), // Morado medio si desbloqueado, morado muy oscuro si bloqueado
                AutoSize = false,                                                              // Tamaño fijo
                Size = new Size(190, 30),                                                    // Alto suficiente para dos líneas de descripción
                Location = new Point(64, 36)                                                 // Posicionado debajo del nombre del logro
            };

            var lblEstado = new Label                                                          // Crea el label que muestra el estado de bloqueo del logro
            {
                Text = desbloqueado ? "✔ DESBLOQUEADO" : "BLOQUEADO",                       // Texto con checkmark si desbloqueado, solo "BLOQUEADO" si no
                Font = new Font("Consolas", 7f, FontStyle.Bold),                             // Fuente muy pequeña en negrita
                ForeColor = desbloqueado ? colorNeon : Color.FromArgb(70, 60, 90),           // Verde neón si desbloqueado, morado muy oscuro si bloqueado
                AutoSize = false,                                                              // Tamaño fijo
                Size = new Size(190, 16),                                                    // Ancho para el texto de estado
                Location = new Point(64, 64)                                                 // Posicionado en la parte inferior de la tarjeta
            };

            card.Controls.AddRange(new Control[] { lblIcono, lblNombreLogro, lblDescLogro, lblEstado }); // Agrega todos los controles de la tarjeta de una sola vez
            return card;                                                                       // Devuelve la tarjeta completamente construida
        }

        // HISTORIAL
        private void CargarHistorial()                                                         // Método que limpia y regenera el historial de partidas en el ListView
        {
            lstHistorial.Items.Clear();                                                       // Elimina todos los elementos anteriores del ListView

            var partidas = _usuario.HistorialPartidas                                         // Obtiene las partidas del historial del usuario
                .OrderByDescending(p => p.Fecha)                                              // Ordena de más reciente a más antigua
                .ToList();                                                                    // Materializa la consulta en una lista

            if (partidas.Count == 0)                                                          // Si el usuario no tiene ninguna partida registrada
            {
                var item = new ListViewItem("—");                                             // Crea un item de fila con "—" en la primera columna (fecha)
                item.SubItems.Add("Sin partidas registradas");                               // Agrega el mensaje explicativo en la segunda columna (juego)
                item.SubItems.Add("—");                                                      // Agrega "—" en la tercera columna (puntaje)
                item.ForeColor = colorGris;                                                   // Aplica color gris al item vacío
                lstHistorial.Items.Add(item);                                                 // Agrega el item al ListView
                return;                                                                        // Sale del método sin continuar el foreach
            }

            foreach (var partida in partidas)                                                 // Itera sobre todas las partidas ordenadas del historial
            {
                var fecha = partida.Fecha?.ToString("dd/MM/yyyy HH:mm") ?? "—";             // Formatea la fecha como "día/mes/año hora:minutos" o "—" si es nula
                var juego = partida.Juego?.Nombre ?? "—";                                    // Obtiene el nombre del juego o "—" si es nulo
                var puntaje = partida.Puntaje.HasValue ? $"{partida.Puntaje:N0} pts" : "En progreso"; // Formatea el puntaje con separador de miles o "En progreso" si aún no tiene puntaje

                var item = new ListViewItem(fecha);                                           // Crea un nuevo item de fila con la fecha como primera columna
                item.SubItems.Add(juego);                                                    // Agrega el nombre del juego como segunda columna
                item.SubItems.Add(puntaje);                                                  // Agrega el puntaje formateado como tercera columna
                item.ForeColor = colorTexto;                                                  // Aplica el color lavanda claro a la fila
                lstHistorial.Items.Add(item);                                                 // Agrega la fila al ListView
            }
        }

        // EVENTOS
        private void btnEditarPerfil_Click(object sender, EventArgs e)                        // Manejador del clic del botón de editar perfil
        {
            // Por ahora permite cambiar el nombre
            string nuevoNombre = Microsoft.VisualBasic.Interaction.InputBox(                  // Muestra un cuadro de diálogo nativo de Visual Basic para pedir texto al usuario
                "Nuevo nombre de usuario:", "Editar perfil", _usuario.Nombre);               // Mensaje del prompt, título del cuadro y valor inicial (nombre actual)

            if (!string.IsNullOrWhiteSpace(nuevoNombre) && nuevoNombre != _usuario.Nombre)   // Valida que el nuevo nombre no esté vacío y sea diferente al actual
            {
                _usuario.CambiarNombre(nuevoNombre);                                          // Actualiza el nombre del usuario en el modelo a través de su método propio
                lblNombre.Text = nuevoNombre.ToUpper();                                       // Actualiza el label del nombre en el sidebar con el nuevo nombre en mayúsculas

                // Notifica a FormMain que el nombre cambió
                PerfilActualizado?.Invoke(this, EventArgs.Empty);                             // Dispara el evento para que FormMain actualice su label de bienvenida
            }
        }

        // Evento que FormMain escucha para actualizar la bienvenida
        public event EventHandler? PerfilActualizado;                                         // Evento público nullable que FormMain suscribe para enterarse de cambios en el perfil

        // Método público para refrescar datos desde FormMain
        public void Refrescar()                                                                // Método público que recarga todos los datos del perfil (llamado desde FormMain)
        {
            CargarDatosSidebar();                                                             // Recalcula y actualiza las estadísticas del sidebar
            EvaluarLogros();                                                                  // Reevalúa las condiciones de todos los logros
            CargarLogros();                                                                   // Regenera las tarjetas de logros con el estado actualizado
            CargarHistorial();                                                                // Recarga el historial de partidas con los datos más recientes
        }

        protected override void OnPaint(PaintEventArgs e)                                     // Sobrescribe el método de pintado de la ventana para añadir la línea decorativa superior
        {
            base.OnPaint(e);                                                                  // Llama al pintado base de la clase Form
            using var brush = new LinearGradientBrush(                                       // Crea un pincel con gradiente horizontal de rosa a cian
                new Point(0, 0), new Point(Width, 0),                                        // De izquierda a derecha a lo largo de todo el ancho del formulario
                colorNeonRosa, colorNeonCian);                                               // Gradiente de rosa neón a cian neón
            using var pen = new Pen(brush, 2f);                                              // Lápiz que usa el pincel degradado, de 2px de grosor
            e.Graphics.DrawLine(pen, 0, 1, Width, 1);                                       // Dibuja la línea decorativa en el borde superior del formulario
        }
    }
}