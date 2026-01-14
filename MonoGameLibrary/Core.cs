namespace MonoGameLibrary;

public class Core : Game
{
    internal static Core? s_Instance;
    private static GraphicsDeviceManager? s_Graphics;
    private static GraphicsDevice? s_GraphicsDevice;
    private static SpriteBatch? s_SpriteBatch;
    private static ContentManager? s_Content;
    private static InputManager? s_Input;


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
    }

    protected override void Update(GameTime gameTime)
    {
        // Update the input manager.
        Input.Update(gameTime);

        if (ExitOnEscape && Input.Keyboard.IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        base.Update(gameTime);
    }
}
