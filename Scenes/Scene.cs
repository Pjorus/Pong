using Microsoft.Xna.Framework;

namespace Pong.Scenes;

public abstract class Scene
{
    public abstract void Update(GameTime gameTime);
    public abstract void Draw(GameTime gameTime);
}
