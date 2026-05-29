using BurdiGames.Clases;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BurdiGames.GUI
{
    public partial class FormMain : Form
    {
        private readonly Usuario _usuario;

        public FormMain(Usuario usuario)
        {
            InitializeComponent();
            _usuario = usuario;

            // Cargar las tarjetas de juegos
            int x = 20;
            int y = 20;
            foreach (var juego in Program.BurdiGames.CatalogoJuegos)
            {
                var tarjeta = CrearTarjeta(juego);
                tarjeta.Left = x;
                tarjeta.Top = y;
                this.Controls.Add(tarjeta);
                x += 220; // siguiente tarjeta a la derecha
                if (x > 600) // si se pasa del ancho, baja a la siguiente fila
                {
                    x = 20;
                    y += 140;
                }
            }

        }

        //private void CargarDatos()
        //{
        //}

        private Panel CrearTarjeta(Juego juego)
        {
            // Panel que actúa como GroupBox/tarjeta
            var card = new Panel
            {
                Width = 200,
                Height = 120,
                BackColor = Color.FromArgb(20, 20, 40),
                Margin = new Padding(8),
                Padding = new Padding(8)
            };

            var lblNombre = new Label
            {
                Text = juego.Nombre,
                ForeColor = Color.White,
                Font = new Font("Consolas", 10f, FontStyle.Bold),
                AutoSize = false,
                Width = 184,
                Height = 20,
                Top = 8,
                Left = 8
            };

            var lblGenero = new Label
            {
                Text = $"Género: {juego.Genero}",
                ForeColor = Color.Gray,
                Font = new Font("Consolas", 8f),
                AutoSize = false,
                Width = 184,
                Height = 16,
                Top = 32,
                Left = 8
            };

            var btnJugar = new Button
            {
                Text = "▶ Jugar",
                Top = 80,
                Left = 8,
                Width = 80,
                Height = 26,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 200, 100),
                ForeColor = Color.Black,
                Font = new Font("Consolas", 8f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnJugar.FlatAppearance.BorderSize = 0;
            btnJugar.Click += (s, e) =>
            {
                // Por ahora solo registra la partida en el historial
                var partida = new Partida(_usuario, juego) { Puntaje = 0, Fecha = DateTime.Now };
                _usuario.HistorialPartidas.Add(partida);
                juego.Jugar(); // llama al método abstracto
                MessageBox.Show($"Iniciando {juego.Nombre}...\n(Implementación pendiente)",
                    "Jugar", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            var btnPerfil = new Button
            {
                Text = "Info",
                Top = 80,
                Left = 96,
                Width = 50,
                Height = 26,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 40, 60),
                ForeColor = Color.White,
                Font = new Font("Consolas", 8f),
                Cursor = Cursors.Hand
            };
            btnPerfil.FlatAppearance.BorderSize = 0;
            btnPerfil.Click += (s, e) =>
            {
                MessageBox.Show(
                    $"Nombre: {juego.Nombre}\nGénero: {juego.Genero}\n{juego.Descripcion}",
                    "Info del juego", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            card.Controls.AddRange(new Control[] { lblNombre, lblGenero, btnJugar, btnPerfil });
            return card;
        }

        private void btnPerfil_Click(object sender, EventArgs e)
        {
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
