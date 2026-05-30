using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using BurdiGames.Clases.Juegos;

namespace BurdiGames.UI                                                        
{
    public class FormPacman : Form                                              // Clase que representa la ventana del juego Pac-Girl Rosa, hereda de Form
    {
        private MotorPacman _motor;                                             // Referencia al motor de juego que contiene toda la lógica de Pac-Man
        private System.Windows.Forms.Timer _timerJuego;                        // Temporizador que dispara el bucle del juego a intervalos regulares
        private Panel _canvas;                                                  // Panel donde se dibuja visualmente el juego (mapa, personajes, etc.)
        private Label _lblPuntuacion;                                           // Label del HUD que muestra la puntuación actual del jugador
        private Label _lblVidas;                                                // Label del HUD que muestra las vidas restantes como corazones
        private Label _lblNivel;                                                // Label del HUD que muestra el nivel actual de la partida
        private Label _lblOverlay;                                              // Label semitransparente que se superpone para mostrar mensajes de estado (Game Over, Ganaste, Ouch)
        private Panel _panelHud;                                                // Panel superior que contiene los labels de puntuación, nivel y vidas
        private Button _btnVolver;                                              // Botón para cerrar el juego y volver al menú principal
        private Button _btnReiniciar;                                           // Botón para reiniciar la partida desde cero (visible solo en Game Over)
        private Button _btnPausa;                                               // Botón para pausar o reanudar el juego
        private Label _lblPausa;                                                // Label semitransparente que se superpone cuando el juego está pausado

        private float _pacLerpX, _pacLerpY;                                    // Coordenadas interpoladas (lerp) del jugador para suavizar su movimiento visual

        public FormPacman()                                                     // Constructor de la ventana del juego
        {
            InicializarMotor();                                                 // Crea el motor de juego y suscribe los eventos
            InicializarUI();                                                    // Construye y configura todos los controles visuales de la ventana
            IniciarTimer();                                                     // Crea y arranca el temporizador del bucle de juego
            this.Focus();                                                       // Da foco a la ventana para que el teclado funcione de inmediato
        }

        // ── Motor ─────────────────────────────────────────────────
        private void InicializarMotor()                                         // Método que crea el motor y conecta sus eventos a la interfaz
        {
            _motor = new MotorPacman();                                         // Instancia un nuevo motor de juego con el estado inicial
            _motor.AlCambiarPuntuacion += _ => ActualizarHUD();                 // Cuando cambia la puntuación, actualiza el HUD automáticamente
            _motor.AlCambiarEstado += MostrarOverlay;                           // Cuando cambia el estado del juego, muestra el overlay correspondiente
            _pacLerpX = _motor.Jugador.Posicion.X;                             // Inicializa la posición X de interpolación con la posición real del jugador
            _pacLerpY = _motor.Jugador.Posicion.Y;                             // Inicializa la posición Y de interpolación con la posición real del jugador
        }

        // ── UI ────────────────────────────────────────────────────
        private void InicializarUI()                                            // Método que crea y configura todos los controles visuales de la ventana
        {
            int mapaW = MapaPacman.Cols * MapaPacman.Celda;                    // Calcula el ancho total del mapa en píxeles (columnas × tamaño de celda)
            int mapaH = MapaPacman.Filas * MapaPacman.Celda;                   // Calcula el alto total del mapa en píxeles (filas × tamaño de celda)

            this.Text = "🌸 Pac-Girl Rosa 🌸";                                 // Establece el título de la ventana con emojis decorativos
            this.FormBorderStyle = FormBorderStyle.FixedSingle;                // Borde fijo sin posibilidad de redimensionar la ventana
            this.MaximizeBox = false;                                           // Deshabilita el botón de maximizar la ventana
            this.StartPosition = FormStartPosition.CenterScreen;               // La ventana aparece centrada en la pantalla al abrirse
            this.BackColor = Color.FromArgb(255, 230, 245);                    // Color de fondo de la ventana: rosa pastel muy claro
            this.ClientSize = new Size(mapaW, mapaH + 100);                    // Tamaño de la ventana: ancho del mapa × alto del mapa más 100px para HUD y botones
            this.KeyDown += Form_KeyDown;                                       // Suscribe el manejador de teclado para controlar el juego con flechas
            this.KeyPreview = true;                                             // Permite que la ventana capture las teclas antes que sus controles hijos

            // HUD
            _panelHud = new Panel { Location = new Point(0, 0), Size = new Size(mapaW, 60), BackColor = PaletaPacman.HudFondo }; // Crea el panel del HUD en la parte superior con el color de fondo de la paleta
            _lblPuntuacion = CrearLabelHud("⭐ 0", 10, 15);                    // Crea el label de puntuación alineado a la izquierda del HUD
            _lblNivel = CrearLabelHud("🌸 Nivel 1", mapaW / 2 - 40, 15);      // Crea el label de nivel centrado horizontalmente en el HUD
            _lblVidas = CrearLabelHud("🩷 🩷 🩷", mapaW - 130, 15);           // Crea el label de vidas alineado a la derecha del HUD (3 corazones iniciales)
            _panelHud.Controls.AddRange(new Control[] { _lblPuntuacion, _lblNivel, _lblVidas }); // Agrega los tres labels al panel HUD de una sola vez
            this.Controls.Add(_panelHud);                                      // Agrega el panel HUD a la ventana principal

            // Canvas
            _canvas = new Panel                                                 // Crea el panel de dibujo donde se renderizará todo el juego
            {
                Location = new Point(0, 60),                                   // Posicionado justo debajo del HUD (60px desde arriba)
                Size = new Size(mapaW, mapaH),                                 // Ocupa exactamente el tamaño del mapa
                BackColor = PaletaPacman.FondoColor,                           // Color de fondo inicial del canvas según la paleta del juego
                TabStop = false,                                                // El canvas no recibe foco al presionar Tab (para no interferir con el teclado)
            };
            typeof(Panel).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)! // Accede a la propiedad privada DoubleBuffered del Panel por reflexión
                .SetValue(_canvas, true);                                       // Activa el doble buffer en el canvas para eliminar el parpadeo al redibujar
            _canvas.Paint += Canvas_Paint;                                      // Suscribe el método de dibujo personalizado al evento Paint del canvas
            this.Controls.Add(_canvas);                                        // Agrega el canvas a la ventana principal

            // Overlay principal (Game Over / Ganaste / Ouch)
            _lblOverlay = new Label                                             // Crea el label semitransparente que cubre el canvas con mensajes de estado
            {
                AutoSize = false,                                               // Tamaño fijo, no se ajusta al texto
                Size = new Size(mapaW, mapaH),                                 // Cubre exactamente todo el área del canvas
                Location = new Point(0, 60),                                   // Mismo origen que el canvas (debajo del HUD)
                TextAlign = ContentAlignment.MiddleCenter,                     // El texto siempre aparece centrado vertical y horizontalmente
                Font = new Font("Segoe UI", 18, FontStyle.Bold),               // Fuente grande y en negrita para los mensajes de estado
                ForeColor = PaletaPacman.TextoColor,                           // Color del texto según la paleta del juego
                BackColor = Color.FromArgb(160, 255, 230, 242),                // Fondo rosa semitransparente (alpha 160 de 255)
                Visible = false                                                 // Invisible por defecto, solo se muestra en eventos de estado
            };
            this.Controls.Add(_lblOverlay);                                    // Agrega el overlay a la ventana
            _lblOverlay.BringToFront();                                        // Lo trae al frente para que quede por encima del canvas y demás controles

            // Botón Volver
            _btnVolver = new Button                                             // Crea el botón para salir del juego y volver al menú
            {
                Text = "⬅ Volver al Menú",                                    // Texto del botón con flecha decorativa
                Location = new Point(10, mapaH + 68),                         // Posicionado en la barra inferior, debajo del canvas
                Size = new Size(160, 26),                                      // Tamaño del botón
                FlatStyle = FlatStyle.Flat,                                    // Estilo plano sin relieve
                BackColor = Color.FromArgb(255, 182, 213),                     // Fondo rosa claro
                ForeColor = Color.FromArgb(180, 60, 100),                      // Texto rojo rosado oscuro
                Font = new Font("Segoe UI", 9, FontStyle.Bold),                // Fuente pequeña en negrita
                Cursor = Cursors.Hand                                           // Cursor de mano al pasar por encima
            };
            _btnVolver.FlatAppearance.BorderSize = 0;                          // Sin borde visible en el botón
            _btnVolver.Click += (s, e) => { _timerJuego?.Stop(); this.Close(); }; // Al hacer clic: detiene el timer y cierra la ventana del juego
            this.Controls.Add(_btnVolver);                                     // Agrega el botón a la ventana

            // Botón Reiniciar (oculto hasta game over)
            _btnReiniciar = new Button                                          // Crea el botón para reiniciar la partida
            {
                Text = "🔄 Volver a intentar",                                 // Texto del botón con emoji de recarga
                Location = new Point(185, mapaH + 68),                        // Posicionado a la derecha del botón Volver en la barra inferior
                Size = new Size(160, 26),                                      // Mismo tamaño que el botón Volver
                FlatStyle = FlatStyle.Flat,                                    // Estilo plano sin relieve
                BackColor = Color.FromArgb(255, 182, 213),                     // Mismo fondo rosa claro
                ForeColor = Color.FromArgb(180, 60, 100),                      // Mismo texto rojo rosado oscuro
                Font = new Font("Segoe UI", 9, FontStyle.Bold),                // Fuente pequeña en negrita
                Cursor = Cursors.Hand,                                          // Cursor de mano al pasar por encima
                Visible = false                                                 // Oculto por defecto, solo aparece en estado Game Over
            };
            _btnReiniciar.FlatAppearance.BorderSize = 0;                       // Sin borde visible en el botón
            _btnReiniciar.Click += (s, e) => ReiniciarJuego();                 // Al hacer clic, llama al método que reinicia la partida completa
            this.Controls.Add(_btnReiniciar);                                  // Agrega el botón a la ventana

            // Botón Pausa
            _btnPausa = new Button                                              // Crea el botón para pausar y reanudar el juego
            {
                Text = "⏸ Pausa",                                             // Texto inicial con icono de pausa
                Location = new Point(360, mapaH + 68),                        // Posicionado a la derecha del botón Reiniciar en la barra inferior
                Size = new Size(110, 26),                                      // Botón más pequeño que los anteriores
                FlatStyle = FlatStyle.Flat,                                    // Estilo plano sin relieve
                BackColor = Color.FromArgb(255, 182, 213),                     // Mismo fondo rosa claro
                ForeColor = Color.FromArgb(180, 60, 100),                      // Mismo texto rojo rosado oscuro
                Font = new Font("Segoe UI", 9, FontStyle.Bold),                // Fuente pequeña en negrita
                Cursor = Cursors.Hand                                           // Cursor de mano al pasar por encima
            };
            _btnPausa.FlatAppearance.BorderSize = 0;                           // Sin borde visible en el botón
            _btnPausa.Click += (s, e) => TogglePausa();                        // Al hacer clic, alterna entre pausado y reanudado
            this.Controls.Add(_btnPausa);                                      // Agrega el botón a la ventana

            // Overlay pausa
            _lblPausa = new Label                                               // Crea el label semitransparente que se muestra cuando el juego está pausado
            {
                AutoSize = false,                                               // Tamaño fijo, no se ajusta al texto
                Size = new Size(mapaW, mapaH),                                 // Cubre exactamente todo el área del canvas
                Location = new Point(0, 60),                                   // Mismo origen que el canvas
                TextAlign = ContentAlignment.MiddleCenter,                     // Texto centrado vertical y horizontalmente
                Font = new Font("Segoe UI", 20, FontStyle.Bold),               // Fuente grande en negrita, algo más grande que el overlay principal
                ForeColor = PaletaPacman.TextoColor,                           // Color del texto según la paleta
                BackColor = Color.FromArgb(180, 255, 240, 248),                // Fondo ligeramente más opaco que el overlay principal (alpha 180)
                Text = "⏸ PAUSA\n\nP o ESC para continuar",                   // Texto fijo con instrucciones para salir de la pausa
                Visible = false                                                 // Invisible por defecto, solo aparece cuando el juego está pausado
            };
            this.Controls.Add(_lblPausa);                                      // Agrega el overlay de pausa a la ventana
            _lblPausa.BringToFront();                                          // Lo trae al frente para que quede por encima de todos los demás controles
        }

        private Label CrearLabelHud(string texto, int x, int y) => new Label   // Método auxiliar que crea y devuelve un label con el estilo estándar del HUD
        {
            Text = texto,                                                       // Texto inicial del label
            Font = new Font("Segoe UI", 12, FontStyle.Bold),                   // Fuente mediana en negrita para el HUD
            ForeColor = PaletaPacman.TextoColor,                               // Color del texto según la paleta del juego
            AutoSize = true,                                                    // El label se ajusta automáticamente al contenido
            Location = new Point(x, y),                                        // Posición recibida como parámetro
            BackColor = Color.Transparent                                       // Fondo transparente para que se vea el fondo del panel HUD
        };

        // ── HUD ───────────────────────────────────────────────────
        private void ActualizarHUD()                                            // Método que refresca los valores mostrados en el HUD (puntuación, nivel y vidas)
        {
            if (this.InvokeRequired) { this.Invoke(ActualizarHUD); return; }   // Si se llama desde un hilo distinto al principal, lo reencamina al hilo de UI
            _lblPuntuacion.Text = $"⭐ {_motor.Puntuacion}";                   // Actualiza el texto de puntuación con el valor actual del motor
            _lblNivel.Text = $"🌸 Nivel {_motor.Nivel}";                       // Actualiza el texto del nivel con el nivel actual del motor
            _lblVidas.Text = string.Join(" ", Enumerable.Repeat("🩷", Math.Max(0, _motor.Jugador.Vidas))); // Genera tantos corazones como vidas tenga el jugador (mínimo 0)
        }

        // ── Overlay ───────────────────────────────────────────────
        private void MostrarOverlay(string estado)                              // Método que muestra u oculta el overlay según el estado del juego recibido
        {
            if (this.InvokeRequired) { this.Invoke(() => MostrarOverlay(estado)); return; } // Si se llama desde otro hilo, lo reencamina al hilo de UI con el estado capturado

            switch (estado)                                                     // Evalúa el estado del juego para determinar qué mensaje mostrar
            {
                case "ganaste":                                                 // El jugador completó el nivel al comer todos los puntos
                    _lblOverlay.Text = "🌸 ¡Ganaste! 🌸\n\nPresiona ENTER para el siguiente nivel"; // Mensaje de victoria con instrucción para avanzar al siguiente nivel
                    _lblOverlay.Visible = true;                                // Hace visible el overlay sobre el canvas
                    _btnReiniciar.Visible = false;                             // Oculta el botón de reiniciar (no aplica en victoria)
                    this.Focus();                                               // Devuelve el foco a la ventana para capturar la tecla ENTER
                    break;

                case "gameover":                                                // El jugador perdió todas sus vidas
                    _lblOverlay.Text = $"💔 Game Over 💔\n\n⭐ Puntaje final: {_motor.Puntuacion}\n\n¿Volver a intentar?"; // Mensaje de derrota mostrando el puntaje final obtenido
                    _lblOverlay.Visible = true;                                // Hace visible el overlay sobre el canvas
                    _btnReiniciar.Visible = true;                              // Muestra el botón de reiniciar solo en Game Over
                    _btnReiniciar.BringToFront();                              // Trae el botón al frente para que sea clickeable sobre el overlay
                    this.Focus();                                               // Devuelve el foco a la ventana
                    break;

                case "muerto":                                                  // El jugador fue atrapado por un fantasma pero aún tiene vidas
                    _lblOverlay.Text = "💫 ¡Ouch! 💫";                        // Muestra un mensaje de golpe breve
                    _lblOverlay.Visible = true;                                // Hace visible el overlay con el mensaje "Ouch"
                    var t = new System.Windows.Forms.Timer { Interval = 1200 }; // Crea un temporizador de un solo uso que se dispara después de 1.2 segundos
                    t.Tick += (s, e) =>                                        // Define lo que ocurre cuando el temporizador dispara
                    {
                        if (_motor.Estado == "jugando")                        // Solo oculta el overlay si el juego sigue activo (no derivó en Game Over)
                            _lblOverlay.Visible = false;                       // Oculta el mensaje "Ouch" tras los 1.2 segundos
                        t.Stop();                                               // Detiene el temporizador para que no vuelva a disparar
                        t.Dispose();                                            // Libera los recursos del temporizador temporal
                    };
                    t.Start();                                                  // Inicia el temporizador de 1.2 segundos
                    break;

                default:                                                        // Cualquier otro estado no reconocido (por ejemplo "jugando")
                    _lblOverlay.Visible = false;                               // Oculta el overlay principal
                    _btnReiniciar.Visible = false;                             // Oculta el botón de reiniciar
                    break;
            }
        }

        // ── Timer y bucle ─────────────────────────────────────────
        private void IniciarTimer()                                             // Método que crea y arranca el temporizador del bucle principal de juego
        {
            _timerJuego = new System.Windows.Forms.Timer { Interval = 100 };  // Crea un timer que dispara cada 100 ms (10 veces por segundo)
            _timerJuego.Tick += BucleJuego;                                    // Suscribe el método del bucle al evento Tick del timer
            _timerJuego.Start();                                               // Inicia el temporizador para comenzar el bucle de juego
        }

        private void BucleJuego(object? s, EventArgs e)                        // Método que se ejecuta cada 100 ms como bucle principal del juego
        {
            _motor.Tick();                                                      // Avanza un paso la lógica del juego (movimiento, colisiones, puntos, etc.)

            if (!_motor.Pausado)                                               // Solo interpola la posición visual si el juego no está pausado
            {
                _pacLerpX += (_motor.Jugador.Posicion.X - _pacLerpX) * 0.5f;  // Interpola la posición X del jugador al 50% hacia la posición real (suaviza el movimiento)
                _pacLerpY += (_motor.Jugador.Posicion.Y - _pacLerpY) * 0.5f;  // Interpola la posición Y del jugador al 50% hacia la posición real (suaviza el movimiento)
            }

            _canvas.Invalidate();                                              // Marca el canvas como sucio para forzar su redibujado en el próximo ciclo de UI
        }

        // ── Dibujo ────────────────────────────────────────────────
        private void Canvas_Paint(object? sender, PaintEventArgs e)            // Método de dibujo llamado automáticamente cada vez que el canvas se invalida
        {
            var g = e.Graphics;                                                 // Obtiene el objeto Graphics para dibujar sobre el canvas
            g.SmoothingMode = SmoothingMode.AntiAlias;                         // Activa el suavizado de bordes para dibujos más limpios
            g.CompositingQuality = CompositingQuality.HighSpeed;               // Prioriza la velocidad sobre la calidad máxima de composición

            using var fondo = new LinearGradientBrush(                         // Crea un pincel con gradiente vertical para el fondo del canvas
                _canvas.ClientRectangle,                                        // El gradiente abarca todo el área del canvas
                PaletaPacman.FondoColor,                                       // Color superior: color de fondo de la paleta
                Color.FromArgb(255, 250, 240),                                 // Color inferior: blanco cremoso ligeramente cálido
                LinearGradientMode.Vertical);                                  // Dirección del gradiente: de arriba hacia abajo
            g.FillRectangle(fondo, _canvas.ClientRectangle);                   // Dibuja el fondo degradado en todo el canvas

            _motor.Mapa.Dibujar(g);                                            // Delega al mapa la tarea de dibujarse (paredes, puntos, power-ups, etc.)

            foreach (var fantasma in _motor.Fantasmas)                         // Itera sobre todos los fantasmas activos en el motor
                fantasma.Dibujar(g, fantasma.Posicion.X, fantasma.Posicion.Y); // Dibuja cada fantasma en su posición lógica actual (sin interpolación)

            _motor.Jugador.Dibujar(g, _pacLerpX, _pacLerpY);                  // Dibuja al jugador en su posición interpolada (suavizada) para animación fluida
            DibujarDestellos(g);                                               // Dibuja pequeños destellos decorativos aleatorios sobre celdas vacías
        }

        private void DibujarDestellos(Graphics g)                              // Método que añade destellos decorativos dorados en celdas vacías aleatorias del mapa
        {
            var rng = new Random(Environment.TickCount / 200);                 // Crea un generador de números aleatorios con semilla basada en el tiempo (cambia lentamente para que los destellos no parpadeen demasiado rápido)
            var destellos = Enumerable.Range(0, 3)                             // Genera 3 posiciones candidatas para destellos
                .Select(_ => new { X = rng.Next(MapaPacman.Cols), Y = rng.Next(MapaPacman.Filas) }) // Asigna coordenadas aleatorias de celda a cada candidato
                .Where(p => _motor.Mapa.Obtener(p.Y, p.X) == TipoCelda.Vacio) // Filtra: solo se permiten destellos en celdas vacías (no en paredes ni puntos)
                .Take(2);                                                       // Toma como máximo 2 destellos válidos para dibujar

            foreach (var d in destellos)                                       // Itera sobre los destellos válidos seleccionados
            {
                int px = d.X * MapaPacman.Celda + MapaPacman.Celda / 2;       // Calcula el centro horizontal del destello en píxeles
                int py = d.Y * MapaPacman.Celda + MapaPacman.Celda / 2;       // Calcula el centro vertical del destello en píxeles
                using var sb = new SolidBrush(Color.FromArgb(80, 255, 215, 0)); // Crea un pincel dorado semitransparente (alpha 80 de 255)
                g.FillEllipse(sb, px - 3, py - 3, 6, 6);                      // Dibuja un pequeño círculo dorado de 6px de diámetro centrado en la celda
            }
        }

        // ── Teclado ───────────────────────────────────────────────
        private void Form_KeyDown(object? sender, KeyEventArgs e)              // Manejador del evento de tecla presionada para controlar el juego
        {
            switch (e.KeyCode)                                                  // Evalúa qué tecla fue presionada
            {
                case Keys.Up: _motor.EncolarDireccion(DireccionPac.Arriba); break;      // Flecha arriba: encola la dirección hacia arriba en el motor
                case Keys.Down: _motor.EncolarDireccion(DireccionPac.Abajo); break;     // Flecha abajo: encola la dirección hacia abajo en el motor
                case Keys.Left: _motor.EncolarDireccion(DireccionPac.Izquierda); break; // Flecha izquierda: encola la dirección hacia la izquierda en el motor
                case Keys.Right: _motor.EncolarDireccion(DireccionPac.Derecha); break;  // Flecha derecha: encola la dirección hacia la derecha en el motor

                case Keys.Enter:                                               // Tecla Enter: acción contextual según el estado actual del juego
                    if (_motor.Estado == "ganaste")                            // Si el jugador ganó el nivel
                    {
                        _motor.SiguienteNivel();                               // Avanza al siguiente nivel en el motor
                        ActualizarHUD();                                       // Refresca el HUD con los datos del nuevo nivel
                        _lblOverlay.Visible = false;                           // Oculta el mensaje de victoria
                    }
                    else if (_motor.Estado == "gameover")                      // Si el juego terminó por perder todas las vidas
                    {
                        ReiniciarJuego();                                      // Reinicia la partida completa desde el principio
                    }
                    break;

                case Keys.R:                                                   // Tecla R: atajo directo para reiniciar la partida en cualquier momento
                    ReiniciarJuego();
                    break;

                case Keys.P:                                                   // Tecla P: alterna entre pausado y reanudado
                case Keys.Escape:                                              // Tecla Escape: también alterna la pausa (ambas teclas hacen lo mismo)
                    TogglePausa();
                    break;
            }

            e.Handled = true;                                                  // Indica que el evento de teclado fue manejado por este código
            e.SuppressKeyPress = true;                                         // Suprime el sonido del sistema y evita que el evento siga propagándose
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)  // Sobrescribe el procesamiento de teclas de comando para capturar las flechas antes de que lleguen a otros controles
        {
            switch (keyData)                                                    // Evalúa qué tecla de comando fue presionada
            {
                case Keys.Up: _motor.EncolarDireccion(DireccionPac.Arriba); return true;      // Flecha arriba capturada: encola dirección y consume el evento
                case Keys.Down: _motor.EncolarDireccion(DireccionPac.Abajo); return true;     // Flecha abajo capturada: encola dirección y consume el evento
                case Keys.Left: _motor.EncolarDireccion(DireccionPac.Izquierda); return true; // Flecha izquierda capturada: encola dirección y consume el evento
                case Keys.Right: _motor.EncolarDireccion(DireccionPac.Derecha); return true;  // Flecha derecha capturada: encola dirección y consume el evento
            }
            return base.ProcessCmdKey(ref msg, keyData);                       // Para cualquier otra tecla, delega el procesamiento al comportamiento base del formulario
        }

        // ── Acciones ──────────────────────────────────────────────
        private void TogglePausa()                                              // Método que alterna el estado de pausa del juego
        {
            if (_motor.Estado != "jugando") return;                            // Solo permite pausar si el juego está activamente en curso (no en Game Over ni victoria)
            _motor.Pausar();                                                   // Alterna el estado de pausa dentro del motor
            _lblPausa.Visible = _motor.Pausado;                                // Muestra u oculta el overlay de pausa según el nuevo estado
            _btnPausa.Text = _motor.Pausado ? "▶ Reanudar" : "⏸ Pausa";      // Cambia el texto del botón según si está pausado o activo
            if (!_motor.Pausado) this.Focus();                                 // Al reanudar, devuelve el foco a la ventana para que el teclado funcione
        }

        private void ReiniciarJuego()                                          // Método que reinicia la partida completa desde cero
        {
            // 1. Detener timer existente
            _timerJuego?.Stop();                                               // Detiene el timer actual para que no siga ejecutando el bucle de juego

            // 2. Crear motor nuevo con suscripciones frescas
            InicializarMotor();                                                // Crea un motor nuevo con estado inicial y reconecta todos los eventos

            // 3. Limpiar UI
            _lblOverlay.Visible = false;                                       // Oculta el overlay de Game Over / victoria
            _lblPausa.Visible = false;                                         // Oculta el overlay de pausa
            _btnReiniciar.Visible = false;                                     // Oculta el botón de reiniciar
            _btnPausa.Text = "⏸ Pausa";                                       // Restaura el texto del botón de pausa a su estado inicial

            // 4. Actualizar HUD con datos del nuevo motor
            ActualizarHUD();                                                   // Refresca puntuación, nivel y vidas con los valores iniciales del nuevo motor

            // 5. Reiniciar timer
            _timerJuego.Start();                                               // Vuelve a arrancar el timer para comenzar el nuevo bucle de juego

            this.Focus();                                                       // Devuelve el foco a la ventana para que el teclado funcione inmediatamente
        }

        protected override void OnFormClosed(FormClosedEventArgs e)            // Sobrescribe el evento de cierre del formulario para liberar recursos
        {
            _timerJuego?.Stop();                                               // Detiene el timer si aún está activo al cerrar la ventana
            _timerJuego?.Dispose();                                            // Libera los recursos del timer para evitar fugas de memoria
            base.OnFormClosed(e);                                              // Llama al comportamiento base del cierre del formulario
        }
    }
}