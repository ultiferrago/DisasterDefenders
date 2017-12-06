using System.Collections.Generic;
using UnityEngine;
using ProgressBar;

/// <summary>
/// Represents the game's gameboard.
/// </summary>
public class GameBoard {

    private PersistentData.Settings settings = PersistentData.GetSettings();

    // 2D Array storing active locations of tiles.
    private Tile[,] _boardGrid;

    // Determines the size of the gameboard.
    private int _numberOfRows { get { return settings.NumberOfBoardRows; } }
    private int _numberOfColumns { get { return settings.NumberOfBoardColumns; } }

    // Our random deck of tiles for replacement purposes.
    private ResourceDeck _resourceDeck;

    // Prefab object for creating disaster tiles.
    private GameObject _disasterPrefab;

    // Currently being used to keep track of disasters on the map.
    private readonly HashSet<DisasterTile> _disasters = new HashSet<DisasterTile>();
    private readonly HashSet<DisasterTile> _destroyedDisasters = new HashSet<DisasterTile>();

    // Disaster settings
    //private int _startUpdateTime { get { return settings.InitialDisasterMoveTime; } }
    private int _disasterMoveTime { get { return settings.DisasterTimeDelta; } }
    //private float _updateSpeed;

    // Current wave
    private int _wave = 1;
    private int _wavesPerDisaster;
    private int _maxDisasters;

    // Score stuff
    private int _scorePerDisaster { get { return settings.ScorePerDisaster; } }

    // Progress bar UI for keeping track of disaster time.
    private ProgressBarBehaviour _progressBar;

    /// <summary>
    /// Initializes a new instance of the <see cref="T:GameBoard"/> class.
    /// </summary>
    /// 
    /// <param name="resourcePrefabs">Prefabricated resource tile designs.</param>
    /// <param name="disasterPrefab">Disaster tile prefabricated design.</param>
    public GameBoard( GameObject[] resourcePrefabs, GameObject disasterPrefab ) {
        _wave = 1;

        // Create the array to hold the tiles.
        this._boardGrid = new Tile[_numberOfColumns, _numberOfRows];

        // Create and balance the resource deck.
        _resourceDeck = new ResourceDeck( resourcePrefabs );

        this._disasterPrefab = disasterPrefab;
    }

    /// <summary>
    /// Retrieves a tile using its (x,y) position.
    /// </summary>
    /// 
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// 
    /// <returns>The tile at the given location on the board.</returns>
    public Tile GetTile( int x, int y ) {
        // Ensure that the given location is within our bounds.
        if ( x < 0 || x >= _numberOfColumns || y < 0 || y >= _numberOfRows ) {
            return null;
        }

        // Use the 2D array to determine tile to return.
        return _boardGrid[x, y];
    }

    /// <summary>
    /// Retrieves a tile using it's game object.
    /// </summary>
    /// 
    /// <param name="obj">The gameobject to search for.</param>
    /// 
    /// <returns>The tile containing the gameobject, or null.</returns>
    public Tile GetTile( GameObject obj ) {
        return GetTile( ( int )obj.transform.position.x, ( int )obj.transform.position.y );
    }


    /// <summary>
    /// Handles the initial setup for the grid.
    /// </summary>
    public void Setup() {
        // The number of rows dictates how tall the board is.
        for ( int y = 0; y < _numberOfRows; y++ ) {
            // The number of columns dictates how long each row is.
            for ( int x = 0; x < _numberOfColumns; x++ ) {
                // Grab a random tile from the deck.
                Tile tile = _resourceDeck.DrawNewResourceTile();

                // Ensure no matches are formed.
                while ( WillCauseMatch( x, y, tile ) ) {
                    tile = _resourceDeck.DrawNewResourceTile();
                }

                // Ensure that the gameobject is located on the grid.
                tile.GetGameObject().transform.position = new Vector3( x, y, 0 );
                tile.SetCoordinates( x, y );
                tile.Activate();
                _boardGrid[x, y] = tile;
            }
        }

        SpawnDisaster();
    }

    /// <summary>
    /// Determines if moving the selected tile to the new location will result
    /// in any matches throughout the gameboard.
    /// </summary>
    /// 
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <param name="selectedTile">The tiles that will move.</param>
    /// 
    /// <returns><c>true</c>, if move will cause any matches to be made, <c>false</c> otherwise.</returns>
    private bool WillCauseMatch( int x, int y, Tile selectedTile ) {
        // Check to the left.
        if ( x - 2 >= 0 ) {
            Tile tile1 = GetTile( x - 2, y );
            Tile tile2 = GetTile( x - 1, y );

            if ( tile1 != null && tile2 != null ) {
                if ( selectedTile.DoesMatchTile( tile1 ) && selectedTile.DoesMatchTile( tile2 ) ) {
                    return true;
                }
            }
        }

        // Check to the right
        if ( x + 2 < _numberOfColumns ) {
            Tile tile1 = GetTile( x + 2, y );
            Tile tile2 = GetTile( x + 1, y );

            if ( tile1 != null && tile2 != null ) {
                if ( selectedTile.DoesMatchTile( tile1 ) && selectedTile.DoesMatchTile( tile2 ) ) {
                    return true;
                }
            }
        }

        // check below
        if ( y - 2 >= 0 ) {
            Tile tile1 = GetTile( x, y - 2 );
            Tile tile2 = GetTile( x, y - 1 );

            if ( tile1 != null && tile2 != null ) {
                if ( selectedTile.DoesMatchTile( tile1 ) && selectedTile.DoesMatchTile( tile2 ) ) {
                    return true;
                }
            }
        }

        // check above
        if ( y + 2 < _numberOfRows ) {
            Tile tile1 = GetTile( x, y + 2 );
            Tile tile2 = GetTile( x, y + 1 );

            if ( tile1 != null && tile2 != null ) {
                if ( selectedTile.DoesMatchTile( tile1 ) && selectedTile.DoesMatchTile( tile2 ) ) {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if a tile meets all of the requirements needed to be moved.
    /// </summary>
    /// 
    /// <param name="tile1">The first tile</param>
    /// <param name="tile2">The second tile</param>
    /// 
    /// <returns><c>true</c>, if a swap is possible, <c>false</c> otherwise.</returns>
    public bool CanSwap( Tile tile1, Tile tile2 ) {

        int horzDist = ( int )Mathf.Abs( tile1.GetGridCoordinates().x - tile2.GetGridCoordinates().x );
        int vertDist = ( int )Mathf.Abs( tile1.GetGridCoordinates().y - tile2.GetGridCoordinates().y );

        // If the total distance moved is greater than a single tile then it isn't legit.
        if ( horzDist + vertDist != 1 ) {
            return false;
        }

        if ( tile1.GetType() == typeof( DisasterTile ) || tile2.GetType() == typeof( DisasterTile ) ) {
            return false;
        }

        // Check tiles 2 to the right, 2 to the left, 2 up, 2 down.
        int x1 = ( int )tile1.GetGridCoordinates().x;
        int x2 = ( int )tile2.GetGridCoordinates().x;
        int y1 = ( int )tile1.GetGridCoordinates().y;
        int y2 = ( int )tile2.GetGridCoordinates().y;

        // Swap them on the board so getTile works for the following calls.
        _boardGrid[x1, y1] = tile2;
        _boardGrid[x2, y2] = tile1;

        // Check for matches.
        if ( CheckVerticalMatches( x1, y1 ) || CheckVerticalMatches( x2, y2 ) || CheckHorizontalMatches( x1, y1 ) || CheckHorizontalMatches( x2, y2 ) ) {
            _boardGrid[x1, y1] = tile1;
            _boardGrid[x2, y2] = tile2;
            return true;
        }

        // Always ensure tiles get flopped back in grid.    
        _boardGrid[x1, y1] = tile1;
        _boardGrid[x2, y2] = tile2;
        return false;
    }

    /// <summary>
    /// Checks the vertical matches.
    /// </summary>
    /// <returns><c>true</c>, if vertical matches was checked, <c>false</c> otherwise.</returns>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    private bool CheckVerticalMatches( int x, int y ) {
        Tile tile = GetTile( x, y );

        // How am I supposed to check for matches when the shit is null.
        if ( tile == null ) {
            return false;
        }

        Tile oneBelow = GetTile( x, y - 1 );
        Tile twoBelow = GetTile( x, y - 2 );
        Tile oneAbove = GetTile( x, y + 1 );
        Tile twoAbove = GetTile( x, y + 2 );

        // If tile above and below match me.
        if ( oneBelow != null && oneAbove != null && tile.DoesMatchTile( oneBelow ) && tile.DoesMatchTile( oneAbove ) ) {
            return true;

            // If two below match me.
        } else if ( oneBelow != null && tile.DoesMatchTile( oneBelow ) && twoBelow != null && tile.DoesMatchTile( twoBelow ) ) {
            return true;

            // If two above match me.
        } else if ( oneAbove != null && tile.DoesMatchTile( oneAbove ) && twoAbove != null && tile.DoesMatchTile( twoAbove ) ) {
            return true;
        }

        // We made it here, meaning no this is not a match in any way.
        return false;
    }

    /// <summary>
    /// Checks if the provided location is within a set of horizontally 
    /// matching tiles. (A row of matching tiles).
    /// </summary>
    /// 
    /// <returns><c>true</c>, if this tile is within a column match, <c>false</c> otherwise.</returns>
    /// 
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    private bool CheckHorizontalMatches( int x, int y ) {
        Tile tile = GetTile( x, y );

        // How am I supposed to check for matches when the shit is null.
        if ( tile == null ) {
            return false;
        }

        Tile oneLeft = GetTile( x - 1, y );
        Tile twoLeft = GetTile( x - 2, y );
        Tile oneRight = GetTile( x + 1, y );
        Tile twoRight = GetTile( x + 2, y );

        // If the tile matches the right and left tiles, then we are a match.
        if ( oneLeft != null && oneRight != null && tile.DoesMatchTile( oneLeft ) && tile.DoesMatchTile( oneRight ) ) {
            return true;

            // If the two tiles to the left of the current tile match the current tile.
        } else if ( oneLeft != null && tile.DoesMatchTile( oneLeft ) && twoLeft != null && tile.DoesMatchTile( twoLeft ) ) {
            return true;

            // If the two tiles to the right of the current tile match the current tile.
        } else if ( oneRight != null && tile.DoesMatchTile( oneRight ) && twoRight != null && tile.DoesMatchTile( twoRight ) ) {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Swap to tiles on the gameboard. 
    /// </summary>
    /// 
    /// <returns>The score generated by the performed swap.</returns>
    /// 
    /// <param name="tile1">The first tile involved in the swap.</param>
    /// <param name="tile2">The second tile involved in the swap.</param>
    public int SwapTiles( Tile tile1, Tile tile2 ) {

        if ( tile1 == null ) {
            Debug.Log( "This shit fucked" );
        }
        // Swap tiles in grid.
        int x1 = ( int )tile1.GetGridCoordinates().x;
        int y1 = ( int )tile1.GetGridCoordinates().y;
        int x2 = ( int )tile2.GetGridCoordinates().x;
        int y2 = ( int )tile2.GetGridCoordinates().y;

        PlaceTile( x1, y1, tile2 );
        PlaceTile( x2, y2, tile1 );

        // Remove Matches
        bool foundMoreMatches = true;
        int totalRemoved = 0;

        // Fill in holes.
        while ( foundMoreMatches ) {
            int removedThisLoop = RemoveMatches();

            // If nothing was removed, no more holes to fill, we are done.
            if ( removedThisLoop == 0 ) {
                foundMoreMatches = false;
                break;
            }

            totalRemoved += removedThisLoop;

            RefillBoard();
        }
        // Calculate score - for now 100 * number matched

        return totalRemoved * 100;
    }

    /// <summary>
    /// Scans the board grid and removes all tiles involved in matches.
    /// </summary>
    /// 
    /// <returns>The number of removed tiles.</returns>
    private int RemoveMatches() {
        int removed = 0;

        for ( int y = 0; y < _numberOfRows; y++ ) {
            for ( int x = 0; x < _numberOfColumns; x++ ) {
                Tile currentTile = GetTile( x, y );

                // If this one is in the middle of two tiles in a row
                if ( x - 1 >= 0 && x + 1 < _numberOfColumns ) {
                    Tile leftTile = GetTile( x - 1, y );
                    Tile rightTile = GetTile( x + 1, y );

                    // If all three of these match
                    if ( currentTile.DoesMatchTile( leftTile ) && currentTile.DoesMatchTile( rightTile ) ) {
                        // We have a match. Update all involved
                        leftTile.IsInRowMatch = true;
                        currentTile.IsInRowMatch = true;
                        rightTile.IsInRowMatch = true;
                    }
                }

                if ( y - 1 >= 0 && y + 1 < _numberOfRows ) {
                    Tile downTile = GetTile( x, y - 1 );
                    Tile upTile = GetTile( x, y + 1 );

                    if ( currentTile.DoesMatchTile( upTile ) && currentTile.DoesMatchTile( downTile ) ) {
                        downTile.IsInColumnMatch = true;
                        currentTile.IsInColumnMatch = true;
                        upTile.IsInColumnMatch = true;
                    }
                }
            }
        }

        for ( int y = 0; y < _numberOfRows; y++ ) {
            for ( int x = 0; x < _numberOfColumns; x++ ) {
                if ( GetTile( x, y ) != null && GetTile( x, y ).IsInMatch() ) {
                    int disasterScore = RemoveAdjacentDisasters( GetTile( x, y ) );
                    removed += disasterScore;
                    removed++;
                    RemoveTile( x, y );
                }
            }
        }

        return removed;
    }

    /// <summary>
    /// Repopulates the board with tiles by dropping tiles down into 
    /// empty tile spots, and then spawning new replacement tiles for 
    /// empty grid locations.
    /// </summary>
    void RefillBoard() {
        for ( int y = 0; y < _numberOfRows; y++ ) {
            for ( int x = 0; x < _numberOfColumns; x++ ) {
                Tile tile = GetTile( x, y );

                if ( tile != null ) {
                    continue;
                }

                // This is a null tile, move tiles above it down.
                for ( int aboveY = y; aboveY < _numberOfRows; aboveY++ ) {
                    Tile aboveTile = GetTile( x, aboveY );

                    // If this above tile is null, then move on to next above
                    if ( aboveTile == null ) {
                        continue;
                    }

                    // Swap these guys.
                    PlaceTile( x, y, aboveTile );
                    _boardGrid[x, aboveY] = null;
                    break;
                }
            }
        }

        // Any remaining nulls need to be filled with random card
        for ( int y = 0; y < _numberOfRows; y++ ) {
            for ( int x = 0; x < _numberOfColumns; x++ ) {
                if ( GetTile( x, y ) == null ) {
                    Tile tile = _resourceDeck.DrawNewResourceTile();
                    tile.Activate();

                    PlaceTile( x, y, tile );
                }
            }
        }
    }

    /// <summary>
    /// Places a given tile at a given location.
    /// </summary>
    /// 
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <param name="selectedTile">The tile to place.</param>
    private void PlaceTile( int x, int y, Tile selectedTile ) {
        selectedTile.SetCoordinates( x, y );
        _boardGrid[x, y] = selectedTile;
    }

    /// <summary>
    /// Removes a specific tile at a given location.
    /// </summary>
    /// 
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    private void RemoveTile( int x, int y ) {
        Tile tile = GetTile( x, y );

        if ( tile == null ) {
            return;
        }

        tile.SetCoordinates( -10, -10 );
        tile.Deactivate();

        _resourceDeck.PlaceCardOnBottomOfDeck( tile );

        _boardGrid[x, y] = null;
    }

    /// <summary>
    /// Removes a disaster tile from the game.
    /// </summary>
    /// 
    /// <param name="disasterTile">The disaster to remove from the game.</param>
    private void RemoveDisasterTile( Tile disasterTile ) {

        // Ensure that only disasters are removed here.
        if ( disasterTile == null || disasterTile.GetType() != typeof( DisasterTile ) ) {
            return;
        }


        int x = ( int )disasterTile.GetGridCoordinates().x;
        int y = ( int )disasterTile.GetGridCoordinates().y;

        disasterTile.SetCoordinates( -10, -10 );
        disasterTile.Deactivate();

        _destroyedDisasters.Add( ( DisasterTile )disasterTile );

        if ( x >= 0 && x <= _numberOfColumns && y >= 0 && y <= _numberOfRows ) {
            _boardGrid[x, y] = null;
        }
    }

    /// <summary>
    /// Spawn a single disaster at a random location along the top row of the grid.
    /// </summary>
    private void SpawnDisaster() {

        // Pick a random spot [0...Cols)
        int spot = Random.Range( 0, _numberOfColumns - 1 );

        int tries = 0;

        while ( GetTile( 0, spot ) != null && GetTile( 0, spot ).GetType() == typeof( DisasterTile ) && tries < 7 ) {
            if ( tries++ == _numberOfColumns ) {
                return;
            }

            spot = Random.Range( 0, _numberOfColumns - 1 );
        }

        RemoveTile( spot, _numberOfRows - 1 );

        GameObject disasterClone = Object.Instantiate( _disasterPrefab, new Vector3( spot, _numberOfRows - 1, 0 ), _disasterPrefab.transform.rotation );
        DisasterTile tile = new DisasterTile( disasterClone, _disasterMoveTime );
        _disasters.Add( tile );

        UpdateProgressBar();


        _boardGrid[spot, _numberOfRows - 1] = tile;
    }

    /// <summary>
    /// Returns max disasters allowed on the board currently.
    /// </summary>

    /// <returns>The max active disasters.</returns>
    private int GetMaxActiveDisasters() {

        if ( _wave < _wavesPerDisaster ) {
            return 1;
        }

        // ensure value within range.
        if ( _wave / _wavesPerDisaster > _maxDisasters ) {
            return _maxDisasters;
        }

        return ( int )Mathf.Floor( ( _wave * 1.0f ) / ( _wavesPerDisaster * 1.0f ) );
    }

    /// <summary>
    /// Instantiates progress bar variables.
    /// </summary>
    private void SetupProgressBar() {
        if ( _progressBar == null ) {
            GameObject progressBarObj = GameObject.FindGameObjectWithTag( "ProgressBar" );
            if ( progressBarObj != null && progressBarObj.activeSelf ) {
                _progressBar = progressBarObj.GetComponent<ProgressBarBehaviour>();
            }
        }
    }

    /// <summary>
    /// Update the progress bar with current information.
    /// </summary>
    private void UpdateProgressBar() {
        if ( _progressBar == null ) {
            SetupProgressBar();
        } else {
            _progressBar.SetFillerSizeAsPercentage( 100 - ( ( ( PersistentData.GetSettings().InitialDisasterMoveTime - GetTimeUntilNextDisasterMove() ) / PersistentData.GetSettings().InitialDisasterMoveTime ) * 100 ) );
        }
    }

    /// <summary>
    /// Calculates the time between disasters moving.
    /// </summary>
    /// 
    /// <returns>The amount of time between disaster's movement.</returns>
    //private float GetDisasterTimeBetweenMoves() {

    //}

    /// <summary>
    /// Handles moving disasters if they meet movement requirements.
    /// </summary>
    public void MoveDisasters() {

        if ( _destroyedDisasters.Count > 0 ) {
            foreach ( DisasterTile disaster in _destroyedDisasters ) {
                _disasters.Remove( disaster );
            }
            _destroyedDisasters.Clear();
        }

        if ( _disasters.Count < GetMaxActiveDisasters() ) {
            SpawnDisaster();
        }

        foreach ( DisasterTile disaster in _disasters ) {
            if ( disaster.ReadyForMove() ) {

                int minRange = 1;
                int maxRange = 3;

                if ( disaster.GetGridCoordinates().x - 1 < 0 ) {
                    minRange += 1;
                } else if ( disaster.GetGridCoordinates().x + 1 >= _numberOfColumns ) {
                    maxRange -= 1;
                }

                int which = Random.Range( minRange, maxRange );


                int deltaX = 0;

                // Which random tile was selected
                switch ( which ) {
                    case 1:
                        deltaX = 0;
                        break;
                    case 2:
                        deltaX = 1;
                        break;
                    case 3:
                        deltaX = -1;
                        break;

                }

                if ( disaster.GetGridCoordinates().y - 1 < 0 ) {
                    // Player loses.
                    GameManager.EndGame();
                    return;
                }

                SwapTiles( GetTile( ( int )disaster.GetGridCoordinates().x + deltaX, ( int )disaster.GetGridCoordinates().y - 1 ), disaster );
                disaster.Update();
            }
        }
    }

    /// <summary>
    /// Handles displaying disaster animations.
    /// </summary>
    public void AnimateDisasters() {


        foreach ( DisasterTile disaster in _disasters ) {
            if ( !disaster.IsActive() ||
                disaster.GetGridCoordinates().x < 0 || disaster.GetGridCoordinates().x >= _numberOfColumns ||
                disaster.GetGridCoordinates().y < 0 || disaster.GetGridCoordinates().y >= _numberOfRows ) {

                RemoveDisasterTile( disaster );
                continue;
            }

            disaster.Animate();
        }

        UpdateProgressBar();
    }

    /// <summary>
    /// Gets the current set of active disasters.
    /// </summary>
    /// 
    /// <returns>Disasters actively on the grid.</returns>
    public HashSet<DisasterTile> GetDisasters() {
        return _disasters;
    }

    /// <summary>
    /// Gets the time until the next disaster moves.
    /// </summary>
    /// 
    /// <returns>The time until the next disaster moves.</returns>
    public float GetTimeUntilNextDisasterMove() {
        float minTime = float.MaxValue;
        foreach ( DisasterTile disaster in _disasters ) {
            if ( disaster.GetTimeUntilUpdate() < minTime ) {
                minTime = disaster.GetTimeUntilUpdate();
            }
        }
        return minTime;
    }

    /// <summary>
    /// Removes any disasters that are currently located next to matching 
    /// sets of tiles.
    /// </summary>
    /// 
    /// <returns>The score generated from removed disasters.</returns>
    /// 
    /// <param name="selectedTile">Tile to check for adjacent disasters.</param>
    public int RemoveAdjacentDisasters( Tile selectedTile ) {
        int x = ( int )selectedTile.GetGridCoordinates().x;
        int y = ( int )selectedTile.GetGridCoordinates().y;

        int disastersRemoved = 0;


        Tile left = GetTile( x - 1, y );
        Tile right = GetTile( x + 1, y );
        Tile above = GetTile( x, y + 1 );
        Tile below = GetTile( x, y - 1 );

        if ( left != null && left.GetType() == typeof( DisasterTile ) ) {
            RemoveDisasterTile( left );
            disastersRemoved++;
        }

        if ( right != null && right.GetType() == typeof( DisasterTile ) ) {
            RemoveDisasterTile( right );
            disastersRemoved++;
        }

        if ( above != null && above.GetType() == typeof( DisasterTile ) ) {
            RemoveDisasterTile( above );
            disastersRemoved++;
        }

        if ( below != null && below.GetType() == typeof( DisasterTile ) ) {
            RemoveDisasterTile( below );
            disastersRemoved++;
        }

        return disastersRemoved * _scorePerDisaster;
    }

    /// <summary>
    /// Gets the current wave.
    /// </summary>
    /// 
    /// <returns>The current wave.</returns>
    public int GetCurrentWave() {
        return _wave;
    }

    public void NextWave() {
        _wave++;
    }

    /// <summary>
    /// Stops all active animations.
    /// </summary>
    public void FinishTileAnimations() {
        for ( int y = 0; y < _numberOfRows; y++ ) {
            for ( int x = 0; x < _numberOfColumns; x++ ) {
                _boardGrid[x, y].FinishMovementAnimation();
            }
        }
    }


    /// <summary>
    /// Animates the tiles currently displayed.
    /// </summary>
    public void AnimateTiles() {
        for ( int y = 0; y < _numberOfRows; y++ ) {
            for ( int x = 0; x < _numberOfColumns; x++ ) {
                _boardGrid[x, y].AnimateMovement();
            }
        }
    }
}