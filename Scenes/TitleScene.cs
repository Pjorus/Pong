using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Pong.Scenes;

public class TitleScene : Scene
{
    private SpriteBatch _spriteBatch;
    private SpriteFont _font;
    private SpriteFont _titleFont;
    private GraphicsDevice _graphics;
    private Song _backGroundMusic;

    private double _blinkTimer = 0;
    private bool _showText = true;
    private int _selectedOption = 0;

    public TitleScene(GraphicsDevice graphics, SpriteBatch spriteBatch, SpriteFont font, Song backgroundMusic, SpriteFont titleFont)
    {
        _graphics = graphics;
        _spriteBatch = spriteBatch;
        _font = font;
        _backGroundMusic = backgroundMusic;
        _titleFont = titleFont;
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

        if (Keyboard.GetState().IsKeyDown(Keys.Up) || Keyboard.GetState().IsKeyDown(Keys.W))
        {
            _selectedOption = 0;
        }
        else if (Keyboard.GetState().IsKeyDown(Keys.Down) || Keyboard.GetState().IsKeyDown(Keys.S))
        {
            _selectedOption = 1;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.Enter))
        {
            MediaPlayer.Stop();
            MediaPlayer.IsRepeating = false;
            RequestStartGame = true;
        }
    }

    public override void Draw(GameTime gameTime)
    {
        _graphics.Clear(Color.Black);

        _spriteBatch.Begin();
        _spriteBatch.DrawString(_font, "PONG", new Vector2(750, 100), Color.White);

        if (_showText && _selectedOption == 0)
        {
            _spriteBatch.DrawString(_titleFont, "1 PLAYER", new Vector2(410, 700), Color.White);
        }
        else if (_showText && _selectedOption == 1)
        {
            _spriteBatch.DrawString(_titleFont, "2 PLAYER", new Vector2(410, 700), Color.Gray);
        }

        _spriteBatch.End();
    }
}
