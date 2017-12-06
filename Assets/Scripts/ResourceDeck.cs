using System.Collections.Generic;
using CustomExtensions;
using UnityEngine;

/// <summary>
/// Handles the available tiles to be placed onto grid.
/// </summary>
public class ResourceDeck {
    // Settings
    private PersistentData.Settings _settings = PersistentData.GetSettings();

    // Keep a copy of the originals we are using to make our clone army. 
    private GameObject[] _resourcePrefabs;

    // The list containing our cards. 
    private readonly Queue<Tile> _resourceTileDeck = new Queue<Tile>();

    // How many times can a card be requested before a reshuffle occurs. (Set to 0 for dynamically set)
    private int _drawsUntilShuffle = 0;
    private int _currentNumOfDraws = 0;

    // Settings
    private int _numOfEachResourceType { get { return _settings.NumOfDuplicateResourceTypes; } }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:ResourceDeck"/> class.
    /// </summary>
    /// 
    /// <param name="prefabs">Prefabsicated resources for tiles.</param>
    public ResourceDeck( GameObject[] prefabs ) {
        this._resourcePrefabs = prefabs;

        // Create clones for each of our prefabs
        List<Tile> resourceTileList = new List<Tile>();


        _drawsUntilShuffle = ( int )System.Math.Ceiling( _resourcePrefabs.Length * _numOfEachResourceType * 0.75 );

        foreach ( GameObject prefab in _resourcePrefabs ) {
            // Create however many duplicates we want for each prefab.
            for ( int i = 0; i < _numOfEachResourceType; i++ ) {
                GameObject clone = ( GameObject )Object.Instantiate( prefab, new Vector3( -10, -10, 0 ), prefab.transform.rotation );
                clone.SetActive( false );
                Tile cloneTile = new Tile( clone );
                resourceTileList.Add( cloneTile );
            }
        }

        resourceTileList.Shuffle();

        foreach ( Tile tile in resourceTileList ) {
            _resourceTileDeck.Enqueue( tile );
        }
    }

    /// <summary>
    /// Shuffles the resource deck.
    /// </summary>
	private void ShuffleDeck() {
        List<Tile> resourceList = new List<Tile>( _resourceTileDeck.ToArray() );

        // Clear current tile deck.
        _resourceTileDeck.Clear();

        // Shuffle using extension
        resourceList.Shuffle();

        // Add back to the queue.
        foreach ( Tile tile in resourceList ) {
            _resourceTileDeck.Enqueue( tile );
        }

        _drawsUntilShuffle = 0;
    }

    /// <summary>
    /// Draws the next resource card from the top of the deck.
    /// </summary>
    /// 
    /// <returns>The next resource card.</returns>
    public Tile DrawNewResourceTile() {
        // If the deck needs to be reshuffled.
        if ( _currentNumOfDraws == _drawsUntilShuffle ) {
            ShuffleDeck();
        }

        // Get next card off top of deck
        Tile tile = _resourceTileDeck.Dequeue();
        tile.SetJustDrawn();

        _drawsUntilShuffle++;

        return tile;
    }

    /// <summary>
    /// Returns the given tile to the bottom of the deck.
    /// </summary>
    /// 
    /// <param name="discardedTile">The tile to return to the deck.</param>
    public void PlaceCardOnBottomOfDeck( Tile discardedTile ) {
        _resourceTileDeck.Enqueue( discardedTile );
    }
}
