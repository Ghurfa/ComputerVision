using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVPong
{
    class Paddle
    {
        public Rectangle Position;
        public object PositionLock;
        public Paddle(Point position, Point size)
        {
            Position = new Rectangle(position, size);
            PositionLock = new object();
        }
        public void Update(Ball ball)
        {
            if (ball.Position.Intersects(Position))
            {
                if(ball.Position.X < Position.X)
                {
                    ball.Velocity.X = -Math.Abs(ball.Velocity.X);
                }
                else
                {
                    ball.Velocity.X = Math.Abs(ball.Velocity.X);
                }
            }
        }
        public void SetY(int y)
        {
            Position.Y = y;
        }
    }
}
