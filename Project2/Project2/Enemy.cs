using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceInvaders
{
    public class Enemy
    {
        public Texture2D Texture;
        public Vector2 StartPosition;
        public Vector2 Position;
        public bool IsActive = true;

        public int Width = 30;
        public int Height = 30;

        private float moveAmplitude = 10f;
        private float moveSpeed = 2f;
        private float moveTimer = 0f;

        public Rectangle BoundingBox => new Rectangle((int)Position.X, (int)Position.Y, Width, Height);

        public Enemy(Texture2D texture, Vector2 position)
        {
            Texture = texture;
            StartPosition = position;
            Position = position;
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            moveTimer += dt;
            float offsetX = (float)System.Math.Sin(moveTimer * moveSpeed) * moveAmplitude;
            Position.X = StartPosition.X + offsetX;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (IsActive && Texture != null)
                spriteBatch.Draw(Texture, new Rectangle((int)Position.X, (int)Position.Y, Width, Height), Color.White);
        }
    }
}
