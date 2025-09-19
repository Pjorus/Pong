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
    public int _selectedOption = 1;
    public bool RequestStartGame = false;
    private double _inputDelayTimer = 0;
    private bool _inputDelayActive = true;
    public int SelectedOption; // Set to public to be accessed in Game1.cs and determine game mode

    private double _optionChangeDelay = 0.2;
    private double _optionChangeTimer = 0;

    public TitleScene(GraphicsDevice graphics, SpriteBatch spriteBatch, SpriteFont font, Song backgroundMusic, SpriteFont titleFont)
    {
        // Load all assets and set them to variables
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
        // Play background music
        MediaPlayer.IsRepeating = true;
        MediaPlayer.Volume = 0.5f;
        MediaPlayer.Play(_backGroundMusic);
    }


    public override void Update(GameTime gameTime)
    {
        // Blinking text timer for selected option
        _blinkTimer += gameTime.ElapsedGameTime.TotalSeconds;
        if (_blinkTimer > 0.4)
        {
            _showText = !_showText;
            _blinkTimer = 0;
        }

        // Input delay to prevent immediate selection of game mode when returning to title screen after a game
        if (_inputDelayActive)
        {
            _inputDelayTimer += gameTime.ElapsedGameTime.TotalSeconds;
            if (_inputDelayTimer >= 1.0)
            {
                _inputDelayActive = false;
            }
            return;
        }

        // Timer for delay between changing options to make selecting easier
        _optionChangeTimer += gameTime.ElapsedGameTime.TotalSeconds;

        KeyboardState keyboard = Keyboard.GetState();
        bool moved = false;

        if (_optionChangeTimer >= _optionChangeDelay) //Delay to make selecting options easier
        {
            // Switching between game mode options
            if (keyboard.IsKeyDown(Keys.Down) || keyboard.IsKeyDown(Keys.S))
            {
                if (_selectedOption < 3)
                {
                    _selectedOption++;
                    moved = true;
                }
            }
            else if (keyboard.IsKeyDown(Keys.Up) || keyboard.IsKeyDown(Keys.W))
            {
                if (_selectedOption > 1)
                {
                    _selectedOption--;
                    moved = true;
                }
            }

            if (moved)
            {
                _optionChangeTimer = 0;
            }
        }

        // Sync the public property with the private field
        SelectedOption = _selectedOption;

        // Check for Enter key press to start the game based on the selected option
        if (keyboard.IsKeyDown(Keys.Enter) || keyboard.IsKeyDown(Keys.Space))
        {
            MediaPlayer.Stop();
            MediaPlayer.IsRepeating = false;
            RequestStartGame = true;
        }
    }

    public override void Draw(GameTime gameTime)
    {
        // Draw the game assets to the screen
        _graphics.Clear(Color.Black);

        _spriteBatch.DrawString(_font, "PONG", new Vector2(_graphics.Viewport.Width / 2 - _font.MeasureString("PONG").X / 2, 100), Color.White);

        // The blinking selected option
        if (_showText && _selectedOption == 1)
        {
            _spriteBatch.DrawString(_titleFont, "1 PLAYER", new Vector2(_graphics.Viewport.Width / 2 - _titleFont.MeasureString("1 PLAYER").X / 2, 600), Color.White);
        }
        else if (_showText && _selectedOption == 2)
        {
            _spriteBatch.DrawString(_titleFont, "2 PLAYER", new Vector2(_graphics.Viewport.Width / 2 - _titleFont.MeasureString("2 PLAYER").X / 2, 700), Color.White);
        }
        else if (_showText && _selectedOption == 3)
        {
            _spriteBatch.DrawString(_titleFont, "4 PLAYER", new Vector2(_graphics.Viewport.Width / 2 - _titleFont.MeasureString("4 PLAYER").X / 2, 800), Color.White);
        }

        // The place holder for when the other option is the currently selected option
        if (_selectedOption == 1)
        {
            _spriteBatch.DrawString(_titleFont, "2 PLAYER", new Vector2(_graphics.Viewport.Width / 2 - _titleFont.MeasureString("2 PLAYER").X / 2, 700), Color.Gray);
            _spriteBatch.DrawString(_titleFont, "4 PLAYER", new Vector2(_graphics.Viewport.Width / 2 - _titleFont.MeasureString("4 PLAYER").X / 2, 800), Color.Gray);
        }
        else if (_selectedOption == 2)
        {
            _spriteBatch.DrawString(_titleFont, "4 PLAYER", new Vector2(_graphics.Viewport.Width / 2 - _titleFont.MeasureString("4 PLAYER").X / 2, 800), Color.Gray);
            _spriteBatch.DrawString(_titleFont, "1 PLAYER", new Vector2(_graphics.Viewport.Width / 2 - _titleFont.MeasureString("1 PLAYER").X / 2, 600), Color.Gray);
        }
        else if (_selectedOption == 3)
        {
            _spriteBatch.DrawString(_titleFont, "1 PLAYER", new Vector2(_graphics.Viewport.Width / 2 - _titleFont.MeasureString("1 PLAYER").X / 2, 600), Color.Gray);
            _spriteBatch.DrawString(_titleFont, "2 PLAYER", new Vector2(_graphics.Viewport.Width / 2 - _titleFont.MeasureString("2 PLAYER").X / 2, 700), Color.Gray);
        }
    }
}
