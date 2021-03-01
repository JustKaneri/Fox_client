using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Net.Sockets;
using System.Threading;
using DLl_Profile;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Net;
using Microsoft.Win32;

namespace Fox_client
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Клиент.
        /// </summary>
        private TcpClient client;
        private string IpAdress = "127.0.0.1";
        private int Port = 9000;
        /// <summary>
        /// Поток подключения
        /// </summary>
        private Thread th1;
        private Thread th2;

        private Info ByteToInfo(byte[] buff)
        {
            BinaryFormatter bf = new BinaryFormatter();

            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(buff, 0, buff.Length);
                ms.Position = 0;
                return (Info)bf.Deserialize(ms);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //using (var reg = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run",true))
            //{
            //    if (reg.GetValue("Shark_Client") == null)
            //    {
            //        string exepath = Application.ExecutablePath;
            //        reg.SetValue("Shark_Client", "\"" + exepath + "\" min");
            //    }
            //}

            this.Hide();
            this.ShowInTaskbar = false;
            WindowState = FormWindowState.Minimized;

            ReadFromFile();
            th2 = new Thread(Connect);
            th2.Start();

        }

        /// <summary>
        /// Чтение из файла.
        /// </summary>
        private void ReadFromFile()
        {
            string path = Path.GetDirectoryName(Application.ExecutablePath) + "/Foregin.key";

            if (File.Exists(path))
                using (BinaryReader br = new BinaryReader(File.Open(path, FileMode.Open)))
                {
                    IpAdress = br.ReadString();
                    Port = int.Parse(br.ReadString());
                }
        }

        /// <summary>
        /// Установка изображения.
        /// </summary>
        /// <param name="image"></param>
        private void SetImage(Image image)
        {
            PbxMain.Image = image;
        }

        /// <summary>
        /// Получения сообщения.
        /// </summary>
        private void GetMessage()
        {
            NetworkStream stream = null;
            try
            {
                stream = client.GetStream();

                while (true)
                {


                    Info inf = new Info();

                    BinaryFormatter bf = new BinaryFormatter();
                    //inf = ByteToInfo(byff);
                    inf = (Info)bf.Deserialize(stream);
                    bf = null;

                    if (inf.Command == "Start")
                    {
                        SetImage(inf.Screen);
                        if (ShowInTaskbar == false)
                        {
                            notifyIcon1.Text = "Fox_клиент\nСтатус: идет демонстрация экрана";
                            ShowForm();

                        }

                    }
                    else
                    {
                        if (ShowInTaskbar)
                        {
                            notifyIcon1.Text = "Fox_клиент\nСтатус: соединение установлено";
                            HideForm();
                        }

                    }

                    inf = null;
                }
            }
            catch (Exception e)
            {
               // MessageBox.Show(e.ToString());
            }
            finally
            {
                stream?.Close();
            }

        }

        /// <summary>
        /// Закрытие формы.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            client?.Close();
            th1?.Abort();
            th2?.Abort();
            e.Cancel = true;
        }

        /// <summary>
        /// Скрытие формы.
        /// </summary>
        private void HideForm()
        {
            Invoke(new MethodInvoker(delegate
            {
                this.WindowState = FormWindowState.Minimized;
                this.Hide();
                this.ShowInTaskbar = false;

            }));


        }

        /// <summary>
        /// Показ формы
        /// </summary>
        private void ShowForm()
        {
            Invoke(new MethodInvoker(delegate
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;

            }));
        }

        /// <summary>
        /// Соединение с сервером
        /// </summary>
        private void Connect()
        {
            while (true)
            {
                try
                {
                    client = new TcpClient(IpAdress, Port);
                    th1 = new Thread(GetMessage);

                    notifyIcon1.Text = "Fox_клиент\nСтатус: соединение установленно";
                    th1.Start();
                    th1.Join();

                    notifyIcon1.Text = "Fox_клиент\nСтатус: соединение разоравнно";
                    PbxMain.Image = Properties.Resources.resize;
                    HideForm();

                }
                catch
                {

                }
                finally
                {
                    client?.Close();
                    th1?.Abort();
                }

                Thread.Sleep(1000);
            }

        }

    }
}
