using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace MiniJuegos
{
    // Panel con doble buffer para eliminar el flickering
    // Clase que extiende Panel activando el doble búfer para evitar parpadeo al redibujar
    public class PanelSuave : Panel
    {
        // Constructor: configura el panel con doble búfer y estilos de pintura optimizados
        public PanelSuave()
        {
            this.DoubleBuffered = true;  // Activa el doble búfer nativo del control para evitar parpadeo
            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint |   // Evita que el fondo se pinte por separado (reduce parpadeo)
                ControlStyles.UserPaint |              // El control se pinta a sí mismo en lugar de usar el sistema
                ControlStyles.OptimizedDoubleBuffer,   // Activa el búfer optimizado para animaciones suaves
                true);
            this.UpdateStyles(); // Aplica los estilos configurados al control
        }
    }

    // Formulario principal del juego Cohete Espacial con su interfaz visual completa
    public class FormCohete : Form
    {
        private JuegoCohete juego;                                    // Instancia del motor de lógica del juego
        private System.Windows.Forms.Timer timerJuego;               // Timer principal del game loop (~60 FPS con intervalo de 16ms)
        private System.Windows.Forms.Timer timerEstrellas;           // Timer para actualizar el movimiento del fondo estrellado (cada 50ms)
        private DateTime ultimoFrame;                                 // Marca de tiempo del último frame para calcular el delta time

        private readonly HashSet<Keys> teclasPresionadas = new();    // Conjunto de teclas actualmente presionadas (permite movimiento simultáneo)

        private PanelSuave panelJuego;  // Panel de doble búfer donde se dibuja el juego
        private Button btnPausa;        // Botón para pausar o reanudar el juego
        private Button btnReiniciar;    // Botón para reiniciar el juego desde cero
        private Label lblPuntos;        // Etiqueta que muestra la puntuación actual
        private Label lblVidas;         // Etiqueta que muestra las vidas restantes con corazones
        private Label lblNivel;         // Etiqueta que muestra el nivel actual

        private readonly List<(float x, float y, float brillo, float vel)> estrellas = new();            // Lista de estrellas del fondo (posición, brillo y velocidad de cada una)
        private static readonly Random rng = new Random();                                               // Instancia estática de Random para generar valores aleatorios
        private readonly List<(float x, float y, float vx, float vy, float vida, Color color)> particulas = new(); // Lista de partículas de efectos visuales (posición, velocidad, vida y color)

        // Constructor: inicializa los componentes, genera las estrellas, crea el juego y arranca los timers
        public FormCohete()
        {
            InicializarComponentes(); // Crea y configura todos los controles visuales del formulario
            GenerarEstrellas();       // Genera las 140 estrellas del fondo estrellado

            juego = new JuegoCohete(panelJuego.Width, panelJuego.Height); // Crea el motor del juego con las dimensiones del panel
            juego.JuegoTerminado += (s, e) => MostrarGameOver();          // Al terminar el juego, deshabilita el botón de pausa

            timerJuego = new System.Windows.Forms.Timer { Interval = 16 }; // Timer del game loop con intervalo de ~16ms (aprox. 60 FPS)
            timerJuego.Tick += GameLoop;                                    // Suscribe el método GameLoop al tick del timer

            timerEstrellas = new System.Windows.Forms.Timer { Interval = 50 }; // Timer para mover las estrellas cada 50ms
            timerEstrellas.Tick += (s, e) => ActualizarEstrellas();             // Al tick, actualiza las posiciones de las estrellas

            ultimoFrame = DateTime.Now; // Registra el tiempo inicial para el cálculo del primer delta
            timerJuego.Start();         // Inicia el game loop
            timerEstrellas.Start();     // Inicia el movimiento de estrellas

            panelJuego.Focus(); // Da el foco al panel de juego para recibir los eventos de teclado
        }

        // Crea, configura y agrega al formulario todos los controles visuales
        private void InicializarComponentes()
        {
            Text = "🚀 COSMIC FURY";                                   // Título de la ventana del formulario
            ClientSize = new Size(800, 700);                           // Tamaño del área cliente del formulario (800x700)
            BackColor = Color.FromArgb(5, 0, 20);                     // Color de fondo del formulario (azul noche casi negro)
            FormBorderStyle = FormBorderStyle.FixedSingle;            // Borde fijo: no se puede redimensionar
            MaximizeBox = false;                                       // Deshabilita el botón de maximizar
            Font = new Font("Consolas", 9f, FontStyle.Bold);          // Fuente base del formulario (Consolas negrita 9pt)
            KeyPreview = true;                                         // Permite que el formulario reciba eventos de teclado antes que los controles hijos
            KeyDown += Form_KeyDown;                                   // Suscribe el método de tecla presionada al formulario
            KeyUp += Form_KeyUp;                                       // Suscribe el método de tecla soltada al formulario

            // ── Panel HUD superior ──────────────────────────────────
            // Panel que contiene las etiquetas y botones del HUD en la parte superior
            var panelHud = new Panel
            {
                Location = new Point(0, 0),         // Ubicado en la esquina superior izquierda
                Size = new Size(800, 70),            // Ocupa todo el ancho con el alto del HUD
                BackColor = Color.FromArgb(15, 0, 40) // Color de fondo azul-morado muy oscuro
            };
            panelHud.Paint += (s, e) => // Evento Paint del HUD: dibuja una línea neón en su borde inferior
            {
                using var pen = new Pen(Color.FromArgb(180, 0, 255), 2f);      // Pluma morada neón de 2px
                e.Graphics.DrawLine(pen, 0, 69, 800, 69);                       // Dibuja la línea morada en el borde inferior del HUD
                using var glow = new Pen(Color.FromArgb(60, 180, 0, 255), 6f); // Pluma morada semitransparente gruesa para el brillo
                e.Graphics.DrawLine(glow, 0, 68, 800, 68);                      // Dibuja el efecto de brillo justo encima de la línea
            };

            // Columna izquierda: puntos
            lblPuntos = new Label
            {
                Text = "PUNTOS: 000000",                                // Texto inicial de la puntuación en ceros
                ForeColor = Color.FromArgb(220, 180, 255),              // Color del texto: lila claro
                BackColor = Color.Transparent,                          // Fondo transparente para mostrar el HUD detrás
                Font = new Font("Consolas", 11f, FontStyle.Bold),       // Fuente Consolas negrita 11pt
                AutoSize = true,                                        // Ajusta el tamaño automáticamente al texto
                Location = new Point(16, 10)                           // Posición en la parte izquierda del HUD
            };

            // Columna centro: vidas
            lblVidas = new Label
            {
                Text = "♥ ♥ ♥",                                         // Texto inicial: 3 corazones llenos
                ForeColor = Color.FromArgb(255, 80, 180),              // Color del texto: rosa intenso
                BackColor = Color.Transparent,                          // Fondo transparente
                Font = new Font("Consolas", 15f, FontStyle.Bold),       // Fuente grande para destacar las vidas
                AutoSize = true,                                        // Ajusta el tamaño automáticamente
                Location = new Point(310, 6)                           // Posición centrada en el HUD
            };

            // Columna centro-bajo: nivel
            lblNivel = new Label
            {
                Text = "NIVEL: 1",                                      // Texto inicial: nivel 1
                ForeColor = Color.FromArgb(0, 220, 255),               // Color del texto: cian neón
                BackColor = Color.Transparent,                          // Fondo transparente
                Font = new Font("Consolas", 9f, FontStyle.Bold),        // Fuente Consolas negrita 9pt
                AutoSize = true,                                        // Ajusta el tamaño automáticamente
                Location = new Point(326, 40)                          // Posición debajo del indicador de vidas
            };

            // Botón Pausa — esquina derecha arriba
            btnPausa = new Button
            {
                Text = "⏸ PAUSA",                                        // Texto inicial del botón
                Location = new Point(570, 10),                          // Posición en el lado derecho del HUD
                Size = new Size(100, 34),                               // Tamaño del botón
                FlatStyle = FlatStyle.Flat,                             // Estilo plano sin relieve
                ForeColor = Color.White,                                // Color del texto del botón
                BackColor = Color.FromArgb(100, 0, 200),               // Color de fondo morado
                Cursor = Cursors.Hand,                                  // Cursor de mano al pasar por encima
                Font = new Font("Consolas", 8.5f, FontStyle.Bold)      // Fuente del botón
            };
            btnPausa.FlatAppearance.BorderColor = Color.FromArgb(220, 150, 255); // Color del borde del botón: lila claro
            btnPausa.FlatAppearance.BorderSize = 1;                              // Grosor del borde del botón: 1px
            btnPausa.MouseEnter += (s, e) => btnPausa.BackColor = Color.FromArgb(140, 30, 240); // Al pasar el mouse, aclara el fondo del botón
            btnPausa.MouseLeave += (s, e) => btnPausa.BackColor = Color.FromArgb(100, 0, 200);  // Al salir el mouse, restaura el color original
            btnPausa.Click += (s, e) => // Al hacer clic en Pausa
            {
                juego.TogglePausa();    // Alterna entre pausado y jugando en el motor del juego
                btnPausa.Text = juego.Estado.Estado == EstadoJuego.Pausado ? "▶ SEGUIR" : "⏸ PAUSA"; // Cambia el texto según el nuevo estado
                panelJuego.Focus();     // Devuelve el foco al panel de juego
            };

            // Botón Reiniciar — a la derecha del de pausa
            btnReiniciar = new Button
            {
                Text = "↺ RESET",                                        // Texto del botón de reinicio
                Location = new Point(682, 10),                          // Posición en el extremo derecho del HUD
                Size = new Size(100, 34),                               // Tamaño del botón
                FlatStyle = FlatStyle.Flat,                             // Estilo plano sin relieve
                ForeColor = Color.White,                                // Color del texto del botón
                BackColor = Color.FromArgb(180, 0, 80),                // Color de fondo rosa oscuro
                Cursor = Cursors.Hand,                                  // Cursor de mano al pasar por encima
                Font = new Font("Consolas", 8.5f, FontStyle.Bold)      // Fuente del botón
            };
            btnReiniciar.FlatAppearance.BorderColor = Color.FromArgb(255, 100, 180); // Color del borde del botón: rosa neón
            btnReiniciar.FlatAppearance.BorderSize = 1;                              // Grosor del borde del botón: 1px
            btnReiniciar.MouseEnter += (s, e) => btnReiniciar.BackColor = Color.FromArgb(220, 20, 100);  // Al pasar el mouse, aclara el fondo del botón
            btnReiniciar.MouseLeave += (s, e) => btnReiniciar.BackColor = Color.FromArgb(180, 0, 80);   // Al salir el mouse, restaura el color original
            btnReiniciar.Click += (s, e) => // Al hacer clic en Reiniciar
            {
                juego.Reiniciar();          // Reinicia el motor del juego a su estado inicial
                btnPausa.Text = "⏸ PAUSA"; // Restaura el texto del botón de pausa
                btnPausa.Enabled = true;    // Vuelve a habilitar el botón de pausa
                particulas.Clear();         // Limpia todas las partículas de efectos visuales
                panelJuego.Focus();         // Devuelve el foco al panel de juego
            };

            // Agrega todos los controles al panel HUD
            panelHud.Controls.AddRange(new Control[]
            {
                lblPuntos, lblVidas, lblNivel, btnPausa, btnReiniciar // Agrega las etiquetas y botones al HUD
            });

            // ── Panel de juego con doble buffer ─────────────────────
            // Crea el panel de doble búfer donde se dibuja el juego
            panelJuego = new PanelSuave
            {
                Location = new Point(0, 70),          // Posicionado justo debajo del HUD
                Size = new Size(800, 630),             // Ocupa el resto del formulario (800x630)
                BackColor = Color.FromArgb(5, 0, 30)  // Color de fondo azul noche muy oscuro
            };
            panelJuego.Paint += PanelJuego_Paint; // Suscribe el método de dibujo al evento Paint del panel

            Controls.Add(panelHud);    // Agrega el panel HUD al formulario
            Controls.Add(panelJuego);  // Agrega el panel de juego al formulario
        }

        // Genera 140 estrellas con posición, brillo y velocidad aleatorias para el fondo
        private void GenerarEstrellas()
        {
            for (int i = 0; i < 140; i++) // Genera 140 estrellas
            {
                estrellas.Add((
                    rng.Next(0, 800),               // Posición X aleatoria dentro del ancho del formulario
                    rng.Next(0, 700),               // Posición Y aleatoria dentro del alto del formulario
                    rng.NextSingle(),               // Brillo aleatorio entre 0.0 y 1.0
                    rng.NextSingle() * 0.6f + 0.1f // Velocidad aleatoria entre 0.1 y 0.7 píxeles por tick
                ));
            }
        }

        // Actualiza la posición vertical de cada estrella para crear el efecto de movimiento hacia abajo
        private void ActualizarEstrellas()
        {
            for (int i = 0; i < estrellas.Count; i++) // Recorre todas las estrellas
            {
                var (x, y, br, vel) = estrellas[i]; // Descompone la tupla de la estrella
                float ny = y + vel;                  // Calcula la nueva posición Y sumando la velocidad
                if (ny > 700) ny = 0;               // Si la estrella salió por abajo, reaparece por arriba
                estrellas[i] = (x, ny, br, vel);    // Actualiza la posición de la estrella en la lista
            }
        }

        // Método principal del game loop: calcula el delta time, procesa entradas, actualiza el juego y redibuja
        private void GameLoop(object? sender, EventArgs e)
        {
            var ahora = DateTime.Now;                                      // Obtiene el tiempo actual
            float delta = (float)(ahora - ultimoFrame).TotalSeconds;      // Calcula el tiempo transcurrido desde el último frame en segundos
            ultimoFrame = ahora;                                           // Actualiza la marca de tiempo del último frame
            delta = Math.Min(delta, 0.05f);                                // Limita el delta a 50ms máximo para evitar saltos grandes en frames lentos

            if (juego.Estado.Estado == EstadoJuego.Jugando) // Solo procesa si el juego está activo
            {
                float vel = 220f * delta; // Calcula el desplazamiento del jugador en este frame (220 píxeles/segundo)
                if (teclasPresionadas.Contains(Keys.Left) || teclasPresionadas.Contains(Keys.A))
                    juego.MoverJugador(-vel, 0);  // Mueve el cohete hacia la izquierda
                if (teclasPresionadas.Contains(Keys.Right) || teclasPresionadas.Contains(Keys.D))
                    juego.MoverJugador(vel, 0);   // Mueve el cohete hacia la derecha
                if (teclasPresionadas.Contains(Keys.Up) || teclasPresionadas.Contains(Keys.W))
                    juego.MoverJugador(0, -vel);  // Mueve el cohete hacia arriba
                if (teclasPresionadas.Contains(Keys.Down) || teclasPresionadas.Contains(Keys.S))
                    juego.MoverJugador(0, vel);   // Mueve el cohete hacia abajo
                if (teclasPresionadas.Contains(Keys.Space))
                    juego.Disparar();              // Dispara una bala si el cooldown lo permite

                juego.Actualizar(delta);           // Actualiza toda la lógica del juego (entidades, colisiones, spawn)
                ActualizarParticulas(delta);        // Actualiza las partículas de efectos visuales

                // Actualizar HUD
                lblPuntos.Text = $"PUNTOS: {juego.Estado.Puntos:D6}"; // Actualiza la etiqueta de puntos con formato de 6 dígitos

                int nivelVisual = juego.Estado.Puntos / 150 + 1;       // Calcula el nivel visual basado en los puntos (1 nivel cada 150 puntos)
                lblNivel.Text = $"NIVEL: {nivelVisual}";                // Actualiza la etiqueta del nivel

                lblVidas.Text = juego.Jugador.Vidas switch // Actualiza el indicador de vidas con corazones llenos y vacíos
                {
                    3 => "♥ ♥ ♥", // 3 vidas: todos los corazones llenos
                    2 => "♥ ♥ ♡", // 2 vidas: dos llenos y uno vacío
                    1 => "♥ ♡ ♡", // 1 vida: uno lleno y dos vacíos
                    _ => "♡ ♡ ♡"  // 0 o menos vidas: todos los corazones vacíos
                };
            }

            panelJuego.Invalidate(); // Solicita redibujar el panel de juego en cada frame
        }

        // Actualiza la posición, velocidad y vida de cada partícula de efecto visual
        private void ActualizarParticulas(float delta)
        {
            for (int i = particulas.Count - 1; i >= 0; i--) // Recorre la lista de atrás hacia adelante para poder eliminar sin romper el índice
            {
                var p = particulas[i];                        // Obtiene la partícula actual
                float vida = p.vida - delta;                  // Reduce la vida restante de la partícula
                if (vida <= 0) { particulas.RemoveAt(i); continue; } // Si la partícula murió, la elimina y pasa a la siguiente
                particulas[i] = (p.x + p.vx * delta,         // Actualiza la posición X según la velocidad horizontal
                                 p.y + p.vy * delta,          // Actualiza la posición Y según la velocidad vertical
                                 p.vx,                        // La velocidad horizontal no cambia
                                 p.vy * 0.95f + 20 * delta,   // La velocidad vertical se desacelera un 5% y aumenta por gravedad
                                 vida, p.color);               // Actualiza la vida y mantiene el color
            }
        }

        // Dibuja todos los elementos visuales del juego: fondo, nebulosas, estrellas, entidades, partículas y overlays
        private void PanelJuego_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;                            // Obtiene el contexto gráfico del evento
            g.SmoothingMode = SmoothingMode.AntiAlias;    // Activa el anti-aliasing para bordes suaves

            // Fondo degradado
            using var fondo = new LinearGradientBrush(
                new Point(0, 0), new Point(0, panelJuego.Height),            // Degradado vertical de arriba a abajo
                Color.FromArgb(5, 0, 30), Color.FromArgb(10, 0, 55));        // Del azul muy oscuro al azul noche profundo
            g.FillRectangle(fondo, 0, 0, panelJuego.Width, panelJuego.Height); // Rellena el fondo del panel con el degradado

            // Nebulosas decorativas
            using var neb1 = new SolidBrush(Color.FromArgb(15, 100, 0, 180)); // Pincel morado muy semitransparente para la nebulosa izquierda
            g.FillEllipse(neb1, -80, 80, 380, 200);                            // Dibuja la nebulosa izquierda parcialmente fuera del panel
            using var neb2 = new SolidBrush(Color.FromArgb(10, 180, 0, 100)); // Pincel rosa muy semitransparente para la nebulosa derecha
            g.FillEllipse(neb2, 480, 280, 320, 200);                           // Dibuja la nebulosa en la zona derecha-central

            // Estrellas
            foreach (var (sx, sy, br, _) in estrellas) // Recorre todas las estrellas del fondo
            {
                int alpha = (int)(br * 255);            // Calcula la transparencia de la estrella según su brillo
                using var sb = new SolidBrush(Color.FromArgb(alpha, 220, 200, 255)); // Pincel lila con la transparencia calculada
                float sz = br < 0.3f ? 1f : br < 0.7f ? 1.5f : 2.5f; // Tamaño de la estrella según su brillo (1, 1.5 o 2.5px)
                g.FillEllipse(sb, sx, sy, sz, sz);      // Dibuja la estrella como un pequeño círculo
            }

            // Entidades del juego
            juego.Meteoritos.ForEach(m => m.Dibujar(e)); // Dibuja todos los meteoritos activos
            juego.Corazones.ForEach(c => c.Dibujar(e));  // Dibuja todos los corazones (proyectiles alien) activos
            juego.Balas.ForEach(b => b.Dibujar(e));      // Dibuja todas las balas activas del jugador
            juego.Aliens.ForEach(a => a.Dibujar(e));     // Dibuja todos los aliens activos
            if (juego.Jugador.Activo) juego.Jugador.Dibujar(e); // Dibuja el cohete solo si está activo (no ha muerto)

            // Partículas
            foreach (var (px, py, _, _, vida, color) in particulas) // Recorre todas las partículas de efectos
            {
                int alpha = Math.Clamp((int)(vida / 0.8f * 200), 0, 255); // Calcula la transparencia según la vida restante (máximo 200)
                using var pb = new SolidBrush(Color.FromArgb(alpha, color)); // Pincel del color de la partícula con la transparencia calculada
                g.FillEllipse(pb, px - 3, py - 3, 6, 6);                    // Dibuja la partícula como un círculo de 6px centrado en su posición
            }

            // Overlays según el estado del juego
            if (juego.Estado.Estado == EstadoJuego.Pausado)     // Si el juego está pausado
                DibujarOverlay(g,
                    "⏸  PAUSADO",
                    "Presiona PAUSA para continuar",
                    Color.FromArgb(150, 0, 0, 60),              // Fondo semitransparente azul oscuro
                    Color.FromArgb(200, 150, 255));             // Título en lila claro

            if (juego.Estado.Estado == EstadoJuego.GameOver)    // Si el jugador perdió
                DibujarOverlay(g,
                    "💀  GAME OVER",
                    $"Puntuación: {juego.Estado.Puntos:D6}\nPresiona RESET para volver a jugar",
                    Color.FromArgb(170, 60, 0, 0),              // Fondo semitransparente rojo oscuro
                    Color.FromArgb(255, 80, 120));              // Título en rosa intenso
        }

        // Dibuja un overlay semitransparente con título y subtítulo centrados (pausa o game over)
        private void DibujarOverlay(Graphics g, string titulo, string subtitulo,
                                     Color colorFondo, Color colorTitulo)
        {
            using var fondo = new SolidBrush(colorFondo);                          // Pincel semitransparente para el fondo del overlay
            g.FillRectangle(fondo, 0, 0, panelJuego.Width, panelJuego.Height);    // Cubre todo el panel de juego con el color de fondo

            var rect = new Rectangle(130, 210, 540, 210);                          // Rectángulo de la caja de mensaje centrada en el panel
            using var caja = new SolidBrush(Color.FromArgb(210, 10, 0, 40));      // Pincel azul-morado muy opaco para el fondo de la caja
            g.FillRectangle(caja, rect);                                            // Rellena el fondo de la caja de mensaje
            using var borde = new Pen(Color.FromArgb(255, 180, 0, 255), 3f);      // Pluma morada neón de 3px para el borde de la caja
            g.DrawRectangle(borde, rect);                                           // Dibuja el borde morado neón de la caja
            using var glow = new Pen(Color.FromArgb(80, 180, 0, 255), 8f);        // Pluma morada semitransparente gruesa para el brillo
            g.DrawRectangle(glow, rect);                                            // Dibuja el efecto de brillo alrededor de la caja

            using var fTitulo = new Font("Consolas", 28f, FontStyle.Bold);         // Fuente grande en negrita para el título del overlay
            using var bTitulo = new SolidBrush(colorTitulo);                       // Pincel del color del título pasado como parámetro
            var sf = new StringFormat { Alignment = StringAlignment.Center };      // Configuración de alineación centrada para el texto
            g.DrawString(titulo, fTitulo, bTitulo, new RectangleF(130, 230, 540, 80), sf); // Dibuja el título centrado en la caja

            using var fSub = new Font("Consolas", 11f, FontStyle.Regular);              // Fuente más pequeña para el subtítulo
            using var bSub = new SolidBrush(Color.FromArgb(210, 200, 180, 255));       // Pincel lila claro semitransparente para el subtítulo
            g.DrawString(subtitulo, fSub, bSub, new RectangleF(130, 320, 540, 90), sf); // Dibuja el subtítulo centrado debajo del título
        }

        // Deshabilita el botón de pausa al terminar la partida
        private void MostrarGameOver()
        {
            btnPausa.Enabled = false; // Deshabilita el botón de pausa ya que el juego terminó
        }

        // Procesa los eventos de tecla presionada para movimiento, pausa y reinicio
        private void Form_KeyDown(object? sender, KeyEventArgs e)
        {
            teclasPresionadas.Add(e.KeyCode); // Agrega la tecla presionada al conjunto de teclas activas
            if (e.KeyCode == Keys.Escape)     // Si se presionó Escape
            {
                juego.TogglePausa();           // Alterna entre pausado y jugando en el motor
                btnPausa.Text = juego.Estado.Estado == EstadoJuego.Pausado ? "▶ SEGUIR" : "⏸ PAUSA"; // Actualiza el texto del botón de pausa
            }
            if (e.KeyCode == Keys.R) // Si se presionó R
            {
                juego.Reiniciar();          // Reinicia el motor del juego
                btnPausa.Text = "⏸ PAUSA"; // Restaura el texto del botón de pausa
                btnPausa.Enabled = true;    // Vuelve a habilitar el botón de pausa
                particulas.Clear();         // Limpia todas las partículas de efectos
            }
        }

        // Procesa los eventos de tecla soltada eliminando la tecla del conjunto de teclas activas
        private void Form_KeyUp(object? sender, KeyEventArgs e)
        {
            teclasPresionadas.Remove(e.KeyCode); // Elimina la tecla soltada del conjunto de teclas presionadas
        }
    }
}