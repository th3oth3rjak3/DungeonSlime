namespace MonoGameLibrary.Graphics;

public class TextureAtlas
{
    /// <summary>
    /// A collection of all the texture regions managed by this atlas.
    /// </summary>
    private readonly Dictionary<string, TextureRegion> _regions;

    /// <summary>
    /// A collection of animations for each texture region managed by this atlas.
    /// </summary>
    private readonly Dictionary<string, Animation> _animations;

    /// <summary>
    /// Gets or sets the source texture represented by this texture atlas.
    /// </summary>
    public required Texture2D Texture { get; set; }

    /// <summary>
    /// Creates a new texture atlas to be configured later.
    /// </summary>
    public TextureAtlas()
    {
        _regions = [];
        _animations = [];
    }

    /// <summary>
    /// Creates a new texture atlas using the provided source texture.
    /// </summary>
    /// <param name="texture">The source texture represented by the texture atlas.</param>
    public TextureAtlas(Texture2D texture)
    {
        Texture = texture;
        _regions = [];
        _animations = [];
    }

    /// <summary>
    /// Create a new region and add it to this texture atlas.
    /// </summary>
    /// <param name="name">The name of the region.</param>
    /// <param name="x">The top-left x-coordinate position of the region boundary relative to the source texture boundary.</param>
    /// <param name="y">The top-left y-coordinate position of the region boundary relative to the source texture boundary.</param>
    /// <param name="width">The width, in pixels, of the region.</param>
    /// <param name="height">The height, in pixels, of the region.</param>
    public void AddRegion(string name, int x, int y, int width, int height)
    {
        if (Texture is null)
        {
            throw new NullReferenceException("Texture must be set before adding regions");
        }

        var region = new TextureRegion(Texture, x, y, width, height);
        _regions.Add(name, region);
    }

    /// <summary>
    /// Gets the region from this texture atlas with the specified name.
    /// </summary>
    /// <param name="name">The name of the region to retrieve.</param>
    /// <returns>The TextureRegion with the specified name.</returns>
    public TextureRegion GetRegion(string name)
    {
        if (_regions.TryGetValue(name, out var texture))
        {
            return texture;
        }

        throw new InvalidDataException($"Region does not exist: {name}");
    }

    /// <summary>
    /// Removes the region from this texture atlas with the specified name.
    /// </summary>
    /// <param name="name">The name of the region to remove.</param>
    /// <returns>True when found and removed, otherwise false.</returns>
    public bool RemoveRegion(string name)
    {
        return _regions.Remove(name);
    }

    /// <summary>
    /// Removes all regions from this texture atlas.
    /// </summary>
    public void Clear()
    {
        _regions.Clear();
    }

    /// <summary>
    /// Adds the given animation to this texture atlas with the specified name.
    /// </summary>
    /// <param name="animationName">The name of the animation to add.</param>
    /// <param name="animation">The animation to add.</param>
    public void AddAnimation(string animationName, Animation animation)
    {
        _animations.Add(animationName, animation);
    }

    /// <summary>
    /// Gets the animation from this texture atlas with the specified name.
    /// </summary>
    /// <param name="animationName">The name of the animation to retrieve.</param>
    /// <returns>The animation with the specified name.</returns>
    public Animation GetAnimation(string animationName)
    {
        if (_animations.TryGetValue(animationName, out var animation))
        {
            return animation;
        }

        throw new InvalidDataException($"Animation does not exist: {animationName}");
    }

    /// <summary>
    /// Removes the animation with the specified name from this texture atlas.
    /// </summary>
    /// <param name="animationName">The name of the animation to remove.</param>
    /// <returns>true if the animation is removed successfully; otherwise, false.</returns>
    public bool RemoveAnimation(string animationName)
    {
        return _animations.Remove(animationName);
    }

    /// <summary>
    /// Creates a new texture atlas based on a texture atlas xml configuration file.
    /// </summary>
    /// <param name="content">The ContentManager used to load the texture for the atlas.</param>
    /// <param name="fileName">The path to the xml file, relative to the content root directory.</param>
    /// <returns>A new texture atlas.</returns>
    public static TextureAtlas FromFile(ContentManager content, string fileName)
    {
        string filePath = Path.Combine(content.RootDirectory, fileName);

        using var stream = TitleContainer.OpenStream(filePath);
        using var reader = XmlReader.Create(stream);
        var doc = XDocument.Load(reader);
        var root = doc.Root
            ?? throw new NullReferenceException("XML Document root was null when trying to load the texture atlas");

        // The <Texture> element contains the content path for the Texture2D to load.
        // So we will retrieve that value and then use the content manager to load the texture.
        var texturePath = root.Element("Texture")?.Value
            ?? throw new InvalidDataException("Texture configuration incorrect, missing Texture element");

        var atlas = new TextureAtlas()
        {
            Texture = content.Load<Texture2D>(texturePath),
        };

        // The <Regions> element contains individual <Region> elements, each one describing
        // a different texture region within the atlas.
        //
        // Example: 
        // <Regions>
        //     <Region name="spriteOne" x="0" y="0" width="32" height="32" />
        //     <Region name="spriteTwo" x="32" y="0" width="32" height="32 />
        // </Regions>
        //
        // So, we retrieve all of the <Region> elements, then loop through each one
        // and generate a new TextureRegion instance from it and add it to the atlas.

        var regions = root.Element("Regions")?.Elements("Region")
            ?? throw new InvalidDataException("Texture configuration incorrect, missing Regions element");

        foreach (var region in regions)
        {
            var name = region.Attribute("name")?.Value
                ?? throw new InvalidDataException("Texture configuration incorrect, Region element requires the name property");

            var x = int.Parse(region.Attribute("x")?.Value
                ?? throw new InvalidDataException("Texture configuration incorrect, Region element requires the x property"));

            var y = int.Parse(region.Attribute("y")?.Value
                ?? throw new InvalidDataException("Texture configuration incorrect, Region element requires the y property"));

            var width = int.Parse(region.Attribute("width")?.Value
                ?? throw new InvalidDataException("Texture configuration incorrect, Region element requires the width property"));

            var height = int.Parse(region.Attribute("height")?.Value
                ?? throw new InvalidDataException("Texture configuration incorrect, Region element requires the height property"));

            atlas.AddRegion(name, x, y, width, height);
        }

        // The <Animations> element contains individual <Animation> elements, each one describing
        // a different animation within the atlas.
        //
        // Example:
        // <Animations>
        //      <Animation name="animation" delay="100">
        //          <Frame region="spriteOne" />
        //          <Frame region="spriteTwo" />
        //      </Animation>
        // </Animations>
        //
        // So we retrieve all of the <Animation> elements then loop through each one
        // and generate a new Animation instance from it and add it to this atlas.
        var animationElements = root.Element("Animations")?.Elements("Animation") ?? [];

        foreach (var animationElement in animationElements)
        {
            var name = animationElement.Attribute("name")?.Value
                ?? throw new InvalidDataException("Animation configuration incorrect, Animation element requires the name property");

            float delayInMilliseconds = float.Parse(animationElement.Attribute("delay")?.Value
                ?? throw new InvalidDataException("Animation configuration incorrect, Animation element requires the delay property"));

            var delay = TimeSpan.FromMilliseconds(delayInMilliseconds);

            List<TextureRegion> frames = [];

            var frameElements = animationElement.Elements("Frame") ??
                throw new InvalidDataException("Animation configuration incorrect. Animation must have Frame elements.");

            foreach (var frameElement in frameElements)
            {
                var regionName = frameElement.Attribute("region")?.Value
                    ?? throw new InvalidDataException("Animation configuration incorrect. Frame must have a region property.");

                TextureRegion region = atlas.GetRegion(regionName);
                frames.Add(region);
            }

            var animation = new Animation(frames, delay);
            atlas.AddAnimation(name, animation);
        }

        return atlas;
    }

    /// <summary>
    /// Creates a new sprite using the region from this texture atlas with the specified name.
    /// </summary>
    /// <param name="regionName">The name of the region to create the sprite with.</param>
    /// <returns>A new Sprite using the texture region with the specified name.</returns>
    public Sprite CreateSprite(string regionName)
    {
        var region = GetRegion(regionName);
        return new Sprite(region);
    }

    /// <summary>
    /// Creates a new animated sprite using the animation from this texture atlas with the specified name.
    /// </summary>
    /// <param name="animationName">The name of the animation to use.</param>
    /// <returns>A new AnimatedSprite using the animation with the specified name.</returns>
    public AnimatedSprite CreateAnimatedSprite(string animationName)
    {
        var animation = GetAnimation(animationName);
        return new AnimatedSprite(animation);
    }
}
