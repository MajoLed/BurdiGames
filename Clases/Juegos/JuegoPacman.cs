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
    // ============================================================
    //  ENUMS
    // ============================================================
    public enum DireccionPac { Ninguna, Arriba, Abajo, Izquierda, Derecha }
    public enum EstadoFantasma { Perseguir, Asustado, Comido }
    public enum TipoCelda { Pared, Camino, Punto, PuntoPoder, Vacio }

    // ============================================================
    //  DELEGATES (programación orientada a eventos)
    // ============================================================
    public delegate void PuntuacionCambiadaHandler(int nuevaPuntuacion);
    public delegate void VidasCambiadasHandler(int vidasRestantes);
    public delegate void EstadoJuegoCambiadoHandler(string estado);

    // ============================================================
    //  PALETA DE COLORES KAWAII
    // ============================================================
    public static class PaletaPacman
    {
        public static readonly Color FondoColor = Color.FromArgb(255, 240, 248);
        public static readonly Color ParedColor = Color.FromArgb(255, 182, 213);
        public static readonly Color ParedBorde = Color.FromArgb(219, 112, 147);
        public static readonly Color PuntoColor = Color.FromArgb(255, 215, 0);
        public static readonly Color PuntoPoderColor = Color.FromArgb(255, 105, 180);
        public static readonly Color PacColor = Color.FromArgb(255, 20, 147);
        public static readonly Color TextoColor = Color.FromArgb(180, 60, 100);
        public static readonly Color HudFondo = Color.FromArgb(255, 220, 235);

        public static readonly Color[] ColoresFantasmas = new[]
        {
            Color.FromArgb(255, 182, 193), // rosa bebé   - Rosie
            Color.FromArgb(221, 160, 221), // lila        - Lily
            Color.FromArgb(255, 218, 185), // melocotón   - Peachy
            Color.FromArgb(176, 224, 230), // azul pastel - Bowie
        };
        public static readonly string[] NombresFantasmas = { "Rosie", "Lily", "Peachy", "Bowie" };
        public static readonly Color ColorAsustado = Color.FromArgb(200, 200, 255);
    }

    // ============================================================
    //  VECTOR2 — posición en la cuadrícula
    // ============================================================
    public struct Vec2Pac
    {
        public int X, Y;
        public Vec2Pac(int x, int y) { X = x; Y = y; }
        public static Vec2Pac operator +(Vec2Pac a, Vec2Pac b) => new(a.X + b.X, a.Y + b.Y);
        public static bool operator ==(Vec2Pac a, Vec2Pac b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(Vec2Pac a, Vec2Pac b) => !(a == b);
        public override bool Equals(object? obj) => obj is Vec2Pac v && v == this;
        public override int GetHashCode() => HashCode.Combine(X, Y);
    }

    // ============================================================
    //  MAPA DEL JUEGO
    // ============================================================
    public class MapaPacman
    {
        public const int Cols = 21;
        public const int Filas = 21;
        public const int Celda = 28;

        // 0=pared, 1=camino+punto, 2=camino vacío, 3=punto poder, 4=casa fantasmas
        private static readonly int[,] _layout = new int[,]
        {
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,1,1,1,1,1,1,1,1,1,0,1,1,1,1,1,1,1,1,1,0},
            {0,3,0,0,1,0,0,0,1,0,0,0,1,0,0,0,1,0,0,3,0},
            {0,1,0,0,1,0,0,0,1,0,0,0,1,0,0,0,1,0,0,1,0},
            {0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0},
            {0,1,0,0,1,0,1,0,0,0,0,0,0,0,1,0,1,0,0,1,0},
            {0,1,1,1,1,0,1,1,1,0,0,0,1,1,1,0,1,1,1,1,0},
            {0,0,0,0,1,0,0,0,2,0,0,0,2,0,0,0,1,0,0,0,0},
            {0,0,0,0,1,0,2,2,2,2,2,2,2,2,2,0,1,0,0,0,0},
            {0,0,0,0,1,0,2,0,0,0,0,0,0,0,2,0,1,0,0,0,0},
            {1,1,1,1,1,2,2,0,2,2,2,2,2,0,2,2,1,1,1,1,1},
            {0,0,0,0,1,0,2,0,0,0,0,0,0,0,2,0,1,0,0,0,0},
            {0,0,0,0,1,0,2,2,2,2,2,2,2,2,2,0,1,0,0,0,0},
            {0,0,0,0,1,0,2,0,0,0,0,0,0,0,2,0,1,0,0,0,0},
            {0,1,1,1,1,1,1,1,1,0,0,0,1,1,1,1,1,1,1,1,0},
            {0,1,0,0,1,0,0,0,1,0,0,0,1,0,0,0,1,0,0,1,0},
            {0,3,1,0,1,1,1,1,1,1,2,1,1,1,1,1,1,0,1,3,0},
            {0,0,1,0,1,0,1,0,0,0,0,0,0,0,1,0,1,0,1,0,0},
            {0,1,1,1,1,0,1,1,1,0,0,0,1,1,1,0,1,1,1,1,0},
            {0,1,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,1,0},
            {0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0},
        };

        private TipoCelda[,] _celdas = new TipoCelda[Filas, Cols];
        public int TotalPuntos { get; private set; }
        public int PuntosRestantes { get; private set; }

        public MapaPacman() => Reiniciar();

        public void Reiniciar()
        {
            TotalPuntos = 0;
            PuntosRestantes = 0;
            for (int f = 0; f < Filas; f++)
                for (int c = 0; c < Cols; c++)
                {
                    _celdas[f, c] = _layout[f, c] switch
                    {
                        0 => TipoCelda.Pared,
                        1 => TipoCelda.Punto,
                        2 => TipoCelda.Vacio,
                        3 => TipoCelda.PuntoPoder,
                        4 => TipoCelda.Vacio,
                        _ => TipoCelda.Vacio
                    };
                    if (_celdas[f, c] == TipoCelda.Punto || _celdas[f, c] == TipoCelda.PuntoPoder)
                    {
                        TotalPuntos++;
                        PuntosRestantes++;
                    }
                }
        }

        public TipoCelda Obtener(int f, int c)
            => (f < 0 || f >= Filas || c < 0 || c >= Cols) ? TipoCelda.Pared : _celdas[f, c];
        public TipoCelda Obtener(Vec2Pac p) => Obtener(p.Y, p.X);
        public bool EsPared(Vec2Pac p) => Obtener(p) == TipoCelda.Pared;

        public TipoCelda Consumir(Vec2Pac p)
        {
            var t = Obtener(p);
            if (t == TipoCelda.Punto || t == TipoCelda.PuntoPoder)
            {
                _celdas[p.Y, p.X] = TipoCelda.Vacio;
                PuntosRestantes--;
            }
            return t;
        }

        public void Dibujar(Graphics g)
        {
            for (int f = 0; f < Filas; f++)
                for (int c = 0; c < Cols; c++)
                {
                    int px = c * Celda, py = f * Celda;
                    var rect = new Rectangle(px, py, Celda, Celda);

                    switch (_celdas[f, c])
                    {
                        case TipoCelda.Pared:
                            using (var wb = new SolidBrush(PaletaPacman.ParedColor))
                                g.FillRectangle(wb, rect);
                            using (var wp = new Pen(PaletaPacman.ParedBorde, 2))
                                g.DrawRectangle(wp, px + 1, py + 1, Celda - 2, Celda - 2);
                            DibujarDecoPared(g, px + Celda / 2, py + Celda / 2);
                            break;
                        case TipoCelda.Punto:
                            DibujarEstrella(g, PaletaPacman.PuntoColor, px + Celda / 2, py + Celda / 2, 5, 3);
                            break;
                        case TipoCelda.PuntoPoder:
                            DibujarCorazon(g, PaletaPacman.PuntoPoderColor, px + Celda / 2, py + Celda / 2, 10);
                            break;
                    }
                }
        }

        private static void DibujarDecoPared(Graphics g, int cx, int cy)
        {
            using var p = new Pen(Color.FromArgb(40, 255, 255, 255), 1);
            g.DrawLine(p, cx - 3, cy, cx + 3, cy);
            g.DrawLine(p, cx, cy - 3, cx, cy + 3);
        }

        public static void DibujarEstrella(Graphics g, Color c, int cx, int cy, int rExt, int rInt)
        {
            var pts = new PointF[10];
            for (int i = 0; i < 10; i++)
            {
                double ang = Math.PI / 5 * i - Math.PI / 2;
                float r = (i % 2 == 0) ? rExt : rInt;
                pts[i] = new PointF(cx + r * (float)Math.Cos(ang), cy + r * (float)Math.Sin(ang));
            }
            using var b = new SolidBrush(c);
            g.FillPolygon(b, pts);
        }

        public static void DibujarCorazon(Graphics g, Color c, int cx, int cy, int size)
        {
            using var b = new SolidBrush(c);
            float s = size * 0.5f;
            using var path = new GraphicsPath();
            path.AddBezier(cx, cy - s * 0.5f, cx - s * 2, cy - s * 2, cx - s * 2, cy + s, cx, cy + s * 1.5f);
            path.AddBezier(cx, cy + s * 1.5f, cx + s * 2, cy + s, cx + s * 2, cy - s * 2, cx, cy - s * 0.5f);
            g.FillPath(b, path);
        }
    }

    // ============================================================
    //  ENTIDAD BASE — polimorfismo real: Pacman y Fantasma heredan de esta
    // ============================================================
    public abstract class EntidadPacman
    {
        public Vec2Pac Posicion { get; protected set; }
        protected DireccionPac _direccionActual;

        protected static Vec2Pac DirAVec(DireccionPac d) => d switch
        {
            DireccionPac.Arriba => new Vec2Pac(0, -1),
            DireccionPac.Abajo => new Vec2Pac(0, 1),
            DireccionPac.Izquierda => new Vec2Pac(-1, 0),
            DireccionPac.Derecha => new Vec2Pac(1, 0),
            _ => new Vec2Pac(0, 0)
        };

        // Método abstracto: cada entidad se dibuja diferente — polimorfismo
        public abstract void Dibujar(Graphics g, float lerpX, float lerpY);

        // Método virtual: comportamiento por defecto, puede sobreescribirse
        public virtual void Reiniciar() { }
    }

    // ============================================================
    //  JUGADOR — hereda de EntidadPacman
    // ============================================================
    public class JugadorPacman : EntidadPacman
    {
        public DireccionPac Encolada { get; private set; }
        public int Vidas { get; private set; }
        public bool EstaMuerto => Vidas <= 0;

        private int _anguloBoca;
        private int _dirBoca = 1;
        private bool _animMuerte;
        private int _frameMuerte;

        // Eventos del jugador
        public event PuntuacionCambiadaHandler? AlComer;
        public event VidasCambiadasHandler? AlMorir;

        public JugadorPacman()
        {
            Vidas = 3;
            Reiniciar();
        }

        public override void Reiniciar()
        {
            Posicion = new Vec2Pac(10, 16);
            _direccionActual = DireccionPac.Ninguna;
            Encolada = DireccionPac.Ninguna;
            _anguloBoca = 30;
            _animMuerte = false;
            _frameMuerte = 0;
        }

        public void EncolarDireccion(DireccionPac d) => Encolada = d;

        public void Actualizar(MapaPacman mapa)
        {
            if (_animMuerte) return;

            // Animar boca
            _anguloBoca += _dirBoca * 6;
            if (_anguloBoca >= 45) _dirBoca = -1;
            if (_anguloBoca <= 0) _dirBoca = 1;

            // Intentar dirección encolada
            Vec2Pac sigEncolada = Posicion + DirAVec(Encolada);
            if (!mapa.EsPared(sigEncolada)) _direccionActual = Encolada;

            Vec2Pac siguiente = Posicion + DirAVec(_direccionActual);
            if (mapa.EsPared(siguiente)) return;

            // Túnel lateral
            if (siguiente.X < 0) siguiente = new Vec2Pac(MapaPacman.Cols - 1, siguiente.Y);
            if (siguiente.X >= MapaPacman.Cols) siguiente = new Vec2Pac(0, siguiente.Y);

            Posicion = siguiente;

            var celda = mapa.Consumir(Posicion);
            if (celda == TipoCelda.Punto) AlComer?.Invoke(10);
            if (celda == TipoCelda.PuntoPoder) AlComer?.Invoke(50);
        }

        public bool ActivarMuerte()
        {
            Vidas--;
            _animMuerte = true;
            _frameMuerte = 0;
            AlMorir?.Invoke(Vidas);
            return Vidas > 0;
        }

        public bool AvanzarAnimMuerte()
        {
            _frameMuerte++;
            return _frameMuerte < 20;
        }

        public override void Dibujar(Graphics g, float lerpX, float lerpY)
        {
            float px = lerpX * MapaPacman.Celda + MapaPacman.Celda / 2f;
            float py = lerpY * MapaPacman.Celda + MapaPacman.Celda / 2f;
            float r = MapaPacman.Celda / 2f - 3;

            if (_animMuerte) r *= 1f - _frameMuerte / 20f;

            int anguloInicio = _direccionActual switch
            {
                DireccionPac.Derecha => _anguloBoca,
                DireccionPac.Izquierda => 180 + _anguloBoca,
                DireccionPac.Arriba => 270 + _anguloBoca,
                DireccionPac.Abajo => 90 + _anguloBoca,
                _ => _anguloBoca
            };
            int anguloBarrido = 360 - _anguloBoca * 2;

            using var b = new SolidBrush(PaletaPacman.PacColor);
            g.FillPie(b, px - r, py - r, r * 2, r * 2, anguloInicio, anguloBarrido);

            if (!_animMuerte)
            {
                float eyeOff = r * 0.4f;
                using var eb = new SolidBrush(Color.White);
                g.FillEllipse(eb, px - 3, py - eyeOff - r * 0.3f, 6, 6);
                using var ep = new SolidBrush(Color.Black);
                g.FillEllipse(ep, px - 2, py - eyeOff - r * 0.2f, 4, 4);
                DibujarLazo(g, px + r * 0.2f, py - r * 0.8f, PaletaPacman.PuntoPoderColor, 5);
            }
        }

        private static void DibujarLazo(Graphics g, float cx, float cy, Color c, float size)
        {
            using var b = new SolidBrush(c);
            g.FillEllipse(b, cx - size * 2, cy - size, size * 2, size * 2);
            g.FillEllipse(b, cx, cy - size, size * 2, size * 2);
            using var mb = new SolidBrush(Color.FromArgb(200, 255, 255, 255));
            g.FillEllipse(mb, cx - size * 0.5f, cy - size * 0.5f, size, size);
        }
    }

    // ============================================================
    //  FANTASMA — hereda de EntidadPacman
    // ============================================================
    public class Fantasma : EntidadPacman
    {
        public EstadoFantasma Estado { get; private set; }
        public Color ColorBase { get; }
        public string Nombre { get; }

        private readonly Vec2Pac _casa;
        private readonly Vec2Pac _dispersion;
        private int _timerAsustado;
        private readonly Random _rng = new();
        private float _bobOffset;
        private int _bobTimer;

        // Si es true, se mueve aleatorio en vez de perseguir al jugador
        public bool EsAleatorio { get; private set; }

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

        public override void Reiniciar()
        {
            Posicion = _casa;
            Estado = EstadoFantasma.Perseguir;
            _timerAsustado = 0;
            _direccionActual = DireccionPac.Izquierda;
        }

        public void Asustar()
        {
            if (Estado != EstadoFantasma.Comido)
            {
                Estado = EstadoFantasma.Asustado;
                _timerAsustado = 40;
            }
        }

        public void SerComido() => Estado = EstadoFantasma.Comido;

        public void Actualizar(MapaPacman mapa, Vec2Pac posPac)
        {
            _bobTimer++;
            _bobOffset = (float)Math.Sin(_bobTimer * 0.2f) * 2f;

            if (Estado == EstadoFantasma.Asustado)
            {
                _timerAsustado--;
                if (_timerAsustado <= 0) Estado = EstadoFantasma.Perseguir;
            }

            if (Estado == EstadoFantasma.Comido && Posicion == _casa)
                Estado = EstadoFantasma.Perseguir;

            Vec2Pac objetivo = Estado switch
            {
                EstadoFantasma.Perseguir => posPac,
                EstadoFantasma.Asustado => _dispersion,
                EstadoFantasma.Comido => _casa,
                _ => _dispersion
            };

            MoverHacia(mapa, objetivo);
        }

        private void MoverHacia(MapaPacman mapa, Vec2Pac objetivo)
        {
            // LINQ funcional — obtener direcciones válidas y elegir la mejor
            var dirs = new[] { DireccionPac.Arriba, DireccionPac.Abajo, DireccionPac.Izquierda, DireccionPac.Derecha };
            var opuesta = Opuesta(_direccionActual);

            // LINQ: filtra direcciones inválidas y selecciona candidatas
            var validas = dirs
                .Where(d => d != opuesta)
                .Select(d => (dir: d, siguiente: Posicion + DirAVec(d)))
                .Where(t => !mapa.EsPared(t.siguiente))
                .ToList();

            // FIX: si no hay direcciones válidas sin contar la opuesta,
            // intentar también la opuesta para no quedar pegado en pared
            if (!validas.Any())
            {
                var opuestaVec = Posicion + DirAVec(opuesta);
                if (!mapa.EsPared(opuestaVec))
                    validas.Add((opuesta, opuestaVec));
                else
                    return; // completamente rodeado de paredes (no debería ocurrir)
            }

            DireccionPac elegida;
            if (Estado == EstadoFantasma.Asustado || EsAleatorio)
            {
                // Movimiento aleatorio — fantasmas de apoyo y estado asustado
                elegida = validas[_rng.Next(validas.Count)].dir;
            }
            else
            {
                // LINQ funcional: elige la dirección que minimiza distancia al objetivo
                elegida = validas
                    .OrderBy(t => Math.Pow(t.siguiente.X - objetivo.X, 2) + Math.Pow(t.siguiente.Y - objetivo.Y, 2))
                    .First().dir;
            }

            _direccionActual = elegida;
            var siguiente = Posicion + DirAVec(elegida);

            if (siguiente.X < 0) siguiente = new Vec2Pac(MapaPacman.Cols - 1, siguiente.Y);
            if (siguiente.X >= MapaPacman.Cols) siguiente = new Vec2Pac(0, siguiente.Y);

            Posicion = siguiente;
        }

        public override void Dibujar(Graphics g, float lerpX, float lerpY)
        {
            float px = lerpX * MapaPacman.Celda + MapaPacman.Celda / 2f;
            float py = lerpY * MapaPacman.Celda + MapaPacman.Celda / 2f + _bobOffset;
            float r = MapaPacman.Celda / 2f - 2;

            if (Estado == EstadoFantasma.Comido)
            {
                DibujarOjos(g, px, py, true);
                return;
            }

            Color cuerpo = Estado == EstadoFantasma.Asustado ? PaletaPacman.ColorAsustado : ColorBase;

            using var path = new GraphicsPath();
            path.AddArc(px - r, py - r, r * 2, r * 2, 180, 180);
            float fondo = py + r;
            float ondaW = r * 2 / 3f;
            path.AddLine(px + r, fondo - 2, px + r, fondo);
            for (int i = 0; i < 3; i++)
            {
                float wx = px + r - ondaW * (i + 0.5f);
                path.AddBezier(
                    px + r - ondaW * i, fondo,
                    px + r - ondaW * i + ondaW * 0.3f, fondo + 5,
                    wx + ondaW * 0.3f, fondo + 5,
                    px + r - ondaW * (i + 1), fondo);
            }
            path.AddLine(px - r, fondo, px - r, fondo - 2);

            using var ghostBrush = new SolidBrush(cuerpo);
            g.FillPath(ghostBrush, path);

            if (Estado == EstadoFantasma.Perseguir)
            {
                DibujarOjos(g, px, py, false);
                DibujarLazoFantasma(g, px, py - r + 2);
            }
            else if (Estado == EstadoFantasma.Asustado)
            {
                using var mp = new Pen(Color.White, 2);
                g.DrawLine(mp, px - 6, py + 3, px + 6, py + 3);
                using var ep = new SolidBrush(Color.White);
                g.FillEllipse(ep, px - 8, py - 5, 6, 7);
                g.FillEllipse(ep, px + 2, py - 5, 6, 7);
            }
        }

        private static void DibujarOjos(Graphics g, float px, float py, bool soloOjos)
        {
            using var blanco = new SolidBrush(Color.White);
            using var pupila = new SolidBrush(Color.FromArgb(50, 80, 200));
            g.FillEllipse(blanco, px - 9, py - 7, 8, 9);
            g.FillEllipse(blanco, px + 1, py - 7, 8, 9);
            g.FillEllipse(pupila, px - 7, py - 5, 5, 6);
            g.FillEllipse(pupila, px + 3, py - 5, 5, 6);
        }

        private static void DibujarLazoFantasma(Graphics g, float cx, float ty)
        {
            Color colorLazo = Color.FromArgb(255, 20, 147);
            Color colorClaro = Color.FromArgb(255, 182, 193);
            float bx = cx, by = ty - 4, s = 5f;
            using var bb = new SolidBrush(colorLazo);
            g.FillEllipse(bb, bx - s * 2 - 1, by - s, s * 2, s * 2);
            g.FillEllipse(bb, bx + 1, by - s, s * 2, s * 2);
            using var bc = new SolidBrush(colorClaro);
            g.FillEllipse(bc, bx - s * 0.6f, by - s * 0.6f, s * 1.2f, s * 1.2f);
        }

        private static DireccionPac Opuesta(DireccionPac d) => d switch
        {
            DireccionPac.Arriba => DireccionPac.Abajo,
            DireccionPac.Abajo => DireccionPac.Arriba,
            DireccionPac.Izquierda => DireccionPac.Derecha,
            DireccionPac.Derecha => DireccionPac.Izquierda,
            _ => DireccionPac.Ninguna
        };
    }

    // ============================================================
    //  MOTOR DEL JUEGO — orquesta toda la lógica
    // ============================================================
    public class MotorPacman
    {
        public MapaPacman Mapa { get; private set; }
        public JugadorPacman Jugador { get; private set; }
        public List<Fantasma> Fantasmas { get; private set; }

        public int Puntuacion { get; private set; }
        public int Nivel { get; private set; }
        public string Estado { get; private set; } = "jugando";
        public bool Pausado { get; private set; } = false;

        private int _comboFantasmas = 0;
        private int _tickContador = 0;
        private int _pausaMuerte = 0;
        private int _tasaFantasmas = 2;

        // Eventos del motor
        public event PuntuacionCambiadaHandler? AlCambiarPuntuacion;
        public event EstadoJuegoCambiadoHandler? AlCambiarEstado;

        public MotorPacman()
        {
            Nivel = 1;
            Mapa = new MapaPacman();
            Jugador = new JugadorPacman();
            Fantasmas = ConstruirFantasmas();

            // Lambda para manejar eventos del jugador
            Jugador.AlComer += pts =>
            {
                Puntuacion += pts;
                AlCambiarPuntuacion?.Invoke(Puntuacion);
                if (pts == 50)
                {
                    _comboFantasmas = 0;
                    Fantasmas.ForEach(f => f.Asustar());
                }
            };

            Jugador.AlMorir += vidas =>
            {
                if (vidas <= 0) CambiarEstado("gameover");
                else _pausaMuerte = 30;
            };
        }

        private List<Fantasma> ConstruirFantasmas() => new()
        {
            new Fantasma(new Vec2Pac(8,  8), new Vec2Pac(0,  0), PaletaPacman.ColoresFantasmas[0], PaletaPacman.NombresFantasmas[0], esAleatorio: false),
            new Fantasma(new Vec2Pac(12, 8), new Vec2Pac(20, 0), PaletaPacman.ColoresFantasmas[1], PaletaPacman.NombresFantasmas[1], esAleatorio: true),
            new Fantasma(new Vec2Pac(8, 12), new Vec2Pac(0, 20), PaletaPacman.ColoresFantasmas[2], PaletaPacman.NombresFantasmas[2], esAleatorio: true),
            new Fantasma(new Vec2Pac(12,12), new Vec2Pac(20,20), PaletaPacman.ColoresFantasmas[3], PaletaPacman.NombresFantasmas[3], esAleatorio: true),
        };

        public void EncolarDireccion(DireccionPac d) => Jugador.EncolarDireccion(d);

        public void Pausar()
        {
            if (Estado == "jugando") Pausado = !Pausado;
        }

        public void Tick()
        {
            if (Pausado) return;
            if (Estado == "gameover" || Estado == "ganaste") return;

            if (_pausaMuerte > 0)
            {
                _pausaMuerte--;
                if (_pausaMuerte == 0)
                {
                    Jugador.Reiniciar();
                    Fantasmas.ForEach(f => f.Reiniciar());
                    CambiarEstado("jugando");  // FIX: volver a jugando tras pausa de muerte
                }
                return;
            }

            _tickContador++;
            Jugador.Actualizar(Mapa);

            if (_tickContador % _tasaFantasmas == 0)
                Fantasmas.ForEach(f => f.Actualizar(Mapa, Jugador.Posicion));

            VerificarColisiones();

            if (Mapa.PuntosRestantes == 0) CambiarEstado("ganaste");
        }

        private void VerificarColisiones()
        {
            // LINQ funcional: detectar fantasmas que colisionan con el jugador
            var colisionando = Fantasmas
                .Where(f => f.Posicion == Jugador.Posicion && f.Estado != EstadoFantasma.Comido)
                .ToList();

            foreach (var fantasma in colisionando)
            {
                if (fantasma.Estado == EstadoFantasma.Asustado)
                {
                    fantasma.SerComido();
                    _comboFantasmas++;
                    int pts = 200 * (int)Math.Pow(2, _comboFantasmas - 1);
                    Puntuacion += pts;
                    AlCambiarPuntuacion?.Invoke(Puntuacion);
                }
                else
                {
                    // FIX: ActivarMuerte primero, luego decidir el estado correcto
                    // Evita que "muerto" sobreescriba "gameover" en el overlay
                    bool sobrevivio = Jugador.ActivarMuerte();
                    if (!sobrevivio)
                        CambiarEstado("gameover");
                    else
                        CambiarEstado("muerto");
                    break;
                }
            }
        }

        public void SiguienteNivel()
        {
            Nivel++;
            _tasaFantasmas = Math.Max(1, _tasaFantasmas - 1);
            Mapa = new MapaPacman();
            Jugador.Reiniciar();
            Fantasmas = ConstruirFantasmas();
            CambiarEstado("jugando");
        }

        private void CambiarEstado(string s)
        {
            Estado = s;
            AlCambiarEstado?.Invoke(s);
        }
    }

    // ============================================================
    //  CLASE JUEGO PACMAN — conecta con el sistema del proyecto
    //  Hereda de Juego (clase abstracta del proyecto)
    // ============================================================
    internal class JuegoPacman : Arcade
    {
        public JuegoPacman()
            : base(
                nombre: "Pacman Rosa",
                descripcion: "Juego arcade PacMan Come puntos, evita fantasmas y usa corazones de poder.",
                rutaImagen: "Sources/Imagenes/logo_galaxian.png",
                vidas: 3)
        {
        }

        // Sobreescribe Jugar() — polimorfismo sobre Arcade que sobreescribe Juego
        public override void Jugar()
        {
            var formPacman = new FormPacman();
            formPacman.ShowDialog();
        }
    }
}