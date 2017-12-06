using UnityEngine;

/// <summary>
/// Implementation of a single tile object.
/// </summary>
public class Tile {

    private GameObject _tileObj;        // Keeps a pointer to the specific tile. 
    private ResourceType _type;         // Keep track of what type of tile this is.

    // Enum resource types.
    public enum ResourceType { Sun, Earth, Wind, Water, Tree };

    // Current grid location
    private Vector3 _gridCoords;

    // Keeps track of matches.
    public bool IsInColumnMatch {
        get; set;
    }
    public bool IsInRowMatch {
        get; set;
    }

    // How long should it the move animation take, in seconds. 
    private static float _moveSpeed = 0.5f;
    private float _xDelta = 0;
    private float _yDelta = 0;
    private float _totalDistance = 0;

    // State information.
    private bool _justDrawn = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="T:Tile"/> class.
    /// </summary>
    /// <param name="obj">Gameobject this tile will represent.</param>
    public Tile( GameObject obj ) {
        _tileObj = obj;

        _gridCoords = new Vector3( obj.transform.position.x,obj.transform.position.y,0 );

        // Determine resource type from prefab name.
        if ( _tileObj.name.ToLower().Contains( "sun" ) ) {
            _type = ResourceType.Sun;
        } else if ( _tileObj.name.ToLower().Contains( "earth" ) ) {
            _type = ResourceType.Earth;
        } else if ( _tileObj.name.ToLower().Contains( "wind" ) ) {
            _type = ResourceType.Wind;
        } else if ( _tileObj.name.ToLower().Contains( "water" ) ) {
            _type = ResourceType.Water;
        } else if ( _tileObj.name.ToLower().Contains( "tree" ) ) {
            _type = ResourceType.Tree;
        }

        // Reset match info.
        IsInRowMatch = false;
        IsInColumnMatch = false;
    }

    /// <summary>
    /// Sets the tile's coordiantes.
    /// </summary>
    /// 
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
	public void SetCoordinates( int x,int y ) {

        // If we move then reset match info.
        IsInColumnMatch = false;
        IsInRowMatch = false;

        // Don't animate initial move.
        if ( x == -10 && y == -10 ) {
            _tileObj.transform.position = new Vector3( x,y,0 );
        }

        // If the object isn't active we don't care if object teleports
        if ( !IsActive() ) {
            _tileObj.transform.position = new Vector3( x,y,0 );
        }

        // Determine what direction, if any, the tile is moving in.
        bool xSame = ( int )_tileObj.transform.position.x == ( int )_gridCoords.x;
        bool ySame = ( int )_tileObj.transform.position.y == ( int )_gridCoords.y;
        bool previouslyInactive = ( int )_tileObj.transform.position.x == -10 || ( int )_tileObj.transform.position.y == -10;

        // They are not resetting it to (-10, -10) and it was previously at (-10, -10)
        if ( !xSame && !ySame && previouslyInactive ) {
            _tileObj.transform.position = new Vector3( x,y,0 );
        }

        // Reset delta information.
        _xDelta = 0;
        _yDelta = 0;
        _totalDistance = 0;

        // Ultimate goal of where we want to go.
        _gridCoords = new Vector3( x,y,0 );

    }

    /// <summary>
    /// Gets the grid coordinates.
    /// </summary>
    /// 
    /// <returns>The location within the grid.</returns>
	public Vector3 GetGridCoordinates() {
        return _gridCoords;
    }

    /// <summary>
    /// Activate the gameobject this tile represents.
    /// </summary>
	public void Activate() {
        _tileObj.SetActive( true );
    }

    /// <summary>
    /// Deactivate the gameobject this tile represents.
    /// </summary>
	public void Deactivate() {
        _tileObj.SetActive( false );
    }

    /// <summary>
    /// Checks if the tile is within the grid.
    /// </summary>
    /// 
    /// <returns><c>true</c>, if the tile is active <c>false</c> otherwise.</returns>
	public bool IsActive() {
        return _tileObj.activeSelf;
    }

    /// <summary>
    /// Gets the type of resource this tile is.
    /// </summary>
    /// 
    /// <returns>The resource type of this tile.</returns>
	public ResourceType GetResourceType() {
        return _type;
    }

    /// <summary>
    /// Checks if this tile matches the provided tile
    /// </summary>
    /// 
    /// <returns><c>true</c>, if this tile matches the passed tile, <c>false</c> otherwise.</returns>
    /// 
    /// <param name="testTile">Tile to check for equality.</param>
    public bool DoesMatchTile( Tile testTile ) {
        return testTile.GetResourceType() == _type;
    }

    /// <summary>
    /// Checks if this tile is in a matching set.
    /// </summary>
    /// 
    /// <returns><c>true</c>, if this tile is in a matching set, <c>false</c> otherwise.</returns>
	public virtual bool IsInMatch() {
        return IsInColumnMatch || IsInRowMatch;
    }

    /// <summary>
    /// Gets the game object this tile represents.
    /// </summary>
    /// 
    /// <returns>The tile game object.</returns>
	public GameObject GetGameObject() {
        return _tileObj;
    }

    /// <summary>
    /// Animate this tile.
    /// </summary>
	public void Animate() {
        AnimateMovement();
    }

    /// <summary>
    /// Animates tile's movement if tile was moved.
    /// </summary>
	public void AnimateMovement() {
        if ( _justDrawn ) {
            // This object was just drawn, meaning that it needs to fall from the top into its spot.
            _tileObj.transform.position = new Vector3( _gridCoords.x,9,0 );
            _justDrawn = false;
        }

        if ( _gridCoords != _tileObj.transform.position ) {
            if ( _xDelta == 0 && _yDelta == 0 && _tileObj.transform.position.x != -10 && _tileObj.transform.position.y != -10 && _gridCoords.x != -10 && _gridCoords.y != -10 ) {
                //float framesPerSecond = (60 / Time.deltaTime);

                // Units needed to move
                float xDistance = _gridCoords.x - _tileObj.transform.position.x;
                float yDistance = _gridCoords.y - _tileObj.transform.position.y;

                _totalDistance = Mathf.Abs( xDistance ) + Mathf.Abs( yDistance );

                _xDelta = xDistance / ( _moveSpeed / Time.deltaTime );
                _yDelta = yDistance / ( _moveSpeed / Time.deltaTime );
            }

            if ( _totalDistance < ( Mathf.Abs( _xDelta ) + Mathf.Abs( _yDelta ) ) ) {
                FinishMovementAnimation();
                _totalDistance = 0;
                _xDelta = 0;
                _yDelta = 0;
            } else {
                _tileObj.transform.position = new Vector3( _tileObj.transform.position.x + _xDelta,_tileObj.transform.position.y + _yDelta );
                _totalDistance -= ( Mathf.Abs( _xDelta ) + Mathf.Abs( _yDelta ) );
            }

        }
    }

    /// <summary>
    /// Animate the destroy animation.
    /// </summary>
    //TODO: Create animation.
    public void AnimateRemoval() {

    }

    /// <summary>
    /// Immediately stop the animation. 
    /// </summary>
	public void FinishMovementAnimation() {
        _tileObj.transform.position = _gridCoords;
    }

    /// <summary>
    /// Sets the object as being new.
    /// </summary>
	public void SetJustDrawn() {
        _justDrawn = true;
    }
}
