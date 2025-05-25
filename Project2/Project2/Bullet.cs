using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceInvaders
{
    public class Bullet
    {
        public Texture2D Texture;
        public Vector2 Position;
        public float Speed;
        public bool IsActive = true;

        public int Width = 10;
        public int Height = 20;

        public Rectangle BoundingBox => new Rectangle((int)Position.X, (int)Position.Y, Width, Height);

        public Bullet(Texture2D texture, Vector2 startPosition, float speed)
        {
            Texture = texture;
            Position = startPosition;
            Speed = speed;
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Position.Y += Speed * dt;

            if (Position.Y < -Height || Position.Y > 800)
                IsActive = false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (IsActive)
                spriteBatch.Draw(Texture, new Rectangle((int)Position.X, (int)Position.Y, Width, Height), Color.White);
        }
    }
}
