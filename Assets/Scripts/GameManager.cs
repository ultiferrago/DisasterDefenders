//
// GameManager.cs
//
// Author:
//       ultiferrago <tkh0006@auburn.edu>
//
// Copyright (c) 2017 
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the game's mechanics and different components.
/// </summary>
public class GameManager : MonoBehaviour {

    // Game settings
    private PersistentData.Settings _settings = PersistentData.GetSettings();

    // Prefabricated design objects.
    public GameObject[] ResourcePrefabs;
    public GameObject DisasterPrefab;

    // Score GUI
    public Text ScoreText;
    private int _score;

    // Timer GUI
    public Text TimerText;
    public GameObject TimerProgressBar;

    // Losing Screen resources
    public GameObject LosingScreen;
    private static bool gameOver;
    private static bool gameReady;

    // Keeps track of tiles being actively moved.
    public static bool TilesUpdating = false;

    // Handles the gameboard interactions.
    private GameBoard _board;

    // Used to monitor user clicks
    private Tile _clicked;
    private Tile _dropped;

    // Keeps track of the last time the wave updated.
    public Text WaveText;
    public GameObject WaveProgressBar;
    public GameObject DisasterGraphic;
    public GameObject HourglassGraphic;
    public Text LevelText;
    private float waveStartTime;

    /// <summary>
    /// Lifecycle method called when attached camera is loaded.
    /// </summary>
    private void Start() {
        Debug.Log( "Loading Main Scene..." );
        gameReady = false;
        // Reset score GUI
        _score = 0;
        ScoreText.text = "0";

        // Ensure game doesn't start as being done. (for restarts)
        CreateGameBoard();


        // Link menu button to appropriate action.
        GameObject.FindGameObjectWithTag( "MainMenuButton" ).GetComponent<Button>().onClick.AddListener( EndGameMenuCallback );

        gameOver = false;

        // Enable debug stuff.
        if ( _settings.DEBUG ) {
            GameObject failObj = GameObject.FindGameObjectWithTag( "FailButton" );
            failObj.SetActive( true );
            failObj.GetComponent<Button>().onClick.AddListener( EndGame );
        }

        gameReady = true;
        Debug.Log( "Main Scene Loaded" );
    }

    /// <summary>
    /// Lifecycle method called once per game frame.
    /// </summary>
	private void Update() {

        if ( !gameReady ) {
            return;
        }

        // If the game is over no need to apply any game logic. 
        if ( gameOver ) {
            // Display losing screen if game is over and not showing.
            if ( !LosingScreen.activeSelf ) {
                LosingScreen.SetActive( true );
                GameObject.FindGameObjectWithTag( "RestartButton" ).GetComponent<Button>().onClick.AddListener( EndGameRestartCallback );
                GameObject.FindGameObjectWithTag( "MainMenuButton2" ).GetComponent<Button>().onClick.AddListener( EndGameMenuCallback );
            }

            return;
        }

        // Check if the user has clicked down.
        if ( Input.GetMouseButtonDown( 0 ) && _clicked == null ) {
            Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );
            RaycastHit2D hit = Physics2D.GetRayIntersection( ray, 1000 );

            // If the user actually clicked on something
            if ( hit ) {
                _clicked = _board.GetTile( hit.collider.gameObject );

                // We hit something lets end all active movement animations
                UpdateBoard();

                // If the user clicked a disaster ignore it.
                // TODO: Make disasters no clickable.
                if ( _clicked.GetType() == typeof( DisasterTile ) ) {
                    _clicked = null;
                    return;
                }


                // Hide the clicked item so that we can display a nice cursor animation.
                _clicked.Deactivate();

                Debug.Log( "Clicked on (" + _clicked.GetGridCoordinates().x + ", " + _clicked.GetGridCoordinates().y + ")" );
            }


        } else if ( Input.GetMouseButtonUp( 0 ) && _clicked != null ) {
            // At this point the user has clicked on an item, and released it somewhere.
            Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );
            RaycastHit2D hit = Physics2D.GetRayIntersection( ray, 1000 );

            // If the user released the clicked object at a legit location.
            if ( hit ) {

                UpdateBoard();
                _dropped = _board.GetTile( hit.collider.gameObject );

                // Ignore them dropping in the same spot.
                if ( _clicked == _dropped ) {
                    _clicked.Activate();
                    _clicked = null;
                    _dropped = null;
                    return;
                }

                // Check if the icons can be swapped.
                if ( _board.CanSwap( _clicked, _dropped ) ) {
                    // Swap tiles
                    _clicked.Activate();
                    _score += _board.SwapTiles( _clicked, _dropped );
                    _clicked = null;
                    _dropped = null;
                } else {
                    // Play bad sound. Reset clicked/dropped.
                    GetComponent<AudioSource>().Play();
                    _clicked.Activate();
                    _clicked = null;
                    _dropped = null;
                }

                // update score
            } else {
                // Ensure that the clicked is reset anytime the user lets go of mouse button.
                _clicked.Activate();
                _clicked = null;
            }
        }

        // Handle cursor animation.
        if ( _clicked != null ) {

#if UNITY_EDITOR
            Sprite spriteClone = ( Sprite )Object.Instantiate( _clicked.GetGameObject().GetComponent<SpriteRenderer>().sprite );
            Texture2D textureClone = ( Texture2D )Object.Instantiate( spriteClone.texture );
            Texture2D cursorTexture = new Texture2D( textureClone.width, textureClone.height, TextureFormat.ARGB32, false );
            cursorTexture.alphaIsTransparency = true;
            cursorTexture.SetPixels( textureClone.GetPixels() );
            Cursor.SetCursor( cursorTexture, new Vector2( 32f, 32f ), CursorMode.ForceSoftware );
#else
            Cursor.SetCursor( _clicked.GetGameObject().GetComponent<SpriteRenderer>().sprite.texture, new Vector2( 32f,32f ), CursorMode.ForceSoftware ) ;      
#endif


        } else {
            Cursor.SetCursor( null, Vector2.zero, CursorMode.Auto );
        }

        // Update GUI components.
        UpdateScoreText();
        UpdateDisasters();
        UpdateWave();
        UpdateBoard();

    }

    /// <summary>
    /// Creates the gameboard object with appropriate information
    /// </summary>
	private void CreateGameBoard() {
        int columns = _settings.NumberOfBoardColumns;
        int rows = _settings.NumberOfBoardRows;
        int duplicateResources = _settings.NumOfDuplicateResourceTypes;
        // Check to make sure params are good
        if ( ResourcePrefabs.Length < columns * rows ) {
            Debug.LogError( "There are not enough duplicate resources to create" +
            "the board. Please increase NumOfEachResource" );
            Application.Quit();
        } else if ( duplicateResources * ResourcePrefabs.Length * 1.5 < columns * rows ) {
            Debug.LogWarning( "It is recommended to have atleat 1.5x as many tiles as there are " +
            "available places on the board. This helps ensure that the game isn't too easy" );
        }

        // Create our gameboard object
        _board = new GameBoard( ResourcePrefabs, DisasterPrefab );

        // Initialize the board
        _board.Setup();
    }

    /// <summary>
    /// Updates the score GUI
    /// </summary>
	private void UpdateScoreText() {
        int currentScore = int.Parse( ScoreText.text );

        float pctUpdate = 0.10f;
        int scoreDiff = _score - currentScore;

        if ( scoreDiff * pctUpdate < 1 ) {
            currentScore = _score;
            ScoreText.text = currentScore.ToString( "D7" );
            return;
        }

        if ( currentScore < _score ) {
            currentScore += ( int )( scoreDiff * pctUpdate );
            ScoreText.text = currentScore.ToString( "D7" );
        }
    }

    /// <summary>
    /// Animate disasters and move if applicable.
    /// </summary>
    private void UpdateDisasters() {
        // Animate disasters
        _board.AnimateDisasters();
        // Check for moves
        _board.MoveDisasters();
        // Update timer
        float time = _board.GetTimeUntilNextDisasterMove();
        TimerText.text = SecondsToString( time );
    }

    /// <summary>
    /// Convert time in seconds to a string.
    /// </summary>
    /// 
    /// <returns>The string representation of the time.</returns>
    /// 
    /// <param name="time">Time in seconds.</param>
    private string SecondsToString( float time ) {
        int minutes = ( int )System.Math.Floor( time / 60 );
        time -= ( minutes * 60 );
        int seconds = ( int )time;

        return string.Format( "{0:D2}:{1:D2}", minutes, seconds );
    }

    /// <summary>
    /// Update the wave if applicable.
    /// </summary>
    private void UpdateWave() {
        if ( _settings.WaveIncreasesByScore ) {
            int scorePerWave = _settings.ScorePerWaveIncrease;
            double multiplier = _settings.ScorePerWaveIncreaseMultiplier;
            // Some random value...
            if ( _score >= scorePerWave * ( _board.GetCurrentWave() - 1 ) * multiplier ) {
                _board.NextWave();
            }

            UpdateWaveScore();

        } else if ( _settings.WaveIncreasesByTime ) {
            if ( Time.time >= ( _settings.TimePerWave * _board.GetCurrentWave() ) ) {
                _board.NextWave();
                waveStartTime = Time.time;
            }

            UpdateWaveTimer();
        }

        // Animate UI Graphics
        DisasterGraphic.transform.Rotate( 0, 0, Time.deltaTime * 180 );
    }

    /// <summary>
    /// Updates the Wave Progress GUI when wave is determined by time.
    /// </summary>
    private void UpdateWaveTimer() {
        // Get time left in current wave.
        float timePerWave = _settings.TimePerWave;
        float nextWaveTime = waveStartTime + timePerWave;
        float timeUntilNextWave = nextWaveTime - Time.time;

        // Update timer text on GUI.
        WaveText.text = "Level " + _board.GetCurrentWave().ToString() + "\nNext Level:\n" + SecondsToString( timeUntilNextWave );
        LevelText.text = "Level " + _board.GetCurrentWave().ToString();

        // Update timer progress on GUI.
        float progressPct = timeUntilNextWave / timePerWave;
        WaveProgressBar.GetComponent<Image>().fillAmount = 1 - progressPct;
    }

    /// <summary>
    /// Updates the Wave Progress GUI when wave is determined by score.
    /// </summary>
    private void UpdateWaveScore() {
        // Get score until next wave
        int scorePerWave = _settings.ScorePerWaveIncrease;
        double scorePerWaveMultiplier = _settings.ScorePerWaveIncreaseMultiplier;
        int scoreRequiredForNextWave = ( int )( scorePerWave * scorePerWaveMultiplier * _board.GetCurrentWave() );
        int scoreUntilNextWave = scoreRequiredForNextWave - _score;

        // Update wave score text
        WaveText.text = scoreUntilNextWave.ToString();
        LevelText.text = "Level " + _board.GetCurrentWave().ToString();

        // Update wave score progress
        float scorePct = scoreUntilNextWave / scoreRequiredForNextWave;
        WaveProgressBar.GetComponent<Image>().fillAmount = 1 - scorePct;
    }

    /// <summary>
    /// Updates the board by stopping animations as well as animating tiles.
    /// </summary>
    private void UpdateBoard() {
        if ( _clicked != null || _dropped != null ) {
            _board.FinishTileAnimations();
            return;
        }

        _board.AnimateTiles();
    }

    /// <summary>
    /// Ends the game.
    /// </summary>
	public static void EndGame() {
        gameOver = true;
    }

    /// <summary>
    /// Returns to the menu.
    /// </summary>
    public static void EndGameMenuCallback() {
        SceneManager.LoadScene( "MainMenu" );
    }

    /// <summary>
    /// Restarts the game.
    /// </summary>
    public static void EndGameRestartCallback() {
        SceneManager.LoadScene( SceneManager.GetActiveScene().name );
    }
}