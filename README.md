# BurdiGames

## Plataforma de Minijuegos en C#

**Programación Orientada a Objetos · Funcional · Orientada a Eventos**

### Proyecto Final — Fundamentos de Programación

**2025**

**Integrantes:** Maria José Ledesma Cordoba, Laura Camila Heredia Uribe, Valentina Velez Cano, Samuel Botero Gallo

---

# 1. Descripción del Proyecto

BurdiGames es una plataforma de entretenimiento digital desarrollada en C# con Windows Forms, que reúne cuatro minijuegos distintos bajo una misma interfaz estilo arcade. El proyecto nació como ejercicio académico con un objetivo claro: demostrar en la práctica los tres paradigmas de programación vistos en clase — orientado a objetos, funcional y orientado a eventos — sin que ninguno de ellos quede como adorno.

La plataforma cuenta con inicio de sesión, un catálogo visual de juegos con tarjetas interactivas y acceso al perfil con historial de partidas. Cada juego es completamente autónomo, pero todos comparten la misma jerarquía de clases, lo que hace que agregar un juego nuevo sea tan simple como crear una clase que herede de `Juego`.

### Los cuatro minijuegos incluidos

- **Pac-Girl Rosa** — versión kawaii del clásico Pac-Man, con fantasmas de colores pastel y power-ups en forma de corazón.
- **Cohete Espacial** — shooter vertical donde se esquivan meteoritos rosas y se eliminan aliens maquillados que lanzan corazones verdes.
- **Blossom Mines** — Buscaminas con estética girlie: celdas rosas, banderas moradas y overlay de victoria floral.
- **Tamagotchi** — mascota virtual con pantalla de configuración inicial (nombre, tipo y género), estadísticas de hambre, energía y felicidad, y un sistema de escape si se abandona.

---

# 2. Diagrama de Clases Principal

A continuación se presenta el mapa de todas las clases del proyecto, con su tipo, de quién heredan y cuál es su rol dentro del sistema.

| Clase / Interfaz | Tipo | Hereda de | Responsabilidad |
|------------------|-------|-----------|----------------|
| Juego | `abstract class` | — | Clase base de todos los juegos. Define Nombre, Género, Descripción, Imagen y el método abstracto `Jugar()`. |
| Arcade | `class` | Juego | Agrega Vidas y HighScore. Implementa `Jugar()` con un mensaje por consola; es la base de todos los minijuegos. |
| JuegoPacman | `class` | Arcade | Configura el catálogo y abre `FormPacman` al llamar `Jugar()`. |
| JuegoCoheteArcade | `class` | Arcade | Registra el cohete en el catálogo y abre `FormCohete`. |
| JuegoBuscaminasArcade | `class` | Arcade | Registra el buscaminas en el catálogo y abre `FormBuscaminas`. |
| JuegoTamagotchiWrapper | `class` | Arcade | Registra el Tamagotchi en el catálogo y abre `FormTamagotchi`. |
| Usuario | `class` | — | Representa a un jugador registrado. Encapsula contraseña, avatar, historial de partidas y conteo estático. |
| Partida | `class` | IGuardable | Registra una sesión de juego: quién jugó, a qué, cuándo y con qué puntaje. |
| Logro | `class` | — | Almacena un logro del jugador (nombre, ícono, estado desbloqueado). |
| PlataformaJuegos | `class` | — | Motor central: catálogo de juegos y lista de usuarios. Maneja registro y autenticación con LINQ. |
| IGuardable | `interface` | — | Contrato que obliga a implementar `Guardar()` y `Cargar()`. |
| IRepositorioUsuario | `interface` | — | Contrato para agregar y buscar usuarios. |
| ManejadorSesion | `static class` | — | Gestiona la sesión activa: quién está conectado y métodos de inicio/cierre. |
| EntidadJuego | `abstract class` | — | Base de todas las entidades del cohete: posición, tamaño, hitbox y métodos abstractos. |
| Cohete | `class` | EntidadJuego | Jugador del cohete. |
| Meteorito | `class` | EntidadJuego | Obstáculo rosa poligonal. |
| Alien | `class` | EntidadJuego | Enemigo maquillado que dispara corazones verdes mediante eventos. |
| Corazon | `class` | EntidadJuego | Proyectil del alien. |
| Bala | `class` | EntidadJuego | Proyectil del jugador. |
| EstadoCohete | `class` | — | Gestiona estado del juego, puntos y nivel. |
| JuegoCohete | `class` | — | Motor del juego: spawn, colisiones y reinicio. |
| EntidadPacman | `abstract class` | — | Base de Pacman y Fantasma. |
| JugadorPacman | `class` | EntidadPacman | Control del Pac-Man. |
| Fantasma | `class` | EntidadPacman | IA del fantasma usando LINQ. |
| MapaPacman | `class` | — | Tablero del laberinto. |
| MotorPacman | `class` | — | Actualiza jugador, fantasmas y colisiones. |
| Celda | `class` | — | Celda del buscaminas. |
| Tablero | `class` | — | Matriz de celdas. |
| MotorBuscaminas | `class` | — | Gestiona clics, banderas y victoria. |
| Mascota | `class` | — | Estado vivo del Tamagotchi. |
| EstadisticasMascota | `class` | — | Hambre, energía y felicidad. |
| DibujadorMascota | `static class` | — | Renderiza pixel-art GDI+. |
| FormLogin | `Form` | Form | Inicio de sesión y registro. |
| FormMain | `Form` | Form | Catálogo visual de juegos. |
| FormPerfilUsuario | `Form` | Form | Estadísticas y logros. |
| FormPacman | `Form` | Form | Interfaz del Pac-Man. |
| FormCohete | `Form` | Form | Interfaz del cohete. |
| FormBuscaminas | `Form` | Form | Interfaz del buscaminas. |
| FormTamagotchi | `Form` | Form | Interfaz del Tamagotchi. |
| FormConfigTamagotchi | `Form` | Form | Configuración inicial de la mascota. |
| PanelSuave* | `class` | Panel | Panel con doble buffer activado. |

---

# 3. Paradigmas Aplicados: POO y Funcional

El proyecto no usa estos paradigmas como decoración. Cada uno está donde tiene sentido y puede justificarse técnicamente.

## 3.1 Programación Orientada a Objetos

### Herencia con propósito

```text
Juego → Arcade → JuegoPacman / JuegoCoheteArcade / ...
```

Cada nivel de la jerarquía agrega algo concreto: `Juego` define qué es un juego, `Arcade` agrega vidas y puntaje, y cada subclase concreta configura su propio juego.

### Polimorfismo real

```csharp
FormMain llama juego.Jugar();
```

El catálogo guarda objetos de tipo `Juego`, pero al llamar `Jugar()` cada uno abre su propio formulario.

### Encapsulamiento

`Usuario` protege `_contrasena` mediante un campo privado y propiedades controladas.

### Abstracción

`EntidadJuego`, `EntidadPacman` y `Juego` son clases abstractas que obligan a implementar comportamientos específicos.

### Interfaces como contratos

`IGuardable` e `IRepositorioUsuario` establecen compromisos que las clases deben cumplir.

### Relaciones entre objetos

- `Partida` conoce a `Usuario` y a `Juego` (asociación).
- `PlataformaJuegos` contiene listas de ambos (composición).

---

## 3.2 Programación Funcional con LINQ

### PlataformaJuegos — Autenticación y unicidad

```csharp
return Usuarios.FirstOrDefault(
    u => u.Nombre == nickname &&
         u.Contrasenia == contra);

if (Usuarios.Any(u => u.Nombre == nickname))
    return false;
```

### JuegoCohete — Detección de colisiones

```csharp
var impactos =
(
    from bala in Balas
    from met in Meteoritos
    where bala.ColisionaCon(met)
    select (bala, met)
).ToList();
```

### Tablero (Buscaminas)

```csharp
var posiciones =
(
    from f in Enumerable.Range(0, Filas)
    from c in Enumerable.Range(0, Columnas)
    where !(f == filaSegura && c == colSegura)
    select (f, c)
)
.OrderBy(_ => rng.Next())
.Take(TotalMinas)
.ToList();
```

### Fantasma (Pac-Man)

```csharp
elegida = validas
    .OrderBy(t =>
        Math.Pow(t.siguiente.X - objetivo.X, 2) +
        Math.Pow(t.siguiente.Y - objetivo.Y, 2))
    .First()
    .dir;
```

### EstadisticasMascota

```csharp
public IEnumerable<string> AlertasCriticas() =>
    new (float val, string msg)[]
    {
        (Hambre, "¡Tengo hambre!"),
        (Energia, "¡Estoy cansado!")
    }
    .Where(t => t.val < 25f)
    .Select(t => t.msg);
```

---

## 3.3 Programación Orientada a Eventos

### Eventos de dominio

- `Jugador.AlComer`
- `Jugador.AlMorir`
- `Motor.AlCambiarPuntuacion`
- `Alien.DisparoCorazon`
- `Mascota.MascotaEscapo`

### Eventos de UI

- `Timer.Tick`
- `Paint`
- `KeyDown` / `KeyUp`
- `MouseClick`
- `Click`

---

# 4. Instrucciones de Ejecución

## Requisitos previos

- Visual Studio 2022 o superior
- .NET 8.0 SDK (Windows)
- Windows 10 o Windows 11

## Paso 1 — Abrir el proyecto

Abrir:

```text
BurdiGames.sln
```

## Paso 2 — Verificar referencias

El proyecto utiliza:

- `System.Windows.Forms`
- `System.Drawing`

No requiere paquetes NuGet adicionales.

## Paso 3 — Compilar y ejecutar

Presionar:

```text
F5
```

o el botón:

```text
▶ Iniciar
```

### Usuarios de prueba

```text
Usuario: PlayerOne
Contraseña: 1234
```

```text
Usuario: Majo
Contraseña: abcd
```

## Controles de los juegos

### Cohete

- Flechas o WASD
- Espacio para disparar
- ESC para pausar

### Pac-Man

- Flechas direccionales

### Buscaminas

- Clic izquierdo: revelar
- Clic derecho: bandera

### Tamagotchi

- Comer
- Dormir
- Jugar

---

# 5. Reflexión Final

## ¿Qué fue fácil?

Arrancar con la jerarquía base fue bastante fluido. Una vez que `Juego` y `Arcade` quedaron bien definidos, agregar cada minijuego nuevo fue cuestión de heredar y sobreescribir `Jugar()`.

El polimorfismo permitió que `FormMain` soportara múltiples juegos sin modificaciones.

Los eventos también resultaron naturales una vez comprendido el patrón de desacoplamiento entre lógica y presentación.

## ¿Qué fue difícil?

### Flickering

El parpadeo de los juegos fue uno de los retos técnicos más importantes. La solución consistió en crear paneles personalizados con doble buffer:

- `PanelSuave`
- `PanelSuaveBuscaminas`
- `PanelSuaveTamagotchi`

### Integración de minijuegos

Fue necesario crear clases wrapper como:

- `JuegoCoheteArcade`
- `JuegoBuscaminasArcade`
- `JuegoTamagotchiWrapper`

para integrarlos correctamente a la jerarquía principal.

### Problemas de encoding

Se presentaron inconvenientes menores derivados de diferencias entre archivos UTF-8 y UTF-16 generados por Visual Studio.

## ¿Qué aprendieron?

La principal lección fue comprobar el valor del diseño orientado a objetos. Sin la jerarquía:

```text
Juego → Arcade → Subclase
```

el catálogo habría requerido grandes bloques `if` o `switch`.

También se evidenció que LINQ permite expresar intenciones de forma más clara y legible que múltiples bucles anidados.

Finalmente, la programación orientada a eventos demostró su utilidad al desacoplar motores de juego e interfaces gráficas, facilitando el mantenimiento y evolución del sistema.
