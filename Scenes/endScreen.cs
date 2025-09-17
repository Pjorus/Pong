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

    private bool gameWon = false; // True means the Player won against the AI, false means the AI won or 2 Player Mode was used
    public bool RequestTitleScreen = false; // Set to public to be accessed in Game1.cs to determine when to return to the title screen
    int gameWinner = 0; // 0 means there was no winner (used for single player mode), 1 means left player won, 2 means right player won

    public endScreen(GraphicsDevice graphics, SpriteBatch spriteBatch, SpriteFont font, Song backgroundMusic, SpriteFont titleFont, bool _gameWon, int _gameWinner)
    {
        // Load all assets and set them to variables
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
        // Play background music
        MediaPlayer.IsRepeating = true;
        MediaPlayer.Volume = 0.5f;
        MediaPlayer.Play(_backGroundMusic);
    }

    public bool RequestStartGame { get; private set; } = false;

    public override void Update(GameTime gameTime)
    {
        _blinkTimer += gameTime.ElapsedGameTime.TotalSeconds;
        if (_blinkTimer > 0.4) // Speed at which "Press Enter to Restart" blinks
        {
            _showText = !_showText;
            _blinkTimer = 0;
        }

        // Check for Enter key press to restart the game
        if (Keyboard.GetState().IsKeyDown(Keys.Enter))
        {
            MediaPlayer.Stop();
            MediaPlayer.IsRepeating = false;
            RequestTitleScreen = true;
        }
    }

    public override void Draw(GameTime gameTime)
    {
        // Draw the game assets to the screen
        if (gameWon && gameWinner == 0)
        {
            // You won against the AI
            _spriteBatch.DrawString(_titleFont, "You Won! :-)", new Vector2(_graphics.Viewport.Width / 2 - _titleFont.MeasureString("You Won! :-)").X / 2, 400), Color.White);
        }
        else if (gameWon == false && gameWinner == 0)
        {
            // You lost against the AI
            _spriteBatch.DrawString(_titleFont, "You Lost... :-(", new Vector2(_graphics.Viewport.Width / 2 - _titleFont.MeasureString("You Lost... :-(").X, 400) / 2, Color.White);
        }



        if (gameWinner == 1)
        {
            // Left Player won with 2 Player Mode
            _spriteBatch.DrawString(_titleFont, "Left Player Wins! :-)", new Vector2(_graphics.Viewport.Width / 2 - _titleFont.MeasureString("Left Player Wins! :-)").X/2, 400), Color.White);
        }
        else if (gameWinner == 2)
        {
            // Right Player won with 2 Player Mode
            _spriteBatch.DrawString(_titleFont, "Right Player Wins! :-)", new Vector2(_graphics.Viewport.Width / 2 - _titleFont.MeasureString("Right Player Wins! :-)").X/2, 400), Color.White);
        }



        _spriteBatch.DrawString(_font, "PONG", new Vector2(_graphics.Viewport.Width / 2 - _font.MeasureString("PONG").X/2, 100), Color.White);


        // Text that blinks to tells the Player to restart using Enter
        if (_showText)
        {
            _spriteBatch.DrawString(_titleFont, "Press Enter to Restart", new Vector2(_graphics.Viewport.Width / 2 - _titleFont.MeasureString("Press Enter to Restart").X / 2, 800), Color.White);
        }
        
    }
}
