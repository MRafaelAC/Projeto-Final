using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;

namespace SpaceInvaders
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Texture2D playerTexture;
        Vector2 playerPosition;
        float playerSpeed = 300f;

        List<Bullet> bullets = new List<Bullet>();
        List<Bullet> enemyBullets = new List<Bullet>();
        List<Enemy> enemies = new List<Enemy>();

        Texture2D bulletTexture;
        Texture2D enemyTexture;

        SpriteFont font;

        float shootCooldown = 0.25f;
        float shootTimer = 0f;

        int playerWidth = 40;
        int playerHeight = 40;

        int bulletWidth = 10;
        int bulletHeight = 20;

        int score = 0;
        int highScore = 0;
        string highScoreFile = "highscore.txt";

        Random random = new Random();

        float enemyShootCooldown = 2.5f;
        float enemyShootTimer = 0f;
        float bulletSpeed = 200f;

        float enemyMoveTimer = 0f;
        float enemyMoveInterval = 0.5f;
        int enemyMoveDirection = 1;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
            graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            playerPosition = new Vector2(
                (GraphicsDevice.Viewport.Width - playerWidth) / 2,
                GraphicsDevice.Viewport.Height - playerHeight - 10
            );

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>("font");
            playerTexture = Content.Load<Texture2D>("player");
            bulletTexture = Content.Load<Texture2D>("bullet");
            enemyTexture = Content.Load<Texture2D>("enemy");

            LoadHighScore();
            SpawnEnemies(3, 10);
        }

        private void LoadHighScore()
        {
            if (File.Exists(highScoreFile))
            {
                string text = File.ReadAllText(highScoreFile);
                int.TryParse(text, out highScore);
            }
        }

        private void SaveHighScore()
        {
            File.WriteAllText(highScoreFile, highScore.ToString());
        }

        private void SpawnEnemies(int rows, int columns)
        {
            enemies.Clear();
            int spacingX = 60;
            int spacingY = 50;
            int startX = 100;
            int startY = 50;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    Vector2 enemyPosition = new Vector2(startX + col * spacingX, startY + row * spacingY);
                    enemies.Add(new Enemy(enemyTexture, enemyPosition));
                }
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var keyboardState = Keyboard.GetState();

            // Movimento jogador
            if (keyboardState.IsKeyDown(Keys.Left))
                playerPosition.X -= playerSpeed * dt;
            if (keyboardState.IsKeyDown(Keys.Right))
                playerPosition.X += playerSpeed * dt;

            playerPosition.X = MathHelper.Clamp(playerPosition.X, 0, GraphicsDevice.Viewport.Width - playerWidth);

            // Disparo do jogador
            shootTimer += dt;
            if (keyboardState.IsKeyDown(Keys.Space) && shootTimer >= shootCooldown)
            {
                Vector2 bulletStartPos = new Vector2(playerPosition.X + playerWidth / 2 - bulletWidth / 2, playerPosition.Y);
                bullets.Add(new Bullet(bulletTexture, bulletStartPos, -500f));
                shootTimer = 0f;
            }

            // Atualizar balas do jogador
            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                bullets[i].Update(gameTime);
                if (!bullets[i].IsActive)
                    bullets.RemoveAt(i);
            }

            // Atualizar balas dos inimigos
            for (int i = enemyBullets.Count - 1; i >= 0; i--)
            {
                enemyBullets[i].Update(gameTime);
                if (!enemyBullets[i].IsActive)
                    enemyBullets.RemoveAt(i);
            }

            // Movimento dos inimigos (lado a lado)
            enemyMoveTimer += dt;
            if (enemyMoveTimer >= enemyMoveInterval)
            {
                foreach (var enemy in enemies)
                    enemy.Position.X += 10 * enemyMoveDirection;

                enemyMoveTimer = 0f;

                // Muda direção se atingir bordas
                foreach (var enemy in enemies)
                {
                    if (enemy.Position.X <= 10 || enemy.Position.X >= GraphicsDevice.Viewport.Width - enemy.Width - 10)
                    {
                        enemyMoveDirection *= -1;
                        break;
                    }
                }
            }

            // Disparo dos inimigos
            enemyShootTimer += dt;
            if (enemyShootTimer >= enemyShootCooldown)
            {
                if (enemies.Count > 0)
                {
                    Enemy shooter = enemies[random.Next(enemies.Count)];
                    Vector2 bulletPos = new Vector2(shooter.Position.X + shooter.Width / 2 - bulletWidth / 2, shooter.Position.Y + shooter.Height);
                    enemyBullets.Add(new Bullet(bulletTexture, bulletPos, bulletSpeed));
                }

                enemyShootTimer = 0f;
            }

            // Atualizar inimigos
            foreach (var enemy in enemies)
                enemy.Update(gameTime);

            // Colisão bala do jogador vs inimigo
            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                for (int j = enemies.Count - 1; j >= 0; j--)
                {
                    if (bullets[i].BoundingBox.Intersects(enemies[j].BoundingBox))
                    {
                        bullets[i].IsActive = false;
                        enemies[j].IsActive = false;
                        score += 100;

                        bullets.RemoveAt(i);
                        enemies.RemoveAt(j);
                        break;
                    }
                }
            }

            // Colisão bala do inimigo vs jogador
            foreach (var bullet in enemyBullets)
            {
                Rectangle bulletBox = bullet.BoundingBox;
                Rectangle playerBox = new Rectangle((int)playerPosition.X, (int)playerPosition.Y, playerWidth, playerHeight);

                if (bulletBox.Intersects(playerBox))
                {
                    if (score > highScore)
                    {
                        highScore = score;
                        SaveHighScore();
                    }

                    score = 0;
                    bullets.Clear();
                    enemyBullets.Clear();
                    enemyShootCooldown = 2.5f;
                    bulletSpeed = 200f;
                    SpawnEnemies(3, 10);
                    return;
                }
            }

            // Nova onda de inimigos
            if (enemies.Count == 0)
            {
                enemyShootCooldown = Math.Max(0.5f, enemyShootCooldown - 0.2f); // disparam mais rápido
                bulletSpeed += 20f; // balas mais rápidas
                SpawnEnemies(3, 10);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            // Jogador
            spriteBatch.Draw(playerTexture, new Rectangle(
                (int)playerPosition.X,
                (int)playerPosition.Y,
                playerWidth,
                playerHeight), Color.White);

            // Balas
            foreach (var bullet in bullets)
                bullet.Draw(spriteBatch);
            foreach (var bullet in enemyBullets)
                bullet.Draw(spriteBatch);

            // Inimigos
            foreach (var enemy in enemies)
                enemy.Draw(spriteBatch);

            // Pontuação
            spriteBatch.DrawString(font, $"Score: {score}", new Vector2(10, 10), Color.White);
            spriteBatch.DrawString(font, $"High Score: {highScore}", new Vector2(10, 30), Color.Yellow);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
