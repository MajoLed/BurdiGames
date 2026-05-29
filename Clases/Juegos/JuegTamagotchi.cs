using MiniJuegos;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace MiniJuegos
{
    // ─── Enums ───────────────────────────────────────────────────────────────

    public enum TipoMascota { Perro, Gato, Pato, Zorro }
    public enum GeneroMascota { Nino, Nina }
    public enum EstadoAnimo { Feliz, Normal, Aburrido, Dormido, Comiendo, Jugando, Escapando }
    public enum EstadoTamagotchi { Configuracion, Jugando, Escapado, Muerto }

    // ─── Estadísticas de la mascota ──────────────────────────────────────────

    public class EstadisticasMascota
    {
        public float Hambre { get; private set; } = 80f;  // 0=muerto de hambre, 100=lleno
        public float Energia { get; private set; } = 80f;  // 0=agotado, 100=descansado
        public float Felicidad { get; private set; } = 80f;  // 0=se escapa, 100=muy feliz
        public float Edad { get; private set; } = 0f;   // en minutos de juego

        private const float TASA_HAMBRE = 1.2f;
        private const float TASA_ENERGIA = 0.8f;
        private const float TASA_FELICIDAD = 1.0f;

        public void Actualizar(float delta, EstadoAnimo animo)
        {
            if (animo == EstadoAnimo.Dormido)
            {
                Energia = Math.Min(100f, Energia + 15f * delta);
                Hambre = Math.Max(0f, Hambre - 0.3f * delta);
                Felicidad = Math.Max(0f, Felicidad - 0.2f * delta);
            }
            else
            {
                Hambre = Math.Max(0f, Hambre - TASA_HAMBRE * delta);
                Energia = Math.Max(0f, Energia - TASA_ENERGIA * delta);
                Felicidad = Math.Max(0f, Felicidad - TASA_FELICIDAD * delta);
            }

            if (animo == EstadoAnimo.Jugando)
            {
                Felicidad = Math.Min(100f, Felicidad + 18f * delta);
                Energia = Math.Max(0f, Energia - 4f * delta);
                Hambre = Math.Max(0f, Hambre - 2f * delta);
            }

            if (animo == EstadoAnimo.Comiendo)
            {
                Hambre = Math.Min(100f, Hambre + 20f * delta);
                Felicidad = Math.Min(100f, Felicidad + 5f * delta);
            }

            Edad += delta / 60f;
        }

        public void Comer() { Hambre = Math.Min(100f, Hambre + 35f); }
        public void Dormir() { /* la energía sube en Actualizar */        }
        public void Jugar() { Felicidad = Math.Min(100f, Felicidad + 30f); }

        // LINQ: resumen de estado crítico
        public IEnumerable<string> AlertasCriticas() =>
            new (float val, string msg)[]
            {
                (Hambre,    "¡Tengo hambre!"),
                (Energia,   "¡Estoy cansado!"),
                (Felicidad, "¡Estoy aburrido!")
            }
            .Where(t => t.val < 25f)
            .Select(t => t.msg);

        public float PromedioEstado() =>
            new[] { Hambre, Energia, Felicidad }.Average();
    }

    // ─── Mascota ─────────────────────────────────────────────────────────────

    public class Mascota
    {
        public string Nombre { get; }
        public TipoMascota Tipo { get; }
        public GeneroMascota Genero { get; }
        public EstadisticasMascota Stats { get; } = new();
        public EstadoAnimo Animo { get; private set; } = EstadoAnimo.Normal;

        private float timerAnimo = 0f;
        private float timerEscape = 0f;
        private float animFrame = 0f;
        private static readonly Random rng = new Random();

        // Posición para animación de escape
        public float EscapeX { get; private set; } = 200f;
        public float EscapeY { get; private set; } = 220f;
        public bool EscapeVisible { get; private set; } = true;

        // Evento que avisa cuando se escapa
        public event EventHandler? MascotaEscapo;

        public Mascota(string nombre, TipoMascota tipo, GeneroMascota genero)
        {
            Nombre = nombre;
            Tipo = tipo;
            Genero = genero;
        }

        public void Actualizar(float delta)
        {
            animFrame += delta;
            Stats.Actualizar(delta, Animo);

            // Bajar timer de acción
            if (Animo == EstadoAnimo.Comiendo || Animo == EstadoAnimo.Jugando)
            {
                timerAnimo += delta;
                if (timerAnimo >= 3f)
                {
                    timerAnimo = 0f;
                    Animo = EstadoAnimo.Normal;
                }
            }

            if (Animo == EstadoAnimo.Dormido)
            {
                if (Stats.Energia >= 98f)
                    Animo = EstadoAnimo.Normal;
            }

            // Determinar ánimo automático
            if (Animo == EstadoAnimo.Normal || Animo == EstadoAnimo.Feliz || Animo == EstadoAnimo.Aburrido)
            {
                if (Stats.Felicidad < 15f)
                    Animo = EstadoAnimo.Aburrido;
                else if (Stats.PromedioEstado() > 70f)
                    Animo = EstadoAnimo.Feliz;
                else
                    Animo = EstadoAnimo.Normal;
            }

            // Lógica de escape
            if (Animo == EstadoAnimo.Aburrido)
            {
                timerEscape += delta;
                if (timerEscape >= 8f)
                {
                    Animo = EstadoAnimo.Escapando;
                    timerEscape = 0f;
                }
            }
            else if (Animo != EstadoAnimo.Escapando)
            {
                timerEscape = 0f;
            }

            if (Animo == EstadoAnimo.Escapando)
            {
                EscapeX += 90f * delta;
                if (EscapeX > 500f)
                {
                    EscapeVisible = false;
                    MascotaEscapo?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public void AccionComer()
        {
            if (Animo == EstadoAnimo.Dormido || Animo == EstadoAnimo.Escapando) return;
            Animo = EstadoAnimo.Comiendo;
            timerAnimo = 0f;
            Stats.Comer();
        }

        public void AccionDormir()
        {
            if (Animo == EstadoAnimo.Escapando) return;
            Animo = EstadoAnimo.Dormido;
        }

        public void AccionJugar()
        {
            if (Animo == EstadoAnimo.Dormido || Animo == EstadoAnimo.Escapando) return;
            Animo = EstadoAnimo.Jugando;
            timerAnimo = 0f;
            Stats.Jugar();
        }

        public float AnimFrame => animFrame;

        public string ObtenerMensaje()
        {
            var alertas = Stats.AlertasCriticas().ToList();
            if (alertas.Any()) return alertas.First();
            return Animo switch
            {
                EstadoAnimo.Feliz => $"¡{Nombre} está muy feliz! 🌸",
                EstadoAnimo.Dormido => $"{Nombre} está durmiendo... 💤",
                EstadoAnimo.Comiendo => $"¡{Nombre} está comiendo! 🍎",
                EstadoAnimo.Jugando => $"¡{Nombre} está jugando! 🎮",
                EstadoAnimo.Aburrido => $"{Nombre} se está aburriendo... ⚠️",
                EstadoAnimo.Escapando => $"¡{Nombre} se está escapando! 🚨",
                _ => $"{Nombre} está bien~"
            };
        }
    }

    // ─── Dibujador de mascotas pixel art GDI+ ────────────────────────────────

    public static class DibujadorMascota
    {
        public static void Dibujar(Graphics g, Mascota mascota, int cx, int cy)
        {
            float bounce = (float)Math.Sin(mascota.AnimFrame * 4f) * 4f;
            if (mascota.Animo == EstadoAnimo.Dormido) bounce = 0f;

            bool esNina = mascota.Genero == GeneroMascota.Nina;

            switch (mascota.Tipo)
            {
                case TipoMascota.Perro: DibujarPerro(g, cx, cy + (int)bounce, esNina, mascota.Animo); break;
                case TipoMascota.Gato: DibujarGato(g, cx, cy + (int)bounce, esNina, mascota.Animo); break;
                case TipoMascota.Pato: DibujarPato(g, cx, cy + (int)bounce, esNina, mascota.Animo); break;
                case TipoMascota.Zorro: DibujarZorro(g, cx, cy + (int)bounce, esNina, mascota.Animo); break;
            }

            if (mascota.Animo == EstadoAnimo.Dormido)
                DibujarZZZ(g, cx + 45, cy - 55);
            if (mascota.Animo == EstadoAnimo.Comiendo)
                DibujarComida(g, cx + 50, cy - 30);
            if (mascota.Animo == EstadoAnimo.Jugando)
                DibujarNotaMusical(g, cx - 55, cy - 40);
        }

        // ── PERRO ──
        private static void DibujarPerro(Graphics g, int cx, int cy, bool nina, EstadoAnimo animo)
        {
            Color pelaje = nina ? Color.FromArgb(255, 200, 210) : Color.FromArgb(210, 180, 140);
            Color oscuro = nina ? Color.FromArgb(220, 150, 170) : Color.FromArgb(160, 120, 80);

            // Cola
            var puntosCola = new PointF[]
            {
                new(cx + 35, cy + 10),
                new(cx + 55, cy - 20),
                new(cx + 65, cy - 10),
                new(cx + 45, cy + 15)
            };
            using var brCola = new SolidBrush(pelaje);
            g.FillPolygon(brCola, puntosCola);

            // Cuerpo
            using var brCuerpo = new SolidBrush(pelaje);
            g.FillEllipse(brCuerpo, cx - 40, cy - 10, 80, 55);

            // Patas
            using var brPata = new SolidBrush(oscuro);
            g.FillEllipse(brPata, cx - 35, cy + 35, 22, 16);
            g.FillEllipse(brPata, cx - 5, cy + 35, 22, 16);
            g.FillEllipse(brPata, cx + 18, cy + 35, 22, 16);
            g.FillEllipse(brPata, cx - 18, cy + 35, 22, 16);

            // Cabeza
            g.FillEllipse(brCuerpo, cx - 35, cy - 65, 70, 60);

            // Orejas caídas
            using var brOreja = new SolidBrush(oscuro);
            g.FillEllipse(brOreja, cx - 42, cy - 68, 22, 35);
            g.FillEllipse(brOreja, cx + 20, cy - 68, 22, 35);

            // Hocico
            using var brHocico = new SolidBrush(Color.FromArgb(255, 220, 210));
            g.FillEllipse(brHocico, cx - 15, cy - 40, 30, 22);
            using var brNariz = new SolidBrush(Color.FromArgb(60, 30, 30));
            g.FillEllipse(brNariz, cx - 6, cy - 42, 12, 9);

            DibujarOjos(g, cx - 12, cy - 52, cx + 8, cy - 52, animo, nina);
            if (nina) DibujarLazo(g, cx + 20, cy - 72, Color.FromArgb(255, 100, 150));
            DibujarBoca(g, cx, cy - 32, animo);
        }

        // ── GATO ──
        private static void DibujarGato(Graphics g, int cx, int cy, bool nina, EstadoAnimo animo)
        {
            Color pelaje = nina ? Color.FromArgb(255, 210, 230) : Color.FromArgb(150, 150, 170);
            Color oscuro = nina ? Color.FromArgb(230, 160, 190) : Color.FromArgb(100, 100, 120);

            // Cola larga curva
            using var penCola = new Pen(pelaje, 10f) { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round };
            g.DrawBezier(penCola, cx + 35, cy + 20, cx + 70, cy - 10, cx + 80, cy - 50, cx + 55, cy - 55);

            // Cuerpo
            using var brCuerpo = new SolidBrush(pelaje);
            g.FillEllipse(brCuerpo, cx - 38, cy - 8, 76, 52);

            // Patas
            using var brPata = new SolidBrush(oscuro);
            g.FillEllipse(brPata, cx - 30, cy + 34, 20, 14);
            g.FillEllipse(brPata, cx + 10, cy + 34, 20, 14);

            // Cabeza
            g.FillEllipse(brCuerpo, cx - 33, cy - 65, 66, 60);

            // Orejas puntiagudas
            var oreja1 = new PointF[] { new(cx - 28, cy - 60), new(cx - 15, cy - 88), new(cx - 5, cy - 60) };
            var oreja2 = new PointF[] { new(cx + 5, cy - 60), new(cx + 18, cy - 88), new(cx + 28, cy - 60) };
            g.FillPolygon(brCuerpo, oreja1);
            g.FillPolygon(brCuerpo, oreja2);
            using var brInterior = new SolidBrush(Color.FromArgb(255, 180, 200));
            var int1 = new PointF[] { new(cx - 24, cy - 62), new(cx - 15, cy - 82), new(cx - 7, cy - 62) };
            var int2 = new PointF[] { new(cx + 7, cy - 62), new(cx + 18, cy - 82), new(cx + 24, cy - 62) };
            g.FillPolygon(brInterior, int1);
            g.FillPolygon(brInterior, int2);

            // Bigotes
            using var penBigote = new Pen(Color.FromArgb(120, 100, 120), 1.2f);
            g.DrawLine(penBigote, cx - 15, cy - 38, cx - 45, cy - 33);
            g.DrawLine(penBigote, cx - 15, cy - 35, cx - 45, cy - 35);
            g.DrawLine(penBigote, cx + 15, cy - 38, cx + 45, cy - 33);
            g.DrawLine(penBigote, cx + 15, cy - 35, cx + 45, cy - 35);

            // Nariz
            var nariz = new PointF[] { new(cx - 5, cy - 42), new(cx + 5, cy - 42), new(cx, cy - 37) };
            using var brNariz = new SolidBrush(Color.FromArgb(255, 120, 160));
            g.FillPolygon(brNariz, nariz);

            DibujarOjos(g, cx - 12, cy - 52, cx + 8, cy - 52, animo, nina);
            if (nina) DibujarLazo(g, cx + 22, cy - 78, Color.FromArgb(255, 80, 140));
            DibujarBoca(g, cx, cy - 32, animo);
        }

        // ── PATO ──
        private static void DibujarPato(Graphics g, int cx, int cy, bool nina, EstadoAnimo animo)
        {
            Color amarillo = Color.FromArgb(255, 230, 100);
            Color naranja = Color.FromArgb(255, 160, 50);

            // Alas
            using var brAla = new SolidBrush(Color.FromArgb(240, 210, 80));
            g.FillEllipse(brAla, cx - 55, cy, 30, 40);
            g.FillEllipse(brAla, cx + 25, cy, 30, 40);

            // Cuerpo redondo
            using var brCuerpo = new SolidBrush(amarillo);
            g.FillEllipse(brCuerpo, cx - 40, cy - 15, 80, 65);

            // Cabeza
            g.FillEllipse(brCuerpo, cx - 28, cy - 68, 56, 56);

            // Pico
            var pico = new PointF[] { new(cx - 10, cy - 45), new(cx + 10, cy - 45), new(cx + 15, cy - 35), new(cx - 15, cy - 35) };
            using var brPico = new SolidBrush(naranja);
            g.FillPolygon(brPico, pico);

            // Línea pico
            using var penPico = new Pen(Color.FromArgb(200, 120, 20), 1.5f);
            g.DrawLine(penPico, cx - 12, cy - 40, cx + 12, cy - 40);

            // Patitas
            using var brPata = new SolidBrush(naranja);
            g.FillEllipse(brPata, cx - 20, cy + 48, 18, 10);
            g.FillEllipse(brPata, cx + 2, cy + 48, 18, 10);

            DibujarOjos(g, cx - 10, cy - 55, cx + 8, cy - 55, animo, nina);
            if (nina) DibujarLazo(g, cx + 18, cy - 75, Color.FromArgb(255, 100, 180));
            DibujarBoca(g, cx, cy - 30, animo);
        }

        // ── ZORRO ──
        private static void DibujarZorro(Graphics g, int cx, int cy, bool nina, EstadoAnimo animo)
        {
            Color naranja = nina ? Color.FromArgb(255, 180, 140) : Color.FromArgb(230, 120, 50);
            Color blanco = Color.FromArgb(245, 240, 235);
            Color negro = Color.FromArgb(40, 30, 30);

            // Cola con punta blanca
            using var penCola = new Pen(naranja, 16f) { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round };
            g.DrawBezier(penCola, cx + 32, cy + 15, cx + 75, cy + 5, cx + 80, cy - 40, cx + 50, cy - 60);
            using var penPunta = new Pen(blanco, 10f) { EndCap = System.Drawing.Drawing2D.LineCap.Round };
            g.DrawBezier(penPunta, cx + 68, cy - 35, cx + 75, cy - 45, cx + 70, cy - 55, cx + 50, cy - 60);

            // Cuerpo
            using var brCuerpo = new SolidBrush(naranja);
            g.FillEllipse(brCuerpo, cx - 38, cy - 10, 76, 55);

            // Pechera blanca
            using var brBlanco = new SolidBrush(blanco);
            g.FillEllipse(brBlanco, cx - 15, cy, 30, 38);

            // Patas
            using var brPata = new SolidBrush(negro);
            g.FillEllipse(brPata, cx - 32, cy + 36, 18, 12);
            g.FillEllipse(brPata, cx + 14, cy + 36, 18, 12);

            // Cabeza
            g.FillEllipse(brCuerpo, cx - 32, cy - 65, 64, 58);

            // Orejas puntiagudas bicolor
            var oreja1 = new PointF[] { new(cx - 30, cy - 60), new(cx - 18, cy - 90), new(cx - 5, cy - 60) };
            var oreja2 = new PointF[] { new(cx + 5, cy - 60), new(cx + 18, cy - 90), new(cx + 30, cy - 60) };
            g.FillPolygon(brCuerpo, oreja1);
            g.FillPolygon(brCuerpo, oreja2);
            using var brInterior = new SolidBrush(negro);
            var int1 = new PointF[] { new(cx - 26, cy - 62), new(cx - 18, cy - 83), new(cx - 8, cy - 62) };
            var int2 = new PointF[] { new(cx + 8, cy - 62), new(cx + 18, cy - 83), new(cx + 26, cy - 62) };
            g.FillPolygon(brInterior, int1);
            g.FillPolygon(brInterior, int2);

            // Hocico
            g.FillEllipse(brBlanco, cx - 14, cy - 42, 28, 20);
            g.FillEllipse(brPata, cx - 5, cy - 44, 10, 8);

            // Mejillas
            using var brMejilla = new SolidBrush(Color.FromArgb(60, 255, 120, 100));
            g.FillEllipse(brMejilla, cx - 28, cy - 42, 14, 10);
            g.FillEllipse(brMejilla, cx + 14, cy - 42, 14, 10);

            DibujarOjos(g, cx - 12, cy - 52, cx + 8, cy - 52, animo, nina);
            if (nina) DibujarLazo(g, cx + 22, cy - 80, Color.FromArgb(255, 80, 140));
            DibujarBoca(g, cx, cy - 30, animo);
        }

        // ── Partes compartidas ────────────────────────────────────────────────

        private static void DibujarOjos(Graphics g, int ox, int oy, int ox2, int oy2,
                                         EstadoAnimo animo, bool nina)
        {
            if (animo == EstadoAnimo.Dormido)
            {
                using var pen = new Pen(Color.FromArgb(60, 30, 60), 2.5f);
                g.DrawArc(pen, ox - 7, oy - 4, 14, 10, 0, 180);
                g.DrawArc(pen, ox2 - 7, oy2 - 4, 14, 10, 0, 180);
                return;
            }

            if (animo == EstadoAnimo.Aburrido || animo == EstadoAnimo.Escapando)
            {
                using var br = new SolidBrush(Color.FromArgb(60, 30, 60));
                g.FillEllipse(br, ox - 6, oy - 5, 12, 11);
                g.FillEllipse(br, ox2 - 6, oy2 - 5, 12, 11);
                // Ojos tristes (línea)
                using var pen = new Pen(Color.White, 2f);
                g.DrawLine(pen, ox - 4, oy + 1, ox + 4, oy + 1);
                g.DrawLine(pen, ox2 - 4, oy2 + 1, ox2 + 4, oy2 + 1);
                return;
            }

            // Ojos normales / felices
            using var brBlanco = new SolidBrush(Color.White);
            using var brPupila = new SolidBrush(Color.FromArgb(60, 30, 60));
            using var brBrillo = new SolidBrush(Color.FromArgb(180, 255, 255, 255));

            g.FillEllipse(brBlanco, ox - 8, oy - 8, 16, 16);
            g.FillEllipse(brBlanco, ox2 - 8, oy2 - 8, 16, 16);
            g.FillEllipse(brPupila, ox - 5, oy - 4, 10, 11);
            g.FillEllipse(brPupila, ox2 - 5, oy2 - 4, 10, 11);
            g.FillEllipse(brBrillo, ox - 1, oy - 3, 4, 4);
            g.FillEllipse(brBrillo, ox2 - 1, oy2 - 3, 4, 4);

            if (nina)
            {
                using var penPest = new Pen(Color.FromArgb(80, 30, 60), 1.5f);
                g.DrawLine(penPest, ox - 5, oy - 8, ox - 8, oy - 13);
                g.DrawLine(penPest, ox, oy - 8, ox, oy - 13);
                g.DrawLine(penPest, ox + 5, oy - 8, ox + 8, oy - 13);
                g.DrawLine(penPest, ox2 - 5, oy2 - 8, ox2 - 8, oy2 - 13);
                g.DrawLine(penPest, ox2, oy2 - 8, ox2, oy2 - 13);
                g.DrawLine(penPest, ox2 + 5, oy2 - 8, ox2 + 8, oy2 - 13);
            }
        }

        private static void DibujarBoca(Graphics g, int cx, int cy, EstadoAnimo animo)
        {
            using var pen = new Pen(Color.FromArgb(120, 60, 80), 2f);
            if (animo == EstadoAnimo.Feliz || animo == EstadoAnimo.Jugando || animo == EstadoAnimo.Comiendo)
                g.DrawArc(pen, cx - 10, cy - 5, 20, 12, 0, -180);
            else if (animo == EstadoAnimo.Aburrido || animo == EstadoAnimo.Escapando)
                g.DrawArc(pen, cx - 10, cy, 20, 10, 0, 180);
            else
                g.DrawLine(pen, cx - 8, cy, cx + 8, cy);
        }

        private static void DibujarLazo(Graphics g, int cx, int cy, Color color)
        {
            using var br = new SolidBrush(color);
            using var brClaro = new SolidBrush(Color.FromArgb(180, 255, 220, 240));
            g.FillEllipse(br, cx - 12, cy - 6, 12, 12);
            g.FillEllipse(br, cx + 1, cy - 6, 12, 12);
            g.FillEllipse(brClaro, cx - 4, cy - 4, 8, 8);
        }

        private static void DibujarZZZ(Graphics g, int cx, int cy)
        {
            using var font = new Font("Consolas", 14f, FontStyle.Bold);
            using var brush = new SolidBrush(Color.FromArgb(180, 150, 200, 255));
            g.DrawString("z", font, brush, cx, cy);
            g.DrawString("Z", font, brush, cx + 14, cy - 14);
            g.DrawString("Z", font, brush, cx + 28, cy - 28);
        }

        private static void DibujarComida(Graphics g, int cx, int cy)
        {
            using var brComida = new SolidBrush(Color.FromArgb(255, 100, 80));
            g.FillEllipse(brComida, cx, cy, 22, 22);
            using var brHoja = new SolidBrush(Color.FromArgb(80, 180, 80));
            g.FillEllipse(brHoja, cx + 7, cy - 8, 10, 10);
        }

        private static void DibujarNotaMusical(Graphics g, int cx, int cy)
        {
            using var font = new Font("Consolas", 20f, FontStyle.Bold);
            using var brush = new SolidBrush(Color.FromArgb(200, 100, 220, 180));
            g.DrawString("♪", font, brush, cx, cy);
            g.DrawString("♫", font, brush, cx + 24, cy - 16);
        }
    }
}
namespace BurdiGames.Clases.Juegos
{
    internal class JuegoTamagotchiWrapper : Arcade
    {
        public JuegoTamagotchiWrapper()
            : base(
                nombre: "Tamagotchi",
                descripcion: "Cuida tu mascota virtual. ¡Dale de comer, ponla a dormir y juega con ella!",
                rutaImagen: "Sources/Imagenes/Tamagotchi/logo.png",
                vidas: 1)
        { }

        public override void Jugar()
        {
            var form = new FormTamagotchi();
            form.ShowDialog();
        }
    }
}