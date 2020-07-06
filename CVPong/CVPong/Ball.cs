using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVPong
{
    class Ball
    {
        public Rectangle Position;
        public Point Velocity;
        public int Score;

        private Random random;
        private Point bounds;
        public Ball(Rectangle position, Point velocity, Point bounds)
        {
            Position = position;
            Velocity = velocity;
            this.bounds = bounds;
            random = new Random();
        }
        public void Update()
        {
            Position.Location += Velocity;
            if (Position.X < 0)
            {
                Velocity.X = Math.Abs(Velocity.X);
                Position.Location = new Point(random.Next(100, 200), random.Next(100, 200));
                Score++;
            }
            else if (Position.Y < 0)
            {
                Velocity.Y = Math.Abs(Velocity.Y);
            }
            else if (Position.Right > bounds.X)
            {
                Velocity.X = -Math.Abs(Velocity.X);
            }
            else if(Position.Bottom > bounds.Y)
            {
                Velocity.Y = -Math.Abs(Velocity.Y);
            }
        }
    }
}
