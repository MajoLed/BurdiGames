using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using BurdiGames.Clases;
using BurdiGames.UI;

namespace BurdiGames.Clases.Juegos
{
    public enum DireccionPac { Ninguna, Arriba, Abajo, Izquierda, Derecha } 
    public enum EstadoFantasma { Perseguir, Asustado, Comido }             
    public enum TipoCelda { Pared, Camino, Punto, PuntoPoder, Vacio }       

    //  DELEGATES (programación orientada a eventos)
    public delegate void PuntuacionCambiadaHandler(int nuevaPuntuacion); // Delegate para notificar cambios en la puntuación; pasa el nuevo puntaje
    public delegate void VidasCambiadasHandler(int vidasRestantes);      // Delegate para notificar cambios en las vidas; pasa las vidas restantes
    public delegate void EstadoJuegoCambiadoHandler(string estado);      // Delegate para notificar cambios en el estado del juego; pasa el nuevo estado

    //  PALETA DE COLORES KAWAII
    public static class PaletaPacman
    {
        public static readonly Color FondoColor = Color.FromArgb(255, 240, 248);     // Color de fondo del panel (blanco azulado)
        public static readonly Color ParedColor = Color.FromArgb(255, 182, 213);     // Color de relleno de las paredes (rosa claro)
        public static readonly Color ParedBorde = Color.FromArgb(219, 112, 147);     // Color del borde de las paredes (rosa oscuro)
        public static readonly Color PuntoColor = Color.FromArgb(255, 215, 0);       // Color de los puntos normales (dorado)
        public static readonly Color PuntoPoderColor = Color.FromArgb(255, 105, 180); // Color de los puntos de poder (rosa fuerte)
        public static readonly Color PacColor = Color.FromArgb(255, 20, 147);        // Color del personaje Pacman (rosa intenso)
        public static readonly Color TextoColor = Color.FromArgb(180, 60, 100);      // Color del texto del HUD (rosa oscuro)
        public static readonly Color HudFondo = Color.FromArgb(255, 220, 235);       // Color de fondo del panel de información (rosa muy claro)

        public static readonly Color[] ColoresFantasmas = new[]  // Arreglo con los colores de cada fantasma
        {
            Color.FromArgb(255, 182, 193), // rosa bebé   - Rosie (fantasma 0)
            Color.FromArgb(221, 160, 221), // lila        - Lily  (fantasma 1)
            Color.FromArgb(255, 218, 185), // melocotón   - Peachy (fantasma 2)
            Color.FromArgb(176, 224, 230), // azul pastel - Bowie  (fantasma 3)
        };
        public static readonly string[] NombresFantasmas = { "Rosie", "Lily", "Peachy", "Bowie" }; // Nombres de los cuatro fantasmas en orden
        public static readonly Color ColorAsustado = Color.FromArgb(200, 200, 255); // Color de los fantasmas cuando están asustados (azul lavanda)
    }

    // Estructura que representa una posición (X, Y) en la cuadrícula del mapa
    public struct Vec2Pac
    {
        public int X, Y;                                                             
        public Vec2Pac(int x, int y) { X = x; Y = y; }                              // Constructor: asigna las coordenadas X e Y
        public static Vec2Pac operator +(Vec2Pac a, Vec2Pac b) => new(a.X + b.X, a.Y + b.Y); // Operador suma: combina dos vectores sumando sus componentes
        public static bool operator ==(Vec2Pac a, Vec2Pac b) => a.X == b.X && a.Y == b.Y;    // Operador igualdad: true si ambos vectores tienen las mismas coordenadas
        public static bool operator !=(Vec2Pac a, Vec2Pac b) => !(a == b);                   // Operador desigualdad: true si los vectores son distintos
        public override bool Equals(object? obj) => obj is Vec2Pac v && v == this;           // Sobrescribe Equals para comparar por valor usando el operador ==
        public override int GetHashCode() => HashCode.Combine(X, Y);                         // Genera un hash único basado en las coordenadas X e Y
    }

    public class MapaPacman
    {
        public const int Cols = 21;   // Número de columnas del mapa (constante)
        public const int Filas = 21;  // Número de filas del mapa (constante)
        public const int Celda = 28;  // Tamaño en píxeles de cada celda del mapa (constante)

        // Definición del layout del mapa: 0=pared, 1=camino+punto, 2=camino vacío, 3=punto poder, 4=casa fantasmas
        private static readonly int[,] _layout = new int[,]
        {
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, // Fila 0:  borde superior de paredes
            {0,1,1,1,1,1,1,1,1,1,0,1,1,1,1,1,1,1,1,1,0}, // Fila 1:  pasillo superior con puntos
            {0,3,0,0,1,0,0,0,1,0,0,0,1,0,0,0,1,0,0,3,0}, // Fila 2:  puntos de poder en esquinas y puntos centrales
            {0,1,0,0,1,0,0,0,1,0,0,0,1,0,0,0,1,0,0,1,0}, // Fila 3:  pasillos verticales con puntos
            {0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0}, // Fila 4:  pasillo horizontal completo con puntos
            {0,1,0,0,1,0,1,0,0,0,0,0,0,0,1,0,1,0,0,1,0}, // Fila 5:  pasillos con cruces internas
            {0,1,1,1,1,0,1,1,1,0,0,0,1,1,1,0,1,1,1,1,0}, // Fila 6:  pasillos con sección central vacía
            {0,0,0,0,1,0,0,0,2,0,0,0,2,0,0,0,1,0,0,0,0}, // Fila 7:  borde de zona central (sin puntos)
            {0,0,0,0,1,0,2,2,2,2,2,2,2,2,2,0,1,0,0,0,0}, // Fila 8:  entrada zona de fantasmas (vacío)
            {0,0,0,0,1,0,2,0,0,0,0,0,0,0,2,0,1,0,0,0,0}, // Fila 9:  interior zona de fantasmas
            {1,1,1,1,1,2,2,0,2,2,2,2,2,0,2,2,1,1,1,1,1}, // Fila 10: corredor central con túnel lateral
            {0,0,0,0,1,0,2,0,0,0,0,0,0,0,2,0,1,0,0,0,0}, // Fila 11: interior zona de fantasmas
            {0,0,0,0,1,0,2,2,2,2,2,2,2,2,2,0,1,0,0,0,0}, // Fila 12: salida zona de fantasmas (vacío)
            {0,0,0,0,1,0,2,0,0,0,0,0,0,0,2,0,1,0,0,0,0}, // Fila 13: más zona interior de fantasmas
            {0,1,1,1,1,1,1,1,1,0,0,0,1,1,1,1,1,1,1,1,0}, // Fila 14: pasillo inferior con puntos
            {0,1,0,0,1,0,0,0,1,0,0,0,1,0,0,0,1,0,0,1,0}, // Fila 15: pasillos verticales inferiores
            {0,3,1,0,1,1,1,1,1,1,2,1,1,1,1,1,1,0,1,3,0}, // Fila 16: puntos de poder inferiores y punto de spawn del jugador
            {0,0,1,0,1,0,1,0,0,0,0,0,0,0,1,0,1,0,1,0,0}, // Fila 17: pasillos con separaciones
            {0,1,1,1,1,0,1,1,1,0,0,0,1,1,1,0,1,1,1,1,0}, // Fila 18: pasillos con sección central
            {0,1,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,1,0}, // Fila 19: pasillos con zonas abiertas
            {0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0}, // Fila 20: borde inferior con puntos
        };

        private TipoCelda[,] _celdas = new TipoCelda[Filas, Cols]; // Matriz que almacena el tipo de celda actual del mapa (puede cambiar al consumir puntos)
        public int TotalPuntos { get; private set; }             
        public int PuntosRestantes { get; private set; }        

        public MapaPacman() => Reiniciar(); // Constructor: inicializa el mapa llamando a Reiniciar

        public void Reiniciar()
        {
            TotalPuntos = 0;     // Reinicia el contador de puntos totales
            PuntosRestantes = 0; // Reinicia el contador de puntos restantes
            for (int f = 0; f < Filas; f++)       // Recorre cada fila del mapa
                for (int c = 0; c < Cols; c++)    // Recorre cada columna de la fila actual
                {
                    _celdas[f, c] = _layout[f, c] switch // Convierte el valor numérico del layout al TipoCelda correspondiente
                    {
                        0 => TipoCelda.Pared,      // 0 es pared
                        1 => TipoCelda.Punto,      // 1 es camino con punto
                        2 => TipoCelda.Vacio,      // 2 es camino vacío
                        3 => TipoCelda.PuntoPoder, // 3 es punto de poder
                        4 => TipoCelda.Vacio,      // 4 (casa fantasmas) se trata como vacío
                        _ => TipoCelda.Vacio       // cualquier otro valor también es vacío
                    };
                    if (_celdas[f, c] == TipoCelda.Punto || _celdas[f, c] == TipoCelda.PuntoPoder) // Si la celda tiene un punto (normal o poder)
                    {
                        TotalPuntos++;     // Incrementa el total de puntos del mapa
                        PuntosRestantes++; // Incrementa los puntos restantes
                    }
                }
        }

        public TipoCelda Obtener(int f, int c)                                                           // Retorna el tipo de celda en la posición (fila, columna)
            => (f < 0 || f >= Filas || c < 0 || c >= Cols) ? TipoCelda.Pared : _celdas[f, c];           // Si está fuera del mapa, retorna Pared; si no, retorna la celda real
        public TipoCelda Obtener(Vec2Pac p) => Obtener(p.Y, p.X);                                        // Sobrecarga: obtiene el tipo de celda usando un Vec2Pac (nota: Y=fila, X=columna)
        public bool EsPared(Vec2Pac p) => Obtener(p) == TipoCelda.Pared;                                 // Retorna true si la posición es una pared

        // Consume (elimina) el punto de la celda indicada y actualiza el contador; retorna el tipo de celda que había
        public TipoCelda Consumir(Vec2Pac p)
        {
            var t = Obtener(p);                                            // Obtiene el tipo de celda actual
            if (t == TipoCelda.Punto || t == TipoCelda.PuntoPoder)        // Si hay un punto que consumir
            {
                _celdas[p.Y, p.X] = TipoCelda.Vacio;                     // Reemplaza la celda por vacío (el punto fue recogido)
                PuntosRestantes--;                                         // Reduce el contador de puntos restantes
            }
            return t; // Retorna el tipo de celda original (para saber qué puntos se ganaron)
        }

        // Dibuja todas las celdas del mapa en el contexto gráfico dado
        public void Dibujar(Graphics g)
        {
            for (int f = 0; f < Filas; f++)  
                for (int c = 0; c < Cols; c++) 
                {
                    int px = c * Celda, py = f * Celda;           // Calcula la posición en píxeles de la celda
                    var rect = new Rectangle(px, py, Celda, Celda); // Crea el rectángulo de la celda

                    switch (_celdas[f, c]) // Selecciona el dibujo según el tipo de celda
                    {
                        case TipoCelda.Pared:
                            using (var wb = new SolidBrush(PaletaPacman.ParedColor)) // Pincel rosa para el relleno de la pared
                                g.FillRectangle(wb, rect);                            // Rellena el rectángulo de la pared
                            using (var wp = new Pen(PaletaPacman.ParedBorde, 2))     // Pluma rosa oscuro para el borde
                                g.DrawRectangle(wp, px + 1, py + 1, Celda - 2, Celda - 2); // Dibuja el borde interior de la pared
                            DibujarDecoPared(g, px + Celda / 2, py + Celda / 2);    // Dibuja la decoración de cruz sobre la pared
                            break;
                        case TipoCelda.Punto:
                            DibujarEstrella(g, PaletaPacman.PuntoColor, px + Celda / 2, py + Celda / 2, 5, 3); // Dibuja una estrella dorada como punto normal
                            break;
                        case TipoCelda.PuntoPoder:
                            DibujarCorazon(g, PaletaPacman.PuntoPoderColor, px + Celda / 2, py + Celda / 2, 10); // Dibuja un corazón rosa como punto de poder
                            break;
                    }
                }
        }

        private static void DibujarDecoPared(Graphics g, int cx, int cy)
        {
            using var p = new Pen(Color.FromArgb(40, 255, 255, 255), 1); // Pluma blanca semitransparente
            g.DrawLine(p, cx - 3, cy, cx + 3, cy);   // Dibuja la línea horizontal de la cruz
            g.DrawLine(p, cx, cy - 3, cx, cy + 3);   // Dibuja la línea vertical de la cruz
        }

        public static void DibujarEstrella(Graphics g, Color c, int cx, int cy, int rExt, int rInt)
        {
            var pts = new PointF[10]; // Arreglo de 10 vértices para la estrella de 5 puntas (alternando exterior e interior)
            for (int i = 0; i < 10; i++) // Calcula cada vértice de la estrella
            {
                double ang = Math.PI / 5 * i - Math.PI / 2; // Ángulo del vértice i, comenzando desde arriba
                float r = (i % 2 == 0) ? rExt : rInt;        // Alterna entre radio exterior (puntas) e interior (valles)
                pts[i] = new PointF(cx + r * (float)Math.Cos(ang), cy + r * (float)Math.Sin(ang)); // Calcula la posición del vértice
            }
            using var b = new SolidBrush(c); // Pincel del color especificado
            g.FillPolygon(b, pts);           // Dibuja la estrella rellena
        }

        public static void DibujarCorazon(Graphics g, Color c, int cx, int cy, int size)
        {
            using var b = new SolidBrush(c);           // Pincel del color especificado
            float s = size * 0.5f;                     // Factor de escala base para el corazón
            using var path = new GraphicsPath();        // Crea un path gráfico para trazar el corazón
            path.AddBezier(cx, cy - s * 0.5f, cx - s * 2, cy - s * 2, cx - s * 2, cy + s, cx, cy + s * 1.5f); // Traza el lado izquierdo del corazón
            path.AddBezier(cx, cy + s * 1.5f, cx + s * 2, cy + s, cx + s * 2, cy - s * 2, cx, cy - s * 0.5f); // Traza el lado derecho del corazón
            g.FillPath(b, path); // Dibuja el corazón relleno
        }
    }

    // Clase abstracta base de la que heredan JugadorPacman y Fantasma
    public abstract class EntidadPacman
    {
        public Vec2Pac Posicion { get; protected set; }   // Posición actual de la entidad en la cuadrícula (solo modificable por clases hijas)
        protected DireccionPac _direccionActual;           // Dirección en la que se está moviendo actualmente la entidad

        // Convierte una dirección enum a un vector de desplazamiento en la cuadrícula
        protected static Vec2Pac DirAVec(DireccionPac d) => d switch
        {
            DireccionPac.Arriba => new Vec2Pac(0, -1),  // Arriba: disminuye Y
            DireccionPac.Abajo => new Vec2Pac(0, 1),   // Abajo: aumenta Y
            DireccionPac.Izquierda => new Vec2Pac(-1, 0),  // Izquierda: disminuye X
            DireccionPac.Derecha => new Vec2Pac(1, 0),   // Derecha: aumenta X
            _ => new Vec2Pac(0, 0)    // Ninguna: no hay desplazamiento
        };

        public abstract void Dibujar(Graphics g, float lerpX, float lerpY); // Método abstracto: cada entidad define su propio dibujo (polimorfismo)
        public virtual void Reiniciar() { }                                  // Método virtual: comportamiento por defecto vacío; puede sobreescribirse
    }

    public class JugadorPacman : EntidadPacman
    {
        public DireccionPac Encolada { get; private set; }  // Dirección que el jugador quiere tomar en el próximo movimiento válido
        public int Vidas { get; private set; }              
        public bool EstaMuerto => Vidas <= 0;                // Propiedad calculada: true si el jugador no tiene vidas

        private int _anguloBoca;    
        private int _dirBoca = 1;  
        private bool _animMuerte;  
        private int _frameMuerte;   

        // Eventos del jugador usando delegates definidos al inicio
        public event PuntuacionCambiadaHandler? AlComer; // Se dispara al comer un punto; pasa los puntos ganados
        public event VidasCambiadasHandler? AlMorir;     // Se dispara al perder una vida; pasa las vidas restantes

        // Constructor: inicializa las vidas y el estado del jugador
        public JugadorPacman()
        {
            Vidas = 3;   
            Reiniciar(); // Configura la posición y estado inicial
        }

        // Reinicia la posición y el estado del jugador para una nueva vida o nivel
        public override void Reiniciar()
        {
            Posicion = new Vec2Pac(10, 16);                // Posición inicial del jugador en la cuadrícula (columna 10, fila 16)
            _direccionActual = DireccionPac.Ninguna;       // Sin dirección de movimiento al inicio
            Encolada = DireccionPac.Ninguna;               // Sin dirección encolada al inicio
            _anguloBoca = 30;                              // Ángulo inicial de apertura de la boca
            _animMuerte = false;                           // No está en animación de muerte
            _frameMuerte = 0;                              // Reinicia el frame de la animación de muerte
        }

        public void EncolarDireccion(DireccionPac d) => Encolada = d; // Guarda la dirección que el jugador quiere tomar en cuanto sea posible

        // Actualiza el movimiento y animación del jugador en cada tick
        public void Actualizar(MapaPacman mapa)
        {
            if (_animMuerte) return; // Si está en animación de muerte, no procesa movimiento

            // Animar boca
            _anguloBoca += _dirBoca * 6;                // Incrementa o decrementa el ángulo de la boca según la dirección de animación
            if (_anguloBoca >= 45) _dirBoca = -1;       // Si la boca está muy abierta, empieza a cerrarse
            if (_anguloBoca <= 0) _dirBoca = 1;         // Si la boca está cerrada, empieza a abrirse

            // Intentar dirección encolada
            Vec2Pac sigEncolada = Posicion + DirAVec(Encolada);          // Calcula la siguiente posición si tomara la dirección encolada
            if (!mapa.EsPared(sigEncolada)) _direccionActual = Encolada; // Si no hay pared, adopta la dirección encolada como dirección actual

            Vec2Pac siguiente = Posicion + DirAVec(_direccionActual); // Calcula la siguiente posición con la dirección actual
            if (mapa.EsPared(siguiente)) return;                       // Si hay pared en la dirección actual, no se mueve

            // Túnel lateral
            if (siguiente.X < 0) siguiente = new Vec2Pac(MapaPacman.Cols - 1, siguiente.Y); // Si sale por la izquierda, aparece por la derecha
            if (siguiente.X >= MapaPacman.Cols) siguiente = new Vec2Pac(0, siguiente.Y);    // Si sale por la derecha, aparece por la izquierda

            Posicion = siguiente; // Actualiza la posición del jugador

            var celda = mapa.Consumir(Posicion);                // Consume el punto de la celda actual y obtiene su tipo
            if (celda == TipoCelda.Punto) AlComer?.Invoke(10); // Si era un punto normal, otorga 10 puntos
            if (celda == TipoCelda.PuntoPoder) AlComer?.Invoke(50); // Si era un punto de poder, otorga 50 puntos
        }

        // Activa la muerte del jugador: reduce vidas, inicia animación y dispara el evento
        public bool ActivarMuerte()
        {
            Vidas--;              // Reduce una vida
            _animMuerte = true;   // Activa la animación de muerte
            _frameMuerte = 0;     // Reinicia el contador de frames de la animación
            AlMorir?.Invoke(Vidas); // Dispara el evento de muerte pasando las vidas restantes
            return Vidas > 0;     // Retorna true si el jugador sobrevivió (aún tiene vidas)
        }

        // Avanza un frame en la animación de muerte; retorna true si la animación aún no terminó
        public bool AvanzarAnimMuerte()
        {
            _frameMuerte++;                 // Incrementa el contador de frames
            return _frameMuerte < 20;       // Retorna true si quedan frames por reproducir (animación dura 20 frames)
        }

        // Dibuja al jugador en pantalla usando interpolación de posición para animación suave
        public override void Dibujar(Graphics g, float lerpX, float lerpY)
        {
            float px = lerpX * MapaPacman.Celda + MapaPacman.Celda / 2f; // Calcula la posición X en píxeles (interpolada y centrada)
            float py = lerpY * MapaPacman.Celda + MapaPacman.Celda / 2f; // Calcula la posición Y en píxeles (interpolada y centrada)
            float r = MapaPacman.Celda / 2f - 3;                          // Radio del círculo de Pacman

            if (_animMuerte) r *= 1f - _frameMuerte / 20f; // En animación de muerte, el radio se reduce progresivamente hasta 0

            // Calcula el ángulo de inicio del arco según la dirección de movimiento
            int anguloInicio = _direccionActual switch
            {
                DireccionPac.Derecha => _anguloBoca,         // Mirando derecha: boca se abre desde el lado derecho
                DireccionPac.Izquierda => 180 + _anguloBoca,   // Mirando izquierda: boca se abre desde el lado izquierdo
                DireccionPac.Arriba => 270 + _anguloBoca,   // Mirando arriba: boca se abre desde arriba
                DireccionPac.Abajo => 90 + _anguloBoca,    // Mirando abajo: boca se abre desde abajo
                _ => _anguloBoca           // Sin dirección: posición por defecto
            };
            int anguloBarrido = 360 - _anguloBoca * 2; // Ángulo del arco del cuerpo (360 menos la apertura de la boca)

            using var b = new SolidBrush(PaletaPacman.PacColor); // Pincel rosa intenso para el cuerpo de Pacman
            g.FillPie(b, px - r, py - r, r * 2, r * 2, anguloInicio, anguloBarrido); // Dibuja el cuerpo de Pacman como un sector circular

            if (!_animMuerte) // Solo dibuja los detalles si no está en animación de muerte
            {
                float eyeOff = r * 0.4f;                                          // Desplazamiento vertical del ojo respecto al centro
                using var eb = new SolidBrush(Color.White);                       // Pincel blanco para la esclerótica del ojo
                g.FillEllipse(eb, px - 3, py - eyeOff - r * 0.3f, 6, 6);        // Dibuja la esclerótica del ojo
                using var ep = new SolidBrush(Color.Black);                       // Pincel negro para la pupila
                g.FillEllipse(ep, px - 2, py - eyeOff - r * 0.2f, 4, 4);        // Dibuja la pupila
                DibujarLazo(g, px + r * 0.2f, py - r * 0.8f, PaletaPacman.PuntoPoderColor, 5); // Dibuja el lazo decorativo sobre Pacman
            }
        }

        // Dibuja un lazo decorativo en la posición indicada con el color y tamaño especificados
        private static void DibujarLazo(Graphics g, float cx, float cy, Color c, float size)
        {
            using var b = new SolidBrush(c);                                              // Pincel del color del lazo
            g.FillEllipse(b, cx - size * 2, cy - size, size * 2, size * 2);              // Dibuja el lado izquierdo del lazo
            g.FillEllipse(b, cx, cy - size, size * 2, size * 2);                         // Dibuja el lado derecho del lazo
            using var mb = new SolidBrush(Color.FromArgb(200, 255, 255, 255));           // Pincel blanco semitransparente para el nudo central
            g.FillEllipse(mb, cx - size * 0.5f, cy - size * 0.5f, size, size);          // Dibuja el nudo central del lazo
        }
    }

    public class Fantasma : EntidadPacman
    {
        public EstadoFantasma Estado { get; private set; }  
        public Color ColorBase { get; }                      
        public string Nombre { get; }                        

        private readonly Vec2Pac _casa;           // Posición inicial/casa del fantasma a la que regresa al ser comido
        private readonly Vec2Pac _dispersion;     // Posición objetivo cuando el fantasma huye (esquina del mapa)
        private int _timerAsustado;               // Ticks restantes del estado asustado
        private readonly Random _rng = new();     // Instancia de Random para movimiento aleatorio
        private float _bobOffset;                 // Desplazamiento vertical actual del efecto de flotación
        private int _bobTimer;                    // Contador de ticks para calcular la animación de flotación

        public bool EsAleatorio { get; private set; } // Si es true, el fantasma se mueve aleatoriamente en vez de perseguir

        // Constructor: inicializa el fantasma con su posición, color, nombre y comportamiento
        public Fantasma(Vec2Pac inicio, Vec2Pac dispersion, Color color, string nombre, bool esAleatorio = false)
        {
            Posicion = inicio;           
            _casa = inicio;             
            _dispersion = dispersion;    
            ColorBase = color;           
            Nombre = nombre;             
            EsAleatorio = esAleatorio;   
            _direccionActual = DireccionPac.Izquierda; 
            Estado = EstadoFantasma.Perseguir;        
        }

        // Reinicia el fantasma a su posición y estado inicial
        public override void Reiniciar()
        {
            Posicion = _casa;                     
            Estado = EstadoFantasma.Perseguir;     
            _timerAsustado = 0;                    // Reinicia el timer de asustado
            _direccionActual = DireccionPac.Izquierda; // Vuelve a la dirección inicial
        }

        // Activa el estado asustado del fantasma si no está en estado Comido
        public void Asustar()
        {
            if (Estado != EstadoFantasma.Comido) // Solo puede asustarse si no fue comido
            {
                Estado = EstadoFantasma.Asustado; // Cambia al estado asustado
                _timerAsustado = 40;              // Durará 40 ticks en estado asustado
            }
        }

        public void SerComido() => Estado = EstadoFantasma.Comido; // Cambia el estado del fantasma a Comido

        // Actualiza el movimiento y estado del fantasma en cada tick
        public void Actualizar(MapaPacman mapa, Vec2Pac posPac)
        {
            _bobTimer++;                                             // Incrementa el contador de animación de flotación
            _bobOffset = (float)Math.Sin(_bobTimer * 0.2f) * 2f;   // Calcula el desplazamiento vertical de flotación usando seno

            if (Estado == EstadoFantasma.Asustado) // Si está asustado
            {
                _timerAsustado--;                               // Reduce el timer de asustado
                if (_timerAsustado <= 0) Estado = EstadoFantasma.Perseguir; // Si se agotó, vuelve a perseguir
            }

            if (Estado == EstadoFantasma.Comido && Posicion == _casa) // Si fue comido y llegó a su casa
                Estado = EstadoFantasma.Perseguir;                     // Revive y vuelve a perseguir

            // Determina el objetivo de movimiento según el estado actual
            Vec2Pac objetivo = Estado switch
            {
                EstadoFantasma.Perseguir => posPac,        // Perseguir: objetivo es la posición del jugador
                EstadoFantasma.Asustado => _dispersion,   // Asustado: objetivo es la esquina de dispersión
                EstadoFantasma.Comido => _casa,         // Comido: objetivo es regresar a su casa
                _ => _dispersion    // Por defecto: dispersión
            };

            MoverHacia(mapa, objetivo); // Mueve el fantasma hacia el objetivo calculado
        }

        // Calcula y ejecuta el movimiento del fantasma hacia un objetivo usando LINQ
        private void MoverHacia(MapaPacman mapa, Vec2Pac objetivo)
        {
            // LINQ funcional — obtener direcciones válidas y elegir la mejor
            var dirs = new[] { DireccionPac.Arriba, DireccionPac.Abajo, DireccionPac.Izquierda, DireccionPac.Derecha }; // Lista de todas las direcciones posibles
            var opuesta = Opuesta(_direccionActual); // Calcula la dirección opuesta a la actual (para no retroceder)

            // LINQ: filtra direcciones inválidas y selecciona candidatas
            var validas = dirs
                .Where(d => d != opuesta)                                    // Excluye la dirección opuesta (no puede retroceder)
                .Select(d => (dir: d, siguiente: Posicion + DirAVec(d)))     // Calcula la siguiente posición para cada dirección
                .Where(t => !mapa.EsPared(t.siguiente))                      // Filtra las que llevan a una pared
                .ToList();                                                    // Convierte a lista para poder iterar

            // FIX: si no hay direcciones válidas sin contar la opuesta,
            // intentar también la opuesta para no quedar pegado en pared
            if (!validas.Any()) // Si no hay ninguna dirección válida (fantasma atrapado)
            {
                var opuestaVec = Posicion + DirAVec(opuesta);  // Calcula la posición en la dirección opuesta
                if (!mapa.EsPared(opuestaVec))                  // Si la dirección opuesta no tiene pared
                    validas.Add((opuesta, opuestaVec));          // Agrega la dirección opuesta como opción de emergencia
                else
                    return; // Si está completamente rodeado de paredes, no se mueve (situación excepcional)
            }

            DireccionPac elegida; // Variable que almacenará la dirección de movimiento elegida
            if (Estado == EstadoFantasma.Asustado || EsAleatorio) // Si está asustado o es de comportamiento aleatorio
            {
                // Movimiento aleatorio — fantasmas de apoyo y estado asustado
                elegida = validas[_rng.Next(validas.Count)].dir; // Elige aleatoriamente una de las direcciones válidas
            }
            else
            {
                // LINQ funcional: elige la dirección que minimiza distancia al objetivo
                elegida = validas
                    .OrderBy(t => Math.Pow(t.siguiente.X - objetivo.X, 2) + Math.Pow(t.siguiente.Y - objetivo.Y, 2)) // Ordena por distancia euclidiana al objetivo
                    .First().dir; // Toma la dirección con menor distancia al objetivo
            }

            _direccionActual = elegida; // Actualiza la dirección actual del fantasma
            var siguiente = Posicion + DirAVec(elegida); // Calcula la nueva posición

            if (siguiente.X < 0) siguiente = new Vec2Pac(MapaPacman.Cols - 1, siguiente.Y); // Túnel: si sale por la izquierda, aparece por la derecha
            if (siguiente.X >= MapaPacman.Cols) siguiente = new Vec2Pac(0, siguiente.Y);    // Túnel: si sale por la derecha, aparece por la izquierda

            Posicion = siguiente; // Actualiza la posición del fantasma
        }

        // Dibuja el fantasma en pantalla usando interpolación de posición
        public override void Dibujar(Graphics g, float lerpX, float lerpY)
        {
            float px = lerpX * MapaPacman.Celda + MapaPacman.Celda / 2f; // Calcula la posición X en píxeles (interpolada y centrada)
            float py = lerpY * MapaPacman.Celda + MapaPacman.Celda / 2f + _bobOffset; // Calcula la posición Y en píxeles con efecto de flotación
            float r = MapaPacman.Celda / 2f - 2; // Radio del cuerpo del fantasma

            if (Estado == EstadoFantasma.Comido) // Si el fantasma fue comido, solo dibuja los ojos
            {
                DibujarOjos(g, px, py, true); // Dibuja solo los ojos (modo "comido")
                return;                        // No dibuja el resto del cuerpo
            }

            Color cuerpo = Estado == EstadoFantasma.Asustado ? PaletaPacman.ColorAsustado : ColorBase; // Color del cuerpo: azul lavanda si asustado, color base si no

            using var path = new GraphicsPath();                // Crea un path gráfico para trazar la forma del fantasma
            path.AddArc(px - r, py - r, r * 2, r * 2, 180, 180); // Dibuja la parte superior semicircular del fantasma
            float fondo = py + r;                               // Posición Y del borde inferior del fantasma
            float ondaW = r * 2 / 3f;                          // Ancho de cada onda en la parte inferior
            path.AddLine(px + r, fondo - 2, px + r, fondo);   // Línea desde el borde superior derecho hasta el fondo
            for (int i = 0; i < 3; i++) // Dibuja 3 ondas en la parte inferior del fantasma
            {
                float wx = px + r - ondaW * (i + 0.5f);        // Centro horizontal de la onda actual
                path.AddBezier(                                  // Agrega una curva bezier para cada onda
                    px + r - ondaW * i, fondo,                  // Punto de inicio de la onda
                    px + r - ondaW * i + ondaW * 0.3f, fondo + 5, // Punto de control 1 (hacia abajo)
                    wx + ondaW * 0.3f, fondo + 5,               // Punto de control 2 (hacia abajo)
                    px + r - ondaW * (i + 1), fondo);           // Punto final de la onda
            }
            path.AddLine(px - r, fondo, px - r, fondo - 2); // Línea de cierre en el lado izquierdo

            using var ghostBrush = new SolidBrush(cuerpo); // Pincel del color del cuerpo del fantasma
            g.FillPath(ghostBrush, path);                   // Rellena la forma del fantasma

            if (Estado == EstadoFantasma.Perseguir) // Si está persiguiendo, dibuja ojos y lazo
            {
                DibujarOjos(g, px, py, false);              // Dibuja los ojos normales
                DibujarLazoFantasma(g, px, py - r + 2);    // Dibuja el lazo decorativo en la cabeza
            }
            else if (Estado == EstadoFantasma.Asustado) // Si está asustado, dibuja cara de miedo
            {
                using var mp = new Pen(Color.White, 2);                    // Pluma blanca para la boca
                g.DrawLine(mp, px - 6, py + 3, px + 6, py + 3);          // Dibuja una línea recta como boca asustada
                using var ep = new SolidBrush(Color.White);               // Pincel blanco para los ojos
                g.FillEllipse(ep, px - 8, py - 5, 6, 7);                 // Dibuja el ojo izquierdo (blanco, sin pupila)
                g.FillEllipse(ep, px + 2, py - 5, 6, 7);                 // Dibuja el ojo derecho (blanco, sin pupila)
            }
        }

        // Dibuja los ojos del fantasma en la posición dada
        private static void DibujarOjos(Graphics g, float px, float py, bool soloOjos)
        {
            using var blanco = new SolidBrush(Color.White);                        // Pincel blanco para la esclerótica
            using var pupila = new SolidBrush(Color.FromArgb(50, 80, 200));       // Pincel azul para las pupilas
            g.FillEllipse(blanco, px - 9, py - 7, 8, 9);   // Dibuja la esclerótica del ojo izquierdo
            g.FillEllipse(blanco, px + 1, py - 7, 8, 9);   // Dibuja la esclerótica del ojo derecho
            g.FillEllipse(pupila, px - 7, py - 5, 5, 6);   // Dibuja la pupila del ojo izquierdo
            g.FillEllipse(pupila, px + 3, py - 5, 5, 6);   // Dibuja la pupila del ojo derecho
        }

        // Dibuja el lazo decorativo en la cabeza del fantasma
        private static void DibujarLazoFantasma(Graphics g, float cx, float ty)
        {
            Color colorLazo = Color.FromArgb(255, 20, 147);    // Color rosa oscuro para el lazo
            Color colorClaro = Color.FromArgb(255, 182, 193);  // Color rosa claro para el nudo central
            float bx = cx, by = ty - 4, s = 5f;               // Posición y tamaño del lazo
            using var bb = new SolidBrush(colorLazo);          // Pincel del color del lazo
            g.FillEllipse(bb, bx - s * 2 - 1, by - s, s * 2, s * 2); // Dibuja el lado izquierdo del lazo
            g.FillEllipse(bb, bx + 1, by - s, s * 2, s * 2);          // Dibuja el lado derecho del lazo
            using var bc = new SolidBrush(colorClaro);                 // Pincel del color claro para el nudo
            g.FillEllipse(bc, bx - s * 0.6f, by - s * 0.6f, s * 1.2f, s * 1.2f); // Dibuja el nudo central del lazo
        }

        // Retorna la dirección opuesta a la dada (para evitar que el fantasma retroceda)
        private static DireccionPac Opuesta(DireccionPac d) => d switch
        {
            DireccionPac.Arriba => DireccionPac.Abajo,      // Opuesta de Arriba es Abajo
            DireccionPac.Abajo => DireccionPac.Arriba,     // Opuesta de Abajo es Arriba
            DireccionPac.Izquierda => DireccionPac.Derecha,    // Opuesta de Izquierda es Derecha
            DireccionPac.Derecha => DireccionPac.Izquierda,  // Opuesta de Derecha es Izquierda
            _ => DireccionPac.Ninguna     // Sin dirección no tiene opuesta
        };
    }

    // Clase principal que coordina toda la lógica del juego Pacman
    public class MotorPacman
    {
        public MapaPacman Mapa { get; private set; }       
        public JugadorPacman Jugador { get; private set; }   
        public List<Fantasma> Fantasmas { get; private set; } 

        public int Puntuacion { get; private set; }        
        public int Nivel { get; private set; }           
        public string Estado { get; private set; } = "jugando"; 
        public bool Pausado { get; private set; } = false;   

        private int _comboFantasmas = 0;   // Contador de fantasmas comidos en cadena (para calcular puntos exponenciales)
        private int _tickContador = 0;     // Contador de ticks transcurridos desde el inicio
        private int _pausaMuerte = 0;      // Ticks de pausa restantes tras la muerte del jugador
        private int _tasaFantasmas = 2;    // Cada cuántos ticks se mueven los fantasmas (mayor número = más lentos)

        // Eventos del motor para notificar cambios a la UI
        public event PuntuacionCambiadaHandler? AlCambiarPuntuacion; // Se dispara cuando cambia la puntuación
        public event EstadoJuegoCambiadoHandler? AlCambiarEstado;    // Se dispara cuando cambia el estado del juego

        public MotorPacman()
        {
            Nivel = 1;                         // Comienza en el nivel 1
            Mapa = new MapaPacman();           // Crea el mapa del laberinto
            Jugador = new JugadorPacman();     // Crea el jugador
            Fantasmas = ConstruirFantasmas();  // Crea los cuatro fantasmas

            // Lambda para manejar eventos del jugador al comer un punto
            Jugador.AlComer += pts =>
            {
                Puntuacion += pts;                           // Suma los puntos ganados a la puntuación total
                AlCambiarPuntuacion?.Invoke(Puntuacion);    // Notifica a la UI del nuevo puntaje
                if (pts == 50)                              // Si comió un punto de poder (50 puntos)
                {
                    _comboFantasmas = 0;                     // Reinicia el combo de fantasmas comidos
                    Fantasmas.ForEach(f => f.Asustar());    // Asusta a todos los fantasmas
                }
            };

            // Lambda para manejar el evento de muerte del jugador
            Jugador.AlMorir += vidas =>
            {
                if (vidas <= 0) CambiarEstado("gameover"); // Si no quedan vidas, cambia a gameover
                else _pausaMuerte = 30;                     // Si quedan vidas, inicia pausa de 30 ticks antes de reiniciar
            };
        }

        // Crea y retorna la lista de los cuatro fantasmas con sus posiciones y comportamientos
        private List<Fantasma> ConstruirFantasmas() => new()
        {
            new Fantasma(new Vec2Pac(8,  8), new Vec2Pac(0,  0), PaletaPacman.ColoresFantasmas[0], PaletaPacman.NombresFantasmas[0], esAleatorio: false), // Rosie: perseguidor activo
            new Fantasma(new Vec2Pac(12, 8), new Vec2Pac(20, 0), PaletaPacman.ColoresFantasmas[1], PaletaPacman.NombresFantasmas[1], esAleatorio: true),  // Lily: movimiento aleatorio
            new Fantasma(new Vec2Pac(8, 12), new Vec2Pac(0, 20), PaletaPacman.ColoresFantasmas[2], PaletaPacman.NombresFantasmas[2], esAleatorio: true),  // Peachy: movimiento aleatorio
            new Fantasma(new Vec2Pac(12,12), new Vec2Pac(20,20), PaletaPacman.ColoresFantasmas[3], PaletaPacman.NombresFantasmas[3], esAleatorio: true),  // Bowie: movimiento aleatorio
        };

        public void EncolarDireccion(DireccionPac d) => Jugador.EncolarDireccion(d); // Delega la dirección encolada al jugador

        // Alterna el estado de pausa del juego (solo si está en estado "jugando")
        public void Pausar()
        {
            if (Estado == "jugando") Pausado = !Pausado; // Solo pausa si el juego está activo
        }

        // Procesa un tick del juego: actualiza jugador, fantasmas, colisiones y condiciones de victoria/derrota
        public void Tick()
        {
            if (Pausado) return;                                      // Si está pausado, no procesa nada
            if (Estado == "gameover" || Estado == "ganaste") return;  // Si el juego terminó, no procesa nada

            if (_pausaMuerte > 0) // Si está en pausa por muerte del jugador
            {
                _pausaMuerte--;          // Reduce el contador de pausa
                if (_pausaMuerte == 0)   // Si terminó la pausa
                {
                    Jugador.Reiniciar();                    // Reinicia la posición del jugador
                    Fantasmas.ForEach(f => f.Reiniciar()); // Reinicia la posición de todos los fantasmas
                    CambiarEstado("jugando");               // Vuelve al estado de juego activo
                }
                return; // Durante la pausa de muerte no procesa más lógica
            }

            _tickContador++;             // Incrementa el contador de ticks
            Jugador.Actualizar(Mapa);   // Actualiza el movimiento del jugador

            if (_tickContador % _tasaFantasmas == 0)          // Si corresponde mover los fantasmas este tick
                Fantasmas.ForEach(f => f.Actualizar(Mapa, Jugador.Posicion)); // Actualiza todos los fantasmas

            VerificarColisiones(); // Detecta y procesa colisiones entre jugador y fantasmas

            if (Mapa.PuntosRestantes == 0) CambiarEstado("ganaste"); // Si no quedan puntos, el jugador ganó el nivel
        }

        // Detecta y procesa las colisiones entre el jugador y los fantasmas
        private void VerificarColisiones()
        {
            // LINQ funcional: detectar fantasmas que colisionan con el jugador
            var colisionando = Fantasmas
                .Where(f => f.Posicion == Jugador.Posicion && f.Estado != EstadoFantasma.Comido) // Filtra fantasmas en la misma posición que el jugador y que no estén comidos
                .ToList(); // Convierte a lista para iterar de forma segura

            foreach (var fantasma in colisionando) // Procesa cada fantasma que colisiona
            {
                if (fantasma.Estado == EstadoFantasma.Asustado) // Si el fantasma está asustado
                {
                    fantasma.SerComido();                                          // Marca el fantasma como comido
                    _comboFantasmas++;                                             // Incrementa el combo de fantasmas
                    int pts = 200 * (int)Math.Pow(2, _comboFantasmas - 1);       // Calcula los puntos de forma exponencial (200, 400, 800...)
                    Puntuacion += pts;                                             // Suma los puntos al total
                    AlCambiarPuntuacion?.Invoke(Puntuacion);                      // Notifica el nuevo puntaje a la UI
                }
                else // Si el fantasma no está asustado (fantasma peligroso)
                {
                    // FIX: ActivarMuerte primero, luego decidir el estado correcto
                    // Evita que "muerto" sobreescriba "gameover" en el overlay
                    bool sobrevivio = Jugador.ActivarMuerte();   // Activa la muerte del jugador y verifica si sobrevivió
                    if (!sobrevivio)
                        CambiarEstado("gameover");               // Si no tiene más vidas, cambia a Game Over
                    else
                        CambiarEstado("muerto");                 // Si aún tiene vidas, cambia a estado "muerto" (pausa temporal)
                    break;                                        // Sale del bucle para no procesar más colisiones en este tick
                }
            }
        }

        // Avanza al siguiente nivel: incrementa dificultad y reinicia el mapa y entidades
        public void SiguienteNivel()
        {
            Nivel++;                                              // Incrementa el número de nivel
            _tasaFantasmas = Math.Max(1, _tasaFantasmas - 1);   // Reduce la tasa de movimiento de fantasmas (más rápidos), mínimo 1
            Mapa = new MapaPacman();                             // Crea un nuevo mapa completo con todos los puntos
            Jugador.Reiniciar();                                 // Reinicia la posición del jugador
            Fantasmas = ConstruirFantasmas();                    // Crea nuevos fantasmas desde cero
            CambiarEstado("jugando");                            // Vuelve al estado de juego activo
        }

        // Actualiza el estado del juego y notifica a los suscriptores del evento
        private void CambiarEstado(string s)
        {
            Estado = s;                      // Actualiza el estado del juego
            AlCambiarEstado?.Invoke(s);      // Notifica a la UI del nuevo estado
        }
    }

    // Clase que registra el juego Pacman como un Arcade dentro del sistema BurdiGames
    internal class JuegoPacman : Arcade
    {
        // Constructor: configura el juego con nombre, descripción, imagen y vidas
        public JuegoPacman()
            : base(                                          // Llama al constructor de la clase base Arcade
                nombre: "Pacman Rosa",                       // Nombre del juego
                descripcion: "Juego arcade PacMan Come puntos, evita fantasmas y usa corazones de poder.", // Descripción visible al usuario
                rutaImagen: "Sources/Imagenes/logo_galaxian.png", // Ruta de la imagen del juego
                vidas: 3)                                    // El juego otorga 3 vidas al jugador
        {
        }

        //polimorfismo sobre Arcade que sobreescribe Juego
        public override void Jugar() // Sobreescribe el método Jugar de la clase base Arcade (polimorfismo)
        {
            var formPacman = new FormPacman(); // Crea una instancia del formulario del juego Pacman
            formPacman.ShowDialog();           // Muestra el formulario como diálogo modal (bloquea la ventana padre)
        }
    }
}