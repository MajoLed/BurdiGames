namespace BurdiGames
{
    partial class FormLogin
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormLogin));
            panel1 = new Panel();
            lblContrasenia = new Label();
            lblNickname = new Label();
            bgTxtContrasenia = new Panel();
            txtPassword = new TextBox();
            bgTxtUsuario = new Panel();
            txtUsuario = new TextBox();
            btnRegistrarte = new Button();
            btnIniciarSesion = new Button();
            lblTitulo = new Label();
            textBox1 = new TextBox();
            panel1.SuspendLayout();
            bgTxtContrasenia.SuspendLayout();
            bgTxtUsuario.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            resources.ApplyResources(panel1, "panel1");
            panel1.AllowDrop = true;
            panel1.BackColor = Color.FromArgb(13, 13, 31);
            panel1.Controls.Add(lblContrasenia);
            panel1.Controls.Add(lblNickname);
            panel1.Controls.Add(bgTxtContrasenia);
            panel1.Controls.Add(bgTxtUsuario);
            panel1.Controls.Add(btnRegistrarte);
            panel1.Controls.Add(btnIniciarSesion);
            panel1.Name = "panel1";
            // 
            // lblContrasenia
            // 
            resources.ApplyResources(lblContrasenia, "lblContrasenia");
            lblContrasenia.Name = "lblContrasenia";
            // 
            // lblNickname
            // 
            resources.ApplyResources(lblNickname, "lblNickname");
            lblNickname.Name = "lblNickname";
            // 
            // bgTxtContrasenia
            // 
            resources.ApplyResources(bgTxtContrasenia, "bgTxtContrasenia");
            bgTxtContrasenia.BackColor = Color.FromArgb(48, 48, 46);
            bgTxtContrasenia.Controls.Add(txtPassword);
            bgTxtContrasenia.Name = "bgTxtContrasenia";
            // 
            // txtPassword
            // 
            resources.ApplyResources(txtPassword, "txtPassword");
            txtPassword.BackColor = Color.FromArgb(48, 48, 46);
            txtPassword.BorderStyle = BorderStyle.None;
            txtPassword.Name = "txtPassword";
            // 
            // bgTxtUsuario
            // 
            resources.ApplyResources(bgTxtUsuario, "bgTxtUsuario");
            bgTxtUsuario.BackColor = Color.FromArgb(48, 48, 46);
            bgTxtUsuario.Controls.Add(txtUsuario);
            bgTxtUsuario.Name = "bgTxtUsuario";
            // 
            // txtUsuario
            // 
            resources.ApplyResources(txtUsuario, "txtUsuario");
            txtUsuario.BackColor = Color.FromArgb(48, 48, 46);
            txtUsuario.BorderStyle = BorderStyle.None;
            txtUsuario.Name = "txtUsuario";
            // 
            // btnRegistrarte
            // 
            resources.ApplyResources(btnRegistrarte, "btnRegistrarte");
            btnRegistrarte.BackColor = Color.FromArgb(13, 13, 31);
            btnRegistrarte.ForeColor = Color.White;
            btnRegistrarte.Name = "btnRegistrarte";
            btnRegistrarte.UseVisualStyleBackColor = false;
            // 
            // btnIniciarSesion
            // 
            resources.ApplyResources(btnIniciarSesion, "btnIniciarSesion");
            btnIniciarSesion.BackColor = Color.FromArgb(13, 13, 31);
            btnIniciarSesion.ForeColor = Color.White;
            btnIniciarSesion.Name = "btnIniciarSesion";
            btnIniciarSesion.UseVisualStyleBackColor = false;
            // 
            // lblTitulo
            // 
            resources.ApplyResources(lblTitulo, "lblTitulo");
            lblTitulo.ForeColor = Color.DarkRed;
            lblTitulo.Name = "lblTitulo";
            // 
            // textBox1
            // 
            resources.ApplyResources(textBox1, "textBox1");
            textBox1.Name = "textBox1";
            // 
            // FormLogin
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(8, 8, 24);
            Controls.Add(textBox1);
            Controls.Add(lblTitulo);
            Controls.Add(panel1);
            ForeColor = Color.White;
            Name = "FormLogin";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            bgTxtContrasenia.ResumeLayout(false);
            bgTxtContrasenia.PerformLayout();
            bgTxtUsuario.ResumeLayout(false);
            bgTxtUsuario.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panel1;
        private Label lblTitulo;
        private Button btnRegistrarte;
        private Button btnIniciarSesion;
        private Panel bgTxtContrasenia;
        private Panel bgTxtUsuario;
        private Label lblNickname;
        private TextBox txtUsuario;
        private TextBox textBox1;
        private Label lblContrasenia;
        private TextBox txtPassword;
    }
}
