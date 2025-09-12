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

    public endScreen(GraphicsDevice graphics, SpriteBatch spriteBatch, SpriteFont font, Song backgroundMusic, SpriteFont titleFont)
    {
        _graphics = graphics;
        _spriteBatch = spriteBatch;
        _font = font;
        _backGroundMusic = backgroundMusic;
        titleFont = _titleFont;
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
            MediaPlayer.IsRepeating = true;
            RequestStartGame = true;
        }
    }

    public override void Draw(GameTime gameTime)
    {
        _graphics.Clear(Color.DarkBlue);

        _spriteBatch.Begin();


        if(gameWon)
        {
            // You won
            _spriteBatch.DrawString(_titleFont, "You Won! :-)", new Vector2(400, 800), Color.White);
        }
        else if (gameWon == false)
        {
            // You lost
            _spriteBatch.DrawString(_titleFont, "You Lost... :-(", new Vector2(400, 800), Color.White);
        }

        _spriteBatch.DrawString(_titleFont, "PONG", new Vector2(400, 800), Color.White);
        if (_showText)
        {
            _spriteBatch.DrawString(_font, "Press Enter to Start", new Vector2(620, 700), Color.White);
        }
        _spriteBatch.End();
    }
}
