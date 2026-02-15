using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing.Printing;
using System.Linq;
using System.Media;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;


namespace Doodle_Jump
{
    public class Dudler
    {
        public Item item;

        public Size WinSize = new Size(800, 1200);
        public double x, y;
        public Size size;
        public double vy,vx;

        private MenegerBullet bullets = new MenegerBullet();

        public bool a,d = false;

        bool IsStayOnPlatform;

        public TextureManager Texture;

        public Dudler(int x, int y)
        {
            this.x = x;
            this.y = y;
            size.Width = 90;
            size.Height = 90;
            Texture = new TextureManager(Properties.Resources.dud_left_stay);
            Texture.AddImage(Properties.Resources.dud_rght_stay);
            Texture.AddSprate(new List<Image>() { Properties.Resources.dud_left_down }, 15);
            Texture.AddSprate(new Sprite(new List<Image>() { Properties.Resources.dud_rght_down }, 15));
            Texture.AddSprate(new Sprite(new List<Image>() { Properties.Resources.dud_attack_down }, 10));
        }

        public void Gravity()
        {

            vy += 0.5;
        }

        public (bool,Platform) CanMove(double dx, double dy, PlatformsMeneger platforms)
        {
            var x = this.x + dx;
            var y = this.y + dy;

            foreach (var platform in platforms)
                if (x + size.Width - 20 > platform.x &&
                    x + 20 < platform.x + platform.size.Width &&
                    y + size.Height >= platform.y &&
                    y + size.Height < platform.y + platform.size.Height &&
                    vy > 0)
                        return (false,platform);
            return (true,null);
        }       

        public void Move()
        {
            if (a == true)
            {
                Texture.state = 0;
                vx = -8;
            }
            if (d == true)
            {
                Texture.state = 1;
                vx = 8;
            }
        }

        public void MoveSide()
        {
            if (x > WinSize.Width)
                x = -size.Width;
            if (x + size.Width < 0)
                x = WinSize.Width;
        }

        public void Physic(PlatformsMeneger platforms)
        {
            if (vx != 0)
                x += vx;

            if (vx > 0)
                if (vx - 0.7 > 0)
                    vx -= 0.7;
                else
                    vx = 0;

            if (vx < 0)
                if (vx + 0.7 < 0)
                    vx += 0.7;
                else
                    vx = 0;

            var canMove = CanMove(0, vy, platforms);
            if (canMove.Item1)
                y += vy;
            else
            {
                vy = -17;
                SoundManager.PlaySound(Properties.Resources.jump);
                y += ((canMove.Item2.y - size.Height) - y) * 0.5f;
            }
        }

        public void DrawJump(PlatformsMeneger platforms)
        {
            foreach (var platform in platforms)
                if (x + size.Width > platform.x &&
                    x < platform.x + platform.size.Width &&
                    y + size.Height + 40 >= platform.y &&
                    y + size.Height + 40 < platform.y + platform.size.Height &&
                    vy > 0)
                {
                    Texture.PlaySprite(Texture.state);
                    break;
                }

        }

        public void Attack(Point bulletLocation, double cameraOffsetY)
        {
            double startX = x + size.Width / 2;
            double startY = y + size.Height / 2 + cameraOffsetY;

            double angle = Math.Atan2(
                bulletLocation.Y - startY,
                bulletLocation.X - startX
            );

            if (angle > -Math.PI / 4)
                angle = -Math.PI / 4;

            if ( angle < -Math.PI / 4 * 3)
                angle = -Math.PI / 4 * 3;

                Texture.PlaySprite(2);
                SoundManager.PlaySound(Properties.Resources.attack);
                bullets.CreateBullet(x + this.size.Width / 2, y, angle);
        }

        public void Attack()
        {
            Texture.PlaySprite(2);
            SoundManager.PlaySound(Properties.Resources.attack);
            bullets.CreateBullet(x + this.size.Width/2, y,- Math.PI/2);
        }

        public void CollisionOnItems(PlatformsMeneger platforms)
        {
            foreach (var platform in platforms)
            {
                
                Item item = platform.item;
                if (item == null)
                    continue;
                
                item.IsCollision(this,platform);

            }
        }

        public void Drow(Graphics g, double cameraOffsetY)
        {
            bullets.Draw(g, cameraOffsetY);
            if (item != null) 
                item.Draw(g,cameraOffsetY);

            g.DrawImage(
                Texture.GetImage(),
                new RectangleF(
                    (float)x,
                    (float)(y + cameraOffsetY),
                    size.Width,
                    size.Height
                )
            );
        }

        public void Update(PlatformsMeneger platforms, double cameraOffsetY)
        {
            if (item != null)
                item.Update();
            CollisionOnItems(platforms);
            bullets.Update(cameraOffsetY);
            MoveSide();
            DrawJump(platforms);
            Move();
            Gravity();
            Texture.Update();
            Physic(platforms);
        }
    }

    public class MenegerBullet
    {
        public Size WinSize = new Size(800, 1200);

        List<Bullet> bullets;

        private Size size = new Size(15,15);  

        public MenegerBullet()
        {
            bullets = new List<Bullet>();
        }

        Image Texture = Properties.Resources.bullet;


        public void CreateBullet(double x, double y, double angel)
        {
            bullets.Add(new Bullet(x, y, angel));
        }

        public void Update(double cameraOffsetY)
        {
            foreach (Bullet bullet in bullets)
            {
                double screenX = bullet.x;
                double screenY = bullet.y + cameraOffsetY;

                double realY = cameraOffsetY + bullet.y;
                bullet.Update();

                if (screenY > WinSize.Height ||
                    screenY + size.Height < 0 ||
                    screenX + size.Width < 0 ||
                    screenX > WinSize.Width)
                        bullet.IsAlive = false;
                
            }

            for (int i =0;  i < bullets.Count; i++)
                if (!bullets[i].IsAlive)
                {
                    bullets.RemoveAt(i);
                    break;
                }
        }

        public void Draw(Graphics g, double cameraOffsetY)
        {
            foreach (Bullet bullet in bullets)
            {
                g.DrawImage(
                    Texture,
                    new RectangleF(
                        (float)bullet.x,
                        (float)(bullet.y + cameraOffsetY),
                        size.Width,
                        size.Height
                    )
                );

            }
        }
    }

    public class Bullet
    {
        public bool IsAlive = true;
        public double x, y, angle;
        public double Speed = 35;

        public Bullet(double x, double y, double angle)
        {
            this.x = x;
            this.y = y;
            this.angle = angle;
        }

        public void Update()
        {
            x += Math.Cos(angle) * Speed;
            y += Math.Sin(angle) * Speed;
        }
    }

}
