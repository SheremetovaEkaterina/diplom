using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace diplom
{
    public partial class RegisterForm : Form
    {

        public RegisterForm()
        {
            InitializeComponent();
            enterBtn.Enabled = false;
        }

        private void enterBtn_Click(object sender, EventArgs e)
        {
            string telegramNickname = telegramTextBox.Text.Trim();
            string password = passTextBox.Text;
            string fullName = fioTextBox.Text.Trim();

            // Валидация
            if (string.IsNullOrEmpty(telegramNickname))
            {
                MessageBox.Show("Введи ник Telegram", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrEmpty(password) || password.Length < 6)
            {
                MessageBox.Show("Пароль должен быть не короче 6 символов", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrEmpty(fullName))
            {
                MessageBox.Show("Введи ФИО", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Хеширование пароля
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            Connection.conn.Open();
            try
            {
                int userId;
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = Connection.conn;
                    cmd.CommandText = "INSERT INTO users (tg_nickname, password_hash, role) " +
                                      "VALUES (@telegram_nickname, @password_hash, 'student') RETURNING id";
                    cmd.Parameters.AddWithValue("telegram_nickname", telegramNickname);
                    cmd.Parameters.AddWithValue("password_hash", passwordHash);
                    userId = (int)cmd.ExecuteScalar();
                }


                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = Connection.conn;
                    cmd.CommandText = "INSERT INTO students (user_id, full_name) " +
                                      "VALUES (@user_id, @full_name)";
                    cmd.Parameters.AddWithValue("user_id", userId);
                    cmd.Parameters.AddWithValue("full_name", fullName);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Регистрация успешна! Теперь ты можешь войти.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }

            catch (NpgsqlException ex)
            {
                if (ex.Message.Contains("users_telegram_nickname_key"))
                {
                    MessageBox.Show("Этот ник Telegram уже занят", "Ошибка");
                }
                else
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
                }
            }

            finally
            {
                Connection.conn.Close();
            }
        }

        private void reg_Click(object sender, EventArgs e)
        {

        }

        private void RegisterForm_Activated(object sender, EventArgs e)
        {

        }

        private void guna2CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (guna2CheckBox1.Checked)
            {
                enterBtn.Enabled = true;
            }
            else 
                enterBtn.Enabled = false;

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            PrivatePolicy privatePolicy = new PrivatePolicy();
            privatePolicy.Show();
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            UserAgree userAgree = new UserAgree();
            userAgree.Show();
        }
    }
}
