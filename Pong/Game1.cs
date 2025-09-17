using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Pong.Scenes;

namespace Pong;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Scene _currentScene;
    private SpriteFont _font;
    private SpriteFont _titleFont;
    private Song backgroundMusic;


    // Two Player Scene assets
    private Texture2D ballTexture;
    private Texture2D leftPaddleTexture;
    private Texture2D middleLineTexture;
    private SpriteFont leftPoints;
    private Song pointScored;
    private Song bounceOne;
    private Song bounceTwo;




    private Matrix _scaleMatrix;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);

        // Set resolution no matter the screen size (1440p, 1080p, etc.)
        _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

        // Set the game to fullscreen
        _graphics.IsFullScreen = true;

        Content.RootDirectory = "Content";
        IsMouseVisible = false;
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Set value and calculate the screen resolution scaling
        float scaleX = _graphics.GraphicsDevice.Viewport.Width / 1920f;
        float scaleY = _graphics.GraphicsDevice.Viewport.Height / 1080f;
        float scale = Math.Min(scaleX, scaleY);
        _scaleMatrix = Matrix.CreateScale(scale);

        // Load fonts and music
        _font = Content.Load<SpriteFont>("Score");
        backgroundMusic = Content.Load<Song>("titleScreenMusic");
        _titleFont = Content.Load<SpriteFont>("titleScreen");


        // Load Game Scenes assets
        leftPoints = Content.Load<SpriteFont>("Score");
        ballTexture = Content.Load<Texture2D>("ball");
        leftPaddleTexture = Content.Load<Texture2D>("balk");
        middleLineTexture = Content.Load<Texture2D>("middleLine");

        pointScored = Content.Load<Song>("pointScored");
        bounceOne = Content.Load<Song>("bounceOne");
        bounceTwo = Content.Load<Song>("bounceTwo");

        // Start with the TitleScene
        _currentScene = new TitleScene(GraphicsDevice, _spriteBatch, _font, backgroundMusic, _titleFont);
    }

    protected override void Update(GameTime gameTime)
    {
        if (_currentScene is TitleScene titleScene && titleScene.RequestStartGame) //Check if the current scene is the title scene and if the Player wants to start the game
        {
            if (titleScene.SelectedOption == 1) // Player selected 1 Player Mode
            {
                _currentScene = new onePlayer(GraphicsDevice, _spriteBatch, Content, leftPoints, leftPoints, ballTexture, leftPaddleTexture, middleLineTexture, pointScored, bounceOne, bounceTwo);
                titleScene.RequestStartGame = false; // Reset the request to start game, so it doesn't trigger immediately when the player returns to the title screen after a game
            }
            else if (titleScene.SelectedOption == 2) // Player selected 2 Player Mode
            {
                _currentScene = new twoPlayer(GraphicsDevice, _spriteBatch, Content, leftPoints, leftPoints, ballTexture, leftPaddleTexture, middleLineTexture, pointScored, bounceOne, bounceTwo);
                titleScene.RequestStartGame = false; // Reset the request to start game, so it doesn't trigger immediately when the player returns to the title screen after a game
            }
        }
        else if (_currentScene is onePlayer onePlayer && onePlayer.GameOver) // Check if the Players game against the AI is over
        {
            bool gameWon = onePlayer.gameWon; // store the result of the game (gameWon = true if the Player won, false if the AI won)
            int gameWinner = 0; // Set to 0 because the Endscreen scene expects an int for gameWinner, but in 1 Player Mode it's not used, so a value of 0 is submitted
            _currentScene = new endScreen(GraphicsDevice, _spriteBatch, _font, backgroundMusic, _titleFont, gameWon, gameWinner);

            // Reset the onePlayer scene's GameOver and gameWon variables for when the Player wants to play again
            onePlayer.GameOver = false;
            onePlayer.gameWon = false;
        }
        else if (_currentScene is twoPlayer twoPlayer && twoPlayer.GameOver) // Check if the Players game in 2 Player Mode is over
        {
            int gameWinner = twoPlayer.winner; // Get the result of the game (1 = Left Player won, 2 = Right Player won)
            bool gameWon = false; // Set to 0 because the Endscreen scene expects a bool for gameWon, but in 2 Player Mode it's not used, so a default value of false is submitted
            _currentScene = new endScreen(GraphicsDevice, _spriteBatch, _font, backgroundMusic, _titleFont, gameWon, gameWinner);

            // Reset the twoPlayer scene's GameOver and winner variables for when the Players want to play again
            twoPlayer.GameOver = false;
            twoPlayer.winner = 0;
        }
        else if (_currentScene is endScreen endScreen && endScreen.RequestTitleScreen) // Check if the current scene is the end screen and if the Player wants to return to the title screen
        {
            _currentScene = new TitleScene(GraphicsDevice, _spriteBatch, _font, backgroundMusic, _titleFont);
        }
        else
        {
            _currentScene.Update(gameTime);
        }
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        // Set the default resolution scaling
        _spriteBatch.Begin(transformMatrix: _scaleMatrix);

        _currentScene.Draw(gameTime);

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
