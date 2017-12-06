using UnityEngine;

/// <summary>
/// Disaster Tile - The main antagonist of the game. Races toward the 
/// bottom of the screen in an attempt to destroy the player.
/// </summary>
public class DisasterTile : Tile {

    private float _timeBetweenMoves;    // Delta time for moving.
    private float _nextMoveTime;        // Calculated time for next update.

    /// <summary>
    /// Initializes a new instance of the <see cref="T:DisasterTile"/> class.
    /// </summary>
    /// 
    /// <param name="tileObject">GameObject that this tile represents.</param>
    /// <param name="timePerUpdate">Time in seconds per movement.</param>
    public DisasterTile( GameObject tileObject, float timePerUpdate ) : base( tileObject ) {
        this._timeBetweenMoves = timePerUpdate;
        this._nextMoveTime = Time.time + _timeBetweenMoves;
    }

    /// <summary>
    /// Animate this disaster by spinning it.
    /// </summary>
    public new void Animate() {
        GetGameObject().transform.Rotate( 0, 0, Time.deltaTime * 180 );
    }

    /// <summary>
    /// Checks if this disaster is ready to move through the grid.
    /// </summary>
    /// 
    /// <returns><c>true</c>, if it is time to move, <c>false</c> otherwise.</returns>
	public bool ReadyForMove() {
        return Time.time > _nextMoveTime;
    }

    /// <summary>
    /// Updates the time for the next move. 
    /// </summary>
	public void Update() {
        _nextMoveTime = Time.time + _timeBetweenMoves;
    }

    /// <summary>
    /// Gets the time until the next update.
    /// </summary>
    /// 
    /// <returns>The time left until update.</returns>
	public float GetTimeUntilUpdate() {
        return _nextMoveTime - Time.time;
    }

    /// <summary>
    /// Disasters can not be in matches so always returns false. 
    /// </summary>
    /// <returns><c>false</c> always.</returns>
    public override bool IsInMatch() {
        return false;
    }
}
