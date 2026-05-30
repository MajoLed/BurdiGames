using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MiniJuegos
{
    // Panel con doble buffer — igual que FormCohete
    // Clase que extiende Panel activando el doble búfer para evitar parpadeo al redibujar
    public class PanelSuaveBuscaminas : Panel
    {
        // Constructor: configura el panel con doble búfer y estilos de pintura optimizados
        public PanelSuaveBuscaminas()
        {
            this.DoubleBuffered = true;  // Activa el doble búfer nativo del control
            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint |   // Evita que el fondo se pinte por separado (reduce parpadeo)
                ControlStyles.UserPaint |              // El control se pinta a sí mismo en lugar de usar el sistema
                ControlStyles.OptimizedDoubleBuffer,   // Activa el búfer optimizado para gráficos suaves
                true);
            this.UpdateStyles(); // Aplica los estilos configurados al control
        }
    }

    // Formulario principal del juego Buscaminas con su interfaz visual completa
    public class FormBuscaminas : Form
    {
        private MotorBuscaminas motor;                    // Instancia del motor de lógica del juego Buscaminas
        private System.Windows.Forms.Timer timerHud;     // Timer que actualiza el HUD cada 500ms (puntos, minas, tiempo)

        private PanelSuaveBuscaminas panelJuego;  // Panel de doble búfer donde se dibuja el tablero
        private Button btnPausa;                  // Botón para pausar o reanudar el juego
        private Button btnReiniciar;              // Botón para reiniciar el juego desde cero
        private Label lblPuntos;                  // Etiqueta que muestra la puntuación actual
        private Label lblMinas;                   // Etiqueta que muestra las minas restantes
        private Label lblTiempo;                  // Etiqueta que muestra el tiempo transcurrido

        // Tamaño de cada celda en píxeles
        private const int TAM_CELDA = 36;                        // Tamaño en píxeles de cada celda cuadrada del tablero
        private const int FILAS = 16;                            // Número de filas del tablero
        private const int COLS = 16;                             // Número de columnas del tablero
        private const int ANCHO_TABLERO = COLS * TAM_CELDA;     // Ancho total del tablero en píxeles (576)
        private const int ALTO_TABLERO = FILAS * TAM_CELDA;     // Alto total del tablero en píxeles (576)

        // Paleta girlie — colores de la interfaz
        private static readonly Color C_FONDO = Color.FromArgb(30, 5, 25);           // Color de fondo general del formulario (morado muy oscuro)
        private static readonly Color C_HUD = Color.FromArgb(45, 10, 40);            // Color de fondo del panel HUD (morado oscuro)
        private static readonly Color C_CELDA_OCULTA = Color.FromArgb(220, 100, 160); // Color principal de celda oculta (rosa fuerte)
        private static readonly Color C_CELDA_OCULTA2 = Color.FromArgb(200, 80, 145); // Color alternativo de celda oculta para el damero (rosa más oscuro)
        private static readonly Color C_CELDA_REV = Color.FromArgb(255, 220, 235);   // Color principal de celda revelada (rosa muy claro)
        private static readonly Color C_CELDA_REV2 = Color.FromArgb(250, 200, 220);  // Color alternativo de celda revelada para el damero (rosa claro)
        private static readonly Color C_BORDE_NEON = Color.FromArgb(255, 20, 180);   // Color del borde neón rosa intenso
        private static readonly Color C_BORDE_SUAVE = Color.FromArgb(180, 60, 140);  // Color del borde suave entre celdas (rosa oscuro)
        private static readonly Color C_MINA = Color.FromArgb(30, 0, 20);            // Color del cuerpo de la mina (negro púrpura)
        private static readonly Color C_TEXTO_HUD = Color.FromArgb(255, 180, 220);   // Color del texto del HUD (rosa pastel)
        private static readonly Color C_NEON_ROSA = Color.FromArgb(255, 20, 180);    // Color neón rosa para detalles y bordes
        private static readonly Color C_NEON_MORADO = Color.FromArgb(180, 0, 255);   // Color neón morado para detalles y esquinas
        private static readonly Color C_ACENTO_CIAN = Color.FromArgb(255, 100, 200); // Color acento rosa-cian para el tiempo

        // Colores para los números del 1 al 8 que indican minas vecinas
        private static readonly Color[] C_NUMEROS = {
            Color.FromArgb(180, 0, 255),   // 1 — morado
            Color.FromArgb(255, 20, 147),  // 2 — rosa fuerte
            Color.FromArgb(200, 0, 200),   // 3 — fucsia
            Color.FromArgb(130, 0, 200),   // 4 — violeta
            Color.FromArgb(220, 60, 100),  // 5 — rosa oscuro
            Color.FromArgb(160, 0, 160),   // 6 — morado medio
            Color.FromArgb(80, 0, 100),    // 7 — morado profundo
            Color.FromArgb(50, 0, 50),     // 8 — casi negro
        };

        // Constructor: inicializa el motor, configura los componentes y suscribe los eventos
        public FormBuscaminas()
        {
            motor = new MotorBuscaminas(); // Crea una nueva instancia del motor de lógica del juego
            InicializarComponentes();      // Crea y configura todos los controles visuales del formulario

            timerHud = new System.Windows.Forms.Timer { Interval = 500 }; // Crea el timer del HUD con intervalo de 500ms
            timerHud.Tick += (s, e) => ActualizarHud();                    // Al cada tick, actualiza las etiquetas del HUD
            timerHud.Start();                                               // Inicia el timer del HUD

            motor.JuegoTerminado += (s, e) => { panelJuego.Invalidate(); btnPausa.Enabled = false; };    // Al terminar el juego, redibuja el tablero y deshabilita el botón de pausa
            motor.VictoriaAlcanzada += (s, e) => { panelJuego.Invalidate(); btnPausa.Enabled = false; }; // Al lograr victoria, redibuja el tablero y deshabilita el botón de pausa
        }

        // Crea, configura y agrega al formulario todos los controles visuales (HUD y panel de juego)
        private void InicializarComponentes()
        {
            int anchoForm = ANCHO_TABLERO + 2;  // Ancho del formulario: tablero + 2 píxeles de margen (578)
            int altoHud = 70;                   // Alto del panel HUD en píxeles
            int altoForm = ALTO_TABLERO + altoHud + 2; // Alto total del formulario: tablero + HUD + margen

            Text = "💣 BLOSSOM MINES";                          // Título de la ventana del formulario
            ClientSize = new Size(anchoForm, altoForm);         // Tamaño del área cliente del formulario
            BackColor = C_FONDO;                                // Color de fondo del formulario
            FormBorderStyle = FormBorderStyle.FixedSingle;     // Borde fijo: no se puede redimensionar
            MaximizeBox = false;                                // Deshabilita el botón de maximizar
            Font = new Font("Consolas", 9f, FontStyle.Bold);   // Fuente base del formulario (Consolas negrita 9pt)

            // ── HUD ──────────────────────────────────────────────────
            // Panel que contiene las etiquetas y botones del HUD en la parte superior
            var panelHud = new Panel
            {
                Location = new Point(0, 0),         // Ubicado en la esquina superior izquierda
                Size = new Size(anchoForm, altoHud), // Ocupa todo el ancho del formulario con el alto del HUD
                BackColor = C_HUD                    // Color de fondo morado oscuro
            };
            panelHud.Paint += (s, e) =>             // Evento Paint del HUD: dibuja una línea neón en su borde inferior
            {
                using var pen = new Pen(C_NEON_ROSA, 2f);                                      // Pluma neón rosa de 2px
                using var glow = new Pen(Color.FromArgb(60, 255, 20, 180), 6f);               // Pluma neón rosa semitransparente gruesa para el brillo
                e.Graphics.DrawLine(pen, 0, altoHud - 1, anchoForm, altoHud - 1);            // Dibuja la línea rosa en el borde inferior del HUD
                e.Graphics.DrawLine(glow, 0, altoHud - 2, anchoForm, altoHud - 2);           // Dibuja el brillo rosa justo encima de la línea
            };

            // Puntos — izquierda
            lblPuntos = new Label
            {
                Text = "✦ 000000",                              // Texto inicial: puntuación en ceros
                ForeColor = C_TEXTO_HUD,                        // Color del texto: rosa pastel
                BackColor = Color.Transparent,                  // Fondo transparente para mostrar el HUD detrás
                Font = new Font("Consolas", 11f, FontStyle.Bold), // Fuente Consolas negrita 11pt
                AutoSize = true,                                // Ajusta el tamaño automáticamente al texto
                Location = new Point(12, 10)                   // Posición en la parte izquierda del HUD
            };

            // Minas restantes — centro
            lblMinas = new Label
            {
                Text = "💣 40",                                  // Texto inicial: 40 minas restantes
                ForeColor = C_NEON_ROSA,                        // Color del texto: rosa neón
                BackColor = Color.Transparent,                  // Fondo transparente
                Font = new Font("Consolas", 15f, FontStyle.Bold), // Fuente grande para destacar el contador de minas
                AutoSize = true,                                // Ajusta el tamaño automáticamente
                Location = new Point(220, 6)                   // Posición centrada en el HUD
            };

            // Tiempo — centro-bajo
            lblTiempo = new Label
            {
                Text = "⏱ 00:00",                               // Texto inicial: tiempo en cero
                ForeColor = C_ACENTO_CIAN,                      // Color del texto: rosa-cian
                BackColor = Color.Transparent,                  // Fondo transparente
                Font = new Font("Consolas", 9f, FontStyle.Bold), // Fuente Consolas negrita 9pt
                AutoSize = true,                                // Ajusta el tamaño automáticamente
                Location = new Point(232, 40)                  // Posición debajo del contador de minas
            };

            // Botón Pausa
            btnPausa = new Button
            {
                Text = "⏸ PAUSA",                                    // Texto inicial del botón
                Location = new Point(348, 10),                      // Posición en el HUD (derecha-superior)
                Size = new Size(100, 34),                            // Tamaño del botón
                FlatStyle = FlatStyle.Flat,                         // Estilo plano sin relieve
                ForeColor = Color.White,                            // Color del texto del botón
                BackColor = Color.FromArgb(120, 0, 180),           // Color de fondo morado
                Cursor = Cursors.Hand,                              // Cursor de mano al pasar por encima
                Font = new Font("Consolas", 8.5f, FontStyle.Bold)  // Fuente del botón
            };
            btnPausa.FlatAppearance.BorderColor = C_NEON_MORADO;  // Color del borde del botón: morado neón
            btnPausa.FlatAppearance.BorderSize = 1;                // Grosor del borde del botón: 1px
            btnPausa.MouseEnter += (s, e) => btnPausa.BackColor = Color.FromArgb(160, 20, 220); // Al pasar el mouse, aclara el fondo del botón
            btnPausa.MouseLeave += (s, e) => btnPausa.BackColor = Color.FromArgb(120, 0, 180); // Al salir el mouse, restaura el color original del botón
            btnPausa.Click += (s, e) =>                            // Al hacer clic en Pausa
            {
                motor.TogglePausa();                               // Alterna entre pausado y jugando en el motor
                btnPausa.Text = motor.Estado == EstadoBuscaminas.Pausado ? "▶ SEGUIR" : "⏸ PAUSA"; // Cambia el texto del botón según el estado
                panelJuego.Focus();                                // Devuelve el foco al panel de juego
                panelJuego.Invalidate();                           // Redibuja el tablero para mostrar el overlay de pausa
            };

            // Botón Reiniciar
            btnReiniciar = new Button
            {
                Text = "↺ RESET",                                    // Texto del botón de reinicio
                Location = new Point(460, 10),                      // Posición en el HUD (extremo derecho)
                Size = new Size(100, 34),                            // Tamaño del botón
                FlatStyle = FlatStyle.Flat,                         // Estilo plano sin relieve
                ForeColor = Color.White,                            // Color del texto del botón
                BackColor = Color.FromArgb(160, 0, 100),           // Color de fondo rosa oscuro
                Cursor = Cursors.Hand,                              // Cursor de mano al pasar por encima
                Font = new Font("Consolas", 8.5f, FontStyle.Bold)  // Fuente del botón
            };
            btnReiniciar.FlatAppearance.BorderColor = C_NEON_ROSA; // Color del borde del botón: rosa neón
            btnReiniciar.FlatAppearance.BorderSize = 1;             // Grosor del borde del botón: 1px
            btnReiniciar.MouseEnter += (s, e) => btnReiniciar.BackColor = Color.FromArgb(210, 20, 130); // Al pasar el mouse, aclara el fondo del botón
            btnReiniciar.MouseLeave += (s, e) => btnReiniciar.BackColor = Color.FromArgb(160, 0, 100); // Al salir el mouse, restaura el color original del botón
            btnReiniciar.Click += (s, e) =>                         // Al hacer clic en Reiniciar
            {
                motor.Reiniciar();              // Reinicia el motor del juego a su estado inicial
                btnPausa.Text = "⏸ PAUSA";    // Restaura el texto del botón de pausa
                btnPausa.Enabled = true;        // Vuelve a habilitar el botón de pausa
                panelJuego.Focus();             // Devuelve el foco al panel de juego
                panelJuego.Invalidate();        // Redibuja el tablero limpio
            };

            // Agrega todos los controles al panel HUD
            panelHud.Controls.AddRange(new Control[]
            {
                lblPuntos, lblMinas, lblTiempo, btnPausa, btnReiniciar // Agrega las etiquetas y botones al HUD
            });

            // ── Panel de juego ───────────────────────────────────────
            // Crea el panel de doble búfer donde se dibuja el tablero del Buscaminas
            panelJuego = new PanelSuaveBuscaminas
            {
                Location = new Point(1, altoHud),          // Posicionado debajo del HUD con 1px de margen izquierdo
                Size = new Size(ANCHO_TABLERO, ALTO_TABLERO), // Tamaño exacto del tablero (576x576)
                BackColor = C_FONDO                        // Color de fondo del panel de juego
            };
            panelJuego.Paint += PanelJuego_Paint;          // Suscribe el método de dibujo del tablero al evento Paint
            panelJuego.MouseClick += PanelJuego_MouseClick; // Suscribe el método de clic al evento MouseClick

            Controls.Add(panelHud);    // Agrega el panel HUD al formulario
            Controls.Add(panelJuego);  // Agrega el panel de juego al formulario
        }

        // ── Actualizar HUD ───────────────────────────────────────────
        // Actualiza las etiquetas del HUD con los valores actuales del motor
        private void ActualizarHud()
        {
            lblPuntos.Text = $"✦ {motor.Puntos:D6}";    // Muestra la puntuación con 6 dígitos con ceros a la izquierda
            lblMinas.Text = $"💣 {motor.MinasRestantes}"; // Muestra las minas restantes (total menos banderas colocadas)
            int seg = motor.SegundosTranscurridos;         // Obtiene el tiempo transcurrido en segundos
            lblTiempo.Text = $"⏱ {seg / 60:D2}:{seg % 60:D2}"; // Muestra el tiempo en formato MM:SS
        }

        // ── Clic en el tablero ───────────────────────────────────────
        // Procesa los clics del mouse sobre el tablero y llama al motor según el botón presionado
        private void PanelJuego_MouseClick(object? sender, MouseEventArgs e)
        {
            if (motor.Estado == EstadoBuscaminas.Pausado ||
                motor.Estado == EstadoBuscaminas.GameOver ||
                motor.Estado == EstadoBuscaminas.Victoria) return; // Si el juego no está activo, ignora el clic

            int col = e.X / TAM_CELDA;   // Convierte la posición X del clic a índice de columna
            int fila = e.Y / TAM_CELDA;  // Convierte la posición Y del clic a índice de fila
            if (col < 0 || col >= COLS || fila < 0 || fila >= FILAS) return; // Si el clic está fuera del tablero, lo ignora

            if (e.Button == MouseButtons.Left)        // Si se hizo clic izquierdo
                motor.Revelar(fila, col);             // Revela la celda en el motor
            else if (e.Button == MouseButtons.Right)  // Si se hizo clic derecho
                motor.ToggleBandera(fila, col);       // Alterna la bandera en la celda del motor

            panelJuego.Invalidate(); // Redibuja el tablero para reflejar el cambio
        }

        // ── Pintar tablero ───────────────────────────────────────────
        // Dibuja el tablero completo: fondo, celdas y overlays según el estado del juego
        private void PanelJuego_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;                             // Obtiene el contexto gráfico del evento
            g.SmoothingMode = SmoothingMode.AntiAlias;     // Activa el anti-aliasing para bordes suaves

            // Fondo degradado del tablero
            using var fondoGrad = new LinearGradientBrush(
                new Point(0, 0), new Point(0, ALTO_TABLERO),          // Degradado vertical de arriba a abajo
                Color.FromArgb(35, 8, 30), Color.FromArgb(20, 2, 18)); // Del morado oscuro al casi negro
            g.FillRectangle(fondoGrad, 0, 0, ANCHO_TABLERO, ALTO_TABLERO); // Rellena el fondo con el degradado

            // Dibujar celdas
            for (int f = 0; f < FILAS; f++)    // Recorre cada fila del tablero
                for (int c = 0; c < COLS; c++) // Recorre cada columna de la fila actual
                    DibujarCelda(g, f, c);     // Dibuja la celda en la posición (f, c)

            // Overlays según el estado del juego
            if (motor.Estado == EstadoBuscaminas.Pausado)    // Si el juego está pausado
                DibujarOverlay(g, "⏸  PAUSADO",
                    "Presiona PAUSA para continuar",
                    Color.FromArgb(150, 20, 0, 40),          // Fondo semitransparente morado oscuro
                    Color.FromArgb(220, 150, 255));          // Título en morado claro

            if (motor.Estado == EstadoBuscaminas.GameOver)   // Si el jugador perdió
                DibujarOverlay(g, "💥 BOOM",
                    $"Pisaste una mina...\nPuntuación: {motor.Puntos:D6}\nRESET para volver a jugar",
                    Color.FromArgb(170, 60, 0, 20),          // Fondo semitransparente rojo oscuro
                    Color.FromArgb(255, 80, 160));           // Título en rosa intenso

            if (motor.Estado == EstadoBuscaminas.Victoria)   // Si el jugador ganó
                DibujarOverlay(g, "🌸 ¡GANASTE!",
                    $"¡Despejaste todas las minas!\nPuntuación: {motor.Puntos:D6}\nRESET para jugar de nuevo",
                    Color.FromArgb(160, 0, 30, 20),          // Fondo semitransparente verde oscuro
                    Color.FromArgb(255, 180, 230));          // Título en rosa pastel
        }

        // Dibuja una celda individual del tablero según su estado (revelada u oculta)
        private void DibujarCelda(Graphics g, int fila, int col)
        {
            var celda = motor.Tablero.Celdas[fila, col];              // Obtiene la celda del motor en la posición indicada
            int px = col * TAM_CELDA;                                  // Calcula la posición X en píxeles de la celda
            int py = fila * TAM_CELDA;                                 // Calcula la posición Y en píxeles de la celda
            var rect = new Rectangle(px + 1, py + 1, TAM_CELDA - 2, TAM_CELDA - 2); // Rectángulo interior de la celda (con margen de 1px para el borde)

            if (celda.EsRevelada) // Si la celda fue revelada por el jugador
            {
                // Tablero en damero suave para reveladas
                bool par = (fila + col) % 2 == 0;                                         // Determina si la celda es par (para efecto damero)
                using var br = new SolidBrush(par ? C_CELDA_REV : C_CELDA_REV2);         // Alterna entre los dos tonos de celda revelada
                g.FillRectangle(br, rect);                                                 // Rellena la celda con el color correspondiente

                if (celda.TieneMina) // Si la celda revelada contiene una mina
                {
                    DibujarMina(g, px + TAM_CELDA / 2, py + TAM_CELDA / 2); // Dibuja la mina centrada en la celda
                }
                else if (celda.MinasVecinas > 0) // Si la celda revelada tiene minas vecinas
                {
                    using var font = new Font("Consolas", 13f, FontStyle.Bold);              // Fuente para el número de minas vecinas
                    using var brush = new SolidBrush(C_NUMEROS[celda.MinasVecinas - 1]);    // Color del número según su valor (1-8)
                    var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,      // Centra el número horizontalmente
                        LineAlignment = StringAlignment.Center   // Centra el número verticalmente
                    };
                    g.DrawString(celda.MinasVecinas.ToString(), font, brush,
                        new RectangleF(px, py, TAM_CELDA, TAM_CELDA), sf); // Dibuja el número centrado en la celda
                }
            }
            else // Si la celda no ha sido revelada
            {
                // Celda oculta con gradiente rosa
                bool par = (fila + col) % 2 == 0;                                         // Determina si la celda es par (para efecto damero)
                using var grad = new LinearGradientBrush(
                    new Point(px, py), new Point(px + TAM_CELDA, py + TAM_CELDA),          // Degradado diagonal de esquina a esquina
                    par ? C_CELDA_OCULTA : C_CELDA_OCULTA2,                               // Color inicial según paridad
                    par ? Color.FromArgb(190, 70, 130) : Color.FromArgb(210, 90, 150));   // Color final según paridad
                g.FillRectangle(grad, rect);                                               // Rellena la celda con el degradado

                // Brillo superior izquierdo (efecto 3D suave)
                using var brillo = new SolidBrush(Color.FromArgb(60, 255, 255, 255));    // Pincel blanco semitransparente para el brillo
                g.FillRectangle(brillo, px + 1, py + 1, TAM_CELDA - 2, 3);              // Dibuja la franja horizontal de brillo superior
                g.FillRectangle(brillo, px + 1, py + 1, 3, TAM_CELDA - 2);              // Dibuja la franja vertical de brillo izquierdo

                if (celda.TieneBandera)                                   // Si la celda tiene una bandera colocada
                    DibujarBandera(g, px + TAM_CELDA / 2, py + TAM_CELDA / 2); // Dibuja la bandera centrada en la celda
            }

            // Borde de la celda
            using var borde = new Pen(C_BORDE_SUAVE, 0.8f); // Pluma rosa oscuro muy fina para el borde de la celda
            g.DrawRectangle(borde, px, py, TAM_CELDA, TAM_CELDA); // Dibuja el borde exterior de la celda
        }

        // Dibuja el sprite de una mina con pinchos, brillo y acento neón
        private void DibujarMina(Graphics g, int cx, int cy)
        {
            int r = 9; // Radio del cuerpo circular de la mina en píxeles
            // Destello de fondo rojo-rosa cuando explota
            using var destello = new SolidBrush(Color.FromArgb(80, 255, 50, 100)); // Pincel rosa semitransparente para el destello
            g.FillEllipse(destello, cx - r - 5, cy - r - 5, (r + 5) * 2, (r + 5) * 2); // Dibuja el círculo de destello más grande que la mina

            // Cuerpo de la mina
            using var cuerpo = new SolidBrush(C_MINA);             // Pincel negro-púrpura para el cuerpo
            g.FillEllipse(cuerpo, cx - r, cy - r, r * 2, r * 2);  // Dibuja el cuerpo circular de la mina

            // Pinchos
            using var pincho = new Pen(C_MINA, 2.5f); // Pluma negra gruesa para los pinchos
            int[] angulos = { 0, 45, 90, 135, 180, 225, 270, 315 }; // Ángulos de los 8 pinchos distribuidos uniformemente
            foreach (int ang in angulos) // Dibuja cada pincho en su ángulo correspondiente
            {
                double rad = ang * Math.PI / 180.0;  // Convierte el ángulo de grados a radianes
                g.DrawLine(pincho,
                    cx + (int)((r - 1) * Math.Cos(rad)),   // Punto de inicio del pincho (borde del cuerpo)
                    cy + (int)((r - 1) * Math.Sin(rad)),
                    cx + (int)((r + 5) * Math.Cos(rad)),   // Punto final del pincho (5px fuera del cuerpo)
                    cy + (int)((r + 5) * Math.Sin(rad)));
            }

            // Brillo
            using var brilloMina = new SolidBrush(Color.FromArgb(120, 255, 255, 255)); // Pincel blanco semitransparente para el brillo
            g.FillEllipse(brilloMina, cx - r / 2 - 1, cy - r / 2 - 1, r / 2 + 2, r / 2 + 2); // Dibuja el pequeño reflejo de luz en la mina

            // Acento rosa neón encima
            using var neon = new SolidBrush(Color.FromArgb(60, 255, 20, 147)); // Pincel rosa neón muy semitransparente
            g.FillEllipse(neon, cx - r, cy - r, r * 2, r * 2);                 // Superpone el tinte neón rosa sobre la mina
        }

        // Dibuja el sprite de una bandera con palo, tela triangular y base
        private void DibujarBandera(Graphics g, int cx, int cy)
        {
            // Palo
            using var palo = new Pen(Color.FromArgb(40, 0, 60), 2.5f); // Pluma morada oscura para el palo de la bandera
            g.DrawLine(palo, cx, cy + 9, cx, cy - 9);                   // Dibuja el palo vertical de la bandera

            // Bandera morada con borde negro
            var puntosBandera = new PointF[]  // Define los vértices del triángulo de la tela de la bandera
            {
                new PointF(cx,      cy - 9),  // Vértice superior (unido al palo)
                new PointF(cx + 12, cy - 4),  // Vértice derecho (punta de la bandera)
                new PointF(cx,      cy + 1)   // Vértice inferior (unido al palo)
            };
            using var rellenoBandera = new SolidBrush(Color.FromArgb(140, 0, 200)); // Pincel morado para el relleno de la bandera
            g.FillPolygon(rellenoBandera, puntosBandera);                             // Dibuja el triángulo relleno de la bandera
            using var bordeBandera = new Pen(Color.FromArgb(30, 0, 50), 1.5f);      // Pluma morada muy oscura para el borde
            g.DrawPolygon(bordeBandera, puntosBandera);                               // Dibuja el borde del triángulo de la bandera

            // Brillo en la bandera
            using var brilloBandera = new SolidBrush(Color.FromArgb(60, 200, 150, 255)); // Pincel lila semitransparente para el brillo
            g.FillPolygon(brilloBandera, new PointF[]  // Dibuja un triángulo más pequeño como brillo sobre la bandera
            {
                new PointF(cx,      cy - 9),   // Vértice superior del brillo
                new PointF(cx + 8,  cy - 5),   // Vértice derecho del brillo
                new PointF(cx,      cy - 2)    // Vértice inferior del brillo
            });

            // Base del palo
            using var base1 = new SolidBrush(Color.FromArgb(50, 0, 70)); // Pincel morado muy oscuro para la base
            g.FillEllipse(base1, cx - 5, cy + 7, 10, 4);                  // Dibuja la base ovalada que sostiene el palo
        }

        // Dibuja un overlay semitransparente con título y subtítulo centrados (pausa, game over, victoria)
        private void DibujarOverlay(Graphics g, string titulo, string subtitulo,
                                     Color colorFondo, Color colorTitulo)
        {
            using var fondo = new SolidBrush(colorFondo);                          // Pincel del color de fondo semitransparente del overlay
            g.FillRectangle(fondo, 0, 0, ANCHO_TABLERO, ALTO_TABLERO);            // Cubre todo el tablero con el color de fondo

            int rw = 460, rh = 220;                        // Ancho y alto de la caja de mensaje del overlay
            int rx = (ANCHO_TABLERO - rw) / 2;            // Posición X centrada horizontalmente
            int ry = (ALTO_TABLERO - rh) / 2;             // Posición Y centrada verticalmente
            var rect = new Rectangle(rx, ry, rw, rh);     // Rectángulo de la caja de mensaje

            using var caja = new SolidBrush(Color.FromArgb(220, 25, 5, 30)); // Pincel morado oscuro muy opaco para el fondo de la caja
            g.FillRectangle(caja, rect);                                       // Rellena el fondo de la caja de mensaje

            using var borde = new Pen(C_NEON_ROSA, 3f);    // Pluma rosa neón de 3px para el borde de la caja
            g.DrawRectangle(borde, rect);                    // Dibuja el borde rosa neón de la caja
            using var glow = new Pen(Color.FromArgb(80, 255, 20, 180), 8f); // Pluma rosa neón semitransparente gruesa para el brillo
            g.DrawRectangle(glow, rect);                                       // Dibuja el efecto de brillo alrededor de la caja

            // Decoración de esquinas moradas
            using var esquina = new Pen(C_NEON_MORADO, 2f); // Pluma morada neón para las decoraciones de esquinas
            int k = 14; // Longitud de cada trazo decorativo de esquina en píxeles
            g.DrawLine(esquina, rx, ry, rx + k, ry);                         // Trazo horizontal de la esquina superior izquierda
            g.DrawLine(esquina, rx, ry, rx, ry + k);                         // Trazo vertical de la esquina superior izquierda
            g.DrawLine(esquina, rx + rw, ry, rx + rw - k, ry);              // Trazo horizontal de la esquina superior derecha
            g.DrawLine(esquina, rx + rw, ry, rx + rw, ry + k);              // Trazo vertical de la esquina superior derecha
            g.DrawLine(esquina, rx, ry + rh, rx + k, ry + rh);              // Trazo horizontal de la esquina inferior izquierda
            g.DrawLine(esquina, rx, ry + rh, rx, ry + rh - k);              // Trazo vertical de la esquina inferior izquierda
            g.DrawLine(esquina, rx + rw, ry + rh, rx + rw - k, ry + rh);   // Trazo horizontal de la esquina inferior derecha
            g.DrawLine(esquina, rx + rw, ry + rh, rx + rw, ry + rh - k);   // Trazo vertical de la esquina inferior derecha

            // Configuración de alineación centrada para el texto
            var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,     // Centra el texto horizontalmente
                LineAlignment = StringAlignment.Near    // Alinea el texto desde la parte superior del área
            };

            using var fTitulo = new Font("Consolas", 26f, FontStyle.Bold);   // Fuente grande en negrita para el título del overlay
            using var bTitulo = new SolidBrush(colorTitulo);                 // Pincel del color del título pasado como parámetro
            g.DrawString(titulo, fTitulo, bTitulo,
                new RectangleF(rx, ry + 20, rw, 70), sf); // Dibuja el título centrado con margen superior de 20px

            using var fSub = new Font("Consolas", 10.5f, FontStyle.Regular);        // Fuente más pequeña para el subtítulo
            using var bSub = new SolidBrush(Color.FromArgb(220, 255, 200, 230));   // Pincel rosa muy claro semitransparente para el subtítulo
            g.DrawString(subtitulo, fSub, bSub,
                new RectangleF(rx, ry + 100, rw, 110), sf); // Dibuja el subtítulo centrado debajo del título
        }
    }
}