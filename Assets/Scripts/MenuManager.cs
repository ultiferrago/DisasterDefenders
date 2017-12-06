using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the implementation of the main menu in the game.
/// </summary>
public class MenuManager : MonoBehaviour {

    // GUI objects.
    public GameObject MenuContainer;
    public GameObject SimpleTutorialContainer;
    public GameObject SettingsContainer;
    public GameObject ComingSoon;

    /// <summary>
    /// Lifecycle method called when MainMenuScene is loaded. 
    /// Ensure the main menu is actively displayed.
    /// </summary>
    public void Start () {
        DisplayMainMenu ();
    }

    /// <summary>
    /// Lifecycle method called when the application is quit. Saves data for app.
    /// </summary>
    public void OnApplicationQuit () {
        PersistentData.Save ();
    }

    /// <summary>
    /// Starts gameplay by loading the main scene.
    /// </summary>
    public void StartGame () {
        SceneManager.LoadScene ( "MainScene" );
    }

    /// <summary>
    /// Loads the tutorial scene. (text only currently)
    /// </summary>
    public void LoadTutorial () {
        MenuContainer.SetActive ( false );
        SimpleTutorialContainer.SetActive ( true );
    }

    /// <summary>
    /// Displays the settings GUI
    /// </summary>
    public void OpenSettings () {
        if ( !MenuContainer.activeSelf ) {
            return;
        }

        MenuContainer.SetActive ( false );
        SettingsContainer.SetActive ( true );
    }

    /// <summary>
    /// Handles quitting the game.
    /// </summary>
    public void QuitGame () {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
        Application.Quit ();
    }

    /// <summary>
    /// Open the leaderboard GUI
    /// </summary>
    public void OpenLeaderboard () {
        ComingSoon.SetActive ( true );
    }

    /// <summary>
    /// Displays the main menu GUI.
    /// </summary>
    public void DisplayMainMenu () {
        SettingsContainer.SetActive ( false );
        SimpleTutorialContainer.SetActive ( false );
        ComingSoon.SetActive ( false );
        MenuContainer.SetActive ( true );
    }

}
