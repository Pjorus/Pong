using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using SharpDX.Direct3D9;

namespace Pong.Scenes;

public class twoVsTwo : Scene
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
    private Vector2 leftPaddleOnePosition;
    private Vector2 leftPaddleOneStartPosition;
    private Vector2 leftPaddleTwoPosition;
    private Vector2 leftPaddleTwoStartPosition;
    private float leftPaddleVelocity = 7;

    // Right paddle
    private Vector2 rightPaddleOnePosition;
    private Vector2 rightPaddleOneStartPosition;
    private Vector2 rightPaddleTwoPosition;
    private Vector2 rightPaddleTwoStartPosition;
    private float rightPaddleVelocity = 7;

    // Middle line and game speed increase
    private Texture2D middleLineTexture;
    private Texture2D VerticalMiddleLineTexture;
    private float gameSpeeder = 1.0003f;

    // Fonts and score
    private SpriteFont leftPoints;
    private SpriteFont rightPoints;
    private SpriteFont instructionsFont;
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


    public twoVsTwo(GraphicsDevice graphics, SpriteBatch spriteBatch, ContentManager content, SpriteFont _leftPoints, SpriteFont _rightPoints, Texture2D _ballTexture, Texture2D _leftPaddleTexture, Texture2D _middleLineTexture, Song _pointScored, Song _bounceOne, Song _bounceTwo, Texture2D _verticalMiddleLineTexture, SpriteFont _instructionsFont)
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
        VerticalMiddleLineTexture = _verticalMiddleLineTexture;

        // fonts
        leftPoints = _leftPoints;
        rightPoints = _rightPoints;
        instructionsFont = _instructionsFont;

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
            pW = leftPaddleTexture.Width / 2; // Paddles are drawn at half size
            pH = leftPaddleTexture.Height / 2;
        }

        // Set starting positions of paddles
        leftPaddleOnePosition = new Vector2(0f, (float)screenHeight / 4f - (float)pH / 2f);
        leftPaddleOneStartPosition = new Vector2(leftPaddleOnePosition.X, leftPaddleOnePosition.Y);

        leftPaddleTwoPosition = new Vector2(0f, (float)screenHeight * 3f / 4f - (float)pH / 2f);
        leftPaddleTwoStartPosition = new Vector2(leftPaddleTwoPosition.X, leftPaddleTwoPosition.Y);

        rightPaddleOnePosition = new Vector2((float)screenWidth - (float)(pW == 0 ? 38 : pW), (float)screenHeight / 4f - (float)pH / 2f);
        rightPaddleOneStartPosition = new Vector2(rightPaddleOnePosition.X, rightPaddleOnePosition.Y);

        rightPaddleTwoPosition = new Vector2((float)screenWidth - (float)(pW == 0 ? 38 : pW), (float)screenHeight * 3f / 4f - (float)pH / 2f);
        rightPaddleTwoStartPosition = new Vector2(rightPaddleTwoPosition.X, rightPaddleTwoPosition.Y);

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
        int paddleH = 0;
        if (leftPaddleTexture != null)
        {
            paddleH = leftPaddleTexture.Height / 2;
        }
        if (keyboard.IsKeyDown(Keys.S))
        {
            if (leftPaddleOnePosition.Y < VIRTUAL_HEIGHT / 2 - paddleH)
            {
                leftPaddleOnePosition = new Vector2(leftPaddleOnePosition.X, leftPaddleOnePosition.Y + leftPaddleVelocity);
            }
        }
        else if (keyboard.IsKeyDown(Keys.W))
        {
            if (leftPaddleOnePosition.Y > 0f)
            {
                leftPaddleOnePosition = new Vector2(leftPaddleOnePosition.X, leftPaddleOnePosition.Y - leftPaddleVelocity);
            }
        }

        if (keyboard.IsKeyDown(Keys.G))
        {
            if (leftPaddleTwoPosition.Y < VIRTUAL_HEIGHT - paddleH)
            {
                leftPaddleTwoPosition = new Vector2(leftPaddleTwoPosition.X, leftPaddleTwoPosition.Y + leftPaddleVelocity);
            }
        }
        else if (keyboard.IsKeyDown(Keys.T))
        {
            if (leftPaddleTwoPosition.Y > VIRTUAL_HEIGHT / 2f)
            {
                leftPaddleTwoPosition = new Vector2(leftPaddleTwoPosition.X, leftPaddleTwoPosition.Y - leftPaddleVelocity);
            }
        }

        // Control for the Right Player (Up-Arrow for Up / Down-Arrow for Down)
        if (keyboard.IsKeyDown(Keys.Down))
        {
            if (rightPaddleOnePosition.Y < VIRTUAL_HEIGHT / 2 - paddleH)
            {
                rightPaddleOnePosition = new Vector2(rightPaddleOnePosition.X, rightPaddleOnePosition.Y + rightPaddleVelocity);
            }
        }
        if (keyboard.IsKeyDown(Keys.Up))
        {
            if (rightPaddleOnePosition.Y > 0f)
            {
                rightPaddleOnePosition = new Vector2(rightPaddleOnePosition.X, rightPaddleOnePosition.Y - rightPaddleVelocity);
            }
        }

        if (keyboard.IsKeyDown(Keys.I))
        {
            if (rightPaddleTwoPosition.Y > VIRTUAL_HEIGHT / 2f)
            {
                rightPaddleTwoPosition = new Vector2(rightPaddleTwoPosition.X, rightPaddleTwoPosition.Y - rightPaddleVelocity);
            }
        }
        else if (keyboard.IsKeyDown(Keys.K))
        {
            if (rightPaddleTwoPosition.Y < VIRTUAL_HEIGHT - paddleH)
            {
                rightPaddleTwoPosition = new Vector2(rightPaddleTwoPosition.X, rightPaddleTwoPosition.Y + rightPaddleVelocity);
            }
        }

        // Create rectangles around the paddles and ball to check for collision
        int pW = (leftPaddleTexture != null) ? leftPaddleTexture.Width : 0;
        Rectangle rightPaddleOne = new Rectangle((int)rightPaddleOnePosition.X, (int)rightPaddleOnePosition.Y, pW, paddleH);
        Rectangle rightPaddleTwo = new Rectangle((int)rightPaddleTwoPosition.X, (int)rightPaddleTwoPosition.Y, pW, paddleH);
        Rectangle leftPaddleOne = new Rectangle((int)leftPaddleOnePosition.X, (int)leftPaddleOnePosition.Y, pW, paddleH);
        Rectangle leftPaddleTwo = new Rectangle((int)leftPaddleTwoPosition.X, (int)leftPaddleTwoPosition.Y, pW, paddleH);
        Rectangle ballRect = new Rectangle((int)ballPosition.X, (int)ballPosition.Y, currentBallW, currentBallH);

        // Check if the ball collides with the Left Players paddle and if so, invert its X velocity and add some random Y velocity
        if (leftPaddleOne.Intersects(ballRect) && ballVelocity.X < 0 || leftPaddleTwo.Intersects(ballRect) && ballVelocity.X < 0)
        {
            ballPosition.X = leftPaddleOne.X + leftPaddleTexture.Width;
            ballVelocity = new Vector2(Math.Abs(ballVelocity.X), ballVelocity.Y + rng.Next(-350, 350));
            PlayBounce();
        }

        // Check if the ball collides with the Right Players paddle and if so, invert its X velocity and add some random Y velocity
        if (rightPaddleOne.Intersects(ballRect) && ballVelocity.X > 0 || rightPaddleTwo.Intersects(ballRect) && ballVelocity.X > 0)
        {
            ballPosition.X = rightPaddleOne.X - ballTexture.Width;
            ballVelocity = new Vector2(-Math.Abs(ballVelocity.X), ballVelocity.Y + rng.Next(-350, 350));
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
        leftPaddleOnePosition = new Vector2(leftPaddleOneStartPosition.X, leftPaddleOneStartPosition.Y);
        leftPaddleTwoPosition = new Vector2(leftPaddleTwoStartPosition.X, leftPaddleTwoStartPosition.Y);
        rightPaddleOnePosition = new Vector2(rightPaddleOneStartPosition.X, rightPaddleOneStartPosition.Y);
        rightPaddleTwoPosition = new Vector2(rightPaddleTwoStartPosition.X, rightPaddleTwoStartPosition.Y);
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
        int vMidH = (VerticalMiddleLineTexture != null) ? VerticalMiddleLineTexture.Height : 0;
        _spriteBatch.Draw(middleLineTexture, new Vector2(VIRTUAL_WIDTH / 2f - midW / 2f, 0f), Color.Gray);
        _spriteBatch.Draw(VerticalMiddleLineTexture, new Vector2(0f, VIRTUAL_HEIGHT / 2 - vMidH / 2), Color.Gray);



        _spriteBatch.Draw(leftPaddleTexture, leftPaddleOnePosition, null, Color.White, 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0f);
        _spriteBatch.Draw(leftPaddleTexture, leftPaddleTwoPosition, null, Color.White, 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0f);
        _spriteBatch.Draw(leftPaddleTexture, rightPaddleOnePosition, null, Color.White, 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0f);
        _spriteBatch.Draw(leftPaddleTexture, rightPaddleTwoPosition, null, Color.White, 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0f);


        if (leftPoints != null)
            _spriteBatch.DrawString(leftPoints, leftLives.ToString(), leftFontPos, Color.Gray);
        if (rightPoints != null)
            _spriteBatch.DrawString(rightPoints, rightLives.ToString(), rightFontPos, Color.Gray);

        // Draw instructions for the players
        if (instructionsFont != null)
            _spriteBatch.DrawString(instructionsFont, "W for Up\nS for Down", new Vector2(VIRTUAL_WIDTH / 4f - instructionsFont.MeasureString("W for Up\nS for Down").X / 2f, VIRTUAL_HEIGHT / 4), Color.Gray);
            _spriteBatch.DrawString(instructionsFont, "T for Up\nG for Down", new Vector2(VIRTUAL_WIDTH / 4f - instructionsFont.MeasureString("Y for Up\nH for Down").X / 2f, VIRTUAL_HEIGHT * 3f / 4), Color.Gray);
            _spriteBatch.DrawString(instructionsFont, "Up for Up\nDown for Down", new Vector2(VIRTUAL_WIDTH * 3f / 4f - instructionsFont.MeasureString("Up for Up\nDown for Down").X / 2f, VIRTUAL_HEIGHT / 4), Color.Gray);
            _spriteBatch.DrawString(instructionsFont, "I for Up\nK for Down", new Vector2(VIRTUAL_WIDTH * 3f / 4f - instructionsFont.MeasureString("NumPad9 for Up\nNumPad6 for Down").X / 2f, VIRTUAL_HEIGHT * 3f / 4), Color.Gray);

        // Draw the ball last to ensure it appears above everything else
        _spriteBatch.Draw(ballTexture, ballPosition, Color.White);
    }
}
