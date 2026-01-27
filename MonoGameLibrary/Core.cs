namespace MonoGameLibrary;

public class Core : Game
{
    internal static Core? s_Instance;
    private static GraphicsDeviceManager? s_Graphics;
    private static GraphicsDevice? s_GraphicsDevice;
    private static SpriteBatch? s_SpriteBatch;
    private static ContentManager? s_Content;
    private static InputManager? s_Input;
    private static AudioController? s_Audio;

    // The scene that is currently active.
    private static Scene? s_activeScene;

    // The next scene to switch to, if there is one.
    private static Scene? s_nextScene;


    /// <summary>
    /// Gets a reference to the core instance.
    /// </summary>
    public static Core Instance => s_Instance
        ?? throw new NullReferenceException("A core instance must be created before Instance can be called");

    /// <summary>
    /// Gets the graphics device manager to control the presentation of graphics.
    /// </summary>
    public static GraphicsDeviceManager Graphics
    {
        get => s_Graphics ?? throw new NullReferenceException("A core instance must be created before Graphics can be called");
        private set => s_Graphics = value;
    }

    /// <summary>
    /// Gets the graphics device used to create graphical resources and perform primitive rendering.
    /// </summary>
    public static new GraphicsDevice GraphicsDevice
    {
        get => s_GraphicsDevice
            ?? throw new NullReferenceException("A core instance must be Initialized before GraphicsDevice can be called");
        private set => s_GraphicsDevice = value;
    }

    /// <summary>
    /// Gets the sprite batch used for all 2D rendering.
    /// </summary>
    public static SpriteBatch SpriteBatch
    {
        get => s_SpriteBatch
            ?? throw new NullReferenceException("A core instance must be Initialized before SpriteBatch can be called");
        private set => s_SpriteBatch = value;
    }

    /// <summary>
    /// Gets the content manager used to load global assets.
    /// </summary>
    public static new ContentManager Content
    {
        get => s_Content
            ?? throw new NullReferenceException("A core instance must be created before Content can be called");
        private set => s_Content = value;
    }

    /// <summary>
    /// Gets a reference to the input management system.
    /// </summary>
    public static InputManager Input
    {
        get => s_Input
            ?? throw new NullReferenceException("A core instance must be created before Input can be called");
        private set => s_Input = value;
    }

    /// <summary>
    /// Gets or Sets a value that indicates if the game should exit when the esc key on the keyboard is pressed.
    /// </summary>
    public static bool ExitOnEscape { get; set; }

    /// <summary>
    /// Gets a reference to the audio control system.
    /// </summary>
    public static AudioController Audio
    {
        get => s_Audio
            ?? throw new NullReferenceException("A core instance must be initialized before Audio can be called");
        private set => s_Audio = value;
    }

    /// <summary>
    /// Creates a new Core instance.
    /// </summary>
    /// <param name="title">The title to display.</param>
    /// <param name="width">The initial width, in pixels, of the game window.</param>
    /// <param name="height">The initial height, in pixels, of the game window.</param>
    /// <param name="fullScreen">Indicates if the game should start in fullscreen mode.</param>
    /// <exception cref="InvalidOperationException"></exception>
    public Core(string title, int width, int height, bool fullScreen)
    {
        if (s_Instance is not null)
        {
            throw new InvalidOperationException("Only a single Core instance can be created");
        }

        // Store reference to engine for global member access
        s_Instance = this;

        // Create a new graphics device manager with default values.
        Graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = width,
            PreferredBackBufferHeight = height,
            IsFullScreen = fullScreen
        };

        // Apply any pending property changes.
        Graphics.ApplyChanges();

        // Set the window title.
        Window.Title = title;

        // Set the core's content manager to be the same as the base Game's Content Manager
        Content = base.Content;

        // Set the name of the directory where content will live.
        Content.RootDirectory = "Content";

        // Enable mouse visibility by default.
        IsMouseVisible = true;

        // Exit on escape is true by default.
        ExitOnEscape = true;
    }

    /// <summary>
    /// Initialize the core instance, required to setup the GraphicsDevice and SpriteBatch.
    /// </summary>
    protected override void Initialize()
    {
        base.Initialize();

        // Set the GraphicsDevice to the base game's Graphics Device.
        GraphicsDevice = base.GraphicsDevice;

        // Create a new sprite batch instance.
        SpriteBatch = new SpriteBatch(GraphicsDevice);

        // Create a new InputManager
        Input = new InputManager();

        // Create a new AudioController.
        Audio = new AudioController();
    }

    protected override void LoadContent()
    {
        base.LoadContent();
    }

    protected override void UnloadContent()
    {
        // Dispose the audio controller.
        Audio.Dispose();

        base.UnloadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        // Update the input manager.
        Input.Update(gameTime);

        // Update the audio controller.
        Audio.Update();

        if (ExitOnEscape && Input.Keyboard.IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        // if there is a next scene waiting to be switched to, then transition
        // to that scene
        if (s_nextScene is not null)
        {
            TransitionScene();
        }

        // If there is an active scene, update it.
        s_activeScene?.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // If there is an active scene, draw it.
        s_activeScene?.Draw(gameTime);

        base.Draw(gameTime);
    }

    public static void ChangeScene(Scene next)
    {
        // Only set the next scene value if it is not the same
        // instance as the currently active scene.
        if (s_activeScene != next)
        {
            s_nextScene = next;
        }
    }

    private static void TransitionScene()
    {
        // If there is an active scene, dispose of it.
        s_activeScene?.Dispose();

        // Force the garbage collector to collect to ensure memory is cleared.
        GC.Collect();

        // Change the currently active scene to the new scene.
        s_activeScene = s_nextScene;

        // Null out the next scene value so it does not trigger a change over and over.
        s_nextScene = null;

        // If the active scene now is not null, initialize it.
        // Remember, just like with Game, the Initialize call also calls the
        // Scene.LoadContent
        s_activeScene?.Initialize();
    }
}
