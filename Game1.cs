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
    private Song backgroundMusic;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);

        _graphics.PreferredBackBufferWidth = 1920;
        _graphics.PreferredBackBufferHeight = 1080;

        // Runs the game in "full Screen" mode using the set resolution
        _graphics.IsFullScreen = true;

        Content.RootDirectory = "Content";
        IsMouseVisible = false;
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Use your existing Score.spritefont
        _font = Content.Load<SpriteFont>("titleScreen");
        backgroundMusic = Content.Load<Song>("titleScreenMusic");


        // Start with the TitleScene
        _currentScene = new TitleScene(GraphicsDevice, _spriteBatch, _font, backgroundMusic);
    }

    protected override void Update(GameTime gameTime)
    {
        if (_currentScene is TitleScene titleScene && titleScene.RequestStartGame)
        {
            _currentScene = new GameScene(GraphicsDevice, _spriteBatch, Content);
        }
        else
        {
            _currentScene.Update(gameTime);
        }
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        _currentScene.Draw(gameTime);
        base.Draw(gameTime);
    }
}
