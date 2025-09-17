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
    public int SelectedOption; // Set to public to be accessed in Game1.cs and determine game mode

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
        _blinkTimer += gameTime.ElapsedGameTime.TotalSeconds;
        if (_blinkTimer > 0.4) // Speed at which the currently selected game mode blinks
        {
            _showText = !_showText;
            _blinkTimer = 0;
        }

        // Add a small delay before allowing input to avoid accidentally skipping the title screen and jumping straight into the game
        if (_inputDelayActive)
        {
            _inputDelayTimer += gameTime.ElapsedGameTime.TotalSeconds;
            if (_inputDelayTimer >= 1.0)
            {
                _inputDelayActive = false;
            }
            return;
        }

        // Check for Up/Down arrow or W/S key presses to change the currently selected option
        if (Keyboard.GetState().IsKeyDown(Keys.Up) || Keyboard.GetState().IsKeyDown(Keys.W))
        {
            _selectedOption = 1;
        }
        else if (Keyboard.GetState().IsKeyDown(Keys.Down) || Keyboard.GetState().IsKeyDown(Keys.S))
        {
            _selectedOption = 2;
        }


        // Check for Enter key press to start the game based on the selected option
        if (Keyboard.GetState().IsKeyDown(Keys.Enter))
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

        _spriteBatch.DrawString(_font, "PONG", new Vector2(_graphics.Viewport.Width / 2 - _font.MeasureString("PONG").X/2, 100), Color.White);

        // The blinking selected option
        if (_showText && _selectedOption == 1)
        {
            _spriteBatch.DrawString(_titleFont, "1 PLAYER", new Vector2(_graphics.Viewport.Width / 2 - _titleFont.MeasureString("1 Player").X / 2, 700), Color.White);
        }
        else if (_showText && _selectedOption == 2)
        {
            _spriteBatch.DrawString(_titleFont, "2 PLAYER", new Vector2(_graphics.Viewport.Width / 2 - _titleFont.MeasureString("2 Player").X / 2, 800), Color.White);
        }

        // The place holder for when the other option is the currently selected option
        if (_selectedOption == 1)
        {
            _spriteBatch.DrawString(_titleFont, "2 PLAYER", new Vector2(_graphics.Viewport.Width / 2 - _titleFont.MeasureString("2 Player").X / 2, 800), Color.Gray);
        }
        else if (_selectedOption == 2)
        {
            _spriteBatch.DrawString(_titleFont, "1 PLAYER", new Vector2(_graphics.Viewport.Width / 2 - _titleFont.MeasureString("1 Player").X / 2, 700), Color.Gray);
        }
    }
}
