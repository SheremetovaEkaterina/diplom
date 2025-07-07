using Npgsql;
using System;
using System.Windows.Forms;

namespace diplom
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            RegisterForm registerForm = new RegisterForm();
            registerForm.Show();
        }

        private void enterBtn_Click(object sender, EventArgs e)
        {
            string telegramNickname = loginTextBox.Text.Trim();
            string password = passTextBox.Text;

            if (string.IsNullOrEmpty(telegramNickname) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введи ник и пароль", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            try
            {
                Connection.conn.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = Connection.conn;
                    cmd.CommandText = "SELECT id, password_hash, role FROM users WHERE tg_nickname = @telegram_nickname";
                    cmd.Parameters.AddWithValue("telegram_nickname", telegramNickname);
                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        int userId = reader.GetInt32(0);
                        string passwordHash = reader.GetString(1);
                        string role = reader.GetString(2);
                        reader.Close();
                        if (BCrypt.Net.BCrypt.Verify(password, passwordHash))
                        {
                            if (role == "student")
                            {
                                using (var studentCmd = new NpgsqlCommand())
                                {
                                    studentCmd.Connection = Connection.conn;
                                    studentCmd.CommandText = "SELECT id FROM students WHERE user_id = @user_id";
                                    studentCmd.Parameters.AddWithValue("user_id", userId);
                                    var studentIdResult = studentCmd.ExecuteScalar();
                                    int studentId = (int)studentIdResult;
                                    Connection.conn.Close();
                                    MessageBox.Show("Вход успешен", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    MainStud mainstud = new MainStud(studentId);
                                    mainstud.Show();
                                    this.Hide();

                                }
                            }
                            else if (role == "admin")
                            {
                                reader.Close();
                                Connection.conn.Close();
                                MessageBox.Show("Вход успешен", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                MainTeach mainTeach = new MainTeach();
                                mainTeach.Show();
                                this.Hide();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Неверный пароль", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Пользователь не найден", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
            }

        }
    }
}
