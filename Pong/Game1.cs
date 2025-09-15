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

    //private bool gameWon;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);

        // Set resolution dynamically
        _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

        // Fullscreen mode
        _graphics.IsFullScreen = true;

        Content.RootDirectory = "Content";
        IsMouseVisible = false;
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Calculate the scaling matrix
        float scaleX = (float)_graphics.GraphicsDevice.Viewport.Width / 1920f; // Internal width
        float scaleY = (float)_graphics.GraphicsDevice.Viewport.Height / 1080f; // Internal height
        float scale = Math.Min(scaleX, scaleY); // Maintain aspect ratio
        _scaleMatrix = Matrix.CreateScale(scale);

        // Load fonts and music
        _font = Content.Load<SpriteFont>("Score");
        backgroundMusic = Content.Load<Song>("titleScreenMusic");
        _titleFont = Content.Load<SpriteFont>("titleScreen");


        // Load Two Player Scene assets
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
        if (_currentScene is TitleScene titleScene && titleScene.RequestStartGame)
        {
            if (titleScene.SelectedOption == 1)
            {
                _currentScene = new onePlayer(GraphicsDevice, _spriteBatch, Content, leftPoints, leftPoints, ballTexture, leftPaddleTexture, middleLineTexture, pointScored, bounceOne, bounceTwo);
            }
            else if (titleScene.SelectedOption == 2)
            {
                _currentScene = new twoPlayer(GraphicsDevice, _spriteBatch, Content, leftPoints, leftPoints, ballTexture, leftPaddleTexture, middleLineTexture, pointScored, bounceOne, bounceTwo);
            }
        }
        else if (_currentScene is onePlayer gameScene && gameScene.GameOver)
        {
            //_currentScene = new endScreen(gameWon)
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

        // Begin the SpriteBatch with the scaling matrix
        _spriteBatch.Begin(transformMatrix: _scaleMatrix);

        _currentScene.Draw(gameTime);

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
