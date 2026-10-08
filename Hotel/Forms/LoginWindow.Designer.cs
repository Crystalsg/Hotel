namespace Hotel.UI
{
    partial class LoginWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblLogin = new Label();
            lblPassword = new Label();
            lblCaptcha = new Label();
            txtLogin = new TextBox();
            txtPassword = new TextBox();
            txtCaptcha = new TextBox();
            btnLogin = new Button();
            btnRegister = new Button();
            SuspendLayout();
            // 
            // lblLogin
            // 
            lblLogin.AutoSize = true;
            lblLogin.Location = new Point(231, 117);
            lblLogin.Name = "lblLogin";
            lblLogin.Size = new Size(41, 15);
            lblLogin.TabIndex = 0;
            lblLogin.Text = "Логин";
            //lblLogin.Click += this.lblLogin_Click;
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Location = new Point(231, 157);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(49, 15);
            lblPassword.TabIndex = 1;
            lblPassword.Text = "Пароль";
            //lblPassword.Click += this.lblPassword_Click;
            // 
            // lblCaptcha
            // 
            lblCaptcha.AutoSize = true;
            lblCaptcha.Location = new Point(231, 197);
            lblCaptcha.Name = "lblCaptcha";
            lblCaptcha.Size = new Size(60, 15);
            lblCaptcha.TabIndex = 2;
            lblCaptcha.Text = "CAPTCHA";
            //lblCaptcha.Click += this.lblCaptcha_Click;
            // 
            // txtLogin
            // 
            txtLogin.Location = new Point(321, 117);
            txtLogin.Name = "txtLogin";
            txtLogin.Size = new Size(180, 23);
            txtLogin.TabIndex = 4;
            //txtLogin.TextChanged += this.txtLogin_TextChanged;
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(321, 154);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(180, 23);
            txtPassword.TabIndex = 5;
            txtPassword.UseSystemPasswordChar = true;
            //txtPassword.TextChanged += this.txtPassword_TextChanged;
            // 
            // txtCaptcha
            // 
            txtCaptcha.Location = new Point(431, 197);
            txtCaptcha.Name = "txtCaptcha";
            txtCaptcha.Size = new Size(70, 23);
            txtCaptcha.TabIndex = 6;
            txtCaptcha.TextChanged += txtCaptcha_TextChanged;
            // 
            // btnLogin
            // 
            btnLogin.Location = new Point(321, 247);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(100, 30);
            btnLogin.TabIndex = 7;
            btnLogin.Text = "Войти";
            btnLogin.UseVisualStyleBackColor = true;
            btnLogin.Click += btnLogin_Click;
            // 
            // btnRegister
            // 
            btnRegister.Location = new Point(431, 247);
            btnRegister.Name = "btnRegister";
            btnRegister.Size = new Size(100, 30);
            btnRegister.TabIndex = 8;
            btnRegister.Text = "Регистрация";
            btnRegister.UseVisualStyleBackColor = true;
            btnRegister.Click += btnRegister_Click;
            // 
            // LoginWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnRegister);
            Controls.Add(btnLogin);
            Controls.Add(txtCaptcha);
            Controls.Add(txtPassword);
            Controls.Add(txtLogin);
            Controls.Add(lblCaptcha);
            Controls.Add(lblPassword);
            Controls.Add(lblLogin);
            Name = "LoginWindow";
            Text = "LoginForm";
            //Load += this.LoginWindow_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblLogin;
        private Label lblPassword;
        private Label lblCaptcha;
        private TextBox txtLogin;
        private TextBox txtPassword;
        private TextBox txtCaptcha;
        private Button btnLogin;
        private Button btnRegister;
    }
}