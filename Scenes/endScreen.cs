using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Pong.Scenes;

public class endScreen : Scene
{
    private SpriteBatch _spriteBatch;
    private SpriteFont _font;
    private SpriteFont _titleFont;
    private GraphicsDevice _graphics;
    private Song _backGroundMusic;

    private double _blinkTimer = 0;
    private bool _showText = true;

    private bool gameWon = false;
    public bool RequestTitleScreen = false;
    int gameWinner = 0;

    public endScreen(GraphicsDevice graphics, SpriteBatch spriteBatch, SpriteFont font, Song backgroundMusic, SpriteFont titleFont, bool _gameWon, int _gameWinner)
    {
        _graphics = graphics;
        _spriteBatch = spriteBatch;
        _font = font;
        _backGroundMusic = backgroundMusic;
        _titleFont = titleFont;
        gameWon = _gameWon;
        gameWinner = _gameWinner;
        LoadContent();
    }

    private void LoadContent()
    {
        MediaPlayer.IsRepeating = true;
        MediaPlayer.Volume = 0.5f;
        MediaPlayer.Play(_backGroundMusic);
    }

    public bool RequestStartGame { get; private set; } = false;

    public override void Update(GameTime gameTime)
    {
        _blinkTimer += gameTime.ElapsedGameTime.TotalSeconds;
        if (_blinkTimer > 0.4) // Change blink speed here (0.5 seconds)
        {
            _showText = !_showText;
            _blinkTimer = 0;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.Enter))
        {
            MediaPlayer.Stop();
            MediaPlayer.IsRepeating = false;
            RequestTitleScreen = true;
        }
    }

    public override void Draw(GameTime gameTime)
    {
        if (gameWon && gameWinner == 0)
        {
            // You won
            _spriteBatch.DrawString(_titleFont, "You Won! :-)", new Vector2(_graphics.Viewport.Width / 2 - _titleFont.MeasureString("You Won! :-)").X, 400), Color.White);
        }
        else if (gameWon == false && gameWinner == 0)
        {
            // You lost
            _spriteBatch.DrawString(_titleFont, "You Lost... :-(", new Vector2(_graphics.Viewport.Width / 2 - _titleFont.MeasureString("You Lost... :-(").X, 400), Color.White);
        }



        if (gameWinner == 1)
        {
            // Player 1 won
            _spriteBatch.DrawString(_titleFont, "Left Player Wins! :-)", new Vector2(_graphics.Viewport.Width / 2 - _titleFont.MeasureString("Left Player Wins! :-)").X, 400), Color.White);
        }
        else if (gameWinner == 2)
        {
            // Player 2 won
            _spriteBatch.DrawString(_titleFont, "Right Player Wins! :-)", new Vector2(_graphics.Viewport.Width / 2 - _titleFont.MeasureString("Right Player Wins! :-)").X, 400), Color.White);
        }



        _spriteBatch.DrawString(_font, "PONG", new Vector2(_graphics.Viewport.Width / 2 - _font.MeasureString("PONG").X, 100), Color.White);

        if (_showText)
        {
            _spriteBatch.DrawString(_titleFont, "Press Enter to Restart", new Vector2(_graphics.Viewport.Width / 2 - _titleFont.MeasureString("Press Enter to Restart").X, 800), Color.White);
        }
        
    }
}
