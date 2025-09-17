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
    private float gameSpeeder = 1.0003f;

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
    public bool GameOver = false; // Set to public to be accessed in Game1.cs to determine when to switch to the end screen

    public onePlayer(GraphicsDevice graphics, SpriteBatch spriteBatch, ContentManager content, SpriteFont _points, SpriteFont _AIpoints, Texture2D _ballTexture, Texture2D _paddleTexture, Texture2D _middleLineTexture, Song _pointScored, Song _bounceOne, Song _bounceTwo)
    {
        // Load all assets and set them to variables
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

        points = _points;
        AIpoints = _AIpoints;

        // Store the texture dimensions of the ball
        int bW = 0;
        int bH = 0;
        if (ballTexture != null)
        {
            bW = ballTexture.Width;
            bH = ballTexture.Height;
        }

        // Set starting position of ball
        ballStartPosition = new Vector2(screenWidth / 2f - bW / 2f, screenHeight / 2f - bH / 2f);
        ballPosition = new Vector2(ballStartPosition.X, ballStartPosition.Y);
        ballVelocity = new Vector2(800f, rng.Next(-200, 200));

        // Store the texture dimensions of the paddle
        int pH = 0;
        int pW = 38;
        if (paddleTexture != null)
        {
            pH = paddleTexture.Height;
            pW = paddleTexture.Width;
        }

        // Set starting position of the Players paddle
        paddlePosition = new Vector2(0f, screenHeight / 2f - pH / 2f);
        paddleStartPosition = new Vector2(paddlePosition.X, paddlePosition.Y);

        // Set starting position of the AI's paddle
        paddleAIPosition = new Vector2(screenWidth - pW, screenHeight / 2f - pH / 2f);
        paddleAIStartPosition = new Vector2(paddleAIPosition.X, paddleAIPosition.Y);

        // Calculate text widths for lives text location and set them to variables
        float leftTextWidth = 0f;
        float rightTextWidth = 0f;
        if (points != null) leftTextWidth = points.MeasureString(playerLives.ToString()).X;
        if (AIpoints != null) rightTextWidth = AIpoints.MeasureString(AILives.ToString()).X;

        fontPos = new Vector2(screenWidth / 2f - leftTextWidth - 150f, 100f);
        fontPosAI = new Vector2(screenWidth / 2f - rightTextWidth + 300f, 100f);
    }


    public override void Update(GameTime gameTime)
    {
        KeyboardState keyboard = Keyboard.GetState();

        // Pause the ball for a set amount of time after a point is scored and the scene "resets"
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (ballIsPaused)
        {
            respawnTimer += dt;
            if (respawnTimer >= respawnDelay)
            {
                // Continue game after respawn delay
                ballIsPaused = false;
                respawnTimer = 0f;
                ballVelocity = new Vector2(800, 0);
            }
        }

        // Update the ball position
        ballPosition = new Vector2(ballPosition.X + ballVelocity.X * dt, ballPosition.Y + ballVelocity.Y * dt);

        // Get the texture dimensions of the ball and store them in variables that can be used here
        int currentBallW = 0;
        int currentBallH = 0;
        if (ballTexture != null)
        {
            currentBallW = ballTexture.Width;
            currentBallH = ballTexture.Height;
        }

        float currentBallWf = currentBallW;
        float currentBallHf = currentBallH;


        // Check if the ball goes past the left or right edge of the screen, and a point is scored (The Player or AI loses a life and the scene "resets")
        if (ballPosition.X < 0f - (2f * currentBallWf))
        {
            playerLives = playerLives - 1;
            ResetPositions();
            if (pointScored != null) MediaPlayer.Play(pointScored);

            // Check if the player has no lives left and so the AI wins
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

            // Check if the AI has no lives left and so the Player wins
            if (AILives == 0)
            {
                gameWon = true;
                GameOver = true;
            }
        }


        // Check for ball collision with top and bottom of screen and if so, invert its Y velocity
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

        // Get the texture Height of the paddle and store the height in a variable that can be used here
        int paddleH = 0;
        if (paddleTexture != null)
        {
            paddleH = paddleTexture.Height;
        }
        float paddleHf = paddleH;


        // Check for Player input and move the paddle up or down based on that input
        if (keyboard.IsKeyDown(Keys.S))
        {
            if (paddlePosition.Y < VIRTUAL_HEIGHT - paddleHf) // Check if the paddle is within the screen before moving it
            {
                paddlePosition = new Vector2(paddlePosition.X, paddlePosition.Y + paddleVelocity);
            }
        }
        if (keyboard.IsKeyDown(Keys.W))
        {
            if (paddlePosition.Y > 0f) // Check if the paddle is within the screen before moving it
            {
                paddlePosition = new Vector2(paddlePosition.X, paddlePosition.Y - paddleVelocity);
            }
        }


        // Make the AI paddle follow the ball's Y position when the ball is moving towards the AI
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

        // Set the AI paddle position to stay within the screen bounds
        float aiClampMax = VIRTUAL_HEIGHT - paddleHf;
        paddleAIPosition = new Vector2(paddleAIPosition.X, Math.Clamp(paddleAIPosition.Y, 0f, aiClampMax));

        // Get the texture Width of the paddle and store the width in a variable that can be used here
        int pW = 0;
        if (paddleTexture != null)
        {
            pW = paddleTexture.Width;
        }

        // Create rectangles around the paddles and ball to check for collision
        Rectangle balkAI = new Rectangle((int)paddleAIPosition.X, (int)paddleAIPosition.Y, pW, paddleH);
        Rectangle balk = new Rectangle((int)paddlePosition.X, (int)paddlePosition.Y, pW, paddleH);
        Rectangle ballRect = new Rectangle((int)ballPosition.X, (int)ballPosition.Y, currentBallW, currentBallH);

        // Check if the ball collides with the Players paddle and if so, invert its X velocity and add some random Y velocity
        if (balk.Intersects(ballRect) && balk.X + paddleTexture.Width >= ballPosition.X)
        {
            ballPosition.X = balk.X + paddleTexture.Width; // Move ball to the right edge of the paddle to prevent the ball from clipping into the paddle
            ballVelocity = new Vector2(Math.Abs(ballVelocity.X), ballVelocity.Y + rng.Next(-250, 250));
            PlayBounce();
        }

        // Check if the ball collides with the AI's paddle and if so, invert its X velocity and add some random Y velocity
        if (balkAI.Intersects(ballRect) && balkAI.X <= ballPosition.X + ballTexture.Width)
        {
            ballPosition.X = balkAI.X - ballTexture.Width; // Move ball to the left edge of the AI paddle to prevent the ball from clipping into the paddle
            ballVelocity = new Vector2(-Math.Abs(ballVelocity.X), ballVelocity.Y + rng.Next(-250, 250));
            PlayBounce();
        }

        // Slowly increase the speed of the ball and paddles over time
        ballVelocity = new Vector2(ballVelocity.X * gameSpeeder, ballVelocity.Y * gameSpeeder);
        paddleAIVelocity = paddleAIVelocity * gameSpeeder;
        paddleVelocity = paddleVelocity * gameSpeeder;
    }

    private void ResetPositions()
    {
        // Get the texture Width of the ball and store them in variables that can be used here
        int cw = 0;
        int ch = 0;
        if (ballTexture != null)
        {
            cw = ballTexture.Width;
            ch = ballTexture.Height;
        }

        // Reset the ball position to the center of the screen
        ballPosition = new Vector2(VIRTUAL_WIDTH / 2f - cw / 2f, VIRTUAL_HEIGHT / 2f - ch / 2f);

        // Pause the ball for the set amount of time after a point is scored and the scene "resets"
        ballVelocity = Vector2.Zero;
        ballIsPaused = true;
        respawnTimer = 0f;

        // Reset the paddles to their starting positions
        paddlePosition = new Vector2(paddleStartPosition.X, paddleStartPosition.Y);
        paddleAIPosition = new Vector2(paddleAIStartPosition.X, paddleAIStartPosition.Y);
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

        // Draw the ball last to ensure it appears above everything else
        _spriteBatch.Draw(ballTexture, ballPosition, Color.White);
    }
}
