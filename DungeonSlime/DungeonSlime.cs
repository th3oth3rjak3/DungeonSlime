using DungeonSlime.Scenes;
using Microsoft.Xna.Framework.Media;
using MonoGameLibrary.Audio;

namespace DungeonSlime;

public class DungeonSlime : Core
{
    // The background theme song.
    private Song? _themeSong;

    public DungeonSlime() : base("Dungeon Slime", 1280, 720, false)
    {

    }

    protected override void Initialize()
    {
        base.Initialize();

        ArgumentNullException.ThrowIfNull(_themeSong);

        // Start playing the background music.
        AudioController.PlaySong(_themeSong);

        // Start the game with the title scene.
        ChangeScene(new TitleScene());
    }

    protected override void LoadContent()
    {
        // Load the background theme music.
        _themeSong = Content.Load<Song>("audio/theme");
    }
}
