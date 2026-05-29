using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BurdiGames.Clases.Juegos
{
    // ============================================================
    //  ENUMS & DELEGATES
    // ============================================================
    public enum PetType { Dog, Cat, Duck, Fox }
    public enum PetGender { Male, Female }
    public enum PetMood { Happy, Neutral, Sad, Bored, Sleeping, Escaped }

    public delegate void StatChangedEventHandler(string statName, int newValue);
    public delegate void PetEscapedEventHandler(string petName);
    public delegate void PetActionEventHandler(string action, string petName);

    // ============================================================
    //  CLASE PRINCIPAL DEL JUEGO
    // ============================================================
    internal class Tamagotchi : Juego
    {
        public Pet? Mascota { get; private set; }

        public Tamagotchi(string nombre, string descripcion, string rutaImagen)
            : base(nombre, "Simulación", descripcion, rutaImagen)
        {
        }

        public override void Jugar()
        {
            using (var creation = new PetCreationScreen())
            {
                if (creation.ShowDialog() == DialogResult.OK)
                {
                    Mascota = creation.CreatedPet;

                    new GameForm(Mascota).ShowDialog();
                }
            }
        }
    }

    // ============================================================
    //  BASE CLASS: Pet
    // ============================================================
    public abstract class Pet
    {
        public string Name { get; private set; }
        public PetGender Gender { get; private set; }
        public PetType Type { get; protected set; }

        private int _hunger;
        private int _energy;
        private int _fun;
        private bool _escaped;

        public int Hunger
        {
            get => _hunger;
            private set => _hunger = Math.Clamp(value, 0, 100);
        }

        public int Energy
        {
            get => _energy;
            private set => _energy = Math.Clamp(value, 0, 100);
        }

        public int Fun
        {
            get => _fun;
            private set => _fun = Math.Clamp(value, 0, 100);
        }

        public bool Escaped => _escaped;

        public event StatChangedEventHandler OnStatChanged;
        public event PetEscapedEventHandler OnPetEscaped;
        public event PetActionEventHandler OnPetAction;

        protected Pet(string name, PetGender gender)
        {
            Name = name;
            Gender = gender;

            _hunger = 30;
            _energy = 80;
            _fun = 70;
            _escaped = false;
        }

        public void Feed(FoodItem food)
        {
            if (_escaped) return;

            _hunger = Math.Clamp(_hunger - food.NutritionValue, 0, 100);

            OnStatChanged?.Invoke("Hambre", _hunger);
            OnPetAction?.Invoke($"¡Comió {food.Name}! 🍖", Name);
        }

        public void Sleep()
        {
            if (_escaped) return;

            _energy = Math.Clamp(_energy + 40, 0, 100);
            _fun = Math.Clamp(_fun - 10, 0, 100);

            OnStatChanged?.Invoke("Energía", _energy);
            OnPetAction?.Invoke("¡Se fue a dormir! 💤", Name);
        }

        public void Play(ToyItem toy)
        {
            if (_escaped) return;

            if (_energy < 15)
            {
                OnPetAction?.Invoke("¡Está muy cansado para jugar! 😴", Name);
                return;
            }

            _fun = Math.Clamp(_fun + toy.FunValue, 0, 100);
            _energy = Math.Clamp(_energy - 15, 0, 100);
            _hunger = Math.Clamp(_hunger + 10, 0, 100);

            OnStatChanged?.Invoke("Diversión", _fun);
            OnPetAction?.Invoke($"¡Jugó con {toy.Name}! {toy.Emoji}", Name);
        }

        public void Tick()
        {
            if (_escaped) return;

            _hunger = Math.Clamp(_hunger + 3, 0, 100);
            _energy = Math.Clamp(_energy - 2, 0, 100);
            _fun = Math.Clamp(_fun - 4, 0, 100);

            OnStatChanged?.Invoke("Hambre", _hunger);
            OnStatChanged?.Invoke("Energía", _energy);
            OnStatChanged?.Invoke("Diversión", _fun);

            if (_fun <= 0 && !_escaped)
            {
                _escaped = true;
                OnPetEscaped?.Invoke(Name);
            }
        }

        public PetMood GetMood()
        {
            if (_escaped) return PetMood.Escaped;
            if (_energy < 20) return PetMood.Sleeping;
            if (_fun < 20) return PetMood.Bored;
            if (_hunger > 80) return PetMood.Sad;

            var positiveStats = new[] { 100 - _hunger, _energy, _fun }
                .Where(v => v >= 60)
                .ToList();

            return positiveStats.Count >= 2
                ? PetMood.Happy
                : PetMood.Neutral;
        }

        public abstract void Draw(Graphics g, Rectangle bounds, PetMood mood, bool animate);

        public abstract string GetSoundEmoji();

        public abstract string TypeName { get; }
    }

    // ============================================================
    //  MASCOTAS
    // ============================================================
    public class Dog : Pet
    {
        public override string TypeName => "Perro";

        public Dog(string name, PetGender gender)
            : base(name, gender)
        {
            Type = PetType.Dog;
        }

        public override string GetSoundEmoji() => "🐶";

        public override void Draw(Graphics g, Rectangle bounds, PetMood mood, bool animate)
        {
            PixelPainter.DrawDog(g, bounds, mood, animate, Gender);
        }
    }

    public class Cat : Pet
    {
        public override string TypeName => "Gato";

        public Cat(string name, PetGender gender)
            : base(name, gender)
        {
            Type = PetType.Cat;
        }

        public override string GetSoundEmoji() => "🐱";

        public override void Draw(Graphics g, Rectangle bounds, PetMood mood, bool animate)
        {
            PixelPainter.DrawCat(g, bounds, mood, animate, Gender);
        }
    }

    public class Duck : Pet
    {
        public override string TypeName => "Pato";

        public Duck(string name, PetGender gender)
            : base(name, gender)
        {
            Type = PetType.Duck;
        }

        public override string GetSoundEmoji() => "🦆";

        public override void Draw(Graphics g, Rectangle bounds, PetMood mood, bool animate)
        {
            PixelPainter.DrawDuck(g, bounds, mood, animate, Gender);
        }
    }

    public class Fox : Pet
    {
        public override string TypeName => "Zorro";

        public Fox(string name, PetGender gender)
            : base(name, gender)
        {
            Type = PetType.Fox;
        }

        public override string GetSoundEmoji() => "🦊";

        public override void Draw(Graphics g, Rectangle bounds, PetMood mood, bool animate)
        {
            PixelPainter.DrawFox(g, bounds, mood, animate, Gender);
        }
    }

    // ============================================================
    //  FACTORY
    // ============================================================
    public static class PetFactory
    {
        public static Pet Create(PetType type, string name, PetGender gender)
        {
            return type switch
            {
                PetType.Dog => new Dog(name, gender),
                PetType.Cat => new Cat(name, gender),
                PetType.Duck => new Duck(name, gender),
                PetType.Fox => new Fox(name, gender),
                _ => throw new ArgumentException("Tipo desconocido")
            };
        }
    }

    // ============================================================
    //  ITEMS
    // ============================================================
    public class FoodItem
    {
        public string Name { get; }
        public int NutritionValue { get; }
        public string Emoji { get; }

        public FoodItem(string name, int nutritionValue, string emoji)
        {
            Name = name;
            NutritionValue = nutritionValue;
            Emoji = emoji;
        }
    }

    public class ToyItem
    {
        public string Name { get; }
        public int FunValue { get; }
        public string Emoji { get; }

        public ToyItem(string name, int funValue, string emoji)
        {
            Name = name;
            FunValue = funValue;
            Emoji = emoji;
        }
    }

    // ============================================================
    //  INVENTARIO
    // ============================================================
    public static class Inventory
    {
        public static readonly List<FoodItem> Foods = new()
        {
            new FoodItem("Croquetas", 35, "🍖"),
            new FoodItem("Pescado", 40, "🐟"),
            new FoodItem("Manzana", 20, "🍎"),
            new FoodItem("Galleta", 15, "🍪")
        };

        public static readonly List<ToyItem> Toys = new()
        {
            new ToyItem("Pelota", 30, "⚽"),
            new ToyItem("Laser", 35, "🔴"),
            new ToyItem("Patito", 20, "🐤")
        };
    }

    // ============================================================
    //  PIXEL PAINTER
    // ============================================================
    public static class PixelPainter
    {
        public static void DrawDog(Graphics g, Rectangle b, PetMood mood, bool animate, PetGender gender)
        {
            using var brush = new SolidBrush(Color.BurlyWood);
            g.FillEllipse(brush, b.X + 50, b.Y + 50, 150, 150);
        }

        public static void DrawCat(Graphics g, Rectangle b, PetMood mood, bool animate, PetGender gender)
        {
            using var brush = new SolidBrush(Color.LightGray);
            g.FillEllipse(brush, b.X + 50, b.Y + 50, 150, 150);
        }

        public static void DrawDuck(Graphics g, Rectangle b, PetMood mood, bool animate, PetGender gender)
        {
            using var brush = new SolidBrush(Color.Gold);
            g.FillEllipse(brush, b.X + 50, b.Y + 50, 150, 150);
        }

        public static void DrawFox(Graphics g, Rectangle b, PetMood mood, bool animate, PetGender gender)
        {
            using var brush = new SolidBrush(Color.OrangeRed);
            g.FillEllipse(brush, b.X + 50, b.Y + 50, 150, 150);
        }
    }

    // ============================================================
    //  CREACIÓN DE MASCOTA
    // ============================================================
    public class PetCreationScreen : Form
    {
        private TextBox _nameBox;
        private ComboBox _typeCombo;
        private ComboBox _genderCombo;
        private Button _startBtn;

        public Pet CreatedPet { get; private set; }

        public PetCreationScreen()
        {
            InitUI();
        }

        private void InitUI()
        {
            Text = "Crear Mascota";
            Size = new Size(400, 300);
            StartPosition = FormStartPosition.CenterScreen;

            Controls.Add(new Label
            {
                Text = "Nombre",
                Location = new Point(20, 30),
                AutoSize = true
            });

            _nameBox = new TextBox
            {
                Location = new Point(120, 30),
                Width = 200
            };

            Controls.Add(_nameBox);

            Controls.Add(new Label
            {
                Text = "Tipo",
                Location = new Point(20, 80),
                AutoSize = true
            });

            _typeCombo = new ComboBox
            {
                Location = new Point(120, 80),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            _typeCombo.Items.AddRange(new[]
            {
                "Perro",
                "Gato",
                "Pato",
                "Zorro"
            });

            _typeCombo.SelectedIndex = 0;

            Controls.Add(_typeCombo);

            Controls.Add(new Label
            {
                Text = "Género",
                Location = new Point(20, 130),
                AutoSize = true
            });

            _genderCombo = new ComboBox
            {
                Location = new Point(120, 130),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            _genderCombo.Items.AddRange(new[]
            {
                "Hembra",
                "Macho"
            });

            _genderCombo.SelectedIndex = 0;

            Controls.Add(_genderCombo);

            _startBtn = new Button
            {
                Text = "Adoptar",
                Location = new Point(120, 190),
                Width = 200
            };

            _startBtn.Click += StartBtn_Click;

            Controls.Add(_startBtn);
        }

        private void StartBtn_Click(object sender, EventArgs e)
        {
            string name = _nameBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Escribe un nombre.");
                return;
            }

            CreatedPet = PetFactory.Create(
                GetSelectedType(),
                name,
                GetSelectedGender()
            );

            DialogResult = DialogResult.OK;
            Close();
        }

        private PetType GetSelectedType()
        {
            return _typeCombo.SelectedIndex switch
            {
                0 => PetType.Dog,
                1 => PetType.Cat,
                2 => PetType.Duck,
                3 => PetType.Fox,
                _ => PetType.Cat
            };
        }

        private PetGender GetSelectedGender()
        {
            return _genderCombo.SelectedIndex == 0
                ? PetGender.Female
                : PetGender.Male;
        }
    }

    // ============================================================
    //  GAME FORM
    // ============================================================
    public class GameForm : Form
    {
        private readonly Pet _pet;

        private Panel _petPanel;

        private ProgressBar _hungerBar;
        private ProgressBar _energyBar;
        private ProgressBar _funBar;

        private System.Windows.Forms.Timer _timer;

        public GameForm(Pet pet)
        {
            _pet = pet;

            InitUI();
            StartGame();
        }

        private void InitUI()
        {
            Text = $"Tamagotchi - {_pet.Name}";
            Size = new Size(600, 500);
            StartPosition = FormStartPosition.CenterScreen;

            _petPanel = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(250, 250),
                BackColor = Color.Pink
            };

            _petPanel.Paint += PetPanel_Paint;

            Controls.Add(_petPanel);

            _hungerBar = CreateBar(320, 50);
            _energyBar = CreateBar(320, 120);
            _funBar = CreateBar(320, 190);

            Controls.Add(_hungerBar);
            Controls.Add(_energyBar);
            Controls.Add(_funBar);

            Controls.Add(new Label
            {
                Text = "Hambre",
                Location = new Point(320, 25)
            });

            Controls.Add(new Label
            {
                Text = "Energía",
                Location = new Point(320, 95)
            });

            Controls.Add(new Label
            {
                Text = "Diversión",
                Location = new Point(320, 165)
            });

            var feedBtn = new Button
            {
                Text = "Alimentar",
                Location = new Point(20, 320),
                Width = 150
            };

            feedBtn.Click += (s, e) =>
            {
                _pet.Feed(Inventory.Foods.First());
                RefreshBars();
            };

            Controls.Add(feedBtn);

            var playBtn = new Button
            {
                Text = "Jugar",
                Location = new Point(200, 320),
                Width = 150
            };

            playBtn.Click += (s, e) =>
            {
                _pet.Play(Inventory.Toys.First());
                RefreshBars();
            };

            Controls.Add(playBtn);

            var sleepBtn = new Button
            {
                Text = "Dormir",
                Location = new Point(380, 320),
                Width = 150
            };

            sleepBtn.Click += (s, e) =>
            {
                _pet.Sleep();
                RefreshBars();
            };

            Controls.Add(sleepBtn);

            RefreshBars();
        }

        private ProgressBar CreateBar(int x, int y)
        {
            return new ProgressBar
            {
                Location = new Point(x, y),
                Size = new Size(220, 30),
                Maximum = 100
            };
        }

        private void StartGame()
        {
            _timer = new System.Windows.Forms.Timer
            {
                Interval = 5000
            };

            _timer.Tick += (s, e) =>
            {
                _pet.Tick();
                RefreshBars();
                _petPanel.Invalidate();

                if (_pet.Escaped)
                {
                    _timer.Stop();

                    MessageBox.Show($"{_pet.Name} escapó.");
                }
            };

            _timer.Start();
        }

        private void RefreshBars()
        {
            _hungerBar.Value = _pet.Hunger;
            _energyBar.Value = _pet.Energy;
            _funBar.Value = _pet.Fun;
        }

        private void PetPanel_Paint(object sender, PaintEventArgs e)
        {
            _pet.Draw(
                e.Graphics,
                _petPanel.ClientRectangle,
                _pet.GetMood(),
                true
            );
        }
    }
}