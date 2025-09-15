using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;

namespace Pong.Scenes;

public class onePlayer : Scene
{
    private const int VIRTUAL_WIDTH = 1920;
    private const int VIRTUAL_HEIGHT = 1080;

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
    private float paddleVelocity = 7;

    private Vector2 paddleAIPosition;
    private Vector2 paddleAIStartPosition;
    private float paddleAIVelocity = 7;
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

    private bool ballIsPaused = false;
    private float respawnTimer = 0f;
    private float respawnDelay = 1.0f;

    public onePlayer(GraphicsDevice graphics, SpriteBatch spriteBatch, ContentManager content, SpriteFont _points, SpriteFont _AIpoints, Texture2D _ballTexture, Texture2D _paddleTexture, Texture2D _middleLineTexture, Song _pointScored, Song _bounceOne, Song _bounceTwo)
    {
        _graphics = graphics;
        _spriteBatch = spriteBatch;
        _content = content;

        int screenWidth = VIRTUAL_WIDTH;
        int screenHeight = VIRTUAL_HEIGHT;

        ballTexture = _ballTexture;
        paddleTexture = _paddleTexture;
        middleLineTexture = _middleLineTexture;
        pointScored = _pointScored;
        bounceOne = _bounceOne;
        bounceTwo = _bounceTwo;

        int bW = 0;
        int bH = 0;
        if (ballTexture != null)
        {
            bW = ballTexture.Width;
            bH = ballTexture.Height;
        }

        ballStartPosition = new Vector2(screenWidth / 2f - bW / 2f, screenHeight / 2f - bH / 2f);
        ballPosition = new Vector2(ballStartPosition.X, ballStartPosition.Y);
        ballVelocity = new Vector2(800f, rng.Next(-200, 200));

        int pH = 0;
        int pW = 38;
        if (paddleTexture != null)
        {
            pH = paddleTexture.Height;
            pW = paddleTexture.Width;
        }

        paddlePosition = new Vector2(0f, screenHeight / 2f - pH / 2f);
        paddleStartPosition = new Vector2(paddlePosition.X, paddlePosition.Y);

        paddleAIPosition = new Vector2(screenWidth - pW, screenHeight / 2f - pH / 2f);
        paddleAIStartPosition = new Vector2(paddleAIPosition.X, paddleAIPosition.Y);

        points = _points;
        AIpoints = _AIpoints;

        float leftTextWidth = 0f;
        float rightTextWidth = 0f;
        if (points != null) leftTextWidth = points.MeasureString("3").X;
        if (AIpoints != null) rightTextWidth = AIpoints.MeasureString("3").X;

        fontPos = new Vector2(screenWidth / 2f - leftTextWidth - 150f, 100f);
        fontPosAI = new Vector2(screenWidth / 2f - rightTextWidth + 300f, 100f);

        LoadContent();
    }

    private void LoadContent()
    {
    }

    public bool GameOver { get; private set; } = false;

    public override void Update(GameTime gameTime)
    {
        KeyboardState keyboard = Keyboard.GetState();

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (ballIsPaused)
        {
            respawnTimer += dt;
            if (respawnTimer >= respawnDelay)
            {
                ballIsPaused = false;
                respawnTimer = 0f;
                ballVelocity = new Vector2(800, 0);
            }
        }

        ballPosition = new Vector2(ballPosition.X + ballVelocity.X * dt, ballPosition.Y + ballVelocity.Y * dt);

        int currentBallW = 0;
        int currentBallH = 0;
        if (ballTexture != null)
        {
            currentBallW = ballTexture.Width;
            currentBallH = ballTexture.Height;
        }

        float currentBallWf = currentBallW;
        float currentBallHf = currentBallH;

        if (ballPosition.X < 0f - (2f * currentBallWf))
        {
            playerLives = playerLives - 1;
            ResetPositions();
            if (pointScored != null) MediaPlayer.Play(pointScored);

            if (playerLives == 0)
            {
                gameWon = false;
                GameOver = true;
            }
        }
        else if (ballPosition.X > VIRTUAL_WIDTH + 2f * currentBallWf)
        {
            AILives = AILives - 1;
            ResetPositions();
            if (pointScored != null) MediaPlayer.Play(pointScored);

            if (AILives == 0)
            {
                gameWon = true;
                GameOver = true;
            }
        }

        if (ballPosition.Y < 0f)
        {
            ballPosition = new Vector2(ballPosition.X, 0f);
            ballVelocity = new Vector2(ballVelocity.X, ballVelocity.Y * -1f);
            PlayBounce();
        }
        else if (ballPosition.Y > VIRTUAL_HEIGHT - currentBallHf)
        {
            ballPosition = new Vector2(ballPosition.X, VIRTUAL_HEIGHT - currentBallHf);
            ballVelocity = new Vector2(ballVelocity.X, ballVelocity.Y * -1f);
            PlayBounce();
        }

        int paddleH = 0;
        if (paddleTexture != null)
        {
            paddleH = paddleTexture.Height;
        }
        float paddleHf = paddleH;

        if (keyboard.IsKeyDown(Keys.S))
        {
            if (paddlePosition.Y < VIRTUAL_HEIGHT - paddleHf)
            {
                paddlePosition = new Vector2(paddlePosition.X, paddlePosition.Y + paddleVelocity);
            }
        }
        if (keyboard.IsKeyDown(Keys.W))
        {
            if (paddlePosition.Y > 0f)
            {
                paddlePosition = new Vector2(paddlePosition.X, paddlePosition.Y - paddleVelocity);
            }
        }

        if (ballVelocity.X > 0f)
        {
            float paddleCenter = paddleAIPosition.Y + (paddleHf / 2f);
            if (paddleCenter < ballPosition.Y)
            {
                paddleAIPosition = new Vector2(paddleAIPosition.X, paddleAIPosition.Y + paddleAIVelocity);
            }
            else if (paddleAIPosition.Y > ballPosition.Y)
            {
                paddleAIPosition = new Vector2(paddleAIPosition.X, paddleAIPosition.Y - paddleAIVelocity);
            }
        }

        float aiClampMax = VIRTUAL_HEIGHT - paddleHf;
        paddleAIPosition = new Vector2(paddleAIPosition.X, Math.Clamp(paddleAIPosition.Y, 0f, aiClampMax));

        int pW = 0;
        if (paddleTexture != null)
        {
            pW = paddleTexture.Width;
        }

        Rectangle balkAI = new Rectangle((int)paddleAIPosition.X, (int)paddleAIPosition.Y, pW, paddleH);
        Rectangle balk = new Rectangle((int)paddlePosition.X, (int)paddlePosition.Y, pW, paddleH);
        Rectangle ballRect = new Rectangle((int)ballPosition.X, (int)ballPosition.Y, currentBallW, currentBallH);

        if (balk.Intersects(ballRect) && balk.X + paddleTexture.Width >= ballPosition.X || balkAI.Intersects(ballRect) && balkAI.X <= ballPosition.X + ballTexture.Width)
        {
            ballVelocity = new Vector2(ballVelocity.X * -1, ballVelocity.Y + rng.Next(-250, 250));
            PlayBounce();
        }

        ballVelocity = new Vector2(ballVelocity.X * gameSpeeder, ballVelocity.Y * gameSpeeder);
        paddleAIVelocity = paddleAIVelocity * gameSpeeder;
        paddleVelocity = paddleVelocity * gameSpeeder;
    }

    private void ResetPositions()
    {
        int cw = 0;
        int ch = 0;
        if (ballTexture != null)
        {
            cw = ballTexture.Width;
            ch = ballTexture.Height;
        }

        float cwf = cw;
        float chf = ch;
        ballPosition = new Vector2(VIRTUAL_WIDTH / 2f - cwf / 2f, VIRTUAL_HEIGHT / 2f - chf / 2f);

        ballVelocity = Vector2.Zero;
        ballIsPaused = true;
        respawnTimer = 0f;

        paddlePosition = new Vector2(paddleStartPosition.X, paddleStartPosition.Y);
        paddleAIPosition = new Vector2(paddleAIStartPosition.X, paddleAIStartPosition.Y);
    }

    private void PlayBounce()
    {
        int choice = rng.Next(1, 3);
        if (choice == 1)
        {
            if (bounceOne != null) MediaPlayer.Play(bounceOne);
        }
        else
        {
            if (bounceTwo != null) MediaPlayer.Play(bounceTwo);
        }
    }

    public override void Draw(GameTime gameTime)
    {
        int midW = 0;
        if (middleLineTexture != null)
        {
            midW = middleLineTexture.Width;
        }

        _spriteBatch.Draw(middleLineTexture, new Vector2(VIRTUAL_WIDTH / 2f - midW / 2f, 0f), Color.Gray);
        _spriteBatch.Draw(paddleTexture, paddlePosition, Color.White);
        _spriteBatch.Draw(paddleTexture, paddleAIPosition, Color.White);

        if (points != null)
            _spriteBatch.DrawString(points, playerLives.ToString(), fontPos, Color.Gray);
        if (AIpoints != null)
            _spriteBatch.DrawString(AIpoints, AILives.ToString(), fontPosAI, Color.Gray);

        _spriteBatch.Draw(ballTexture, ballPosition, Color.White);
    }
}
