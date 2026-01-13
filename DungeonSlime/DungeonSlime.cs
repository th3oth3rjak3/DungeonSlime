using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGameLibrary;

namespace DungeonSlime;

public class DungeonSlime : Core
{
    private Texture2D? _logo;

    public DungeonSlime() : base("Dungeon Slime", 1280, 720, false) { }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();
    }

    protected override void LoadContent()
    {
        // TODO: use the MGCB tool to load the logo from the tutorial.
        _logo = Content.Load<Texture2D>("images/logo");

        base.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        SpriteBatch.Begin();

        SpriteBatch.Draw(
            _logo,
            new Vector2(Window.ClientBounds.Width, Window.ClientBounds.Height) * 0.5f,
            null,
            Color.White,
            MathHelper.ToRadians(0),
            new Vector2(_logo?.Width ?? 0, _logo?.Height ?? 0) * 0.5f,
            1.0f,
            SpriteEffects.None,
            0.0f
        );

        SpriteBatch.End();

        base.Draw(gameTime);
    }
}
