using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace Doodle_Jump
{
    public partial class Form1 : Form
    {
        private System.Windows.Forms.Timer timer;
        private const int size = 48;

        bool GameStart = false;

        public Size WinSize = new Size(800, 1200);

        private Random rnd = new Random();

        Point StartPoint = new Point(112, 863);

        Dudler dudler;

        private int frames = 0;        // Счётчик кадров
        private int fps = 0;           // Текущий FPS
        private DateTime lastTime;

        private double cameraOffsetY = 0;

        public Form1()
        {
            InitializeComponent();
            Initialize();
            lastTime = DateTime.Now;

            this.DoubleBuffered = true;
            this.KeyPreview = true;

            // События нажатия и отпускания клавиши
            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;
            this.MouseDown += Form1_MouseDown;

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 10; // ~100 FPS
            timer.Tick += Timer_Tick;
        }

        PlatformsMeneger platforms;

        Image GlovalTexture;
        Bitmap scaledBackground;

        private void Initialize()
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(WinSize.Width, WinSize.Height);
            this.Text = "Doodle Jump";
            InitStartButton();

            dudler = new Dudler(StartPoint.X, StartPoint.Y);

            scaledBackground = new Bitmap(ClientSize.Width, ClientSize.Height);
            using (Graphics g2 = Graphics.FromImage(scaledBackground))
            {
                g2.DrawImage(Properties.Resources.backGround, new Rectangle(0, 0, ClientSize.Width, ClientSize.Height));
            }

            GlovalTexture = Properties.Resources.globalTextures;

            platforms = new PlatformsMeneger(GlovalTexture);
            platforms.AddPlatform(new PlatformDefault(100, 950));
        }

        GameButton startButton;
        bool gameStarted = false;

        void InitStartButton()
        {
            startButton = new GameButton
            {
                Bounds = new Rectangle(this.ClientSize.Width/2-100, 700, 200, 80),
                Icon = ImageCreator.CreateImage(Properties.Resources.start_end_tiles, new Point(231, 100), new Size(225, 80)),
                OnClick = StartGame
            };
        }

        void StartGame()
        {
            cameraOffsetY = 0;
            platforms.RemoveAll();
            platforms.AddPlatform(new PlatformDefault(100, 950));
            platforms.platforms[0].CreateItem(() => new Spring());
            gameStarted = true;
            timer.Start();
            dudler = new Dudler(StartPoint.X, StartPoint.Y);
            for (int i = 0; i < 10; i++)
                platforms.CreateFirstPlatform();

        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (!gameStarted)
            {
                startButton.HandleMouseDown(e.Location);
                
            }

            if (e.Button == MouseButtons.Left && gameStarted)
            {
                dudler.Attack(new Point( e.Location.X, e.Location.Y),cameraOffsetY);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {

            var g = e.Graphics;


            g.DrawImage(scaledBackground, 0, 0);

            dudler.Drow(g, cameraOffsetY);
            platforms.Drow(g,cameraOffsetY);


            if (!gameStarted)
                startButton.Draw(g);
            frames++;
            if ((DateTime.Now - lastTime).TotalSeconds >= 1)
            {
                fps = frames;
                frames = 0;
                lastTime = DateTime.Now;
            }

            g.DrawString($"FPS: {fps}", new Font("Arial", 16), Brushes.Red, new PointF(10, 10));

            base.OnPaint(e);

        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W) { }
            ;
            if (e.KeyCode == Keys.Space)
            {
                if (timer.Enabled)
                    dudler.Attack();
                if (!gameStarted)
                    StartGame();
            }
                
            ;
            if (e.KeyCode == Keys.S) { }
            if (e.KeyCode == Keys.A) dudler.a = true;
            if (e.KeyCode == Keys.D) dudler.d = true;
            ;
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W) { }
            if (e.KeyCode == Keys.S) { }
            if (e.KeyCode == Keys.A) dudler.a = false;
            if (e.KeyCode == Keys.D) dudler.d = false;
        }

        public void GameOver()
        {
            timer.Stop();
            gameStarted = false;
            SoundManager.PlaySound(Properties.Resources.falling);
            
        }

        public void Timer_Tick(object sender, EventArgs e)
        {
            dudler.Update(platforms,cameraOffsetY);
            double screenMiddleY = ClientSize.Height / 3 ;
            double targetOffset = screenMiddleY - dudler.y;

            if (targetOffset > cameraOffsetY)
            {
                cameraOffsetY = targetOffset;
            }
            platforms.Update(cameraOffsetY);
            Invalidate();

            double dudlerScreenY = dudler.y + cameraOffsetY;

            if (dudlerScreenY - 600 > ClientSize.Height)
            {
                GameOver();
            }

        }
    }

}
