using System.Threading;

namespace MonoGameLibrary.Graphics;

/// <summary>
/// Creates a new tilemap.
/// </summary>
/// <param name="tileset">The tileset used by this tilemap.</param>
/// <param name="columns">The total number of columns in this tilemap.</param>
/// <param name="rows">The total number of rows in this tilemap.</param>
public class Tilemap(Tileset tileset, int columns, int rows)
{
	private readonly Tileset _tileset = tileset;
	private readonly int[] _tiles = new int[columns * rows];

	/// <summary>
	/// Gets the total number of rows in this tilemap.
	/// </summary>
	public int Rows { get; } = rows;

	/// <summary>
	/// Gets the total number of columns in this tilemap.
	/// </summary>
	public int Columns { get; } = columns;

	/// <summary>
	/// Gets the total number of tiles in this tilemap.
	/// </summary>
	public int Count { get; } = columns * rows;

	/// <summary>
	/// Gets or Sets the scale factor to draw each tile at.
	/// </summary>
	public Vector2 Scale { get; set; } = Vector2.One;

	/// <summary>
	/// Gets the width, in pixels, each tile is drawn at.
	/// </summary>
	public float TileWidth => _tileset.TileWidth * Scale.X;

	/// <summary>
	/// Gets the height, in pixels, each tile is drawn at.
	/// </summary>
	public float TileHeight => _tileset.TileHeight * Scale.Y;


	/// <summary>
	/// Sets the tile at the given index in this tilemap to use the tile
	/// from the tileset at the specified tileset id.
	/// </summary>
	/// <param name="index">The index of the tile in this tilemap.</param>
	/// <param name="tilesetID">The tileset id of the tile from the tileset to use.</param>
	public void SetTile(int index, int tilesetID)
	{
		_tiles[index] = tilesetID;
	}

	/// <summary>
	/// Sets the tile at the given column and row in this tilemap to use the tile
	/// from the tileset at the specified tileset id.
	/// </summary>
	/// <param name="column">The column of the tile in this tilemap.</param>
	/// <param name="row">The row of the tile in this tilemap.</param>
	/// <param name="tilesetID">The tileset id of the tile from the tileset to use.</param>
	public void SetTile(int column, int row, int tilesetID)
	{
		var index = (row * Columns) + column;
		SetTile(index, tilesetID);
	}

	/// <summary>
	/// Gets the texture region of the tile from this tilemap at the specified index.
	/// </summary>
	/// <param name="index">The index of the tile in this tilemap.</param>
	/// <returns>The texture region of the tile from this tilemap at the specified index.</returns>
	public TextureRegion GetTile(int index)
	{
		var tilesetID = _tiles[index];
		return _tileset.GetTile(tilesetID);
	}

	/// <summary>
	/// Gets the texture region of the tile from this tilemap at the specified column and row.
	/// </summary>
	/// <param name="column">The column of the tile in this tilemap (0-based).</param>
	/// <param name="row">The row of the tile in this tilemap (0-based).</param>
	/// <returns>The texture region of the tile from this tilemap at the specified column and row.</returns>
	public TextureRegion GetTile(int column, int row)
	{
		var index = (row * Columns) + column;
		return GetTile(index);
	}

	/// <summary>
	/// Draws this tilemap using the given sprite batch.
	/// </summary>
	/// <param name="spriteBatch">The sprite batch used to draw this tilemap.</param>
	public void Draw(SpriteBatch spriteBatch)
	{
		for (int i = 0; i < Count; i++)
		{
			var tilesetID = _tiles[i];
			var tile = _tileset.GetTile(tilesetID);

			var x = i % Columns;
			var y = i / Columns;

			var position = new Vector2(
				x * TileWidth,
				y * TileHeight
			);

			tile.Draw(
				spriteBatch,
				position,
				Color.White,
				0.0f,
				Vector2.Zero,
				Scale,
				SpriteEffects.None,
				1.0f
			);
		}
	}

	/// <summary>
	/// Creates a new tilemap based on a tilemap xml configuration file.
	/// </summary>
	/// <param name="content">The content manager used to load the texture for the tileset.</param>
	/// <param name="filename">The path to the XML file, relative to the content root directory.</param>
	/// <returns>The tilemap created by this method.</returns>
	public static Tilemap FromFile(ContentManager content, string filename)
	{
		var filepath = Path.Combine(content.RootDirectory, filename);
		using var stream = TitleContainer.OpenStream(filepath);
		using var reader = XmlReader.Create(stream);
		var doc = XDocument.Load(reader);
		var root = doc.Root ?? throw new NullReferenceException("Tilemap XML Configuration Invalid: Missing Document Root");

		// The <Tileset> element contains the information about the tileset
		// used by the tilemap.
		//
		// Example
		// <Tileset region="0 0 100 100" tileWidth="10" tileHeight="10">contentPath</Tileset>
		//
		// The region attribute represents the x, y, width, and height
		// components of the boundary for the texture region within
		// the texture at the contentPath specified.
		//
		// The tileWidth and the tileHeight attributes specify the width and
		// height of each tile in the tileset.
		// 
		// The contentPath value is the contentPath to the texture to load that
		// contains the tileset.
		var tilesetElement = root.Element("Tileset")
			?? throw new NullReferenceException("Tilemap XML Configuration Invalid: Missing required Tileset tag.");

		var regionAttribute = tilesetElement.Attribute("region")?.Value
			?? throw new NullReferenceException("Tilemap XML Configuration Inavalid: Missing required region attribute in Tileset tag.");

		var split = regionAttribute.Split(" ", StringSplitOptions.RemoveEmptyEntries);
		if (split.Length < 4)
		{
			throw new InvalidDataException("Tilemap XML Configuration Invalid: Missing 4 required region values.");
		}

		if (!int.TryParse(split[0], out var x))
		{
			throw new InvalidCastException("Tilemap XML Configuration Invalid: First region parameter was not a valid integer.");
		}

		if (!int.TryParse(split[1], out var y))
		{
			throw new InvalidCastException("Tilemap XML Configuration Invalid: Second region parameter was not a valid integer.");
		}


		if (!int.TryParse(split[2], out var width))
		{
			throw new InvalidCastException("Tilemap XML Configuration Invalid: Third region parameter was not a valid integer.");
		}

		if (!int.TryParse(split[3], out var height))
		{
			throw new InvalidCastException("Tilemap XML Configuration Invalid: Fourth region parameter was not a valid integer.");
		}

		var tileWidthAttribute = tilesetElement.Attribute("tileWidth")?.Value
			?? throw new NullReferenceException("Tilemap XML Configuraiton Invalid: Missing required tileWidth attribute.");

		var tileHeightAttribute = tilesetElement.Attribute("tileWidth")?.Value
			?? throw new NullReferenceException("Tilemap XML Configuration Invalid: Missing required tileHeight attribute.");

		if (!int.TryParse(tileWidthAttribute, out var tileWidth))
		{
			throw new InvalidCastException("Tilemap XML Configuration Invalid: tileWidth parameter was not a valid integer.");
		}

		if (!int.TryParse(tileHeightAttribute, out var tileHeight))
		{
			throw new InvalidCastException("Tilemap XML Configuration Invalid: tileHeight parameter was not a valid integer.");
		}

		var contentPath = tilesetElement.Value
			?? throw new InvalidDataException("Tilemap XML Configuration Invalid: Missing contentPath data in Tileset tag.");

		// Load the texture at the content path.
		var texture = content.Load<Texture2D>(contentPath);

		// Create the texture region from the texture.
		var textureRegion = new TextureRegion(texture, x, y, width, height);

		// Create the tileset using the texture region.
		var tileset = new Tileset(textureRegion, tileWidth, tileHeight);

		// The <Tiles> element contains lines of strings where each line
		// represents a row in the tilemap. Each line is a space
		// separated string where each element represents a column in that row.
		// The value of the column is the ID of the tile in the tileset to draw
		// for that location.
		//
		// Example
		// <Tiles>
		//		00 01 01 02
		//		03 04 04 05
		//		03 04 04 05
		//		06 07 07 08
		// </Tiles>
		var tilesElement = root.Element("Tiles")
			?? throw new NullReferenceException("Tilemap XML Configuration Invalid: Missing required Tiles element.");

		// Split the value of the tiles data into rows by splitting on
		// the new line character.
		var rows = tilesElement.Value.Trim().Split('\n', StringSplitOptions.RemoveEmptyEntries);

		if (rows.Length < 1)
		{
			throw new InvalidDataException("Tilemap XML Configuration Invalid: Tiles element contained no data.");
		}

		// Check the first row count to determine how many columns there are
		var columnCount = rows[0].Split(" ", StringSplitOptions.RemoveEmptyEntries).Length;

		// Create the new tilemap.
		var tilemap = new Tilemap(tileset, columnCount, rows.Length);

		// Process each row
		for (int row = 0; row < rows.Length; row++)
		{
			// Split the row into individual columns.
			var columns = rows[row].Trim().Split(" ", StringSplitOptions.RemoveEmptyEntries);

			// Process each column of the current row.
			for (var column = 0; column < columnCount; column++)
			{
				// Get the tileset index for this location.
				if (!int.TryParse(columns[column], out var tilesetIndex))
				{
					throw new InvalidDataException("Tilemap XML Configuration Invalid: Tile data was not a valid integer.");
				}

				// Add that region to the tilemap at the row and column location.
				tilemap.SetTile(column, row, tilesetIndex);
			}
		}

		return tilemap;
	}
}
