using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;


/// <summary>
/// Handles the game's settings.
/// </summary>
public static class PersistentData {

    // Stored data.
    private static Settings _data;

    /// <summary>
    /// Loads saved data if there is any. Creates new data if not.
    /// </summary>
    public static void Load() {
        if ( File.Exists( Application.persistentDataPath + "/settings.cfg" ) ) {

            Debug.Log( "Save data located at: " + Application.persistentDataPath );

            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open( Application.persistentDataPath + "/settings.cfg", FileMode.Open );
            _data = ( Settings )bf.Deserialize( file );
            file.Close();
        } else {
            _data = new Settings();
        }

    }

    /// <summary>
    /// Saves the current instance of _data to file.
    /// </summary>
    public static void Save() {

        if ( _data == null ) {
            return;
        }

        // If the file exists delete it (for overwriting purposes)
        if ( File.Exists( Application.persistentDataPath + "/settings.cfg" ) ) {
            File.Delete( Application.persistentDataPath + "/settings.cfg" );
        }

        // Write the file out.
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create( Application.persistentDataPath + "/settings.cfg" );
        bf.Serialize( file, _data );
        file.Flush();
        file.Close();
    }

    /// <summary>
    /// Returns the currently loaded settings..
    /// </summary>
    /// <returns>Current settings.</returns>
    public static Settings GetSettings() {
        if ( _data == null ) {
            Load();
        }

        return _data;
    }

    /// <summary>
    /// Active setting values.
    /// </summary>
    [System.Serializable]
    public class Settings {

        // Debug value(s)
        public bool DEBUG = false;

        // Grid settings.
        private int _numOfRows = 8;
        private int _numOfColumns = 6;

        /// <value>The number of rows in the game board.</value>
        public int NumberOfBoardRows { get { return _numOfRows; } }

        /// <value>The number of columns in the game board.</value>
        public int NumberOfBoardColumns { get { return _numOfColumns; } }

        // Disaster initial move time settings.
        private int _minDisasterStartTime = 5;
        private int _maxDisasterStartTime = 60;
        private int _currentDisasterStartTime = 30;

        /// <value>The disaster timer start value.</value>
        public int InitialDisasterMoveTime {
            get { return _currentDisasterStartTime; }
            set {
                // Ensure set value is within the extrema
                value = ( value < _minDisasterStartTime ) ? _minDisasterStartTime : value;
                value = ( value > _maxDisasterStartTime ) ? _maxDisasterStartTime : value;
                _currentDisasterDelta = value;
            }
        }

        /// <value>The minimum initial disaster move time.</value>
        public int MinInitialDisasterMoveTime { get { return _minDisasterStartTime; } }


        /// <value>The max initial disaster move time.</value>
        public int MaxInitialDisasterMoveTime { get { return _maxDisasterStartTime; } }

        // Movement time change between waves settings.
        private int _minDisasterDelta = 1;
        private int _currentDisasterDelta = 3;

        /// <value>The value subtracted from the disaster timer each wave.</value>
        public int DisasterTimeDelta {
            get { return _currentDisasterDelta; }
            set {
                value = ( value < _minDisasterDelta ) ? _minDisasterDelta : value;
                value = ( value > MaxDisasterTimeDelta ) ? MaxDisasterTimeDelta : value;
                _currentDisasterDelta = value;
            }
        }

        /// <value>The max value that can be deducted each wave.</value>
        public int MaxDisasterTimeDelta { get { return ( InitialDisasterMoveTime - MinDisasterMoveTime ); } }

        // Extrema for movement values.
        private int _minDisasterMoveTime = 1;
        private int _maxDisasterMoveTime = 50;

        /// <value>The minimum disaster move time. Do not make moves faster!</value>
        public int MinDisasterMoveTime { get { return _minDisasterMoveTime; } }

        /// <value>The max disaster move time. Do not make moves take longer than this.</value>
        public int MaxDisasterMoveTime { get { return _maxDisasterMoveTime; } }

        private int _currentMaxActiveDisasters = 5;
        /// <value>The max active disasters possible.</value>
        public int MaxActiveDisastersPossible { get { return NumberOfBoardColumns; } }

        /// <value>The max active disasters.</value>
        public int MaxActiveDisasters {
            get { return _currentMaxActiveDisasters; }
            set {
                value = ( value < 1 ) ? 1 : value;
                value = ( value > MaxActiveDisasters ) ? MaxActiveDisasters : value;
                _currentMaxActiveDisasters = value;
            }
        }


        // Waves per additional disasters being spawned
        private int _minWavesPerDisaster = 1;
        private int _maxWavesPerDisaster = 10;
        private int _currentWavesPerDisaster = 3;

        /// <value>The waves per additional disasters being spawned.</value>
        public int WavesPerAdditionalDisasters {
            get { return _currentDisasterDelta; }
            set {
                value = ( value < _minWavesPerDisaster ) ? _minWavesPerDisaster : value;
                value = ( value > _maxWavesPerDisaster ) ? _maxWavesPerDisaster : value;
                _currentWavesPerDisaster = value;
            }
        }

        /// <value>The minimum waves per additional disasters being spawned.</value>
        public int MinWavesPerAdditionalDisaster { get { return _minWavesPerDisaster; } }

        /// <value>The max waves per additional disaster being spawned.</value>
        public int MaxWavesPerAdditionalDisaster { get { return _maxWavesPerDisaster; } }


        private int _scorePerDisaster = 500;
        /// <value>The score per disaster.</value>
        public int ScorePerDisaster { get { return _scorePerDisaster; } }


        private int _duplicateResources = 10;
        /// <value>The number of duplicate resource type cards.</value>
        public int NumOfDuplicateResourceTypes { get { return _duplicateResources; } }


        // Determine how waves will work.
        private bool _scoreIncreasesWave = false;
        private bool _timeIncreasesWave = false;

        private int _timeDeltaPerWave = 30;
        private int _minTimeDeltaPerWave = 5;
        private int _maxTimeDeltaPerWave = 300;

        private int _scoreDeltaPerWave = 15000;
        private double _scoreDeltaPerWaveMultiplier = 1.5;
        private double _minScoreDeltaPerWaveMultiplier = 1.0;

        /// <value><c>true</c> if score increases the wave; <c>false</c>, if time increases the wave wave.</value>
        public bool WaveIncreasesByScore {
            get { return _scoreIncreasesWave; }
            set {
                _scoreIncreasesWave = value;
                _timeIncreasesWave = !value;
            }
        }

        /// <value><c>true</c> if wave increases with time; <c>false</c> if score increases the wave.</value>
        public bool WaveIncreasesByTime {
            get { return _timeIncreasesWave; }
            set {
                _timeIncreasesWave = value;
                _scoreIncreasesWave = !value;
            }
        }

        /// <value>How long each wave lasts.</value>
        public int TimePerWave {
            get { return _timeDeltaPerWave; }
            set {
                value = ( value < _minTimeDeltaPerWave ) ? _minTimeDeltaPerWave : value;
                value = ( value > _maxTimeDeltaPerWave ) ? _maxTimeDeltaPerWave : value;
                _timeDeltaPerWave = value;
            }
        }

        /// <value>Required score to increase wave.</value>
        public int ScorePerWaveIncrease {
            get { return _scoreDeltaPerWave; }
            set { _scoreDeltaPerWave = value; }
        }

        /// <value>Multiplier for required score each wave.</value>
        public double ScorePerWaveIncreaseMultiplier {
            get { return _scoreDeltaPerWaveMultiplier; }
            set {
                value = ( value < _minScoreDeltaPerWaveMultiplier ) ? _minScoreDeltaPerWaveMultiplier : value;
                _minScoreDeltaPerWaveMultiplier = value;
            }
        }


    }
}