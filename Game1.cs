using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Pong;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D ballTexture;
    private Vector2 ballPosition;
    private Vector2 ballVelocity;
    private Vector2 ballOriginVelocity;
    private Vector2 ballStartPosition;
    private Texture2D paddleTexture;
    private Vector2 paddlePosition;
    private Vector2 paddleStartPosition;
    private float paddleVelocity = 5;
    private Vector2 paddleAIPosition;
    private Vector2 paddleAIStartPosition;
    private float paddleAIVelocity = 5;
    int Score = 0;
    int ScoreAI = 0;
    private Texture2D middleLineTexture;
    private float gameSpeeder = 1.0001f;
    private bool partyMode = false;
    SpriteFont points;
    SpriteFont AIpoints;
    Vector2 fontPos;
    Vector2 fontPosAI;
    Song pointScored;
    Song bounceOne;
    Song bounceTwo;

    private float ballSize = 1f;
    private float originalBallSize;
    private float paddleSize = 1f;
    private float originalPaddleSize;

    private float powerUpSpawnTimer = 0f;
    private float powerUpInterval = 5f;
    private bool spawnedPowerUp = false;
    private bool spawnPowerUp = false;
    private int intPowerupSpawned;
    private Texture2D ballGrow;
    private Texture2D balkGrow;
    private int powerUpAmount = 2;
    private Vector2 powerUpSpawnPoint;
    private Rectangle powerUp;

    // --- Pedro animation ---
    private Texture2D pedroSpriteSheet;
    private int pedroFrameWidth = 64;
    private int pedroFrameHeight = 64;
    private int pedroSheetColumns = 30;
    private int pedroTotalFrames = 880;
    private int pedroCurrentFrame = 0;
    private float pedroFrameTimer = 0f;
    private float pedroFrameDuration = 1f / 6f; // 6 fps

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _graphics.IsFullScreen = true;
    }

    protected override void Initialize()
    {
        _graphics.PreferredBackBufferHeight = 1080;
        _graphics.PreferredBackBufferWidth = 1920;
        _graphics.ApplyChanges();

        ballPosition = new Vector2(100, 100);
        ballVelocity = new Vector2(800, 0);
        ballOriginVelocity = ballVelocity;
        ballStartPosition = new Vector2(960, 450);

        paddlePosition = new Vector2(0, 500);
        paddleStartPosition = paddlePosition;

        paddleAIPosition = new Vector2(1882, 500);
        paddleAIStartPosition = paddleAIPosition;

        fontPos = new Vector2(600, 100);
        fontPosAI = new Vector2(1220, 100);

        originalBallSize = ballSize;
        originalPaddleSize = paddleSize;

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        ballTexture = Content.Load<Texture2D>("ball");
        paddleTexture = Content.Load<Texture2D>("balk");
        middleLineTexture = Content.Load<Texture2D>("middleLine");
        points = Content.Load<SpriteFont>("Score");
        AIpoints = Content.Load<SpriteFont>("Score");

        pointScored = Content.Load<Song>("pointScored");
        bounceOne = Content.Load<Song>("bounceOne");
        bounceTwo = Content.Load<Song>("bounceTwo");

        ballGrow = Content.Load<Texture2D>("ballGrow");
        balkGrow = Content.Load<Texture2D>("balkGrow");

        // load the pedro spritesheet
        pedroSpriteSheet = Content.Load<Texture2D>("pedro_pedro_pe");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (Keyboard.GetState().IsKeyDown(Keys.P))
            partyMode = !partyMode;

        // ball movement
        ballPosition += ballVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (ballPosition.X < 0 - (2 * ballTexture.Width))
        {
            ScoreAI++;
            ballVelocity = ballOriginVelocity;
            ballPosition = ballStartPosition;
            paddlePosition = paddleStartPosition;
            paddleAIPosition = paddleAIStartPosition;
            ballSize = originalBallSize;
            paddleSize = originalPaddleSize;
            MediaPlayer.Play(pointScored);
        }
        else if (ballPosition.X > _graphics.PreferredBackBufferWidth + 2 * ballTexture.Width)
        {
            Score++;
            ballVelocity = ballOriginVelocity;
            ballPosition = ballStartPosition;
            paddlePosition = paddleStartPosition;
            paddleAIPosition = paddleAIStartPosition;
            ballSize = originalBallSize;
            paddleSize = originalPaddleSize;
            MediaPlayer.Play(pointScored);
        }

        if (ballPosition.Y < 0)
        {
            ballPosition.Y = 0;
            ballVelocity.Y *= -1;
            Random s = new Random();
            if (s.Next(1, 3) == 1)
                MediaPlayer.Play(bounceOne);
            else
                MediaPlayer.Play(bounceTwo);
        }
        else if (ballPosition.Y > _graphics.PreferredBackBufferHeight - ballTexture.Height)
        {
            ballPosition.Y = _graphics.PreferredBackBufferHeight - ballTexture.Height;
            ballVelocity.Y *= -1;
            Random s = new Random();
            if (s.Next(1, 3) == 1)
                MediaPlayer.Play(bounceOne);
            else
                MediaPlayer.Play(bounceTwo);
        }

        if (Keyboard.GetState().IsKeyDown(Keys.S) &&
            paddlePosition.Y < _graphics.PreferredBackBufferHeight - paddleTexture.Height)
            paddlePosition.Y += paddleVelocity;

        if (Keyboard.GetState().IsKeyDown(Keys.W) && paddlePosition.Y > 0)
            paddlePosition.Y -= paddleVelocity;

        if (paddleAIPosition.Y + (paddleTexture.Height / 2) < ballPosition.Y && ballVelocity.X > 0)
            paddleAIPosition.Y += paddleAIVelocity;
        else if (paddleAIPosition.Y > ballPosition.Y && ballVelocity.X > 0)
            paddleAIPosition.Y -= paddleAIVelocity;

        if (paddleAIPosition.Y < 0)
            paddleAIPosition.Y = 0;
        else if (paddleAIPosition.Y > _graphics.PreferredBackBufferHeight - paddleTexture.Height)
            paddleAIPosition.Y = _graphics.PreferredBackBufferHeight - paddleTexture.Height;

        Rectangle balkAI = new Rectangle((int)paddleAIPosition.X, (int)paddleAIPosition.Y, paddleTexture.Width, paddleTexture.Height);
        Rectangle balk = new Rectangle((int)paddlePosition.X, (int)paddlePosition.Y, paddleTexture.Width, paddleTexture.Height);
        Rectangle ball = new Rectangle((int)ballPosition.X, (int)ballPosition.Y, ballTexture.Width, ballTexture.Height);

        if (balk.Intersects(ball))
        {
            ballVelocity.X *= -1;
            Random r = new Random();
            ballVelocity.Y += r.Next(-150, 150);
            if (new Random().Next(1, 3) == 1)
                MediaPlayer.Play(bounceOne);
            else
                MediaPlayer.Play(bounceTwo);
        }
        else if (balkAI.Intersects(ball))
        {
            ballVelocity.X *= -1;
            Random r = new Random();
            ballVelocity.Y += r.Next(-150, 150);
            if (new Random().Next(1, 3) == 1)
                MediaPlayer.Play(bounceOne);
            else
                MediaPlayer.Play(bounceTwo);
        }

        if (ball.Intersects(powerUp))
        {
            ballSize = 2f;
            spawnedPowerUp = false;
        }

        ballVelocity.Y *= gameSpeeder;
        ballVelocity.X *= gameSpeeder;

        paddleAIVelocity *= gameSpeeder;
        paddleVelocity *= gameSpeeder;

        powerUpSpawnTimer++;
        if (powerUpSpawnTimer == powerUpInterval && spawnedPowerUp == false)
        {
            powerUpSpawnTimer = 0f;
            Random p = new Random();
            powerUpSpawnPoint = new Vector2(p.Next(200, 1000), p.Next(200, 800));
            spawnPowerUp = true;
            spawnedPowerUp = true;
        }

        // --- update pedro animation ---
        pedroFrameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (pedroFrameTimer >= pedroFrameDuration)
        {
            pedroCurrentFrame++;
            if (pedroCurrentFrame >= pedroTotalFrames)
                pedroCurrentFrame = 0;
            pedroFrameTimer -= pedroFrameDuration;
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        if (partyMode)
        {
            Random r = new Random();
            GraphicsDevice.Clear(new Color(r.Next(256), r.Next(256), r.Next(256)));
        }
        else
        {
            GraphicsDevice.Clear(Color.Black);
        }

        _spriteBatch.Begin();
        _spriteBatch.Draw(middleLineTexture, new Vector2(960 - middleLineTexture.Width / 2, 0), Color.White);
        _spriteBatch.Draw(paddleTexture, paddlePosition, Color.White);
        _spriteBatch.Draw(paddleTexture, paddleAIPosition, Color.White);

        _spriteBatch.DrawString(points, Score.ToString(), fontPos, Color.Gray);
        _spriteBatch.DrawString(points, ScoreAI.ToString(), fontPosAI, Color.Gray);

        // --- draw pedro animation with transparency ---
        int column = pedroCurrentFrame % pedroSheetColumns;
        int row = pedroCurrentFrame / pedroSheetColumns;
        Rectangle source = new Rectangle(column * pedroFrameWidth, row * pedroFrameHeight, pedroFrameWidth, pedroFrameHeight);
        Rectangle dest = new Rectangle((int)ballPosition.X, (int)ballPosition.Y, (int)(pedroFrameWidth * ballSize), (int)(pedroFrameHeight * ballSize));

        _spriteBatch.End(); // close the normal batch

        // new batch just for pedro with correct blending
        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
        _spriteBatch.Draw(pedroSpriteSheet, dest, source, Color.White);
        _spriteBatch.End();

        // draw powerup
        _spriteBatch.Begin();
        if (spawnPowerUp == true)
        {
            Random q = new Random();
            intPowerupSpawned = q.Next(1, powerUpAmount + 1);
            if (intPowerupSpawned == 1)
            {
                _spriteBatch.Draw(ballGrow, powerUpSpawnPoint, Color.White);
            }
            else if (intPowerupSpawned == 2)
            {
                _spriteBatch.Draw(balkGrow, powerUpSpawnPoint, Color.White);
            }
            powerUp = new Rectangle((int)powerUpSpawnPoint.X, (int)powerUpSpawnPoint.Y, 64, 64);
            spawnPowerUp = false;
        }
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
