using System;
using Microsoft.Xna.Framework;
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
    private Vector2 ballStartPosition;

    private Texture2D paddleTexture;
    private Vector2 paddlePosition;
    private Vector2 paddleStartPosition;
    private float paddleVelocity = 5;

    private Vector2 paddleAIPosition;
    private Vector2 paddleAIStartPosition;
    private float paddleAIVelocity = 5; // stays constant now

    private int Score = 0;
    private int ScoreAI = 0;

    private Texture2D middleLineTexture;
    private float gameSpeeder = 1.0001f;

    private SpriteFont points;
    private SpriteFont AIpoints;
    private Vector2 fontPos;
    private Vector2 fontPosAI;

    private Song pointScored;
    private Song bounceOne;
    private Song bounceTwo;

    private Random rng = new Random();

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

        ballStartPosition = new Vector2(960, 450);
        ballPosition = ballStartPosition;
        ballVelocity = new Vector2(800, rng.Next(-200, 200));

        paddlePosition = new Vector2(0, 500);
        paddleStartPosition = paddlePosition;

        paddleAIPosition = new Vector2(1882, 500);
        paddleAIStartPosition = paddleAIPosition;

        fontPos = new Vector2(600, 100);
        fontPosAI = new Vector2(1220, 100);

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
    }

    protected override void Update(GameTime gameTime)
    {
        KeyboardState keyboard = Keyboard.GetState();

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            keyboard.IsKeyDown(Keys.Escape))
            Exit();

        ballPosition += ballVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

        // scoring
        if (ballPosition.X < 0 - (2 * ballTexture.Width))
        {
            ScoreAI++;
            ResetPositions();
            MediaPlayer.Play(pointScored);
        }
        else if (ballPosition.X > _graphics.PreferredBackBufferWidth + 2 * ballTexture.Width)
        {
            Score++;
            ResetPositions();
            MediaPlayer.Play(pointScored);
        }

        // bounce off walls
        if (ballPosition.Y < 0)
        {
            ballPosition.Y = 0;
            ballVelocity.Y *= -1;
            PlayBounce();
        }
        else if (ballPosition.Y > _graphics.PreferredBackBufferHeight - ballTexture.Height)
        {
            ballPosition.Y = _graphics.PreferredBackBufferHeight - ballTexture.Height;
            ballVelocity.Y *= -1;
            PlayBounce();
        }

        // player movement
        if (keyboard.IsKeyDown(Keys.S) &&
            paddlePosition.Y < _graphics.PreferredBackBufferHeight - paddleTexture.Height)
            paddlePosition.Y += paddleVelocity;

        if (keyboard.IsKeyDown(Keys.W) && paddlePosition.Y > 0)
            paddlePosition.Y -= paddleVelocity;

        // AI movement
        // If you want AI to move ALWAYS:
        // if (paddleAIPosition.Y + (paddleTexture.Height / 2) < ballPosition.Y)
        //     paddleAIPosition.Y += paddleAIVelocity;
        // else if (paddleAIPosition.Y > ballPosition.Y)
        //     paddleAIPosition.Y -= paddleAIVelocity;

        // If you want AI to move ONLY when ball is coming at it:
        
        if (ballVelocity.X > 0)
        {
            if (paddleAIPosition.Y + (paddleTexture.Height / 2) < ballPosition.Y)
                paddleAIPosition.Y += paddleAIVelocity;
            else if (paddleAIPosition.Y > ballPosition.Y)
                paddleAIPosition.Y -= paddleAIVelocity;
        }
        

        paddleAIPosition.Y = Math.Clamp(paddleAIPosition.Y, 0, _graphics.PreferredBackBufferHeight - paddleTexture.Height);

        // collision
        Rectangle balkAI = new Rectangle((int)paddleAIPosition.X, (int)paddleAIPosition.Y, paddleTexture.Width, paddleTexture.Height);
        Rectangle balk = new Rectangle((int)paddlePosition.X, (int)paddlePosition.Y, paddleTexture.Width, paddleTexture.Height);
        Rectangle ball = new Rectangle((int)ballPosition.X, (int)ballPosition.Y, ballTexture.Width, ballTexture.Height);

        if (balk.Intersects(ball) || balkAI.Intersects(ball))
        {
            ballVelocity.X *= -1;
            ballVelocity.Y += rng.Next(-150, 150);
            PlayBounce();
        }

        // ball speeds up over time
        ballVelocity.Y *= gameSpeeder;
        ballVelocity.X *= gameSpeeder;

        base.Update(gameTime);
    }

    private void ResetPositions()
    {
        ballPosition = ballStartPosition;
        ballVelocity = new Vector2(800, rng.Next(-200, 200)); // random Y each reset
        paddlePosition = paddleStartPosition;
        paddleAIPosition = paddleAIStartPosition;
    }

    private void PlayBounce()
    {
        if (rng.Next(1, 3) == 1)
            MediaPlayer.Play(bounceOne);
        else
            MediaPlayer.Play(bounceTwo);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin();
        _spriteBatch.Draw(middleLineTexture, new Vector2(960 - middleLineTexture.Width / 2, 0), Color.White);
        _spriteBatch.Draw(paddleTexture, paddlePosition, Color.White);
        _spriteBatch.Draw(paddleTexture, paddleAIPosition, Color.White);

        _spriteBatch.DrawString(points, Score.ToString(), fontPos, Color.Gray);
        _spriteBatch.DrawString(AIpoints, ScoreAI.ToString(), fontPosAI, Color.Gray);
        _spriteBatch.Draw(ballTexture, ballPosition, Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
