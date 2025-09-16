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
    private int _selectedOption = 1;
    public bool RequestStartGame = false;
    private double _inputDelayTimer = 0;
    private bool _inputDelayActive = true;

    public TitleScene(GraphicsDevice graphics, SpriteBatch spriteBatch, SpriteFont font, Song backgroundMusic, SpriteFont titleFont)
    {
        _graphics = graphics;
        _spriteBatch = spriteBatch;
        _font = font;
        _backGroundMusic = backgroundMusic;
        _titleFont = titleFont;
        LoadContent();
    _inputDelayTimer = 0;
    _inputDelayActive = true;
    }

    private void LoadContent()
    {
        MediaPlayer.IsRepeating = true;
        MediaPlayer.Volume = 0.5f;
        MediaPlayer.Play(_backGroundMusic);
    }

    public int SelectedOption => _selectedOption;

    public override void Update(GameTime gameTime)
    {
        _blinkTimer += gameTime.ElapsedGameTime.TotalSeconds;
        if (_blinkTimer > 0.4)
        {
            _showText = !_showText;
            _blinkTimer = 0;
        }

        if (_inputDelayActive)
        {
            _inputDelayTimer += gameTime.ElapsedGameTime.TotalSeconds;
            if (_inputDelayTimer >= 1.0)
            {
                _inputDelayActive = false;
            }
            return;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.Up) || Keyboard.GetState().IsKeyDown(Keys.W))
        {
            _selectedOption = 1;
        }
        else if (Keyboard.GetState().IsKeyDown(Keys.Down) || Keyboard.GetState().IsKeyDown(Keys.S))
        {
            _selectedOption = 2;
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
        // Do not call _spriteBatch.Begin() here
        _graphics.Clear(Color.Black);


    // Centered X position helper
    float CenterX(string text, SpriteFont font) => (_graphics.Viewport.Width - font.MeasureString(text).X) / 2f;

    _spriteBatch.DrawString(_font, "PONG", new Vector2(CenterX("PONG", _font)/2, 100), Color.White);

        if (_showText && _selectedOption == 1)
        {
            _spriteBatch.DrawString(_titleFont, "1 PLAYER", new Vector2(CenterX("1 PLAYER", _titleFont)/2, 700), Color.White);
        }
        else if (_showText && _selectedOption == 2)
        {
            _spriteBatch.DrawString(_titleFont, "2 PLAYER", new Vector2(CenterX("2 PLAYER", _titleFont)/2, 800), Color.White);
        }

        if(_selectedOption == 1)
        {
            _spriteBatch.DrawString(_titleFont, "2 PLAYER", new Vector2(CenterX("2 PLAYER", _titleFont)/2, 800), Color.Gray);
        }
        else if(_selectedOption == 2)
        {
            _spriteBatch.DrawString(_titleFont, "1 PLAYER", new Vector2(CenterX("1 PLAYER", _titleFont)/2, 700), Color.Gray);
        }
    }
}
