using MiniJuegos;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace MiniJuegos
{
    public abstract class EntidadJuego
    {
        public float X { get; set; }              
        public float Y { get; set; }             
        public int Ancho { get; protected set; }  
        public int Alto { get; protected set; }   
        public bool Activo { get; set; } = true;  

        public RectangleF Hitbox => new RectangleF(X, Y, Ancho, Alto); // Propiedad calculada: rectángulo de colisión basado en posición y tamaño

        public abstract void Actualizar(float delta);  // Método abstracto: cada entidad define cómo actualiza su estado por frame
        public abstract void Dibujar(System.Windows.Forms.PaintEventArgs e); // Método abstracto: cada entidad define cómo se dibuja en pantalla
        
        public bool ColisionaCon(EntidadJuego otra) // Verifica si esta entidad colisiona con otra entidad activa
        {
            return Activo && otra.Activo && Hitbox.IntersectsWith(otra.Hitbox); // Retorna true solo si ambas están activas y sus hitboxes se intersectan
        }
    }

   
    public class Cohete : EntidadJuego
    {
        public int Vidas { get; private set; } = 3;             
        public bool Invulnerable { get; private set; } = false; // Indica si el jugador está en período de invulnerabilidad; por defecto false
        private float tiempoInvulnerable = 0f;                  // Acumulador de tiempo transcurrido durante la invulnerabilidad
        private const float DURACION_INVULNERABILIDAD = 2f;     // Duración fija de la invulnerabilidad en segundos (2 segundos)
        private float frameParpadeo = 0f;                       // Acumulador para controlar el efecto de parpadeo durante la invulnerabilidad
        public bool Visible { get; private set; } = true;       // Indica si el cohete es visible en pantalla (parpadeo); por defecto true

        // Constructor: inicializa la posición y el tamaño del cohete
        public Cohete(float x, float y)
        {
            X = x; Y = y;         // Asigna la posición inicial
            Ancho = 40; Alto = 60; // Define el tamaño del cohete en píxeles
        }

        // Método que aplica daño al cohete si no está en período de invulnerabilidad
        public void RecibirDanio()
        {
            if (Invulnerable) return;      // Si está invulnerable, ignora el daño
            Vidas--;                        // Reduce una vida
            Invulnerable = true;            // Activa el período de invulnerabilidad
            tiempoInvulnerable = 0f;        // Reinicia el contador de invulnerabilidad
            if (Vidas <= 0) Activo = false; // Si no quedan vidas, desactiva el cohete (jugador muerto)
        }

        // Actualiza el estado del cohete en cada frame según el tiempo transcurrido
        public override void Actualizar(float delta)
        {
            if (Invulnerable) // Solo ejecuta lógica de invulnerabilidad si está activa
            {
                tiempoInvulnerable += delta;                   // Suma el tiempo transcurrido al acumulador de invulnerabilidad
                frameParpadeo += delta;                        // Suma el tiempo al acumulador de parpadeo
                Visible = (frameParpadeo % 0.2f) < 0.1f;      // Alterna visibilidad cada 0.1 segundos para efecto de parpadeo
                if (tiempoInvulnerable >= DURACION_INVULNERABILIDAD) // Si se cumplió el tiempo de invulnerabilidad
                {
                    Invulnerable = false; // Desactiva la invulnerabilidad
                    Visible = true;       // Asegura que el cohete quede visible
                }
            }
        }

        // Dibuja el cohete en pantalla con todos sus elementos visuales
        public override void Dibujar(System.Windows.Forms.PaintEventArgs e)
        {
            if (!Visible) return; // Si el cohete no es visible (parpadeo), no dibuja nada
            var g = e.Graphics;   // Obtiene el objeto de dibujo del evento

            // Llama del cohete
            using var pincelLlama = new System.Drawing.Drawing2D.LinearGradientBrush( // Crea un pincel de degradado para la llama
                new PointF(X + 10, Y + Alto),           // Punto inicial del degradado (base de la llama)
                new PointF(X + 30, Y + Alto + 20),      // Punto final del degradado (punta de la llama)
                Color.FromArgb(255, 100, 0),            // Color inicial: naranja
                Color.FromArgb(255, 220, 0));           // Color final: amarillo
            g.FillEllipse(pincelLlama, X + 12, Y + Alto - 5, 16, 20); // Dibuja la llama como una elipse con el degradado

            // Cuerpo del cohete
            using var pincelCuerpo = new System.Drawing.Drawing2D.LinearGradientBrush( // Crea un pincel de degradado para el cuerpo
                new PointF(X, Y),            // Punto inicial del degradado (lado izquierdo)
                new PointF(X + Ancho, Y),    // Punto final del degradado (lado derecho)
                Color.FromArgb(180, 0, 255), // Color inicial: violeta
                Color.FromArgb(255, 0, 200)); // Color final: magenta
            var puntosRocket = new PointF[]  // Define los vértices del polígono que forma el cuerpo del cohete
            {
                new PointF(X + Ancho / 2, Y),          // Punta superior (nariz del cohete)
                new PointF(X + Ancho, Y + Alto * 0.6f), // Esquina inferior derecha del cuerpo
                new PointF(X + Ancho, Y + Alto),        // Esquina inferior derecha (base)
                new PointF(X, Y + Alto),                // Esquina inferior izquierda (base)
                new PointF(X, Y + Alto * 0.6f)          // Esquina inferior izquierda del cuerpo
            };
            g.FillPolygon(pincelCuerpo, puntosRocket); // Dibuja el cuerpo del cohete con el degradado

            // Ventana neón
            using var pincelVentana = new SolidBrush(Color.FromArgb(0, 255, 255)); // Pincel cian para la ventana
            g.FillEllipse(pincelVentana, X + 13, Y + 18, 14, 14);                 // Dibuja la ventana circular del cohete
            using var bordeVentana = new Pen(Color.White, 1.5f);                   // Pluma blanca para el borde de la ventana
            g.DrawEllipse(bordeVentana, X + 13, Y + 18, 14, 14);                  // Dibuja el borde de la ventana

            // Alas
            using var pincelAlas = new SolidBrush(Color.FromArgb(200, 0, 180)); // Pincel magenta oscuro para las alas
            var alaIzq = new PointF[] {               // Define los vértices del triángulo del ala izquierda
                new PointF(X, Y + Alto * 0.6f),       // Vértice superior del ala izquierda
                new PointF(X - 12, Y + Alto),          // Vértice inferior externo del ala izquierda
                new PointF(X, Y + Alto)                // Vértice inferior interno del ala izquierda
            };
            var alaDer = new PointF[] {                      // Define los vértices del triángulo del ala derecha
                new PointF(X + Ancho, Y + Alto * 0.6f),     // Vértice superior del ala derecha
                new PointF(X + Ancho + 12, Y + Alto),        // Vértice inferior externo del ala derecha
                new PointF(X + Ancho, Y + Alto)              // Vértice inferior interno del ala derecha
            };
            g.FillPolygon(pincelAlas, alaIzq); // Dibuja el ala izquierda
            g.FillPolygon(pincelAlas, alaDer); // Dibuja el ala derecha
        }
    }

    // Clase que representa un meteorito que cae desde la parte superior de la pantalla
    public class Meteorito : EntidadJuego
    {
        private float velocidad;                      // Velocidad de caída del meteorito en píxeles por segundo
        private float rotacion = 0f;                  // Ángulo de rotación actual del meteorito en grados; comienza en 0
        private static readonly Random rng = new Random(); // Instancia estática de Random para variaciones aleatorias
        private int lados;                            // Número de lados del polígono que representa el meteorito

        // Constructor: inicializa posición, velocidad y forma aleatoria del meteorito
        public Meteorito(float x, float y, float vel)
        {
            X = x; Y = y;                   // Asigna la posición inicial
            velocidad = vel;                 // Asigna la velocidad de caída
            Ancho = rng.Next(20, 45);        // Asigna un ancho aleatorio entre 20 y 44 píxeles
            Alto = Ancho;                    // El alto es igual al ancho (forma cuadrada base)
            lados = rng.Next(5, 9);          // Asigna un número aleatorio de lados entre 5 y 8 para el polígono
        }

        // Actualiza la posición y rotación del meteorito en cada frame
        public override void Actualizar(float delta)
        {
            Y += velocidad * delta;    // Mueve el meteorito hacia abajo según su velocidad y el tiempo transcurrido
            rotacion += 60f * delta;   // Incrementa el ángulo de rotación (60 grados por segundo)
            if (Y > 700) Activo = false; // Si el meteorito sale por la parte inferior de la pantalla, lo desactiva
        }

        // Dibuja el meteorito como un polígono rotado con efecto de brillo
        public override void Dibujar(System.Windows.Forms.PaintEventArgs e)
        {
            var g = e.Graphics;          // Obtiene el objeto de dibujo
            var estado = g.Save();       // Guarda el estado actual del contexto gráfico para restaurarlo después
            g.TranslateTransform(X + Ancho / 2, Y + Alto / 2); // Traslada el origen al centro del meteorito para rotar desde el centro
            g.RotateTransform(rotacion);  // Aplica la rotación actual al contexto gráfico

            var puntos = new PointF[lados]; // Crea el arreglo de vértices del polígono del meteorito
            for (int i = 0; i < lados; i++) // Calcula cada vértice del polígono
            {
                double angulo = (Math.PI * 2 / lados) * i;  // Ángulo del vértice i distribuido uniformemente en el círculo
                float r = Ancho / 2 * (0.7f + (float)(new Random(i * 7 + lados).NextDouble() * 0.3f)); // Radio con variación aleatoria para forma irregular
                puntos[i] = new PointF((float)Math.Cos(angulo) * r, (float)Math.Sin(angulo) * r); // Calcula la posición del vértice usando trigonometría
            }

            using var pincel = new SolidBrush(Color.FromArgb(255, 105, 180)); // Pincel rosa para el relleno del meteorito
            using var borde = new Pen(Color.FromArgb(255, 20, 147), 2f);      // Pluma rosa oscuro para el borde del meteorito
            g.FillPolygon(pincel, puntos); // Dibuja el relleno del polígono del meteorito
            g.DrawPolygon(borde, puntos);  // Dibuja el borde del polígono del meteorito

            // Brillo
            using var brillo = new SolidBrush(Color.FromArgb(80, 255, 255, 255)); // Pincel blanco semitransparente para el brillo
            g.FillEllipse(brillo, -Ancho / 6, -Alto / 6, Ancho / 4, Alto / 4);   // Dibuja una pequeña elipse como reflejo de luz

            g.Restore(estado); // Restaura el estado del contexto gráfico (deshace la rotación y traslación)
        }
    }

    // Clase que representa un alien enemigo que se mueve y dispara corazones
    public class Alien : EntidadJuego
    {
        private float velocidadX;                          // Velocidad horizontal del alien en píxeles por segundo
        private float tiempoDisparo = 0f;                  // Acumulador de tiempo para controlar el intervalo de disparo
        private float intervaloDisparo;                    // Tiempo en segundos entre disparos del alien
        public event EventHandler<PointF>? DisparoCorazon; // Evento que se dispara cuando el alien lanza un corazón; pasa la posición de disparo
        private static readonly Random rng = new Random(); // Instancia estática de Random para valores aleatorios
        private float animFrame = 0f;                      // Acumulador de tiempo para controlar la animación de rebote

        // Constructor: inicializa posición, tamaño, dirección y frecuencia de disparo del alien
        public Alien(float x, float y)
        {
            X = x; Y = y;             // Asigna la posición inicial
            Ancho = 50; Alto = 50;    // Define el tamaño del alien en píxeles
            velocidadX = rng.Next(0, 2) == 0 ? 80f : -80f; // Asigna dirección horizontal aleatoria: derecha (80) o izquierda (-80)
            intervaloDisparo = rng.Next(2, 5);              // Asigna un intervalo de disparo aleatorio entre 2 y 4 segundos
        }

        // Actualiza el movimiento, animación y disparo del alien en cada frame
        public override void Actualizar(float delta)
        {
            X += velocidadX * delta;  // Mueve el alien horizontalmente según su velocidad
            animFrame += delta;        // Incrementa el acumulador de animación
            if (X < 0 || X > 750) velocidadX *= -1; // Si toca los bordes laterales, invierte la dirección de movimiento

            tiempoDisparo += delta;                  // Incrementa el acumulador de tiempo de disparo
            if (tiempoDisparo >= intervaloDisparo)   // Si se cumplió el intervalo de disparo
            {
                tiempoDisparo = 0f;                  // Reinicia el acumulador de disparo
                DisparoCorazon?.Invoke(this, new PointF(X + Ancho / 2, Y + Alto)); // Dispara el evento con la posición de lanzamiento del corazón
            }
        }

        // Dibuja el alien con todos sus elementos visuales (cuerpo, ojos, peluca, antenas, etc.)
        public override void Dibujar(System.Windows.Forms.PaintEventArgs e)
        {
            var g = e.Graphics;                                  // Obtiene el objeto de dibujo
            float bounce = (float)Math.Sin(animFrame * 3) * 3f; // Calcula el desplazamiento vertical de rebote usando una función seno

            // Cuerpo verde alienígena
            using var pincelCuerpo = new SolidBrush(Color.FromArgb(50, 205, 50)); // Pincel verde para el cuerpo
            g.FillEllipse(pincelCuerpo, X + 5, Y + 20 + bounce, 40, 28);          // Dibuja el cuerpo ovalado del alien

            // Cabeza
            using var pincelCabeza = new SolidBrush(Color.FromArgb(80, 220, 80)); // Pincel verde claro para la cabeza
            g.FillEllipse(pincelCabeza, X + 8, Y + bounce, 34, 30);               // Dibuja la cabeza ovalada del alien

            // Peluca rosa
            using var pincelPeluca = new SolidBrush(Color.FromArgb(255, 105, 180)); // Pincel rosa para la peluca
            g.FillEllipse(pincelPeluca, X, Y - 8 + bounce, 50, 20);               // Dibuja la parte superior redondeada de la peluca
            g.FillRectangle(pincelPeluca, X, Y - 2 + bounce, 6, 18);             // Dibuja el mechón izquierdo de la peluca
            g.FillRectangle(pincelPeluca, X + 44, Y - 2 + bounce, 6, 18);        // Dibuja el mechón derecho de la peluca

            // Lazo en la peluca
            using var pincelLazo = new SolidBrush(Color.FromArgb(255, 20, 147)); // Pincel rosa oscuro para el lazo
            var puntosLazo = new PointF[] {                    // Define los vértices del rombo que forma el lazo
                new PointF(X + 20, Y - 10 + bounce),          // Vértice izquierdo del lazo
                new PointF(X + 25, Y - 5 + bounce),           // Vértice inferior del lazo
                new PointF(X + 30, Y - 10 + bounce),          // Vértice derecho del lazo
                new PointF(X + 25, Y - 15 + bounce)           // Vértice superior del lazo
            };
            g.FillPolygon(pincelLazo, puntosLazo); // Dibuja el lazo sobre la peluca

            // Ojos grandes con pestañas
            using var pincelOjo = new SolidBrush(Color.White);                        // Pincel blanco para la esclerótica de los ojos
            g.FillEllipse(pincelOjo, X + 11, Y + 7 + bounce, 12, 12);                // Dibuja el ojo izquierdo (esclerótica)
            g.FillEllipse(pincelOjo, X + 27, Y + 7 + bounce, 12, 12);                // Dibuja el ojo derecho (esclerótica)
            using var pincelPupila = new SolidBrush(Color.FromArgb(180, 0, 180));    // Pincel violeta para las pupilas
            g.FillEllipse(pincelPupila, X + 15, Y + 10 + bounce, 6, 6);              // Dibuja la pupila del ojo izquierdo
            g.FillEllipse(pincelPupila, X + 31, Y + 10 + bounce, 6, 6);              // Dibuja la pupila del ojo derecho

            // Pestañas
            using var pincelPest = new Pen(Color.Black, 1.5f); // Pluma negra para las pestañas
            for (int i = 0; i < 3; i++)                        // Dibuja 3 pestañas por ojo
            {
                g.DrawLine(pincelPest, X + 13 + i * 2, Y + 6 + bounce, X + 11 + i * 2, Y + 2 + bounce); // Dibuja cada pestaña del ojo izquierdo
                g.DrawLine(pincelPest, X + 29 + i * 2, Y + 6 + bounce, X + 27 + i * 2, Y + 2 + bounce); // Dibuja cada pestaña del ojo derecho
            }

            using var pincelLabio = new SolidBrush(Color.FromArgb(220, 20, 60));    // Pincel rojo carmesí para los labios
            g.FillEllipse(pincelLabio, X + 16, Y + 22 + bounce, 18, 8);             // Dibuja la parte inferior de la boca
            using var pincelLabioSup = new SolidBrush(Color.FromArgb(180, 0, 60));  // Pincel rojo oscuro para el labio superior
            g.FillEllipse(pincelLabioSup, X + 16, Y + 20 + bounce, 18, 5);          // Dibuja el labio superior de la boca

            // Antenas
            using var pincelAntena = new Pen(Color.FromArgb(50, 205, 50), 2f);  // Pluma verde para las antenas
            g.DrawLine(pincelAntena, X + 17, Y + bounce, X + 12, Y - 12 + bounce); // Dibuja la antena izquierda
            g.DrawLine(pincelAntena, X + 33, Y + bounce, X + 38, Y - 12 + bounce); // Dibuja la antena derecha
            using var pincelBolita = new SolidBrush(Color.FromArgb(255, 105, 180)); // Pincel rosa para las bolitas de las antenas
            g.FillEllipse(pincelBolita, X + 8, Y - 16 + bounce, 8, 8);              // Dibuja la bolita de la antena izquierda
            g.FillEllipse(pincelBolita, X + 34, Y - 16 + bounce, 8, 8);             // Dibuja la bolita de la antena derecha

            // Brazos con bolso
            using var pincelBrazo = new Pen(Color.FromArgb(50, 205, 50), 3f);   // Pluma verde gruesa para los brazos
            g.DrawLine(pincelBrazo, X + 5, Y + 30 + bounce, X - 8, Y + 38 + bounce);   // Dibuja el brazo izquierdo
            g.DrawLine(pincelBrazo, X + 45, Y + 30 + bounce, X + 55, Y + 38 + bounce); // Dibuja el brazo derecho
            using var pincelBolso = new SolidBrush(Color.FromArgb(255, 20, 147));       // Pincel rosa oscuro para el bolso
            g.FillRectangle(pincelBolso, X + 53, Y + 34 + bounce, 10, 8);              // Dibuja el bolso en la mano derecha del alien
        }
    }

    // Clase que representa un corazón disparado por un alien hacia el jugador
    public class Corazon : EntidadJuego
    {
        private float velocidad = 180f; // Velocidad de caída del corazón en píxeles por segundo

        // Constructor: inicializa la posición y el tamaño del corazón
        public Corazon(float x, float y)
        {
            X = x; Y = y;          // Asigna la posición inicial
            Ancho = 18; Alto = 16; // Define el tamaño del corazón en píxeles
        }

        // Actualiza la posición del corazón en cada frame
        public override void Actualizar(float delta)
        {
            Y += velocidad * delta;    // Mueve el corazón hacia abajo según su velocidad
            if (Y > 700) Activo = false; // Si sale por la parte inferior de la pantalla, lo desactiva
        }

        // Dibuja el corazón como una forma simplificada con dos círculos y un triángulo
        public override void Dibujar(System.Windows.Forms.PaintEventArgs e)
        {
            var g = e.Graphics; // Obtiene el objeto de dibujo
            // Corazón verde tóxico
            using var pincel = new SolidBrush(Color.FromArgb(0, 255, 80));           // Pincel verde tóxico para el corazón
            using var brillo = new Pen(Color.FromArgb(150, 200, 255, 100), 1.5f);    // Pluma verde semitransparente para el brillo neón

            // Forma de corazón simplificada con dos círculos + triángulo
            g.FillEllipse(pincel, X, Y, 10, 10);      // Dibuja el círculo izquierdo de la parte superior del corazón
            g.FillEllipse(pincel, X + 8, Y, 10, 10);  // Dibuja el círculo derecho de la parte superior del corazón
            var puntosCorazon = new PointF[] {         // Define los vértices del triángulo inferior del corazón
                new PointF(X, Y + 6),                  // Vértice izquierdo del triángulo
                new PointF(X + 9, Y + 16),             // Vértice inferior (punta del corazón)
                new PointF(X + 18, Y + 6)              // Vértice derecho del triángulo
            };
            g.FillPolygon(pincel, puntosCorazon); // Dibuja la parte inferior triangular del corazón

            // Brillo neón
            g.DrawEllipse(brillo, X, Y, 10, 10);     // Dibuja el borde luminoso del círculo izquierdo
            g.DrawEllipse(brillo, X + 8, Y, 10, 10); // Dibuja el borde luminoso del círculo derecho
        }
    }

    // Clase que representa una bala disparada por el jugador hacia arriba
    public class Bala : EntidadJuego
    {
        private float velocidad = 400f; // Velocidad de desplazamiento de la bala en píxeles por segundo (hacia arriba)

        // Constructor: inicializa la posición y el tamaño de la bala
        public Bala(float x, float y)
        {
            X = x; Y = y;        // Asigna la posición inicial
            Ancho = 6; Alto = 14; // Define el tamaño de la bala en píxeles
        }

        // Actualiza la posición de la bala en cada frame
        public override void Actualizar(float delta)
        {
            Y -= velocidad * delta;   // Mueve la bala hacia arriba (resta Y) según su velocidad
            if (Y < -20) Activo = false; // Si sale por la parte superior de la pantalla, la desactiva
        }

        // Dibuja la bala con un degradado y un halo neón
        public override void Dibujar(System.Windows.Forms.PaintEventArgs e)
        {
            var g = e.Graphics; // Obtiene el objeto de dibujo
            using var pincelBala = new System.Drawing.Drawing2D.LinearGradientBrush( // Crea un pincel de degradado para la bala
                new PointF(X, Y), new PointF(X, Y + Alto), // Define la dirección del degradado (vertical)
                Color.FromArgb(0, 255, 255),               // Color superior: cian
                Color.FromArgb(180, 0, 255));              // Color inferior: violeta
            g.FillRectangle(pincelBala, X, Y, Ancho, Alto); // Dibuja el rectángulo de la bala con el degradado

            using var halo = new Pen(Color.FromArgb(80, 0, 255, 255), 2f);        // Pluma cian semitransparente para el halo neón
            g.DrawRectangle(halo, X - 1, Y - 1, Ancho + 2, Alto + 2);            // Dibuja el halo alrededor de la bala ligeramente más grande
        }
    }

    // ─── Puntuación y lógica con LINQ ────────────────────────────────────────

    // Clase que lleva el historial de puntajes de las partidas jugadas
    public class RegistroPuntuacion
    {
        private List<int> historial = new List<int>(); // Lista privada que almacena los puntajes de cada partida

        public void Agregar(int puntos) => historial.Add(puntos); // Agrega un nuevo puntaje al historial

        public int MejorPuntaje() => historial.Any() ? historial.Max() : 0; // Retorna el puntaje más alto del historial; 0 si no hay partidas

        public double PromedioPuntajes() => historial.Any() ? historial.Average() : 0; // Retorna el promedio de todos los puntajes; 0 si no hay partidas

        public IEnumerable<int> PuntajesMayoresQue(int umbral) =>         // Retorna los puntajes superiores al umbral dado
            historial.Where(p => p > umbral).OrderByDescending(p => p);   // Filtra y ordena de mayor a menor

        public int TotalPartidas() => historial.Count; // Retorna la cantidad total de partidas registradas
    }

    // ─── Estado del juego ────────────────────────────────────────────────────

    // Enum que define los posibles estados generales del juego
    public enum EstadoJuego { Jugando, Pausado, GameOver, Victoria }

    // Clase que almacena y gestiona el estado actual del juego (puntos, nivel, velocidades)
    public class EstadoCohete
    {
        public EstadoJuego Estado { get; set; } = EstadoJuego.Jugando; // Estado actual del juego; por defecto Jugando
        public int Puntos { get; set; } = 0;                           // Puntuación acumulada del jugador; comienza en 0
        public int Nivel { get; private set; } = 1;                    // Nivel actual del juego; comienza en 1
        private float tiempoNivel = 0f;                                // Acumulador de tiempo para controlar el avance de nivel

        // Actualiza el nivel cada 20 segundos de juego
        public void ActualizarNivel(float delta)
        {
            tiempoNivel += delta;     // Suma el tiempo transcurrido al acumulador
            if (tiempoNivel >= 20f)   // Si pasaron 20 segundos
            {
                tiempoNivel = 0f;     // Reinicia el acumulador de tiempo de nivel
                Nivel++;              // Sube al siguiente nivel
            }
        }

        public float VelocidadMeteoritos => 150f + (Nivel - 1) * 30f;                  // Velocidad de los meteoritos: aumenta 30 por nivel
        public float IntervaloSpawnMeteoritos => Math.Max(0.5f, 1.5f - (Nivel - 1) * 0.15f); // Intervalo de spawn de meteoritos: disminuye por nivel (mínimo 0.5s)
        public float IntervaloSpawnAliens => Math.Max(3f, 8f - (Nivel - 1) * 0.5f);   // Intervalo de spawn de aliens: disminuye por nivel (mínimo 3s)
    }

    // ─── Motor principal del juego ────────────────────────────────────────────

    // Clase principal que coordina toda la lógica del juego del cohete
    public class JuegoCohete
    {
        public Cohete Jugador { get; private set; }                          // Referencia al cohete controlado por el jugador
        public List<Meteorito> Meteoritos { get; private set; } = new();     // Lista de meteoritos activos en pantalla
        public List<Alien> Aliens { get; private set; } = new();             // Lista de aliens activos en pantalla
        public List<Corazon> Corazones { get; private set; } = new();        // Lista de corazones (proyectiles alien) activos en pantalla
        public List<Bala> Balas { get; private set; } = new();               // Lista de balas disparadas por el jugador activas en pantalla
        public EstadoCohete Estado { get; private set; } = new();            // Estado actual del juego (nivel, puntos, estado)
        public RegistroPuntuacion Puntuaciones { get; private set; } = new(); // Registro histórico de puntuaciones del jugador

        private static readonly Random rng = new Random();   // Instancia estática de Random para posiciones aleatorias de spawn
        private float timerMeteoritos = 0f;                   // Acumulador de tiempo para controlar el spawn de meteoritos
        private float timerAliens = 0f;                       // Acumulador de tiempo para controlar el spawn de aliens
        private float timerDisparo = 0f;                      // Acumulador de tiempo para controlar el cooldown de disparo
        private const float COOLDOWN_DISPARO = 0.3f;          // Tiempo mínimo entre disparos del jugador (0.3 segundos)

        private readonly int anchoPanel; // Ancho del panel de juego en píxeles (límite horizontal)
        private readonly int altoPanel;  // Alto del panel de juego en píxeles (límite vertical)

        // Eventos para notificar a la UI sobre cambios importantes del juego
        public event EventHandler? JugadorMurio;    // Evento que se dispara cuando el jugador muere
        public event EventHandler? JuegoTerminado;  // Evento que se dispara cuando termina la partida

        // Constructor: inicializa el motor del juego con las dimensiones del panel
        public JuegoCohete(int ancho, int alto)
        {
            anchoPanel = ancho;                                        
            altoPanel = alto;                                          
            Jugador = new Cohete(ancho / 2 - 20, alto - 100);        // Crea el cohete centrado horizontalmente cerca de la parte inferior
        }

        // Actualiza toda la lógica del juego en cada frame
        public void Actualizar(float delta)
        {
            if (Estado.Estado != EstadoJuego.Jugando) return; // Si el juego no está activo, no actualiza nada

            Estado.ActualizarNivel(delta); // Actualiza el nivel según el tiempo transcurrido
            Jugador.Actualizar(delta);     // Actualiza el estado del cohete del jugador

            // Spawn meteoritos
            timerMeteoritos += delta;                                       // Incrementa el acumulador de spawn de meteoritos
            if (timerMeteoritos >= Estado.IntervaloSpawnMeteoritos)         // Si se cumplió el intervalo de spawn
            {
                timerMeteoritos = 0f;                                       // Reinicia el acumulador
                float xSpawn = rng.Next(10, anchoPanel - 50);              // Genera una posición X aleatoria dentro del panel
                Meteoritos.Add(new Meteorito(xSpawn, -50, Estado.VelocidadMeteoritos)); // Crea un meteorito fuera de pantalla (arriba) y lo agrega
            }

            // Spawn aliens
            timerAliens += delta;                                           // Incrementa el acumulador de spawn de aliens
            if (timerAliens >= Estado.IntervaloSpawnAliens)                 // Si se cumplió el intervalo de spawn de aliens
            {
                timerAliens = 0f;                                           // Reinicia el acumulador
                float xAlien = rng.Next(10, anchoPanel - 60);              // Genera una posición X aleatoria para el alien
                var alien = new Alien(xAlien, rng.Next(30, 150));          // Crea el alien en una posición Y aleatoria en la zona superior
                alien.DisparoCorazon += (s, pos) =>                        // Suscribe el evento de disparo del alien
                    Corazones.Add(new Corazon(pos.X - 9, pos.Y));          // Al disparar, crea un corazón centrado en la posición del alien
                Aliens.Add(alien);                                          // Agrega el alien a la lista de activos
            }

            timerDisparo += delta; // Incrementa el acumulador del cooldown de disparo

            // Actualizar entidades con LINQ para limpiar inactivos
            Meteoritos.ForEach(m => m.Actualizar(delta)); // Actualiza todos los meteoritos activos
            Aliens.ForEach(a => a.Actualizar(delta));     // Actualiza todos los aliens activos
            Corazones.ForEach(c => c.Actualizar(delta));  // Actualiza todos los corazones activos
            Balas.ForEach(b => b.Actualizar(delta));      // Actualiza todas las balas activas

            VerificarColisiones();        // Detecta y procesa todas las colisiones entre entidades
            LimpiarEntidadesInactivas();  // Elimina de las listas las entidades que ya no están activas

            if (!Jugador.Activo) // Si el jugador fue eliminado (quedó sin vidas)
            {
                Estado.Estado = EstadoJuego.GameOver;              // Cambia el estado del juego a Game Over
                Puntuaciones.Agregar(Estado.Puntos);               // Registra el puntaje final en el historial
                JuegoTerminado?.Invoke(this, EventArgs.Empty);     // Dispara el evento de juego terminado
            }
        }

        // Detecta y procesa todas las colisiones entre entidades del juego
        private void VerificarColisiones()
        {
            // Balas vs Meteoritos — LINQ: obtener pares que colisionan
            var impactosMeteoritos = (from bala in Balas          // Recorre todas las balas
                                      from met in Meteoritos       // Cruza con todos los meteoritos
                                      where bala.ColisionaCon(met) // Filtra los pares que están colisionando
                                      select (bala, met)).ToList(); // Guarda los pares colisionantes en una lista

            impactosMeteoritos.ForEach(par =>          // Procesa cada colisión bala-meteorito
            {
                par.bala.Activo = false;               // Desactiva la bala impactada
                par.met.Activo = false;                // Desactiva el meteorito impactado
                Estado.Puntos += 10;                   // Suma 10 puntos por meteorito destruido
            });

            // Balas vs Aliens
            var impactosAliens = (from bala in Balas            // Recorre todas las balas
                                  from alien in Aliens           // Cruza con todos los aliens
                                  where bala.ColisionaCon(alien) // Filtra los pares que están colisionando
                                  select (bala, alien)).ToList(); // Guarda los pares colisionantes en una lista

            impactosAliens.ForEach(par =>              // Procesa cada colisión bala-alien
            {
                par.bala.Activo = false;               // Desactiva la bala impactada
                par.alien.Activo = false;              // Desactiva el alien impactado
                Estado.Puntos += 25;                   // Suma 25 puntos por alien destruido
            });

            // Cohete vs Meteoritos
            Meteoritos.Where(m => m.ColisionaCon(Jugador)).ToList()           // Filtra los meteoritos que colisionan con el jugador
                      .ForEach(m => { m.Activo = false; Jugador.RecibirDanio(); }); // Desactiva el meteorito y aplica daño al jugador

            // Cohete vs Corazones
            Corazones.Where(c => c.ColisionaCon(Jugador)).ToList()            // Filtra los corazones que colisionan con el jugador
                     .ForEach(c => { c.Activo = false; Jugador.RecibirDanio(); }); // Desactiva el corazón y aplica daño al jugador
        }

        // Elimina de las listas todas las entidades que ya no están activas
        private void LimpiarEntidadesInactivas()
        {
            Meteoritos.RemoveAll(m => !m.Activo); // Elimina todos los meteoritos inactivos de la lista
            Aliens.RemoveAll(a => !a.Activo);     // Elimina todos los aliens inactivos de la lista
            Corazones.RemoveAll(c => !c.Activo);  // Elimina todos los corazones inactivos de la lista
            Balas.RemoveAll(b => !b.Activo);      // Elimina todas las balas inactivas de la lista
        }

        // Mueve el cohete del jugador dentro de los límites del panel
        public void MoverJugador(float dx, float dy)
        {
            if (Estado.Estado != EstadoJuego.Jugando) return; // Si el juego no está activo, no mueve al jugador
            float nx = Jugador.X + dx;                         // Calcula la nueva posición X
            float ny = Jugador.Y + dy;                         // Calcula la nueva posición Y
            Jugador.X = Math.Clamp(nx, 0, anchoPanel - Jugador.Ancho); // Asigna la nueva X restringida a los límites del panel
            Jugador.Y = Math.Clamp(ny, 0, altoPanel - Jugador.Alto);   // Asigna la nueva Y restringida a los límites del panel
        }

        // Dispara una bala desde el frente del cohete si el cooldown lo permite
        public void Disparar()
        {
            if (Estado.Estado != EstadoJuego.Jugando) return;   // Si el juego no está activo, no dispara
            if (timerDisparo < COOLDOWN_DISPARO) return;         // Si no pasó el tiempo de cooldown, no dispara
            timerDisparo = 0f;                                   // Reinicia el cooldown de disparo
            Balas.Add(new Bala(Jugador.X + Jugador.Ancho / 2 - 3, Jugador.Y - 14)); // Crea una bala centrada en la nariz del cohete y la agrega
        }

        // Alterna el estado del juego entre Pausado y Jugando
        public void TogglePausa()
        {
            Estado.Estado = Estado.Estado == EstadoJuego.Pausado // Si está pausado
                ? EstadoJuego.Jugando                            // Lo reanuda
                : EstadoJuego.Pausado;                           // Si no, lo pausa
        }

        // Reinicia completamente el juego a su estado inicial
        public void Reiniciar()
        {
            Jugador = new Cohete(anchoPanel / 2 - 20, altoPanel - 100); // Crea un nuevo cohete en la posición inicial
            Meteoritos.Clear();    // Limpia la lista de meteoritos
            Aliens.Clear();        // Limpia la lista de aliens
            Corazones.Clear();     // Limpia la lista de corazones
            Balas.Clear();         // Limpia la lista de balas
            Estado = new EstadoCohete(); // Crea un nuevo estado de juego (reinicia nivel, puntos y estado)
            timerMeteoritos = 0f;  // Reinicia el acumulador de spawn de meteoritos
            timerAliens = 0f;      // Reinicia el acumulador de spawn de aliens
            timerDisparo = 0f;     // Reinicia el acumulador de cooldown de disparo
        }
    }
}

// Al inicio del archivo agrega estos usings si no los tienes:
// using BurdiGames.Clases;           // Importa las clases base del proyecto BurdiGames
// using BurdiGames.Clases.Juegos;    // Importa las clases de juegos del proyecto BurdiGames

namespace BurdiGames.Clases.Juegos
{
    // Clase que representa el juego Cohete Espacial como un Arcade dentro del sistema BurdiGames
    internal class JuegoCoheteArcade : Arcade
    {
        // Constructor: configura el juego con nombre, descripción, imagen y número de vidas
        public JuegoCoheteArcade()
            : base(                                      // Llama al constructor de la clase base Arcade
                nombre: "Cohete Espacial",               // Nombre del juego
                descripcion: "Esquiva meteoritos rosas y elimina aliens maquillados en el espacio. utiliza a,d para ir derecha e izquierda" +
                  "y w, s para ir arriba y abajo. y con Enter Disparas", // Descripción con instrucciones de controles
                rutaImagen: "Sources/Imagenes/logo_galaxian.png",         // Ruta de la imagen representativa del juego
                vidas: 3)                                                  // El juego otorga 3 vidas al jugador
        {
        }

        // Método que lanza el formulario del juego cuando el usuario decide jugar
        public override void Jugar()
        {
            var form = new FormCohete(); // Crea una instancia del formulario del juego del cohete
            form.ShowDialog();           // Muestra el formulario como diálogo modal (bloquea la ventana padre hasta cerrarlo)
        }
    }
}
