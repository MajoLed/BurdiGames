using MiniJuegos;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace MiniJuegos
{
    public enum TipoMascota { Perro, Gato, Pato, Zorro }                                    
    public enum GeneroMascota { Nino, Nina }                                                
    public enum EstadoAnimo { Feliz, Normal, Aburrido, Dormido, Comiendo, Jugando, Escapando } 
    public enum EstadoTamagotchi { Configuracion, Jugando, Escapado, Muerto }                


    // ─── Estadísticas de la mascota ──────────────────────────────────────────
    public class EstadisticasMascota
    {
        public float Hambre { get; private set; } = 80f;     
        public float Energia { get; private set; } = 80f;    
        public float Felicidad { get; private set; } = 80f;  
        public float Edad { get; private set; } = 0f;       

        private const float TASA_HAMBRE = 1.2f;     // Velocidad a la que disminuye el hambre por segundo en estado normal
        private const float TASA_ENERGIA = 0.8f;    // Velocidad a la que disminuye la energía por segundo en estado normal
        private const float TASA_FELICIDAD = 1.0f;  // Velocidad a la que disminuye la felicidad por segundo en estado normal

        // Actualiza todas las estadísticas según el estado de ánimo actual y el tiempo transcurrido
        public void Actualizar(float delta, EstadoAnimo animo)
        {
            if (animo == EstadoAnimo.Dormido) // Si la mascota está dormida, recupera energía y baja hambre y felicidad lentamente
            {
                Energia = Math.Min(100f, Energia + 15f * delta);      // Recupera energía rápidamente mientras duerme (máximo 100)
                Hambre = Math.Max(0f, Hambre - 0.3f * delta);         // El hambre baja lentamente mientras duerme (mínimo 0)
                Felicidad = Math.Max(0f, Felicidad - 0.2f * delta);   // La felicidad baja muy lentamente mientras duerme (mínimo 0)
            }
            else // Si no está dormida, aplica las tasas normales de disminución
            {
                Hambre = Math.Max(0f, Hambre - TASA_HAMBRE * delta);        // Reduce el hambre a tasa normal (mínimo 0)
                Energia = Math.Max(0f, Energia - TASA_ENERGIA * delta);     // Reduce la energía a tasa normal (mínimo 0)
                Felicidad = Math.Max(0f, Felicidad - TASA_FELICIDAD * delta); // Reduce la felicidad a tasa normal (mínimo 0)
            }

            if (animo == EstadoAnimo.Jugando) // Si la mascota está jugando, sube la felicidad pero gasta energía y hambre
            {
                Felicidad = Math.Min(100f, Felicidad + 18f * delta); // Sube la felicidad rápidamente al jugar (máximo 100)
                Energia = Math.Max(0f, Energia - 4f * delta);        // Gasta energía adicional al jugar (mínimo 0)
                Hambre = Math.Max(0f, Hambre - 2f * delta);          // Gasta hambre adicional al jugar (mínimo 0)
            }

            if (animo == EstadoAnimo.Comiendo) // Si la mascota está comiendo, sube el hambre y un poco la felicidad
            {
                Hambre = Math.Min(100f, Hambre + 20f * delta);       // Sube el hambre rápidamente al comer (máximo 100)
                Felicidad = Math.Min(100f, Felicidad + 5f * delta);  // Sube ligeramente la felicidad al comer (máximo 100)
            }

            Edad += delta / 60f; // Incrementa la edad en minutos de juego (delta está en segundos, se convierte a minutos)
        }

        public void Comer() { Hambre = Math.Min(100f, Hambre + 35f); }  // Aumenta el hambre en 35 puntos al comer (máximo 100)
        public void Dormir() { /* la energía sube en Actualizar */ }      // Método vacío: la energía se gestiona en Actualizar al estar dormido
        public void Jugar() { Felicidad = Math.Min(100f, Felicidad + 30f); } // Aumenta la felicidad en 30 puntos al jugar (máximo 100)

        // LINQ: genera una lista de mensajes de alerta para estadísticas críticas (por debajo de 25)
        public IEnumerable<string> AlertasCriticas() =>
            new (float val, string msg)[]            // Crea un arreglo de tuplas con el valor y el mensaje de cada estadística
            {
                (Hambre,    "¡Tengo hambre!"),       // Tupla: valor de hambre y su mensaje de alerta
                (Energia,   "¡Estoy cansado!"),      // Tupla: valor de energía y su mensaje de alerta
                (Felicidad, "¡Estoy aburrido!")      // Tupla: valor de felicidad y su mensaje de alerta
            }
            .Where(t => t.val < 25f)                // Filtra solo las estadísticas que están por debajo de 25 (críticas)
            .Select(t => t.msg);                    // Proyecta solo el mensaje de alerta de cada estadística crítica

        public float PromedioEstado() =>
            new[] { Hambre, Energia, Felicidad }.Average(); // Calcula y retorna el promedio de las tres estadísticas principales
    }

    // ─── Mascota ─────────────────────────────────────────────────────────────
    public class Mascota
    {
        public string Nombre { get; }              
        public TipoMascota Tipo { get; }           
        public GeneroMascota Genero { get; }      
        public EstadisticasMascota Stats { get; } = new(); // Estadísticas de la mascota; se inicializa automáticamente
        public EstadoAnimo Animo { get; private set; } = EstadoAnimo.Normal; // Estado de ánimo actual; por defecto Normal

        private float timerAnimo = 0f;             // Acumulador de tiempo para acciones temporales (comer, jugar)
        private float timerEscape = 0f;            // Acumulador de tiempo que cuenta cuánto lleva aburrida (para el escape)
        private float animFrame = 0f;              // Acumulador de tiempo para la animación de rebote
        private static readonly Random rng = new Random(); // Instancia estática de Random para valores aleatorios

        // Posición para animación de escape
        public float EscapeX { get; private set; } = 200f;     // Posición X inicial de la mascota durante la animación de escape
        public float EscapeY { get; private set; } = 220f;     // Posición Y de la mascota durante la animación de escape
        public bool EscapeVisible { get; private set; } = true; // Indica si la mascota es visible durante el escape; por defecto true

        public event EventHandler? MascotaEscapo; // Evento que se dispara cuando la mascota termina de escaparse (salió de pantalla)

        // Constructor: inicializa la mascota con nombre, tipo y género
        public Mascota(string nombre, TipoMascota tipo, GeneroMascota genero)
        {
            Nombre = nombre; 
            Tipo = tipo;     
            Genero = genero; 
        }

        // Actualiza el estado, ánimo y lógica de escape de la mascota en cada frame
        public void Actualizar(float delta)
        {
            animFrame += delta;                    // Incrementa el acumulador de animación
            Stats.Actualizar(delta, Animo);       // Actualiza las estadísticas según el ánimo actual y el tiempo transcurrido

            // Bajar timer de acción
            if (Animo == EstadoAnimo.Comiendo || Animo == EstadoAnimo.Jugando) // Si está realizando una acción temporal
            {
                timerAnimo += delta;               // Incrementa el acumulador de la acción actual
                if (timerAnimo >= 3f)              // Si la acción duró 3 segundos
                {
                    timerAnimo = 0f;               // Reinicia el timer de acción
                    Animo = EstadoAnimo.Normal;    // Vuelve al estado Normal al terminar la acción
                }
            }

            if (Animo == EstadoAnimo.Dormido) // Si la mascota está dormida
            {
                if (Stats.Energia >= 98f)          // Si la energía está casi al máximo
                    Animo = EstadoAnimo.Normal;    // Se despierta y vuelve al estado Normal
            }

            // Determinar ánimo automático según estadísticas
            if (Animo == EstadoAnimo.Normal || Animo == EstadoAnimo.Feliz || Animo == EstadoAnimo.Aburrido)
            {
                if (Stats.Felicidad < 15f)             // Si la felicidad es muy baja (menos del 15%)
                    Animo = EstadoAnimo.Aburrido;      // La mascota se pone aburrida
                else if (Stats.PromedioEstado() > 70f) // Si el promedio de todas las estadísticas es alto (más del 70%)
                    Animo = EstadoAnimo.Feliz;         // La mascota está feliz
                else
                    Animo = EstadoAnimo.Normal;        // En cualquier otro caso, estado Normal
            }

            // Lógica de escape: si está aburrida acumula tiempo y eventualmente escapa
            if (Animo == EstadoAnimo.Aburrido)
            {
                timerEscape += delta;              // Incrementa el contador de tiempo aburrida
                if (timerEscape >= 8f)             // Si lleva 8 segundos aburrida
                {
                    Animo = EstadoAnimo.Escapando; // Inicia la animación de escape
                    timerEscape = 0f;              // Reinicia el contador de escape
                }
            }
            else if (Animo != EstadoAnimo.Escapando) // Si no está aburrida ni escapando
            {
                timerEscape = 0f; // Reinicia el contador de escape (ya no está aburrida)
            }

            if (Animo == EstadoAnimo.Escapando) // Si está en proceso de escaparse
            {
                EscapeX += 90f * delta;            // Mueve la mascota hacia la derecha a 90 píxeles por segundo
                if (EscapeX > 500f)                // Si salió completamente de la pantalla
                {
                    EscapeVisible = false;          // La mascota ya no es visible
                    MascotaEscapo?.Invoke(this, EventArgs.Empty); // Dispara el evento de escape
                }
            }
        }

        // Acción de alimentar: cambia el ánimo a Comiendo si es posible
        public void AccionComer()
        {
            if (Animo == EstadoAnimo.Dormido || Animo == EstadoAnimo.Escapando) return; // No puede comer si está dormida o escapando
            Animo = EstadoAnimo.Comiendo; // Cambia el ánimo a Comiendo
            timerAnimo = 0f;              // Reinicia el timer de la acción
            Stats.Comer();                // Aplica el efecto inmediato de comer en las estadísticas
        }

        // Acción de dormir: cambia el ánimo a Dormido si es posible
        public void AccionDormir()
        {
            if (Animo == EstadoAnimo.Escapando) return; // No puede dormir si está escapando
            Animo = EstadoAnimo.Dormido;                // Cambia el ánimo a Dormido
        }

        // Acción de jugar: cambia el ánimo a Jugando si es posible
        public void AccionJugar()
        {
            if (Animo == EstadoAnimo.Dormido || Animo == EstadoAnimo.Escapando) return; // No puede jugar si está dormida o escapando
            Animo = EstadoAnimo.Jugando; // Cambia el ánimo a Jugando
            timerAnimo = 0f;             // Reinicia el timer de la acción
            Stats.Jugar();               // Aplica el efecto inmediato de jugar en las estadísticas
        }

        public float AnimFrame => animFrame; // Propiedad pública de solo lectura que expone el acumulador de animación

        // Retorna el mensaje de estado actual de la mascota (alertas críticas o mensaje por ánimo)
        public string ObtenerMensaje()
        {
            var alertas = Stats.AlertasCriticas().ToList(); // Obtiene la lista de alertas críticas activas
            if (alertas.Any()) return alertas.First();       // Si hay alertas, retorna la primera (más urgente)
            return Animo switch // Si no hay alertas, retorna un mensaje según el ánimo actual
            {
                EstadoAnimo.Feliz => $"¡{Nombre} está muy feliz! 🌸",       // Mensaje de ánimo feliz
                EstadoAnimo.Dormido => $"{Nombre} está durmiendo... 💤",       // Mensaje de ánimo dormido
                EstadoAnimo.Comiendo => $"¡{Nombre} está comiendo! 🍎",         // Mensaje de ánimo comiendo
                EstadoAnimo.Jugando => $"¡{Nombre} está jugando! 🎮",          // Mensaje de ánimo jugando
                EstadoAnimo.Aburrido => $"{Nombre} se está aburriendo... ⚠️",   // Mensaje de ánimo aburrido
                EstadoAnimo.Escapando => $"¡{Nombre} se está escapando! 🚨",     // Mensaje de ánimo escapando
                _ => $"{Nombre} está bien~"                  // Mensaje por defecto (Normal)
            };
        }
    }

    // ─── Dibujador de mascotas pixel art GDI+ ────────────────────────────────
    public static class DibujadorMascota
    {
        // Método principal: decide qué mascota dibujar y agrega efectos según el ánimo
        public static void Dibujar(Graphics g, Mascota mascota, int cx, int cy)
        {
            float bounce = (float)Math.Sin(mascota.AnimFrame * 4f) * 4f; // Calcula el rebote vertical usando función seno (4 píxeles de amplitud)
            if (mascota.Animo == EstadoAnimo.Dormido) bounce = 0f;        // Si está dormida, no hay efecto de rebote

            bool esNina = mascota.Genero == GeneroMascota.Nina; // Determina si la mascota es de género femenino para los accesorios

            switch (mascota.Tipo) // Selecciona el método de dibujo según el tipo de mascota
            {
                case TipoMascota.Perro: DibujarPerro(g, cx, cy + (int)bounce, esNina, mascota.Animo); break; // Dibuja el perro con rebote aplicado
                case TipoMascota.Gato: DibujarGato(g, cx, cy + (int)bounce, esNina, mascota.Animo); break;  // Dibuja el gato con rebote aplicado
                case TipoMascota.Pato: DibujarPato(g, cx, cy + (int)bounce, esNina, mascota.Animo); break;  // Dibuja el pato con rebote aplicado
                case TipoMascota.Zorro: DibujarZorro(g, cx, cy + (int)bounce, esNina, mascota.Animo); break; // Dibuja el zorro con rebote aplicado
            }

            if (mascota.Animo == EstadoAnimo.Dormido) DibujarZZZ(g, cx + 45, cy - 55);        // Si está dormida, dibuja las "ZZZ" de sueño
            if (mascota.Animo == EstadoAnimo.Comiendo) DibujarComida(g, cx + 50, cy - 30);      // Si está comiendo, dibuja el ícono de comida
            if (mascota.Animo == EstadoAnimo.Jugando) DibujarNotaMusical(g, cx - 55, cy - 40); // Si está jugando, dibuja notas musicales
        }

        // ── PERRO ──
        private static void DibujarPerro(Graphics g, int cx, int cy, bool nina, EstadoAnimo animo)
        {
            Color pelaje = nina ? Color.FromArgb(255, 200, 210) : Color.FromArgb(210, 180, 140); // Color del pelaje: rosa si es niña, marrón claro si es niño
            Color oscuro = nina ? Color.FromArgb(220, 150, 170) : Color.FromArgb(160, 120, 80);  // Color oscuro del pelaje para orejas y patas

            // Cola
            var puntosCola = new PointF[]  // Define los vértices del polígono que forma la cola del perro
            {
                new(cx + 35, cy + 10),    // Base inferior izquierda de la cola
                new(cx + 55, cy - 20),    // Punta superior izquierda de la cola
                new(cx + 65, cy - 10),    // Punta superior derecha de la cola
                new(cx + 45, cy + 15)     // Base inferior derecha de la cola
            };
            using var brCola = new SolidBrush(pelaje); // Pincel del color del pelaje para la cola
            g.FillPolygon(brCola, puntosCola);          // Dibuja la cola del perro

            // Cuerpo
            using var brCuerpo = new SolidBrush(pelaje);        // Pincel del color del pelaje para el cuerpo
            g.FillEllipse(brCuerpo, cx - 40, cy - 10, 80, 55);  // Dibuja el cuerpo ovalado del perro

            // Patas
            using var brPata = new SolidBrush(oscuro);             // Pincel del color oscuro para las patas
            g.FillEllipse(brPata, cx - 35, cy + 35, 22, 16);      // Dibuja la pata delantera izquierda
            g.FillEllipse(brPata, cx - 5, cy + 35, 22, 16);      // Dibuja la pata delantera derecha
            g.FillEllipse(brPata, cx + 18, cy + 35, 22, 16);      // Dibuja la pata trasera derecha
            g.FillEllipse(brPata, cx - 18, cy + 35, 22, 16);      // Dibuja la pata trasera izquierda

            // Cabeza
            g.FillEllipse(brCuerpo, cx - 35, cy - 65, 70, 60); // Dibuja la cabeza ovalada del perro

            // Orejas caídas
            using var brOreja = new SolidBrush(oscuro);               // Pincel del color oscuro para las orejas
            g.FillEllipse(brOreja, cx - 42, cy - 68, 22, 35);         // Dibuja la oreja izquierda caída
            g.FillEllipse(brOreja, cx + 20, cy - 68, 22, 35);         // Dibuja la oreja derecha caída

            // Hocico
            using var brHocico = new SolidBrush(Color.FromArgb(255, 220, 210)); // Pincel crema para el hocico
            g.FillEllipse(brHocico, cx - 15, cy - 40, 30, 22);                  // Dibuja el hocico ovalado del perro
            using var brNariz = new SolidBrush(Color.FromArgb(60, 30, 30));     // Pincel marrón oscuro para la nariz
            g.FillEllipse(brNariz, cx - 6, cy - 42, 12, 9);                     // Dibuja la nariz del perro

            DibujarOjos(g, cx - 12, cy - 52, cx + 8, cy - 52, animo, nina); // Dibuja los ojos del perro según ánimo y género
            if (nina) DibujarLazo(g, cx + 20, cy - 72, Color.FromArgb(255, 100, 150)); // Si es niña, dibuja el lazo rosa
            DibujarBoca(g, cx, cy - 32, animo); // Dibuja la boca del perro según el ánimo
        }

        // ── GATO ──
        private static void DibujarGato(Graphics g, int cx, int cy, bool nina, EstadoAnimo animo)
        {
            Color pelaje = nina ? Color.FromArgb(255, 210, 230) : Color.FromArgb(150, 150, 170); // Color del pelaje: rosa si es niña, gris azulado si es niño
            Color oscuro = nina ? Color.FromArgb(230, 160, 190) : Color.FromArgb(100, 100, 120); // Color oscuro para patas y detalles

            // Cola larga curva
            using var penCola = new Pen(pelaje, 10f) { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round }; // Pluma gruesa con extremos redondeados para la cola
            g.DrawBezier(penCola, cx + 35, cy + 20, cx + 70, cy - 10, cx + 80, cy - 50, cx + 55, cy - 55); // Dibuja la cola curva larga usando una curva bezier

            // Cuerpo
            using var brCuerpo = new SolidBrush(pelaje);        // Pincel del color del pelaje para el cuerpo
            g.FillEllipse(brCuerpo, cx - 38, cy - 8, 76, 52);  // Dibuja el cuerpo ovalado del gato

            // Patas
            using var brPata = new SolidBrush(oscuro);             // Pincel del color oscuro para las patas
            g.FillEllipse(brPata, cx - 30, cy + 34, 20, 14);      // Dibuja la pata delantera izquierda
            g.FillEllipse(brPata, cx + 10, cy + 34, 20, 14);      // Dibuja la pata delantera derecha

            // Cabeza
            g.FillEllipse(brCuerpo, cx - 33, cy - 65, 66, 60); // Dibuja la cabeza ovalada del gato

            // Orejas puntiagudas
            var oreja1 = new PointF[] { new(cx - 28, cy - 60), new(cx - 15, cy - 88), new(cx - 5, cy - 60) }; // Vértices del triángulo de la oreja izquierda
            var oreja2 = new PointF[] { new(cx + 5, cy - 60), new(cx + 18, cy - 88), new(cx + 28, cy - 60) }; // Vértices del triángulo de la oreja derecha
            g.FillPolygon(brCuerpo, oreja1); // Dibuja la oreja izquierda puntiaguda
            g.FillPolygon(brCuerpo, oreja2); // Dibuja la oreja derecha puntiaguda
            using var brInterior = new SolidBrush(Color.FromArgb(255, 180, 200));              // Pincel rosa para el interior de las orejas
            var int1 = new PointF[] { new(cx - 24, cy - 62), new(cx - 15, cy - 82), new(cx - 7, cy - 62) };  // Vértices del interior de la oreja izquierda
            var int2 = new PointF[] { new(cx + 7, cy - 62), new(cx + 18, cy - 82), new(cx + 24, cy - 62) };  // Vértices del interior de la oreja derecha
            g.FillPolygon(brInterior, int1); // Dibuja el interior rosa de la oreja izquierda
            g.FillPolygon(brInterior, int2); // Dibuja el interior rosa de la oreja derecha

            // Bigotes
            using var penBigote = new Pen(Color.FromArgb(120, 100, 120), 1.2f);               // Pluma fina para los bigotes
            g.DrawLine(penBigote, cx - 15, cy - 38, cx - 45, cy - 33);  // Dibuja el bigote superior izquierdo
            g.DrawLine(penBigote, cx - 15, cy - 35, cx - 45, cy - 35);  // Dibuja el bigote inferior izquierdo
            g.DrawLine(penBigote, cx + 15, cy - 38, cx + 45, cy - 33);  // Dibuja el bigote superior derecho
            g.DrawLine(penBigote, cx + 15, cy - 35, cx + 45, cy - 35);  // Dibuja el bigote inferior derecho

            // Nariz
            var nariz = new PointF[] { new(cx - 5, cy - 42), new(cx + 5, cy - 42), new(cx, cy - 37) }; // Vértices del triángulo de la nariz
            using var brNariz = new SolidBrush(Color.FromArgb(255, 120, 160));                          // Pincel rosa para la nariz
            g.FillPolygon(brNariz, nariz); // Dibuja la nariz triangular del gato

            DibujarOjos(g, cx - 12, cy - 52, cx + 8, cy - 52, animo, nina); // Dibuja los ojos del gato según ánimo y género
            if (nina) DibujarLazo(g, cx + 22, cy - 78, Color.FromArgb(255, 80, 140)); // Si es niña, dibuja el lazo rosa
            DibujarBoca(g, cx, cy - 32, animo); // Dibuja la boca del gato según el ánimo
        }

        // ── PATO ──
        private static void DibujarPato(Graphics g, int cx, int cy, bool nina, EstadoAnimo animo)
        {
            Color amarillo = Color.FromArgb(255, 230, 100); // Color amarillo para el cuerpo y cabeza del pato
            Color naranja = Color.FromArgb(255, 160, 50);   // Color naranja para el pico y las patitas

            // Alas
            using var brAla = new SolidBrush(Color.FromArgb(240, 210, 80)); // Pincel amarillo oscuro para las alas
            g.FillEllipse(brAla, cx - 55, cy, 30, 40);  // Dibuja el ala izquierda
            g.FillEllipse(brAla, cx + 25, cy, 30, 40);  // Dibuja el ala derecha

            // Cuerpo redondo
            using var brCuerpo = new SolidBrush(amarillo);       // Pincel amarillo para el cuerpo
            g.FillEllipse(brCuerpo, cx - 40, cy - 15, 80, 65);  // Dibuja el cuerpo redondo del pato

            // Cabeza
            g.FillEllipse(brCuerpo, cx - 28, cy - 68, 56, 56); // Dibuja la cabeza redonda del pato

            // Pico
            var pico = new PointF[] { new(cx - 10, cy - 45), new(cx + 10, cy - 45), new(cx + 15, cy - 35), new(cx - 15, cy - 35) }; // Vértices del trapecio que forma el pico
            using var brPico = new SolidBrush(naranja); // Pincel naranja para el pico
            g.FillPolygon(brPico, pico);                 // Dibuja el pico del pato

            // Línea pico
            using var penPico = new Pen(Color.FromArgb(200, 120, 20), 1.5f);      // Pluma naranja oscuro para la línea del pico
            g.DrawLine(penPico, cx - 12, cy - 40, cx + 12, cy - 40);              // Dibuja la línea que divide el pico en dos mitades

            // Patitas
            using var brPata = new SolidBrush(naranja);            // Pincel naranja para las patitas
            g.FillEllipse(brPata, cx - 20, cy + 48, 18, 10);       // Dibuja la patita izquierda
            g.FillEllipse(brPata, cx + 2, cy + 48, 18, 10);        // Dibuja la patita derecha

            DibujarOjos(g, cx - 10, cy - 55, cx + 8, cy - 55, animo, nina); // Dibuja los ojos del pato según ánimo y género
            if (nina) DibujarLazo(g, cx + 18, cy - 75, Color.FromArgb(255, 100, 180)); // Si es niña, dibuja el lazo rosa
            DibujarBoca(g, cx, cy - 30, animo); // Dibuja la boca del pato según el ánimo
        }

        // ── ZORRO ──
        private static void DibujarZorro(Graphics g, int cx, int cy, bool nina, EstadoAnimo animo)
        {
            Color naranja = nina ? Color.FromArgb(255, 180, 140) : Color.FromArgb(230, 120, 50); // Color del pelaje: naranja claro si es niña, naranja intenso si es niño
            Color blanco = Color.FromArgb(245, 240, 235);                                         // Color blanco hueso para la pechera y hocico
            Color negro = Color.FromArgb(40, 30, 30);                                             // Color casi negro para patas y interior de orejas

            // Cola con punta blanca
            using var penCola = new Pen(naranja, 16f) { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round }; // Pluma muy gruesa con extremos redondeados para la cola
            g.DrawBezier(penCola, cx + 32, cy + 15, cx + 75, cy + 5, cx + 80, cy - 40, cx + 50, cy - 60); // Dibuja la cola curva del zorro con bezier
            using var penPunta = new Pen(blanco, 10f) { EndCap = System.Drawing.Drawing2D.LineCap.Round }; // Pluma blanca gruesa para la punta de la cola
            g.DrawBezier(penPunta, cx + 68, cy - 35, cx + 75, cy - 45, cx + 70, cy - 55, cx + 50, cy - 60); // Dibuja la punta blanca de la cola

            // Cuerpo
            using var brCuerpo = new SolidBrush(naranja);        // Pincel naranja para el cuerpo
            g.FillEllipse(brCuerpo, cx - 38, cy - 10, 76, 55);  // Dibuja el cuerpo ovalado del zorro

            // Pechera blanca
            using var brBlanco = new SolidBrush(blanco);       // Pincel blanco para la pechera
            g.FillEllipse(brBlanco, cx - 15, cy, 30, 38);      // Dibuja la pechera blanca en el pecho del zorro

            // Patas
            using var brPata = new SolidBrush(negro);               // Pincel negro para las patas (patas oscuras del zorro)
            g.FillEllipse(brPata, cx - 32, cy + 36, 18, 12);        // Dibuja la pata delantera izquierda
            g.FillEllipse(brPata, cx + 14, cy + 36, 18, 12);        // Dibuja la pata delantera derecha

            // Cabeza
            g.FillEllipse(brCuerpo, cx - 32, cy - 65, 64, 58); // Dibuja la cabeza ovalada del zorro

            // Orejas puntiagudas bicolor
            var oreja1 = new PointF[] { new(cx - 30, cy - 60), new(cx - 18, cy - 90), new(cx - 5, cy - 60) }; // Vértices del triángulo de la oreja izquierda
            var oreja2 = new PointF[] { new(cx + 5, cy - 60), new(cx + 18, cy - 90), new(cx + 30, cy - 60) }; // Vértices del triángulo de la oreja derecha
            g.FillPolygon(brCuerpo, oreja1); // Dibuja la oreja izquierda naranja
            g.FillPolygon(brCuerpo, oreja2); // Dibuja la oreja derecha naranja
            using var brInterior = new SolidBrush(negro);                                                          // Pincel negro para el interior de las orejas del zorro
            var int1 = new PointF[] { new(cx - 26, cy - 62), new(cx - 18, cy - 83), new(cx - 8, cy - 62) };      // Vértices del interior de la oreja izquierda
            var int2 = new PointF[] { new(cx + 8, cy - 62), new(cx + 18, cy - 83), new(cx + 26, cy - 62) };      // Vértices del interior de la oreja derecha
            g.FillPolygon(brInterior, int1); // Dibuja el interior negro de la oreja izquierda
            g.FillPolygon(brInterior, int2); // Dibuja el interior negro de la oreja derecha

            // Hocico
            g.FillEllipse(brBlanco, cx - 14, cy - 42, 28, 20); // Dibuja el hocico blanco ovalado del zorro
            g.FillEllipse(brPata, cx - 5, cy - 44, 10, 8);     // Dibuja la nariz oscura del zorro sobre el hocico

            // Mejillas
            using var brMejilla = new SolidBrush(Color.FromArgb(60, 255, 120, 100)); // Pincel rojo semitransparente para las mejillas
            g.FillEllipse(brMejilla, cx - 28, cy - 42, 14, 10); // Dibuja la mejilla izquierda del zorro
            g.FillEllipse(brMejilla, cx + 14, cy - 42, 14, 10); // Dibuja la mejilla derecha del zorro

            DibujarOjos(g, cx - 12, cy - 52, cx + 8, cy - 52, animo, nina); // Dibuja los ojos del zorro según ánimo y género
            if (nina) DibujarLazo(g, cx + 22, cy - 80, Color.FromArgb(255, 80, 140)); // Si es niña, dibuja el lazo rosa
            DibujarBoca(g, cx, cy - 30, animo); // Dibuja la boca del zorro según el ánimo
        }

        // ── Partes compartidas ────────────────────────────────────────────────
        private static void DibujarOjos(Graphics g, int ox, int oy, int ox2, int oy2,
                                         EstadoAnimo animo, bool nina)
        {
            if (animo == EstadoAnimo.Dormido) // Si está dormida, dibuja ojos cerrados como arcos
            {
                using var pen = new Pen(Color.FromArgb(60, 30, 60), 2.5f);          // Pluma morada oscura para los ojos cerrados
                g.DrawArc(pen, ox - 7, oy - 4, 14, 10, 0, 180);                    // Dibuja el ojo izquierdo cerrado (arco hacia abajo)
                g.DrawArc(pen, ox2 - 7, oy2 - 4, 14, 10, 0, 180);                  // Dibuja el ojo derecho cerrado (arco hacia abajo)
                return; // No dibuja más detalles si está dormida
            }

            if (animo == EstadoAnimo.Aburrido || animo == EstadoAnimo.Escapando) // Si está aburrida o escapando, dibuja ojos tristes
            {
                using var br = new SolidBrush(Color.FromArgb(60, 30, 60));         // Pincel morado oscuro para los ojos
                g.FillEllipse(br, ox - 6, oy - 5, 12, 11);                         // Dibuja el ojo izquierdo relleno
                g.FillEllipse(br, ox2 - 6, oy2 - 5, 12, 11);                       // Dibuja el ojo derecho relleno
                // Ojos tristes (línea)
                using var pen = new Pen(Color.White, 2f);                           // Pluma blanca para la expresión triste
                g.DrawLine(pen, ox - 4, oy + 1, ox + 4, oy + 1);                   // Dibuja la línea blanca del ojo izquierdo triste
                g.DrawLine(pen, ox2 - 4, oy2 + 1, ox2 + 4, oy2 + 1);              // Dibuja la línea blanca del ojo derecho triste
                return; // No dibuja más detalles para este ánimo
            }

            // Ojos normales / felices
            using var brBlanco = new SolidBrush(Color.White);                       // Pincel blanco para la esclerótica
            using var brPupila = new SolidBrush(Color.FromArgb(60, 30, 60));       // Pincel morado oscuro para las pupilas
            using var brBrillo = new SolidBrush(Color.FromArgb(180, 255, 255, 255)); // Pincel blanco semitransparente para el brillo

            g.FillEllipse(brBlanco, ox - 8, oy - 8, 16, 16);    // Dibuja la esclerótica del ojo izquierdo
            g.FillEllipse(brBlanco, ox2 - 8, oy2 - 8, 16, 16);  // Dibuja la esclerótica del ojo derecho
            g.FillEllipse(brPupila, ox - 5, oy - 4, 10, 11);    // Dibuja la pupila del ojo izquierdo
            g.FillEllipse(brPupila, ox2 - 5, oy2 - 4, 10, 11);  // Dibuja la pupila del ojo derecho
            g.FillEllipse(brBrillo, ox - 1, oy - 3, 4, 4);      // Dibuja el brillo del ojo izquierdo
            g.FillEllipse(brBrillo, ox2 - 1, oy2 - 3, 4, 4);    // Dibuja el brillo del ojo derecho

            if (nina) // Si es de género femenino, agrega pestañas
            {
                using var penPest = new Pen(Color.FromArgb(80, 30, 60), 1.5f); // Pluma morada para las pestañas
                g.DrawLine(penPest, ox - 5, oy - 8, ox - 8, oy - 13);         // Pestaña izquierda exterior del ojo izquierdo
                g.DrawLine(penPest, ox, oy - 8, ox, oy - 13);                 // Pestaña central del ojo izquierdo
                g.DrawLine(penPest, ox + 5, oy - 8, ox + 8, oy - 13);        // Pestaña derecha interior del ojo izquierdo
                g.DrawLine(penPest, ox2 - 5, oy2 - 8, ox2 - 8, oy2 - 13);    // Pestaña izquierda exterior del ojo derecho
                g.DrawLine(penPest, ox2, oy2 - 8, ox2, oy2 - 13);            // Pestaña central del ojo derecho
                g.DrawLine(penPest, ox2 + 5, oy2 - 8, ox2 + 8, oy2 - 13);   // Pestaña derecha interior del ojo derecho
            }
        }

        // Dibuja la boca de la mascota según su ánimo (sonriente, triste o recta)
        private static void DibujarBoca(Graphics g, int cx, int cy, EstadoAnimo animo)
        {
            using var pen = new Pen(Color.FromArgb(120, 60, 80), 2f); // Pluma rosa oscuro para la boca
            if (animo == EstadoAnimo.Feliz || animo == EstadoAnimo.Jugando || animo == EstadoAnimo.Comiendo)
                g.DrawArc(pen, cx - 10, cy - 5, 20, 12, 0, -180);   // Dibuja sonrisa (arco hacia arriba) si está feliz, jugando o comiendo
            else if (animo == EstadoAnimo.Aburrido || animo == EstadoAnimo.Escapando)
                g.DrawArc(pen, cx - 10, cy, 20, 10, 0, 180);         // Dibuja boca triste (arco hacia abajo) si está aburrida o escapando
            else
                g.DrawLine(pen, cx - 8, cy, cx + 8, cy);             // Dibuja boca recta (línea horizontal) en estado Normal o Dormido
        }

        // Dibuja el lazo decorativo en la cabeza de las mascotas de género femenino
        private static void DibujarLazo(Graphics g, int cx, int cy, Color color)
        {
            using var br = new SolidBrush(color);                                        // Pincel del color del lazo
            using var brClaro = new SolidBrush(Color.FromArgb(180, 255, 220, 240));    // Pincel rosa claro semitransparente para el nudo
            g.FillEllipse(br, cx - 12, cy - 6, 12, 12); // Dibuja el lado izquierdo del lazo
            g.FillEllipse(br, cx + 1, cy - 6, 12, 12);  // Dibuja el lado derecho del lazo
            g.FillEllipse(brClaro, cx - 4, cy - 4, 8, 8); // Dibuja el nudo central del lazo
        }

        // Dibuja las letras "ZZZ" animadas para indicar que la mascota está durmiendo
        private static void DibujarZZZ(Graphics g, int cx, int cy)
        {
            using var font = new Font("Consolas", 14f, FontStyle.Bold);              // Fuente Consolas negrita de 14pt para las Z
            using var brush = new SolidBrush(Color.FromArgb(180, 150, 200, 255));   // Pincel azul lavanda semitransparente
            g.DrawString("z", font, brush, cx, cy);           // Dibuja la "z" pequeña más cercana a la mascota
            g.DrawString("Z", font, brush, cx + 14, cy - 14); // Dibuja la "Z" mediana más arriba y a la derecha
            g.DrawString("Z", font, brush, cx + 28, cy - 28); // Dibuja la "Z" grande más arriba y a la derecha
        }

        // Dibuja el ícono de comida (manzana) cuando la mascota está comiendo
        private static void DibujarComida(Graphics g, int cx, int cy)
        {
            using var brComida = new SolidBrush(Color.FromArgb(255, 100, 80)); // Pincel rojo para la manzana
            g.FillEllipse(brComida, cx, cy, 22, 22);                            // Dibuja el cuerpo de la manzana
            using var brHoja = new SolidBrush(Color.FromArgb(80, 180, 80));    // Pincel verde para la hoja
            g.FillEllipse(brHoja, cx + 7, cy - 8, 10, 10);                     // Dibuja la hoja de la manzana
        }

        // Dibuja notas musicales cuando la mascota está jugando
        private static void DibujarNotaMusical(Graphics g, int cx, int cy)
        {
            using var font = new Font("Consolas", 20f, FontStyle.Bold);               // Fuente Consolas negrita de 20pt para los símbolos musicales
            using var brush = new SolidBrush(Color.FromArgb(200, 100, 220, 180));    // Pincel verde menta semitransparente
            g.DrawString("♪", font, brush, cx, cy);           // Dibuja una corchea
            g.DrawString("♫", font, brush, cx + 24, cy - 16); // Dibuja una doble corchea más arriba y a la derecha
        }
    }
}

namespace BurdiGames.Clases.Juegos
{
    // Clase que registra el juego Tamagotchi como un Arcade dentro del sistema BurdiGames
    internal class JuegoTamagotchiWrapper : Arcade
    {
        // Constructor: configura el juego con nombre, descripción, imagen y número de vidas
        public JuegoTamagotchiWrapper()
            : base(                                                                // Llama al constructor de la clase base Arcade
                nombre: "Tamagotchi",                                             // Nombre del juego
                descripcion: "Cuida tu mascota virtual. ¡Dale de comer, ponla a dormir y juega con ella", // Descripción del juego
                rutaImagen: "Sources/Imagenes/Tamagotchi/logo.png",              // Ruta de la imagen del juego
                vidas: 1)                                                          // El juego otorga 1 vida al jugador
        { }

        // Método que lanza el formulario del juego cuando el usuario decide jugar
        public override void Jugar()
        {
            var form = new FormTamagotchi(); // Crea una instancia del formulario del Tamagotchi
            form.ShowDialog();               // Muestra el formulario como diálogo modal (bloquea la ventana padre hasta cerrarlo)
        }
    }
}