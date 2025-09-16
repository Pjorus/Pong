using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;

namespace Pong.Scenes;

public class twoPlayer : Scene
{
    // Virtual resolution constants (kept the same)
    private const int VIRTUAL_WIDTH = 1920;
    private const int VIRTUAL_HEIGHT = 1080;

    // Graphics and content
    private GraphicsDevice _graphics;
    private SpriteBatch _spriteBatch;
    private ContentManager _content;

    // Ball
    private Texture2D ballTexture;
    private Vector2 ballPosition;
    private Vector2 ballVelocity;
    private Vector2 ballStartPosition;

    // Left paddle
    private Texture2D leftPaddleTexture;
    private Vector2 leftPaddlePosition;
    private Vector2 leftPaddleStartPosition;
    private float leftPaddleVelocity = 7;

    // Right paddle
    private Vector2 rightPaddlePosition;
    private Vector2 rightPaddleStartPosition;
    private float rightPaddleVelocity = 7;

    // Middle line and speed
    private Texture2D middleLineTexture;
    private float gameSpeeder = 1.0001f;

    // Fonts and score
    private SpriteFont leftPoints;
    private SpriteFont rightPoints;
    private Vector2 leftFontPos;
    private Vector2 rightFontPos;
    private int leftLives = 3;
    private int rightLives = 3;

    // Sounds
    private Song pointScored;
    private Song bounceOne;
    private Song bounceTwo;

    // RNG
    private Random rng = new Random();

    // Game state
    public bool twoPlayers = true;
    public int winner = 0; // 1 = left player wins, 2 = right player wins

    // --- Respawn / delay fields (new) ---
    private bool ballIsPaused = false;
    private float respawnTimer = 0f;
    private float respawnDelay = 1.0f; // seconds to wait after a point
    public bool GameOver = false;

    // Constructor - lots of assignments here, keep same param names
    public twoPlayer(GraphicsDevice graphics, SpriteBatch spriteBatch, ContentManager content, SpriteFont _leftPoints, SpriteFont _rightPoints, Texture2D _ballTexture, Texture2D _leftPaddleTexture, Texture2D _middleLineTexture, Song _pointScored, Song _bounceOne, Song _bounceTwo)
    {
        // store references
        _graphics = graphics;
        _spriteBatch = spriteBatch;
        _content = content;

        // virtual screen dims (use constants)
        int screenWidth = VIRTUAL_WIDTH;
        int screenHeight = VIRTUAL_HEIGHT;

        // store textures/sounds first so we can use their sizes below
        ballTexture = _ballTexture;
        leftPaddleTexture = _leftPaddleTexture;
        middleLineTexture = _middleLineTexture;

        // compute start positions carefully (take widths/heights into account)
        int bW = 0;
        int bH = 0;
        if (ballTexture != null)
        {
            bW = ballTexture.Width;
            bH = ballTexture.Height;
        }

        ballStartPosition = new Vector2((float)screenWidth / 2f - (float)bW / 2f, (float)screenHeight / 2f - (float)bH / 2f);
        ballPosition = new Vector2(ballStartPosition.X, ballStartPosition.Y);
        ballVelocity = new Vector2(800f, (float)rng.Next(-200, 200));

        int pW = 0;
        int pH = 0;
        if (leftPaddleTexture != null)
        {
            pW = leftPaddleTexture.Width;
            pH = leftPaddleTexture.Height;
        }

        leftPaddlePosition = new Vector2(0f, (float)screenHeight / 2f - (float)pH / 2f);
        leftPaddleStartPosition = new Vector2(leftPaddlePosition.X, leftPaddlePosition.Y);

        rightPaddlePosition = new Vector2((float)screenWidth - (float)(pW == 0 ? 38 : pW), (float)screenHeight / 2f - (float)pH / 2f);
        rightPaddleStartPosition = new Vector2(rightPaddlePosition.X, rightPaddlePosition.Y);

        // fonts
        leftPoints = _leftPoints;
        rightPoints = _rightPoints;

        // sounds
        pointScored = _pointScored;
        bounceOne = _bounceOne;
        bounceTwo = _bounceTwo;

        // compute font positions (student-style: do this in a clumsy way)
        float leftTextWidth = 0f;
        float rightTextWidth = 0f;
        if (leftPoints != null)
        {
            leftTextWidth = leftPoints.MeasureString(leftLives.ToString()).X;
        }
        if (rightPoints != null)
        {
            rightTextWidth = rightPoints.MeasureString(rightLives.ToString()).X;
        }

        leftFontPos = new Vector2((float)screenWidth / 2f - leftTextWidth / 2f - 200f, 100f);
        rightFontPos = new Vector2((float)screenWidth / 2f - rightTextWidth / 2f + 200f, 100f);

        // call loadcontent (empty, but keep for parity)
        LoadContent();
    }

    private void LoadContent()
    {
        // Intentionally left blank (placeholder for content loading)
    }

    public override void Update(GameTime gameTime)
    {
        // get keyboard state
        KeyboardState keyboard = Keyboard.GetState();

        // delta time
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // If ball is paused after a point, count down the respawn timer and restore velocity when done
        if (ballIsPaused)
        {
            respawnTimer += dt;
            if (respawnTimer >= respawnDelay)
            {
                // resume ball with the same "normal" velocity used before
                ballIsPaused = false;
                respawnTimer = 0f;
                ballVelocity = new Vector2(800f, (float)rng.Next(-200, 200));
            }
        }

        // move the ball (use elapsed seconds)
        ballPosition = new Vector2(ballPosition.X + ballVelocity.X * dt, ballPosition.Y + ballVelocity.Y * dt);

        // scoring checks (use ball texture width if available)
        int currentBallW = (ballTexture != null) ? ballTexture.Width : 0;
        if (ballPosition.X < 0f - (2f * (float)currentBallW))
        {
            leftLives = leftLives - 1;
            ResetPositions();
            if (pointScored != null)
                MediaPlayer.Play(pointScored);
            if (leftLives == 0)
            {
                GameOver = true;
                winner = 2;
            }
        }
        else if (ballPosition.X > (float)VIRTUAL_WIDTH + 2f * (float)currentBallW)
        {
            rightLives = rightLives - 1;
            ResetPositions();
            if (pointScored != null)
                MediaPlayer.Play(pointScored);
            if (rightLives == 0)
            {
                GameOver = true;
                winner = 1;
            }
        }

        // bounce off top and bottom using virtual height
        int currentBallH = (ballTexture != null) ? ballTexture.Height : 0;
        if (ballPosition.Y < 0f)
        {
            ballPosition = new Vector2(ballPosition.X, 0f);
            ballVelocity = new Vector2(ballVelocity.X, ballVelocity.Y * -1f);
            PlayBounce();
        }
        else if (ballPosition.Y > (float)VIRTUAL_HEIGHT - (float)currentBallH)
        {
            ballPosition = new Vector2(ballPosition.X, (float)VIRTUAL_HEIGHT - (float)currentBallH);
            ballVelocity = new Vector2(ballVelocity.X, ballVelocity.Y * -1f);
            PlayBounce();
        }

        // player controls (left player W/S)
        int paddleH = (leftPaddleTexture != null) ? leftPaddleTexture.Height : 0;
        if (keyboard.IsKeyDown(Keys.S))
        {
            if (leftPaddlePosition.Y < (float)VIRTUAL_HEIGHT - (float)paddleH)
            {
                leftPaddlePosition = new Vector2(leftPaddlePosition.X, leftPaddlePosition.Y + leftPaddleVelocity);
            }
        }
        if (keyboard.IsKeyDown(Keys.W))
        {
            if (leftPaddlePosition.Y > 0f)
            {
                leftPaddlePosition = new Vector2(leftPaddlePosition.X, leftPaddlePosition.Y - leftPaddleVelocity);
            }
        }

        // right paddle controls (Up/Down)
        if (keyboard.IsKeyDown(Keys.Down))
        {
            if (rightPaddlePosition.Y < (float)VIRTUAL_HEIGHT - (float)paddleH)
            {
                rightPaddlePosition = new Vector2(rightPaddlePosition.X, rightPaddlePosition.Y + rightPaddleVelocity);
            }
        }
        if (keyboard.IsKeyDown(Keys.Up))
        {
            if (rightPaddlePosition.Y > 0f)
            {
                rightPaddlePosition = new Vector2(rightPaddlePosition.X, rightPaddlePosition.Y - rightPaddleVelocity);
            }
        }

        // collision rectangles (build them manually)
        int pW = (leftPaddleTexture != null) ? leftPaddleTexture.Width : 0;
        Rectangle rightPaddle = new Rectangle((int)rightPaddlePosition.X, (int)rightPaddlePosition.Y, pW, paddleH);
        Rectangle leftPaddle = new Rectangle((int)leftPaddlePosition.X, (int)leftPaddlePosition.Y, pW, paddleH);
        Rectangle ballRect = new Rectangle((int)ballPosition.X, (int)ballPosition.Y, currentBallW, currentBallH);

        if (leftPaddle.Intersects(ballRect) || rightPaddle.Intersects(ballRect))
        {
            // flip X and add some Y randomness
            ballVelocity = new Vector2(ballVelocity.X * -1f, ballVelocity.Y + (float)rng.Next(-200, 200));
            PlayBounce();
        }

        // speed up ball slightly each update (naive approach)
        ballVelocity = new Vector2(ballVelocity.X * gameSpeeder, ballVelocity.Y * gameSpeeder);
    }

    private void ResetPositions()
    {
        int cw = (ballTexture != null) ? ballTexture.Width : 0;
        int ch = (ballTexture != null) ? ballTexture.Height : 0;
        ballPosition = new Vector2((float)VIRTUAL_WIDTH / 2f - (float)cw / 2f, (float)VIRTUAL_HEIGHT / 2f - (float)ch / 2f);

        // NEW: pause the ball on reset, set velocity to zero and start the respawn timer
        ballVelocity = Vector2.Zero;
        ballIsPaused = true;
        respawnTimer = 0f;

        leftPaddlePosition = new Vector2(leftPaddleStartPosition.X, leftPaddleStartPosition.Y);
        rightPaddlePosition = new Vector2(rightPaddleStartPosition.X, rightPaddleStartPosition.Y);
    }

    private void PlayBounce()
    {
        // choose which sound to play
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
        // Do not clear here because Game1.Draw clears the screen already (kept like before)
        int midW = (middleLineTexture != null) ? middleLineTexture.Width : 0;
        _spriteBatch.Draw(middleLineTexture, new Vector2((float)VIRTUAL_WIDTH / 2f - (float)midW / 2f, 0f), Color.Gray);

        _spriteBatch.Draw(leftPaddleTexture, leftPaddlePosition, Color.White);
        _spriteBatch.Draw(leftPaddleTexture, rightPaddlePosition, Color.White);

        // draw scores (student-style: no extra formatting)
        if (leftPoints != null)
            _spriteBatch.DrawString(leftPoints, leftLives.ToString(), leftFontPos, Color.Gray);
        if (rightPoints != null)
            _spriteBatch.DrawString(rightPoints, rightLives.ToString(), rightFontPos, Color.Gray);

        _spriteBatch.Draw(ballTexture, ballPosition, Color.White);
    }
}
