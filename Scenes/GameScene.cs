using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;

namespace Pong.Scenes;

public class GameScene : Scene
{
    private GraphicsDevice _graphics;
    private SpriteBatch _spriteBatch;
    private ContentManager _content;

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
    private float paddleAIVelocity = 5;
    private Texture2D middleLineTexture;
    private float gameSpeeder = 1.0001f;

    private SpriteFont points;
    private SpriteFont AIpoints;
    private Vector2 fontPos;
    private Vector2 fontPosAI;
    private int playerLives = 3;
    private int AILives = 3;

    private Song pointScored;
    private Song bounceOne;
    private Song bounceTwo;

    private Random rng = new Random();

    public bool gameWon = false;

    public GameScene(GraphicsDevice graphics, SpriteBatch spriteBatch, ContentManager content)
    {
        _graphics = graphics;
        _spriteBatch = spriteBatch;
        _content = content;

        // Set up screen dimensions
        int screenWidth = _graphics.Viewport.Width;
        int screenHeight = _graphics.Viewport.Height;

        ballStartPosition = new Vector2(screenWidth / 2, screenHeight / 2);
        ballPosition = ballStartPosition;
        ballVelocity = new Vector2(800, rng.Next(-200, 200));

        paddlePosition = new Vector2(0, screenHeight / 2);
        paddleStartPosition = paddlePosition;

        paddleAIPosition = new Vector2(screenWidth - 38, screenHeight / 2); // 38 = paddle width, adjust as needed
        paddleAIStartPosition = paddleAIPosition;

        fontPos = new Vector2(screenWidth / 3, 100);
        fontPosAI = new Vector2(2 * screenWidth / 3, 100);

        LoadContent();
    }

    private void LoadContent()
    {
        ballTexture = _content.Load<Texture2D>("ball");
        paddleTexture = _content.Load<Texture2D>("balk");
        middleLineTexture = _content.Load<Texture2D>("middleLine");

        points = _content.Load<SpriteFont>("Score");
        AIpoints = _content.Load<SpriteFont>("Score");

        pointScored = _content.Load<Song>("pointScored");
        bounceOne = _content.Load<Song>("bounceOne");
        bounceTwo = _content.Load<Song>("bounceTwo");
    }

    public bool GameOver { get; private set; } = false;

    public override void Update(GameTime gameTime)
    {
        KeyboardState keyboard = Keyboard.GetState();

        ballPosition += ballVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

        // scoring
        if (ballPosition.X < 0 - (2 * ballTexture.Width))
        {
            playerLives--;
            ResetPositions();
            MediaPlayer.Play(pointScored);
            if (playerLives == 0)
            {
                // YOU LOSE
                gameWon = false;
                GameOver = true;
            }
        }
        else if (ballPosition.X > _graphics.Viewport.Width + 2 * ballTexture.Width)
        {
            AILives--;
            ResetPositions();
            MediaPlayer.Play(pointScored);
            if (AILives == 0)
            {
                // YOU WIN
                gameWon = true;
                GameOver = true;
            }
        }

        // bounce off walls
        if (ballPosition.Y < 0)
        {
            ballPosition.Y = 0;
            ballVelocity.Y *= -1;
            PlayBounce();
        }
        else if (ballPosition.Y > _graphics.Viewport.Height - ballTexture.Height)
        {
            ballPosition.Y = _graphics.Viewport.Height - ballTexture.Height;
            ballVelocity.Y *= -1;
            PlayBounce();
        }

        // player movement
        if (keyboard.IsKeyDown(Keys.S) &&
            paddlePosition.Y < _graphics.Viewport.Height - paddleTexture.Height)
            paddlePosition.Y += paddleVelocity;

        if (keyboard.IsKeyDown(Keys.W) && paddlePosition.Y > 0)
            paddlePosition.Y -= paddleVelocity;

        // AI movement (only when ball is moving toward AI)
        if (ballVelocity.X > 0)
        {
            if (paddleAIPosition.Y + (paddleTexture.Height / 2) < ballPosition.Y)
                paddleAIPosition.Y += paddleAIVelocity;
            else if (paddleAIPosition.Y > ballPosition.Y)
                paddleAIPosition.Y -= paddleAIVelocity;
        }

        paddleAIPosition.Y = Math.Clamp(paddleAIPosition.Y, 0, _graphics.Viewport.Height - paddleTexture.Height);

        // collision
        Rectangle balkAI = new Rectangle((int)paddleAIPosition.X, (int)paddleAIPosition.Y, paddleTexture.Width, paddleTexture.Height);
        Rectangle balk = new Rectangle((int)paddlePosition.X, (int)paddlePosition.Y, paddleTexture.Width, paddleTexture.Height);
        Rectangle ball = new Rectangle((int)ballPosition.X, (int)ballPosition.Y, ballTexture.Width, ballTexture.Height);

        if (balk.Intersects(ball) || balkAI.Intersects(ball))
        {
            ballVelocity.X *= -1;
            ballVelocity.Y += rng.Next(-200, 200);
            PlayBounce();
        }

        // ball speeds up over time
        ballVelocity.Y *= gameSpeeder;
        ballVelocity.X *= gameSpeeder;
    }

    private void ResetPositions()
    {
        ballPosition = ballStartPosition;
        ballVelocity = new Vector2(800, rng.Next(-200, 200));
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

    public override void Draw(GameTime gameTime)
    {
        _graphics.Clear(Color.Black);

        _spriteBatch.Begin();
        _spriteBatch.Draw(middleLineTexture, new Vector2(_graphics.Viewport.Width / 2 - middleLineTexture.Width / 2, 0), Color.White);
        _spriteBatch.Draw(paddleTexture, paddlePosition, Color.White);
        _spriteBatch.Draw(paddleTexture, paddleAIPosition, Color.White);

        _spriteBatch.DrawString(points, playerLives.ToString(), fontPos, Color.Gray);
        _spriteBatch.DrawString(AIpoints, AILives.ToString(), fontPosAI, Color.Gray);
        _spriteBatch.Draw(ballTexture, ballPosition, Color.White);
        _spriteBatch.End();
    }
}
