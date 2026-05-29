/*
using BurdiGames.Clases.Juegos;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace BurdiGames.GUI
{
    public class FormTamagotchi : Form
    {
        // ── Mascota actual ────────────────────────
        private Mascota _mascota;

        // ── Timers ────────────────────────────────
        private Timer _timerJuego;        // baja necesidades con el tiempo
        private Timer _timerAnimacion;    // cicla los frames de la animación
        private Timer _timerVolverNormal; // después de X segundos vuelve al estado normal

        // ── Estado animación ──────────────────────
        private int _frameActual = 1;
        private int _totalFrames = 2;

        // ── Controles principales ─────────────────
        private PictureBox _picFondo;
        private PictureBox _picMascota;

        private Panel _panelBarras;
        private Panel _panelBotonesAccion;
        private Button _btnVolver;
        private Label _lblNombreMascota;
        private Label _lblEstado;

        // Indicadores de necesidades (círculos de color)
        private Panel _circHambre;
        private Panel _circHigiene;
        private Panel _circDiversion;
        private Panel _circSueno;

        private Label _lblHambre;
        private Label _lblHigiene;
        private Label _lblDiversion;
        private Label _lblSueno;

        // ── Pantalla de selección ─────────────────
        private Panel _panelSeleccion;
        private Panel _panelJuego;

        public FormTamagotchi()
        {
            this.Text = "Tamagotchi — BurdiGames";
            this.Size = new Size(700, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(15, 15, 30);

            InicializarTimers();
            MostrarPantallaSeleccion();
        }

        // ────────────────────────────────────────────
        //  PANTALLA 1: SELECCIÓN DE MASCOTA
        // ────────────────────────────────────────────

        private void MostrarPantallaSeleccion()
        {
            this.Controls.Clear();

            _panelSeleccion = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(15, 15, 30)
            };

            var lblTitulo = new Label
            {
                Text = "¿Con quién quieres jugar?",
                ForeColor = Color.White,
                Font = new Font("Consolas", 16f, FontStyle.Bold),
                AutoSize = true,
                Left = 0,
                Top = 40
            };
            lblTitulo.Left = (this.ClientSize.Width - 400) / 2;

            var lblSubtitulo = new Label
            {
                Text = "Elige tu mascota",
                ForeColor = Color.Gray,
                Font = new Font("Consolas", 9f),
                AutoSize = true,
                Top = 80
            };

            // Nombre de la mascota
            var lblNombre = new Label
            {
                Text = "Nombre de tu mascota:",
                ForeColor = Color.LightGray,
                Font = new Font("Consolas", 9f),
                AutoSize = true,
                Left = 60,
                Top = 140
            };

            var txtNombre = new TextBox
            {
                Left = 60,
                Top = 165,
                Width = 200,
                BackColor = Color.FromArgb(30, 30, 50),
                ForeColor = Color.White,
                Font = new Font("Consolas", 10f),
                BorderStyle = BorderStyle.FixedSingle,
                Text = "Mi mascota"
            };

            // Corralito — 4 opciones de mascota
            var especies = new[] { ("Gato", "🐱"), ("Perro", "🐶"), ("Pato", "🐥"), ("Zorro", "🦊") };
            int seleccionIndex = 0;
            var botonesEspecie = new List<Panel>();

            var lblElige = new Label
            {
                Text = "Especie:",
                ForeColor = Color.LightGray,
                Font = new Font("Consolas", 9f),
                AutoSize = true,
                Left = 60,
                Top = 220
            };

            int xInicio = 60;
            for (int i = 0; i < especies.Length; i++)
            {
                int idx = i;
                var (nombre, emoji) = especies[i];

                // PictureBox del animal — aquí pones tu imagen
                // Por ahora es un panel con emoji; reemplaza con PictureBox + imagen real
                var picAnimal = new Panel
                {
                    Left = xInicio + i * 140,
                    Top = 250,
                    Width = 100,
                    Height = 100,
                    BackColor = Color.FromArgb(25, 25, 50),
                    Tag = nombre
                };

                // REEMPLAZAR: este Label con emoji es el placeholder de la imagen
                // Cuando tengas las imágenes, cambia esto por un PictureBox con tu .png
                var lblEmoji = new Label
                {
                    Text = emoji,
                    Font = new Font("Segoe UI Emoji", 28f),
                    AutoSize = false,
                    Width = 100,
                    Height = 70,
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent,
                    Top = 5,
                    Left = 0
                };

                var lblNombreEspecie = new Label
                {
                    Text = nombre,
                    ForeColor = idx == 0 ? Color.FromArgb(0, 200, 100) : Color.Gray,
                    Font = new Font("Consolas", 8f, idx == 0 ? FontStyle.Bold : FontStyle.Regular),
                    AutoSize = false,
                    Width = 100,
                    Height = 20,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Top = 75,
                    Left = 0,
                    Tag = "lbl_" + nombre
                };

                picAnimal.Controls.Add(lblEmoji);
                picAnimal.Controls.Add(lblNombreEspecie);

                picAnimal.Click += (s, e) =>
                {
                    seleccionIndex = idx;
                    // Resetear todos los colores
                    foreach (var btn in botonesEspecie)
                    {
                        btn.BackColor = Color.FromArgb(25, 25, 50);
                        foreach (Control c in btn.Controls)
                            if (c is Label l) l.ForeColor = Color.Gray;
                    }
                    // Marcar el seleccionado
                    picAnimal.BackColor = Color.FromArgb(0, 50, 30);
                    lblNombreEspecie.ForeColor = Color.FromArgb(0, 200, 100);
                };

                lblEmoji.Click += (s, e) => picAnimal.PerformLayout();
                lblNombreEspecie.Click += (s, e) => picAnimal.PerformLayout();

                botonesEspecie.Add(picAnimal);
                _panelSeleccion.Controls.Add(picAnimal);
            }

            // Seleccionar el primero por defecto
            if (botonesEspecie.Count > 0)
                botonesEspecie[0].BackColor = Color.FromArgb(0, 50, 30);

            // Botón Empezar
            var btnEmpezar = new Button
            {
                Text = "▶  Empezar",
                Left = 60,
                Top = 400,
                Width = 140,
                Height = 38,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 200, 100),
                ForeColor = Color.Black,
                Font = new Font("Consolas", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnEmpezar.FlatAppearance.BorderSize = 0;
            btnEmpezar.Click += (s, e) =>
            {
                string nombre = string.IsNullOrWhiteSpace(txtNombre.Text) ? "Mi mascota" : txtNombre.Text.Trim();
                _mascota = CrearMascota(especies[seleccionIndex].Item1, nombre);
                IniciarJuego();
            };

            lblSubtitulo.Left = (this.ClientSize.Width - lblSubtitulo.PreferredWidth) / 2;

            _panelSeleccion.Controls.AddRange(new Control[]
            {
                lblTitulo, lblSubtitulo, lblNombre, txtNombre, lblElige, btnEmpezar
            });

            this.Controls.Add(_panelSeleccion);
        }

        private Mascota CrearMascota(string especie, string nombre)
        {
            return especie switch
            {
                "Gato" => new Gato(nombre),
                "Perro" => new Perro(nombre),
                "Pato" => new Pato(nombre),
                "Zorro" => new Zorro(nombre),
                _ => new Gato(nombre)
            };
        }

        // ────────────────────────────────────────────
        //  PANTALLA 2: JUEGO PRINCIPAL
        // ────────────────────────────────────────────

        private void IniciarJuego()
        {
            this.Controls.Clear();

            _panelJuego = new Panel { Dock = DockStyle.Fill };
            this.Controls.Add(_panelJuego);

            ConstruirInterfazJuego();
            ActualizarUI();

            _timerJuego.Start();
        }

        private void ConstruirInterfazJuego()
        {
            // ── Fondo (imagen de fondo, cambia según acción) ──────────
            // REEMPLAZAR: cuando tengas las imágenes de fondo, este PictureBox
            // las mostrará automáticamente. Por ahora muestra un color sólido.
            _picFondo = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.FromArgb(20, 30, 50)  // placeholder — reemplazar con imagen
            };
            _panelJuego.Controls.Add(_picFondo);

            // ── Imagen de la mascota (centro) ─────────────────────────
            // REEMPLAZAR: este PictureBox es donde va la imagen real de tu mascota.
            // Tendrá tamaño 200x200 centrado. El código ya carga el archivo correcto
            // según la acción y el frame. Solo sube tus imágenes a la ruta indicada.
            _picMascota = new PictureBox
            {
                Width = 200,
                Height = 200,
                Left = (this.ClientSize.Width - 200) / 2,
                Top = 150,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };
            _panelJuego.Controls.Add(_picMascota);
            _picMascota.BringToFront();

            // ── Nombre y estado ───────────────────────────────────────
            _lblNombreMascota = new Label
            {
                Text = _mascota.Nombre,
                ForeColor = Color.White,
                Font = new Font("Consolas", 13f, FontStyle.Bold),
                AutoSize = true,
                Top = 110,
                BackColor = Color.Transparent
            };
            _panelJuego.Controls.Add(_lblNombreMascota);
            _lblNombreMascota.Left = (this.ClientSize.Width - _lblNombreMascota.PreferredWidth) / 2;
            _lblNombreMascota.BringToFront();

            _lblEstado = new Label
            {
                Text = "😊 Saludable",
                ForeColor = Color.FromArgb(0, 200, 100),
                Font = new Font("Consolas", 8f),
                AutoSize = true,
                Top = 135,
                BackColor = Color.Transparent
            };
            _panelJuego.Controls.Add(_lblEstado);
            _lblEstado.Left = (this.ClientSize.Width - 120) / 2;
            _lblEstado.BringToFront();

            // ── Panel de barras (necesidades) — lado derecho ──────────
            _panelBarras = new Panel
            {
                Width = 120,
                Height = 300,
                Left = this.ClientSize.Width - 140,
                Top = 80,
                BackColor = Color.Transparent
            };
            _panelJuego.Controls.Add(_panelBarras);
            _panelBarras.BringToFront();

            ConstruirIndicadorNecesidad("Hambre", 0, ref _circHambre, ref _lblHambre);
            ConstruirIndicadorNecesidad("Higiene", 70, ref _circHigiene, ref _lblHigiene);
            ConstruirIndicadorNecesidad("Diversión", 140, ref _circDiversion, ref _lblDiversion);
            ConstruirIndicadorNecesidad("Sueño", 210, ref _circSueno, ref _lblSueno);

            // ── Botones de acción — parte inferior ────────────────────
            _panelBotonesAccion = new Panel
            {
                Height = 70,
                Dock = DockStyle.Bottom,
                BackColor = Color.FromArgb(10, 10, 25)
            };
            _panelJuego.Controls.Add(_panelBotonesAccion);
            _panelBotonesAccion.BringToFront();

            var acciones = new[]
            {
                ("🍖 Alimentar", (Action)(() => EjecutarAccion(() => _mascota.Alimentar(), 3000))),
                ("🛁 Bañar",     (Action)(() => EjecutarAccion(() => _mascota.Bañar(),     3000))),
                ("🎾 Jugar",     (Action)(() => EjecutarAccion(() => _mascota.Jugar(),      3000))),
                ("😴 Dormir",    (Action)(() => EjecutarAccion(() => _mascota.Dormir(),     4000)))
            };

            int xBtn = 20;
            foreach (var (texto, accion) in acciones)
            {
                var accionLocal = accion;
                var btn = new Button
                {
                    Text = texto,
                    Left = xBtn,
                    Top = 15,
                    Width = 130,
                    Height = 40,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(30, 30, 55),
                    ForeColor = Color.White,
                    Font = new Font("Consolas", 9f),
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderColor = Color.FromArgb(0, 200, 100);
                btn.FlatAppearance.BorderSize = 1;
                btn.Click += (s, e) => accionLocal();
                _panelBotonesAccion.Controls.Add(btn);
                xBtn += 145;
            }

            // ── Botón volver al menú ──────────────────────────────────
            _btnVolver = new Button
            {
                Text = "← Menú",
                Left = 10,
                Top = 10,
                Width = 80,
                Height = 28,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.Gray,
                Font = new Font("Consolas", 8f),
                Cursor = Cursors.Hand
            };
            _btnVolver.FlatAppearance.BorderSize = 0;
            _btnVolver.Click += (s, e) =>
            {
                _timerJuego.Stop();
                _timerAnimacion.Stop();
                _timerVolverNormal.Stop();
                this.Close();
            };
            _panelJuego.Controls.Add(_btnVolver);
            _btnVolver.BringToFront();
        }

        private void ConstruirIndicadorNecesidad(string nombre, int top,
            ref Panel circulo, ref Label etiqueta)
        {
            var lbl = new Label
            {
                Text = nombre,
                ForeColor = Color.LightGray,
                Font = new Font("Consolas", 7f),
                AutoSize = true,
                Left = 0,
                Top = top
            };

            // El círculo de color — Panel redondo con esquinas redondeadas
            // REEMPLAZAR: si prefieres otra representación visual (barra, ícono, etc.)
            // solo cambia este Panel por otro control
            var circ = new Panel
            {
                Width = 30,
                Height = 30,
                Left = 0,
                Top = top + 18,
                BackColor = Color.FromArgb(0, 200, 100)
            };

            // Hacer el panel circular con región
            circ.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using var brush = new SolidBrush(circ.BackColor);
                e.Graphics.FillEllipse(brush, 0, 0, circ.Width - 1, circ.Height - 1);
            };
            circ.BackColor = Color.Transparent; // necesario para que se vea solo el paint

            var lblPct = new Label
            {
                Text = "70%",
                ForeColor = Color.White,
                Font = new Font("Consolas", 6f),
                AutoSize = false,
                Width = 80,
                Height = 14,
                Left = 35,
                Top = top + 26,
                BackColor = Color.Transparent
            };

            circulo = circ;
            etiqueta = lblPct;

            _panelBarras.Controls.AddRange(new Control[] { lbl, circ, lblPct });
        }

        // ────────────────────────────────────────────
        //  LÓGICA DE JUEGO
        // ────────────────────────────────────────────

        private void EjecutarAccion(Action accionMascota, int duracionMs)
        {
            if (_mascota == null || !_mascota.EstaViva) return;

            _timerVolverNormal.Stop();
            _timerAnimacion.Stop();
            _frameActual = 1;

            accionMascota(); // llama a Alimentar(), Bañar(), etc.
            _totalFrames = _mascota.FramesPorAccion(_mascota.AccionActual);

            ActualizarUI();
            _timerAnimacion.Start();

            // Después de duracionMs milisegundos, vuelve al estado normal
            _timerVolverNormal.Interval = duracionMs;
            _timerVolverNormal.Start();
        }

        private void InicializarTimers()
        {
            // Timer principal — baja necesidades cada 5 segundos
            _timerJuego = new Timer { Interval = 5000 };
            _timerJuego.Tick += (s, e) =>
            {
                _mascota?.PasarTiempo();
                ActualizarUI();

                if (_mascota != null && !_mascota.EstaViva)
                    MostrarMuertesMascota();
            };

            // Timer de animación — cambia frame cada 300ms
            _timerAnimacion = new Timer { Interval = 300 };
            _timerAnimacion.Tick += (s, e) =>
            {
                _frameActual = (_frameActual % _totalFrames) + 1;
                CargarImagenMascota();
            };

            // Timer volver a normal
            _timerVolverNormal = new Timer();
            _timerVolverNormal.Tick += (s, e) =>
            {
                _timerVolverNormal.Stop();
                _timerAnimacion.Stop();
                _mascota?.VolvserANormal();
                _frameActual = 1;
                ActualizarUI();
            };
        }

        private void ActualizarUI()
        {
            if (_mascota == null) return;

            // Actualizar imagen de fondo
            CargarImagenFondo();

            // Actualizar imagen de la mascota
            CargarImagenMascota();

            // Actualizar indicadores de necesidades
            ActualizarCirculo(_circHambre, _lblHambre, _mascota.Hambre);
            ActualizarCirculo(_circHigiene, _lblHigiene, _mascota.Higiene);
            ActualizarCirculo(_circDiversion, _lblDiversion, _mascota.Diversion);
            ActualizarCirculo(_circSueno, _lblSueno, _mascota.Sueno);

            // Actualizar etiqueta de estado
            if (_lblEstado != null)
            {
                var estado = _mascota.ObtenerEstadoSalud();
                (_lblEstado.Text, _lblEstado.ForeColor) = estado switch
                {
                    EstadoSalud.Saludable => ("😊 Saludable", Color.FromArgb(0, 200, 100)),
                    EstadoSalud.Regular => ("😐 Regular", Color.Yellow),
                    EstadoSalud.Critico => ("😰 ¡Crítico!", Color.OrangeRed),
                    EstadoSalud.Muerta => ("💀 Murió", Color.Gray),
                    _ => ("", Color.White)
                };

                // Mostrar alerta de necesidad urgente si está en crítico
                if (estado == EstadoSalud.Critico)
                    _lblEstado.Text += $" — {_mascota.NecesidadMasUrgente()} urgente";
            }
        }

        private void ActualizarCirculo(Panel circulo, Label etiqueta, int valor)
        {
            if (circulo == null || etiqueta == null) return;

            // Color según el porcentaje
            Color color = valor switch
            {
                > 70 => Color.FromArgb(0, 200, 100),   // verde — saludable
                > 30 => Color.FromArgb(255, 200, 0),   // amarillo — regular
                _ => Color.FromArgb(220, 50, 50)    // rojo — crítico
            };

            // Forzar repintado con el nuevo color
            circulo.Tag = color;
            circulo.Invalidate();
            circulo.Paint -= CirculoPaint; // evitar handlers acumulados
            circulo.Paint += CirculoPaint;

            etiqueta.Text = $"{valor}%";
        }

        private void CirculoPaint(object sender, PaintEventArgs e)
        {
            if (sender is Panel p && p.Tag is Color color)
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using var brush = new SolidBrush(color);
                e.Graphics.FillEllipse(brush, 1, 1, p.Width - 3, p.Height - 3);
            }
        }

        private void CargarImagenMascota()
        {
            if (_picMascota == null || _mascota == null) return;

            string ruta = _mascota.ObtenerRutaImagen(_frameActual);

            // Si la imagen existe la carga, si no deja el PictureBox vacío
            // Cuando subas tus imágenes, automáticamente empezarán a aparecer
            if (File.Exists(ruta))
            {
                try
                {
                    _picMascota.Image?.Dispose();
                    _picMascota.Image = Image.FromFile(ruta);
                }
                catch { /* imagen corrupta o bloqueada — se ignora */   /*}           */




/*

            }
            else
            {
                // Placeholder visual mientras no hay imagen
                // REEMPLAZAR: cuando subas tus imágenes desaparece solo
                _picMascota.BackColor = Color.FromArgb(30, 30, 60);
            }
        }

        private void CargarImagenFondo()
        {
            if (_picFondo == null || _mascota == null) return;

            string ruta = _mascota.ObtenerRutaFondo();

            if (File.Exists(ruta))
            {
                try
                {
                    _picFondo.Image?.Dispose();
                    _picFondo.Image = Image.FromFile(ruta);
                }
                catch { }
            }
        }

        private void MostrarMuertesMascota()
        {
            _timerJuego.Stop();
            _timerAnimacion.Stop();
            _timerVolverNormal.Stop();

            ActualizarUI();

            var resultado = MessageBox.Show(
                $"💀 {_mascota.Nombre} no pudo sobrevivir...\n\n" +
                $"Tiempo crítico: {_mascota.PorcentajeTiempoCritico():F0}% del tiempo\n\n" +
                "¿Quieres intentarlo de nuevo?",
                "Game Over",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (resultado == DialogResult.Yes)
                MostrarPantallaSeleccion();
            else
                this.Close();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _timerJuego?.Stop();
            _timerAnimacion?.Stop();
            _timerVolverNormal?.Stop();
            base.OnFormClosed(e);
        }
    }
}

*/