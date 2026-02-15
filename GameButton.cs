using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doodle_Jump
{
    class GameButton
    {
        public Rectangle Bounds;
        public Image Icon;
        public Action OnClick;

        public void Draw(Graphics g)
        {
            g.DrawImage(Icon, Bounds);
        }

        public void HandleMouseDown(Point mousePosition)
        {
            if (Bounds.Contains(mousePosition))
            {
                OnClick?.Invoke();
            }
        }
    }

    
}

