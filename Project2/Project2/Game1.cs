using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;  // Para música
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

        Texture2D bulletTexture;
        Texture2D enemyTexture;

        SpriteFont font;

        List<Bullet> playerBullets = new List<Bullet>();
        List<Bullet> enemyBullets = new List<Bullet>();
        List<Enemy> enemies = new List<Enemy>();

        SoundEffect shootSound;
        SoundEffect enemyDeathSound;
        SoundEffect playerDeathSound;

        Song backgroundMusic;  // Música de fundo

        int playerWidth = 40;
        int playerHeight = 40;

        int score = 0;
        int highScore = 0;

        float shootCooldown = 0.25f;
        float shootTimer = 0f;

        float enemyShootInterval = 2f;
        float enemyShootTimer = 0f;

        float enemySpeed = 30f;
        float enemyDirection = 1;

        Random random = new Random();

        string highScorePath = "highscore.txt";

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
        }

        protected override void Initialize()
        {
            playerPosition = new Vector2(
                (GraphicsDevice.Viewport.Width - playerWidth) / 2,
                GraphicsDevice.Viewport.Height - playerHeight - 10
            );

            if (File.Exists(highScorePath))
                int.TryParse(File.ReadAllText(highScorePath), out highScore);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>("font");
            playerTexture = Content.Load<Texture2D>("player");
            bulletTexture = Content.Load<Texture2D>("bullet");
            enemyTexture = Content.Load<Texture2D>("enemy");

            shootSound = Content.Load<SoundEffect>("tiro");
            enemyDeathSound = Content.Load<SoundEffect>("inimigo");
            playerDeathSound = Content.Load<SoundEffect>("jogador");

            // Carregar e tocar música de fundo
            backgroundMusic = Content.Load<Song>("musicaf");
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 0.3f;
            MediaPlayer.Play(backgroundMusic);

            SpawnEnemies();
        }

        private void SpawnEnemies()
        {
            enemies.Clear();
            int rows = 3;
            int columns = 8;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    Vector2 pos = new Vector2(100 + col * 60, 50 + row * 50);
                    enemies.Add(new Enemy(enemyTexture, pos));
                }
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var keyboard = Keyboard.GetState();

            // Movimento do jogador
            if (keyboard.IsKeyDown(Keys.Left))
                playerPosition.X -= playerSpeed * dt;
            if (keyboard.IsKeyDown(Keys.Right))
                playerPosition.X += playerSpeed * dt;

            playerPosition.X = MathHelper.Clamp(playerPosition.X, 0, GraphicsDevice.Viewport.Width - playerWidth);

            // Disparo do jogador
            shootTimer += dt;
            if (keyboard.IsKeyDown(Keys.Space) && shootTimer >= shootCooldown)
            {
                Vector2 bulletPos = new Vector2(playerPosition.X + playerWidth / 2 - 5, playerPosition.Y);
                playerBullets.Add(new Bullet(bulletTexture, bulletPos, -500f));
                shootSound.Play();
                shootTimer = 0f;
            }

            // Atualizar balas
            foreach (var bullet in playerBullets)
                bullet.Update(gameTime);
            playerBullets.RemoveAll(b => !b.IsActive);

            foreach (var bullet in enemyBullets)
                bullet.Update(gameTime);
            enemyBullets.RemoveAll(b => !b.IsActive);

            // Movimento lateral dos inimigos
            foreach (var enemy in enemies)
            {
                enemy.Position.X += enemyDirection * enemySpeed * dt;
            }

            // Inverter direção se bater em borda
            foreach (var enemy in enemies)
            {
                if (enemy.Position.X <= 0 || enemy.Position.X + enemy.Width >= GraphicsDevice.Viewport.Width)
                {
                    enemyDirection *= -1;
                    break;
                }
            }

            // Disparo dos inimigos
            enemyShootTimer += dt;
            if (enemyShootTimer >= enemyShootInterval && enemies.Count > 0)
            {
                enemyShootTimer = 0f;
                var shooter = enemies[random.Next(enemies.Count)];
                Vector2 bulletPos = new Vector2(shooter.Position.X + shooter.Width / 2 - 5, shooter.Position.Y + shooter.Height);
                enemyBullets.Add(new Bullet(bulletTexture, bulletPos, 250f));
            }

            // Colisão: jogador atira inimigo
            for (int i = playerBullets.Count - 1; i >= 0; i--)
            {
                for (int j = enemies.Count - 1; j >= 0; j--)
                {
                    if (playerBullets[i].BoundingBox.Intersects(enemies[j].BoundingBox))
                    {
                        playerBullets.RemoveAt(i);
                        enemies.RemoveAt(j);
                        score += 100;
                        enemyDeathSound.Play();
                        break;
                    }
                }
            }

            // Colisão: inimigo acerta jogador
            Rectangle playerRect = new Rectangle((int)playerPosition.X, (int)playerPosition.Y, playerWidth, playerHeight);
            foreach (var bullet in enemyBullets)
            {
                if (bullet.BoundingBox.Intersects(playerRect))
                {
                    playerDeathSound.Play();
                    if (score > highScore)
                    {
                        highScore = score;
                        File.WriteAllText(highScorePath, highScore.ToString());
                    }

                    score = 0;
                    playerBullets.Clear();
                    enemyBullets.Clear();
                    enemyShootInterval = Math.Max(0.5f, enemyShootInterval - 0.2f); // Aumentar dificuldade
                    SpawnEnemies();
                    break;
                }
            }

            // Se eliminar todos os inimigos
            if (enemies.Count == 0)
            {
                enemyShootInterval = Math.Max(0.5f, enemyShootInterval - 0.2f); // Mais difícil
                SpawnEnemies();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            // Jogador
            spriteBatch.Draw(playerTexture, new Rectangle((int)playerPosition.X, (int)playerPosition.Y, playerWidth, playerHeight), Color.White);

            // Balas
            foreach (var bullet in playerBullets)
                bullet.Draw(spriteBatch);
            foreach (var bullet in enemyBullets)
                bullet.Draw(spriteBatch);

            // Inimigos
            foreach (var enemy in enemies)
                enemy.Draw(spriteBatch);

            // Pontuação
            spriteBatch.DrawString(font, $"Score: {score}", new Vector2(10, 10), Color.White);
            spriteBatch.DrawString(font, $"High Score: {highScore}", new Vector2(10, 40), Color.Yellow);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
