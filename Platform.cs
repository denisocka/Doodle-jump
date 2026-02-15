using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Doodle_Jump 
{
    public class ItemSpawnInfoPlatforms
    {
        public Func<Platform> Factory;
        public int Weight;

        public ItemSpawnInfoPlatforms(Func<Platform> factory, int weight)
        {
            Factory = factory;
            Weight = weight;
        }
    }

    public class PlatformsMeneger : IEnumerable<Platform>
    {
        Image DefaultPlatformImage;

        Random rnd = new Random();
        public Size WinSize = new Size(800, 1200);

        public IEnumerator<Platform> GetEnumerator()
            => platforms.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        Image Texure;

        public PlatformsMeneger(Image image)
        {
            Texure = image;
            DefaultPlatformImage = ImageCreator.CreateImage(Texure, new Point(0, 0), new Size(120, 34));
        }

        public List<Platform> platforms = new List<Platform> ();


        public void AddPlatform(Platform platform)
        {
            platforms.Add(platform);
        }

        public void RemoveAll()
        {
            platforms.Clear();
        }

        public void CreateRandomPlatform(double cameraOffsetY)
        {
            int newX = rnd.Next(0, WinSize.Width - platforms[0].size.Width);
            int newY = rnd.Next((int)-cameraOffsetY - 200, (int)-cameraOffsetY - 50);

            int totalWeight = itemFactories.Sum(i => i.Weight);
            int roll = rnd.Next(totalWeight);

            Platform platform;

            int current = 0;
            foreach (var info in itemFactories)
            {
                current += info.Weight;
                if (roll < current)
                {
                    platform = info.Factory();
                    platform.x = newX;
                    platform.y = newY;

                    platform.CreateRandomItem();

                    this.AddPlatform(platform);

                    break;
                }
            }


        }

        private static readonly List<ItemSpawnInfoPlatforms> itemFactories =
        new()
        {
            new ItemSpawnInfoPlatforms(() => new PlatformDefault(), 5),
        };

        public void CreateFirstPlatform()
        {
            int lastPlatform = platforms.Count - 1;
            int newY =  rnd.Next(platforms[lastPlatform].y - 200, platforms[lastPlatform].y - 100);
            var random = rnd.Next(0, WinSize.Width - platforms[lastPlatform].size.Width);
            this.AddPlatform(new PlatformDefault(random, newY));

        }

        

        public void Update(double cameraOffsetY)
        {

            foreach (Platform platform in platforms)
            {
                platform.Update();
            }
            for (int i=0; i<platforms.Count; i++)
            {
                float screenY = platforms[i].y + (float)cameraOffsetY;

                if (screenY - 150 > WinSize.Height || !platforms[i].IsAlive)
                {
                    platforms.RemoveAt(i);
                    CreateRandomPlatform(cameraOffsetY);
                    break;
                }
            }

                

        }

        public void Drow(Graphics g, double cameraOffsetY)
        {
            foreach (var platform in platforms)
            {
                platform.Drow(g, cameraOffsetY);
            }
        }
    }


    public abstract class Platform
    {
        Random rnd = new Random();
        public Item item;
        public static Image Texture;
        public int x, y;
        public Size size;

        public bool IsAlive;

        public Platform(int x=0, int y=0)
        {
            this.x = x;
            this.y = y;
            size.Width = 100;
            size.Height = 30;
            IsAlive = true;
        }

        public virtual void Drow(Graphics g, double cameraOffsetY)
        {
            if (item != null)
                item.Draw(g, cameraOffsetY);
        }
 

        public void Remove()
        {
            IsAlive = false;
        }

        public virtual void Update()
        {
            if (item != null)
                item.Update();
        }

        public void CreateRandomItem()
        {
            if (item != null) return;

            int totalWeight = itemFactories.Sum(i => i.Weight);
            int roll = rnd.Next(totalWeight);

            int current = 0;
            foreach (var info in itemFactories)
            {
                current += info.Weight;
                if (roll < current)
                {
                    if (info.Factory == null)
                        return; // ничего не спавним

                    CreateItem(info.Factory);
                    return;
                }
            }
        }

        private static readonly List<ItemSpawnInfoItems> itemFactories =
        new()
        {
            new ItemSpawnInfoItems(() => new JetPack(), 5),
            new ItemSpawnInfoItems(() => new Spring(), 20),
            new ItemSpawnInfoItems(null, 100),
        };

        public void CreateItem(Func<Item> factory)
        {
            var item = factory();
            
            item.x = this.x + item.StartedPoint.X;
            item.y = this.y + item.StartedPoint.Y;
            
            this.item = item;
        }

    }

    public class PlatformDefault : Platform
    {

        static PlatformDefault()
        {
            Texture = ImageCreator.CreateImage(Properties.Resources.globalTextures, new Point(0, 0), new Size(120, 34));
        }

        public override void Update()
        {
            
        }

        public override void Drow(Graphics g, double cameraOffsetY)
        {
            base.Drow(g, cameraOffsetY);
            float screenX = x;
            float screenY = (float)(y + cameraOffsetY);
            g.DrawImage(PlatformDefault.Texture, new RectangleF(screenX, screenY, size.Width, size.Height));
        }

        public PlatformDefault(int x=0, int y=0) : base(x, y)
        {
        }
        
    }


}
