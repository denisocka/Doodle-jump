using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Doodle_Jump
{
    public static class ImageCreator
    {
        public static Image CreateImage(Image source, Point from, Size size)
        {
            Bitmap result = new Bitmap(size.Width, size.Height);

            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(
                    source,
                    new Rectangle(0, 0, size.Width, size.Height),
                    new Rectangle(from.X, from.Y, size.Width, size.Height),
                    GraphicsUnit.Pixel
                );
            }

            return result;
        }
        public static Image ResizeImage(Image source, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(result))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor; // для пиксель-арта
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;

                g.DrawImage(source, 0, 0, width, height);
            }

            return result;
        }

    }

    public class Sprite
    {
        int loopsTarget = 1;
        int loopsDone = 0;
        public List<Image> frames;
        public int Speed;
        int CountFrames;
        public int FrameScore;

        public bool IsAnimationPlaying;

        public Sprite Clone()
        {
            return new Sprite(
                frames,
                Speed
            )
            {
                FrameScore = 0,
                timer = 0,
                IsAnimationPlaying = false
            };
        }

        public Sprite(Image source, int speed, int count, int from, Size sizeFrameCount)
        {
            Size sizeFrame = new Size();
            sizeFrame.Width = source.Width / sizeFrameCount.Width;
            sizeFrame.Height = source.Height / sizeFrameCount.Height;

            var resulr = new List<Image>();

            for (int i = 0; i< sizeFrameCount.Height; i++)
                for (int j = 0; j  < sizeFrameCount.Width; j++)
                {
                    int index = i * sizeFrameCount.Height + j;

                    if (index >= from &&  index < count)
                    {
                        int x = j * sizeFrame.Width;
                        int y = i * sizeFrame.Height;

                        resulr.Add(ImageCreator.CreateImage(source, new Point(x, y), sizeFrame));
                    }
                }

            frames = resulr;
            Speed = speed;
            CountFrames = count;
        }

        public void Play(int loops = 1)
        {
            loopsTarget = loops;
            loopsDone = 0;

            FrameScore = 0;
            timer = 0;
            IsAnimationPlaying = true;
        }

        public Sprite(List<Image> sources, int speed)
        {
            frames = sources;
            Speed = speed;
            CountFrames = sources.Count;
        }

        public Image GetFrame()
        {
            return frames[FrameScore];
        }


        public int timer;

        public void Update()
        {
            if (!IsAnimationPlaying)
                return;

            if (timer >= Speed)
            {
                timer = 0;

                if (FrameScore < CountFrames - 1)
                {
                    FrameScore++;
                }
                else
                {
                    FrameScore = 0;
                    loopsDone++;

                    if (loopsDone >= loopsTarget)
                    {
                        IsAnimationPlaying = false;
                    }
                }
            }

            timer++;
        }
    }

    public class TextureManager
    {
        public List<Sprite> sprites = new List<Sprite>();
        List<Image> textures = new List<Image>();

        public int state;

        public TextureManager(Image image)
        {
            textures.Add(image);
            state = 0;
        }

        public TextureManager Clone()
        {
            TextureManager clone = new TextureManager(textures[0]);

            clone.state = state;

            clone.textures = new List<Image>(textures);

            clone.sprites = sprites
                .Select(s => s.Clone())
                .ToList();

            return clone;
        }

        public void AddSprate(Image source, int speed, int count, int from, Size sizeFrameCount)
        {
            sprites.Add(new Sprite(source,speed, count, from, sizeFrameCount));
        }

        public void AddSprate(Sprite sprite)
        {
            sprites.Add(sprite);
        }

        public void AddSprate(List<Image> sources, int speed)
        {
            sprites.Add(new Sprite(sources, speed));
        }

        public void RotateSprite(Sprite sprite)
        {
            var newFrames = sprite.frames.ToList();
            foreach (var frame in sprite.frames)
            {
                Image newFrame = (Image) frame.Clone();
                newFrame.RotateFlip(RotateFlipType.RotateNoneFlipY);
                newFrames.Add(newFrame);
            }

            AddSprate(newFrames, sprite.Speed);
        }

        public void AddImage (Image source) => textures.Add(source);

        public Image GetImage()
        {
            foreach (var sprite in sprites)
                if (sprite.IsAnimationPlaying)
                    return sprite.GetFrame();
            return textures[state];
        }

        public void GetState(int state) => this.state = state;

        public void PlaySprite(int spriteNumber, int loops = 1)
        {
            foreach (var s in sprites)
                s.IsAnimationPlaying = false;

            sprites[spriteNumber].Play(loops);
        }


        public void Update()
        {
            for (int i = 0; i < sprites.Count; i++)
                if (sprites[i].IsAnimationPlaying)
                {
                    sprites[i].Update();
                    break;
                }
        }

    }

}
