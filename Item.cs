using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Doodle_Jump
{
    public abstract class Item
    {
        protected Dudler dudler;
        protected bool IsTake = false;
        public Point StartedPoint;

        public double x, y;
        public Size size;
        public TextureManager Texture;

        public Item(double x = 0, double y = 0)
        {
            this.x = x;
            this.y = y;
        }

        public virtual void Draw(Graphics g, double cameraOffsetY) { }

        public virtual void IsCollision(Dudler dudler, Platform platform) { }

        public virtual void Update()
        {
            Texture.Update();
        }
    }

    public class JetPack : Item
    {
        public static TextureManager BaseTexture;

        Size sizeOfAnimation;

        public JetPack(double x = 0, double y = 0) : base(x, y)
        {
            size = new Size(35, 50);
            sizeOfAnimation = new Size(50, 80);
            Texture = BaseTexture.Clone();
            StartedPoint = new Point(40,-size.Width-30);
        }

        static JetPack()
        {
            BaseTexture = new TextureManager(ImageCreator.CreateImage(Properties.Resources.globalTextures, new Point(393, 530), new Size(53, 72)));
            BaseTexture.AddSprate(Properties.Resources.jetPack_Image, 5, 10, 0, new Size(4, 3));
        }

        public override void Draw(Graphics g, double cameraOffsetY)
        {
            if (!(this.Texture.sprites.Any(x => x.IsAnimationPlaying)))
            g.DrawImage(
            Texture.GetImage(),
            new RectangleF(
                (float)x,
                (float)(y + cameraOffsetY),
                size.Width,
                size.Height
               ) 
            );
            else
                g.DrawImage(
            Texture.GetImage(),
            new RectangleF(
                (float)x,
                (float)(y + cameraOffsetY),
                sizeOfAnimation.Width,
                sizeOfAnimation.Height
               )
            );
        }

        public void DudlerTakeItem()
        {
            if (IsTake)
            {
                dudler.vy = -20;
                if (dudler.Texture.state == 1)
                {
                    x = dudler.x - this.sizeOfAnimation.Width + 40;
                    y = dudler.y+ 20;
                }
                if (dudler.Texture.state == 0)
                {
                    x = dudler.x + dudler.size.Width - 40;
                    y = dudler.y + 20;
                }
            }
            
        }

        public override void IsCollision(Dudler dudler, Platform platform)
        {
            if (!IsTake)
            {
                if (dudler.x + dudler.size.Width > this.x && dudler.x < this.x + size.Width && dudler.y + dudler.size.Height > this.y && dudler.y < this.y + size.Height && dudler.item == null)
                {
                    dudler.item = this;
                    platform.item = null;

                    this.dudler = dudler;
                    IsTake = true;
                    this.Texture.PlaySprite(0, int.MaxValue);
                    timerFly = 100;
                    SoundManager.PlaySound(Properties.Resources.jetpack);
                }
            }
            
        }

        public void AnimationPlay(int number)
        {
            Texture.PlaySprite(number);
        }

        int timerFly;

        public override void Update()
        {
            if (timerFly > 0)
                timerFly--;
            else
                if (IsTake)
            {
                dudler.item = null;
            }    
                base.Update();
            DudlerTakeItem();
        }

    }

    public class Spring : Item
    {
        Size sizeOfAnimation;
        public static TextureManager BaseTexture;


        public Spring(double x = 0, double y = 0) : base(x, y)
        {
            size = new Size(20, 50);
            Texture = BaseTexture.Clone();
            StartedPoint = new Point(30, -size.Width -18);

        }

        static Spring()
        {
            BaseTexture = new TextureManager(ImageCreator.CreateImage(Properties.Resources.globalTextures, new Point(808, 172), new Size(34, 58)));
            BaseTexture.AddSprate(new List<Image>() { ImageCreator.CreateImage(Properties.Resources.globalTextures, new Point(808,230), new Size(34,58)) },30);

        }

        public override void Draw(Graphics g, double cameraOffsetY)
        {
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

        public override void IsCollision(Dudler dudler, Platform platform)
        {
            if (dudler.x + dudler.size.Width > this.x && dudler.x < this.x + size.Width && dudler.y + dudler.size.Height > this.y && dudler.y < this.y + size.Height && dudler.vy>0)
            {
                dudler.vy = -20;
                    if (!Texture.sprites[0].IsAnimationPlaying)
                        Texture.PlaySprite(0, 1);
                SoundManager.PlaySound(Properties.Resources.feder);
            }

        }

        public override void Update()
        {
            base.Update();
        }

    }

    public class ItemSpawnInfoItems
    {
        public Func<Item> Factory;
        public int Weight;

        public ItemSpawnInfoItems(Func<Item> factory, int weight)
        {
            Factory = factory;
            Weight = weight;
        }
    }
}
