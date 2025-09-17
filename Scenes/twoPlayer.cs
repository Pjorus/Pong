using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;

namespace Pong.Scenes;

public class twoPlayer : Scene
{
    private const int VIRTUAL_WIDTH = 1920;
    private const int VIRTUAL_HEIGHT = 1080;
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

    // Middle line and game speed increase
    private Texture2D middleLineTexture;
    private float gameSpeeder = 1.0003f;

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

    // Respawn delay
    private bool ballIsPaused = false;
    private float respawnTimer = 0f;
    private float respawnDelay = 1.0f; // seconds to wait after a point
    public bool GameOver = false; // Set to public to be accessed in Game1.cs to determine when to switch to the end screen


    public twoPlayer(GraphicsDevice graphics, SpriteBatch spriteBatch, ContentManager content, SpriteFont _leftPoints, SpriteFont _rightPoints, Texture2D _ballTexture, Texture2D _leftPaddleTexture, Texture2D _middleLineTexture, Song _pointScored, Song _bounceOne, Song _bounceTwo)
    {
        // Load all assets and set them to variables
        _graphics = graphics;
        _spriteBatch = spriteBatch;
        _content = content;

        int screenWidth = VIRTUAL_WIDTH;
        int screenHeight = VIRTUAL_HEIGHT;

        ballTexture = _ballTexture;

        leftPaddleTexture = _leftPaddleTexture;
        middleLineTexture = _middleLineTexture;

        // fonts
        leftPoints = _leftPoints;
        rightPoints = _rightPoints;

        // sounds
        pointScored = _pointScored;
        bounceOne = _bounceOne;
        bounceTwo = _bounceTwo;

        // Store the texture dimensions of the ball
        int bW = 0;
        int bH = 0;
        if (ballTexture != null)
        {
            bW = ballTexture.Width;
            bH = ballTexture.Height;
        }

        // Set starting position of the ball in the center of the screen and give it an initial velocity
        ballStartPosition = new Vector2((float)screenWidth / 2f - (float)bW / 2f, (float)screenHeight / 2f - (float)bH / 2f);
        ballPosition = new Vector2(ballStartPosition.X, ballStartPosition.Y);
        ballVelocity = new Vector2(800f, (float)rng.Next(-200, 200));

        // Store the texture dimensions of the paddles
        int pW = 0;
        int pH = 0;
        if (leftPaddleTexture != null)
        {
            pW = leftPaddleTexture.Width;
            pH = leftPaddleTexture.Height;
        }

        // Set starting positions of paddles
        leftPaddlePosition = new Vector2(0f, (float)screenHeight / 2f - (float)pH / 2f);
        leftPaddleStartPosition = new Vector2(leftPaddlePosition.X, leftPaddlePosition.Y);

        rightPaddlePosition = new Vector2((float)screenWidth - (float)(pW == 0 ? 38 : pW), (float)screenHeight / 2f - (float)pH / 2f);
        rightPaddleStartPosition = new Vector2(rightPaddlePosition.X, rightPaddlePosition.Y);

        // Set the positions for the Lives fonts
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

        leftFontPos = new Vector2(screenWidth / 2f - leftTextWidth / 2f - 200f, 100f);
        rightFontPos = new Vector2(screenWidth / 2f - rightTextWidth / 2f + 200f, 100f);

    }

    public override void Update(GameTime gameTime)
    {
        KeyboardState keyboard = Keyboard.GetState();

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Pause the ball for a set amount of time after a point is scored and the scene "resets"
        if (ballIsPaused)
        {
            respawnTimer += dt;
            if (respawnTimer >= respawnDelay)
            {
                // Continue game after respawn delay
                ballIsPaused = false;
                respawnTimer = 0f;
                ballVelocity = new Vector2(800f, rng.Next(-200, 200));
            }
        }

        // Update the ball position
        ballPosition = new Vector2(ballPosition.X + ballVelocity.X * dt, ballPosition.Y + ballVelocity.Y * dt);

        // Get the texture Width  of the ball and store them in variables that can be used here
        int currentBallW = (ballTexture != null) ? ballTexture.Width : 0;

        // Check if the ball goes past the left or right edge of the screen, and a point is scored (The Left Player or Right Player loses a life and the scene "resets")
        if (ballPosition.X < 0f - (2f * currentBallW))
        {
            leftLives = leftLives - 1;
            ResetPositions();
            if (pointScored != null)
                MediaPlayer.Play(pointScored);

            // Check if the Left Player has no lives left and the game is over, the Right Player wins (So winner is set to 2)
            if (leftLives == 0)
            {
                GameOver = true;
                winner = 2;
            }
        }
        else if (ballPosition.X > VIRTUAL_WIDTH + 2f * currentBallW)
        {
            rightLives = rightLives - 1;
            ResetPositions();
            if (pointScored != null)
                MediaPlayer.Play(pointScored);

            // Check if the Right Player has no lives left and the game is over, the Left Player wins (So winner is set to 1)
            if (rightLives == 0)
            {
                GameOver = true;
                winner = 1;
            }
        }

        // Check for ball collision with top and bottom of screen and if so, invert its Y velocity
        int currentBallH = (ballTexture != null) ? ballTexture.Height : 0;
        if (ballPosition.Y < 0f)
        {
            ballPosition = new Vector2(ballPosition.X, 0f);
            ballVelocity = new Vector2(ballVelocity.X, ballVelocity.Y * -1f);
            PlayBounce();
        }
        else if (ballPosition.Y > VIRTUAL_HEIGHT - currentBallH)
        {
            ballPosition = new Vector2(ballPosition.X, VIRTUAL_HEIGHT - currentBallH);
            ballVelocity = new Vector2(ballVelocity.X, ballVelocity.Y * -1f);
            PlayBounce();
        }

        // Controls for the Left Player (W for Up / S for Down)
        int paddleH = (leftPaddleTexture != null) ? leftPaddleTexture.Height : 0;
        if (keyboard.IsKeyDown(Keys.S))
        {
            if (leftPaddlePosition.Y < VIRTUAL_HEIGHT - paddleH)
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

        // Control for the Right Player (Up-Arrow for Up / Down-Arrow for Down)
        if (keyboard.IsKeyDown(Keys.Down))
        {
            if (rightPaddlePosition.Y < VIRTUAL_HEIGHT - paddleH)
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

        // Create rectangles around the paddles and ball to check for collision
        int pW = (leftPaddleTexture != null) ? leftPaddleTexture.Width : 0;
        Rectangle rightPaddle = new Rectangle((int)rightPaddlePosition.X, (int)rightPaddlePosition.Y, pW, paddleH);
        Rectangle leftPaddle = new Rectangle((int)leftPaddlePosition.X, (int)leftPaddlePosition.Y, pW, paddleH);
        Rectangle ballRect = new Rectangle((int)ballPosition.X, (int)ballPosition.Y, currentBallW, currentBallH);

        // Check if the ball collides with the Left Players paddle and if so, invert its X velocity and add some random Y velocity
        if (leftPaddle.Intersects(ballRect) && ballVelocity.X < 0)
        {
            ballPosition.X = leftPaddle.X + leftPaddleTexture.Width;
            ballVelocity = new Vector2(Math.Abs(ballVelocity.X), ballVelocity.Y + rng.Next(-250, 250));
            PlayBounce();
        }

        // Check if the ball collides with the Right Players paddle and if so, invert its X velocity and add some random Y velocity
        if (rightPaddle.Intersects(ballRect) && ballVelocity.X > 0)
        {
            ballPosition.X = rightPaddle.X - ballTexture.Width;
            ballVelocity = new Vector2(-Math.Abs(ballVelocity.X), ballVelocity.Y + rng.Next(-250, 250));
            PlayBounce();
        }


        // Slowly increase the speed of the ball and paddles over time
        ballVelocity = new Vector2(ballVelocity.X * gameSpeeder, ballVelocity.Y * gameSpeeder);
        leftPaddleVelocity = leftPaddleVelocity * gameSpeeder;
        rightPaddleVelocity = rightPaddleVelocity * gameSpeeder;
    }

    private void ResetPositions()
    {
        // Get the texture dimensions of the ball and store them in variables that can be used here
        int cw = (ballTexture != null) ? ballTexture.Width : 0;
        int ch = (ballTexture != null) ? ballTexture.Height : 0;

        // Set the ball back to the center of the screen
        ballPosition = new Vector2(VIRTUAL_WIDTH / 2f - cw / 2f, VIRTUAL_HEIGHT / 2f - ch / 2f);

        // Pause the ball for the set amount of time after a point is scored and the scene "resets"
        ballVelocity = Vector2.Zero;
        ballIsPaused = true;
        respawnTimer = 0f;

        // Reset paddles to their starting positions
        leftPaddlePosition = new Vector2(leftPaddleStartPosition.X, leftPaddleStartPosition.Y);
        rightPaddlePosition = new Vector2(rightPaddleStartPosition.X, rightPaddleStartPosition.Y);
    }

    private void PlayBounce()
    {
        // Randomly choose one of the two bounce sounds to play when the ball hits a paddle or the top/bottom of the screen
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
        // Draw the game assets to the screen
        int midW = (middleLineTexture != null) ? middleLineTexture.Width : 0;
        _spriteBatch.Draw(middleLineTexture, new Vector2(VIRTUAL_WIDTH / 2f - midW / 2f, 0f), Color.Gray);

        _spriteBatch.Draw(leftPaddleTexture, leftPaddlePosition, Color.White);
        _spriteBatch.Draw(leftPaddleTexture, rightPaddlePosition, Color.White);

        
        if (leftPoints != null)
            _spriteBatch.DrawString(leftPoints, leftLives.ToString(), leftFontPos, Color.Gray);
        if (rightPoints != null)
            _spriteBatch.DrawString(rightPoints, rightLives.ToString(), rightFontPos, Color.Gray);

        // Draw the ball last to ensure it appears above everything else
        _spriteBatch.Draw(ballTexture, ballPosition, Color.White);
    }
}
