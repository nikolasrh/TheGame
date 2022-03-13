using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TheGame.MonoGameClient
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D _playerTexture;
        private Vector2 _playerTextureScale;
        private Vector2 _playerPosition;
        private double _playerSpeed;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _playerTexture = Content.Load<Texture2D>("player");
            _playerTextureScale = new Vector2(0.2f, 0.2f);
            _playerPosition = Vector2.Zero;
            _playerSpeed = 100;

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _playerPosition += GetPlayerPositionChange(gameTime.ElapsedGameTime);

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        private Vector2 GetPlayerPositionChange(TimeSpan elapsedGameTime)
        {
            var direction = Vector2.Zero;

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
                direction -= Vector2.UnitY;
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
                direction += Vector2.UnitY;
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
                direction -= Vector2.UnitX;
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
                direction += Vector2.UnitX;

            if (direction.Length() > 1)
            {
                direction.Normalize();
            }

            var distance = _playerSpeed * elapsedGameTime.TotalSeconds;
            return direction * (float)distance;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            _spriteBatch.Begin();

            _spriteBatch.Draw(
                texture: _playerTexture,
                position: _playerPosition,
                sourceRectangle: null,
                color: Color.Black,
                rotation: 0f,
                origin: Vector2.Zero,
                scale: _playerTextureScale,
                effects: SpriteEffects.None,
                layerDepth: 0f);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
