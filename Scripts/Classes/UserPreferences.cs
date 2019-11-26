using UnityEngine;
using System.Collections;
using SimpleJSON;
using UnityEngine.Networking;
using System.Linq;

namespace unexpected
{
    /// <summary>
    /// This class handles storing and loading of user preferences
    /// For our projects, that one should be used instead of Unity's default "PlayerPrefs", because:
    /// - it stores at a distinct location that the can be accessed on any device
    /// - it stores in a readable format that can be read (and changed) by someone who is not involved in depth
    /// - loading a single option is faster (e.g. if you have to load a full array of options at once)
    /// - you can switch the preferences file via script meaning that there can be preferences for the editor mode and preferences for the final app
    /// - Apart from that it works almost exactly like the PlayerPrefs
    /// 
    /// Usage:
    /// - When starting your app, you should run LoadPreferences() once which loads all the previously stored preferences into the memory
    ///   Note that this is already done automatically using the LanguageManager in version 2 or higher in your app.
    /// - Reading and writing uses the default functions you already know
    /// - If you edit a lot of data (= more than two elements) at once, you should consider setting the values without saving to file (overloaded methods)
    ///   and call SavePreferences() manually at the end of your commands
    /// 
    /// v1.1, 2018/02
    /// Written by Chris Arlt, c.arlt@unexpected.de
    /// </summary>
    public sealed class UserPreferences
    {
        /// <summary>This tells you if the settings have been loaded once</summary>
        private static bool settingsLoaded = false;
        /// <summary>This tells you if the settings have been loaded once</summary>
        public static bool SettingsLoaded { get { return settingsLoaded; } }

        /// <summary>This is the path (not URL!) to the currently loaded preferences file</summary>
        private static string currentPreferencesFile;
        /// <summary>This is the path (not URL!) to the currently loaded preferences file</summary>
        public static string CurrentPreferencesFile { get { return currentPreferencesFile; } }

        /// <summary>The setting for selected languages</summary>
        public const string USERPREFS_SELECTEDLANGUAGES = "Selected languages";

        // Our JSON for data
        private static readonly string DEFAULTJSON = "{ \"bool\": {}, \"int\": {}, \"float\": {}, \"string\": {} }";
        /// <summary>This is the handler for all our data</summary>
        private static JSONNode settings = JSON.Parse(DEFAULTJSON);


        /// <summary>These are all keys of boolean values</summary>
        public static System.Collections.Generic.IEnumerable<string> BoolKeys
        {
            get
            {
                if (settings["bool"] == null)
                    return new string[0];
                else
                    return settings["bool"].Keys;
            }
        }
        /// <summary>These are all keys of integer values</summary>
        public static System.Collections.Generic.IEnumerable<string> IntKeys
        {
            get
            {
                if (settings["int"] == null)
                    return new string[0];
                else
                    return settings["int"].Keys;
            }
        }
        /// <summary>These are all keys of float values</summary>
        public static System.Collections.Generic.IEnumerable<string> FloatKeys
        {
            get
            {
                if (settings["float"] == null)
                    return new string[0];
                else
                    return settings["float"].Keys;
            }
        }
        /// <summary>These are all keys of string values</summary>
        public static System.Collections.Generic.IEnumerable<string> StringKeys
        {
            get
            {
                if (settings["string"] == null)
                    return new string[0];
                else
                    return settings["string"].Keys;
            }
        }


        /// <summary>
        /// Use this for initialization. This loads all the settings into our class. Before this has finished there will not be any settings to look at! (check SettingsLoaded)
        /// Please note that this will block the main thread for a moment!
        /// </summary>
        public static void LoadPreferences()
        {
            // To avoid a bug when there is a corrupt user directory, use streaming assets path on windows
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            LoadPreferences(Application.streamingAssetsPath + "/settings.sve");
#else
            LoadPreferences(Application.persistentDataPath + "/settings.sve");
#endif
        }
        /// <summary>
        /// Use this for initialization. This loads all the settings into our class. Before this has finished there will not be any settings to look at! (check SettingsLoaded)
        /// Please note that this will block the main thread for a moment!
        /// </summary>
        /// <param name="fileName">is the name (not URL!) of the file to load from</param>
        public static void LoadPreferences(string fileName)
        {
            currentPreferencesFile = fileName;

            if (System.IO.File.Exists(fileName))
            {
                settings = JSON.Parse(System.IO.File.ReadAllText(fileName));
            } else
            {
                settings = JSON.Parse(DEFAULTJSON);
            }

            // Tell everyone that we've finished
            settingsLoaded = true;
        }

        /// <summary>This saves all the preferences to our file</summary>
        public static void SavePreferences()
        {
            SavePreferences(CurrentPreferencesFile);
        }

        /// <summary>This saves all the preferences to our file</summary>
        /// <param name="fileName">is the name (not URL!) of the file to save to</param>
        public static void SavePreferences(string fileName)
        {
            // Transform the json into a nice format
            string saveTxt = settings.ToString().Replace("\"},", "\"\n\t},\n").Replace("\":{\"", "\":\n\t{\n\t\t\"").Replace("\", \"", "\",\n\t\t\"").Replace("\"}}", "\"\n\t}\n}").Replace("{\"", "{\n\"");

            // And save
            System.IO.File.WriteAllText(fileName, saveTxt);
        }


        /// <summary>Get a boolean preference</summary>
        /// <param name="key">is the key to look for in our preferences</param>
        /// <param name="defaultValue">is the default value if the key does not exist</param>
        /// <returns>The value of the preference, if it exists. If not, you'll get your default value</returns>
        public static bool GetBool(string key, bool defaultValue)
        {
            // Just get it
            if (settings["bool"] != null && settings["bool"][key] != null)
                return (settings["bool"][key].AsBool);
            else
                return defaultValue;
        }

        /// <summary>Set a boolean preference and save it</summary>
        /// <param name="key">is the key to save it to</param>
        /// <param name="value">is the value to be saved</param>
        public static void SetBool(string key, bool value)
        {
            SetBool(key, value, true);
        }
        /// <summary>Set a boolean preference</summary>
        /// <param name="key">is the key to save it to</param>
        /// <param name="value">is the value to be saved</param>
        /// <param name="savePreferencesFile">set to false, if you only want to use this setting locally without saving it for the next startup</param>
        public static void SetBool(string key, bool value, bool savePreferencesFile)
        {
            // Create data array, if not available yet
            if (settings["bool"] == null)
                settings["bool"] = JSON.Parse("{}");

            // Save value
            settings["bool"][key].AsBool = value;

            // Save file, if user likes to
            if (savePreferencesFile)
                SavePreferences();
        }

        /// <summary>This kills a boolean value and saves</summary>
        /// <param name="key">is the key to kill</param>
        public static void UnsetBool(string key)
        {
            UnsetBool(key, true);
        }
        /// <summary>This kills a boolean value</summary>
        /// <param name="key">is the key to kill</param>
        /// <param name="savePreferencesFile">set to false, if you only want to use this setting locally without saving it for the next startup</param>
        public static void UnsetBool(string key, bool savePreferencesFile)
        {
            if (settings["bool"] != null)
                settings["bool"].Remove(key);

            // Save file, if user likes to
            if (savePreferencesFile)
                SavePreferences();
        }

        /// <summary>Get an integer preference</summary>
        /// <param name="key">is the key to look for in our preferences</param>
        /// <param name="defaultValue">is the default value if the key does not exist</param>
        /// <returns>The value of the preference, if it exists. If not, you'll get your default value</returns>
        public static int GetInt(string key, int defaultValue)
        {
            // Just get it
            if (settings["int"] != null && settings["int"][key] != null)
                return (settings["int"][key].AsInt);
            else
                return defaultValue;
        }

        /// <summary>Set an integer preference and save it</summary>
        /// <param name="key">is the key to save it to</param>
        /// <param name="value">is the value to be saved</param>
        public static void SetInt(string key, int value)
        {
            SetInt(key, value, true);
        }
        /// <summary>Set an integer preference</summary>
        /// <param name="key">is the key to save it to</param>
        /// <param name="value">is the value to be saved</param>
        /// <param name="savePreferencesFile">set to false, if you only want to use this setting locally without saving it for the next startup</param>
        public static void SetInt(string key, int value, bool savePreferencesFile)
        {
            // Create data array, if not available yet
            if (settings["int"] == null)
                settings["int"] = JSON.Parse("{}");

            // Save value
            settings["int"][key].AsInt = value;

            // Save file, if user likes to
            if (savePreferencesFile)
                SavePreferences();
        }

        /// <summary>This kills an integer value and saves</summary>
        /// <param name="key">is the key to kill</param>
        public static void UnsetInt(string key)
        {
            UnsetInt(key, true);
        }
        /// <summary>This kills an integer value</summary>
        /// <param name="key">is the key to kill</param>
        /// <param name="savePreferencesFile">set to false, if you only want to use this setting locally without saving it for the next startup</param>
        public static void UnsetInt(string key, bool savePreferencesFile)
        {
            if (settings["int"] != null)
                settings["int"].Remove(key);

            // Save file, if user likes to
            if (savePreferencesFile)
                SavePreferences();
        }

        /// <summary>Get a float preference</summary>
        /// <param name="key">is the key to look for in our preferences</param>
        /// <param name="defaultValue">is the default value if the key does not exist</param>
        /// <returns>The value of the preference, if it exists. If not, you'll get your default value</returns>
        public static float GetFloat(string key, float defaultValue)
        {
            // Just get it
            if (settings["float"] != null && settings["float"][key] != null)
                return (settings["float"][key].AsFloat);
            else
                return defaultValue;
        }

        /// <summary>Set a float preference and save it</summary>
        /// <param name="key">is the key to save it to</param>
        /// <param name="value">is the value to be saved</param>
        public static void SetFloat(string key, float value)
        {
            SetFloat(key, value, true);
        }
        /// <summary>Set a float preference</summary>
        /// <param name="key">is the key to save it to</param>
        /// <param name="value">is the value to be saved</param>
        /// <param name="savePreferencesFile">set to false, if you only want to use this setting locally without saving it for the next startup</param>
        public static void SetFloat(string key, float value, bool savePreferencesFile)
        {
            // Create data array, if not available yet
            if (settings["float"] == null)
                settings["float"] = JSON.Parse("{}");

            // Save value
            settings["float"][key].AsFloat = value;

            // Save file, if user likes to
            if (savePreferencesFile)
                SavePreferences();
        }

        /// <summary>This kills a float value and saves</summary>
        /// <param name="key">is the key to kill</param>
        public static void UnsetFloat(string key)
        {
            UnsetFloat(key, true);
        }
        /// <summary>This kills a float value</summary>
        /// <param name="key">is the key to kill</param>
        /// <param name="savePreferencesFile">set to false, if you only want to use this setting locally without saving it for the next startup</param>
        public static void UnsetFloat(string key, bool savePreferencesFile)
        {
            if (settings["float"] != null)
                settings["float"].Remove(key);

            // Save file, if user likes to
            if (savePreferencesFile)
                SavePreferences();
        }

        /// <summary>Get a string preference</summary>
        /// <param name="key">is the key to look for in our preferences</param>
        /// <param name="defaultValue">is the default value if the key does not exist</param>
        /// <returns>The value of the preference, if it exists. If not, you'll get your default value</returns>
        public static string GetString(string key, string defaultValue)
        {
            // Just get it
            if (settings["string"] != null && settings["string"][key] != null)
                return (settings["string"][key].Value);
            else
                return defaultValue;
        }

        /// <summary>Set a string preference and save it</summary>
        /// <param name="key">is the key to save it to</param>
        /// <param name="value">is the value to be saved</param>
        public static void SetString(string key, string value)
        {
            SetString(key, value, true);
        }
        /// <summary>Set a string preference</summary>
        /// <param name="key">is the key to save it to</param>
        /// <param name="value">is the value to be saved</param>
        /// <param name="savePreferencesFile">set to false, if you only want to use this setting locally without saving it for the next startup</param>
        public static void SetString(string key, string value, bool savePreferencesFile)
        {
            // Create data array, if not available yet
            if (settings["string"] == null)
                settings["string"] = JSON.Parse("{}");

            // Save value
            settings["string"][key] = value;

            // Save file, if user likes to
            if (savePreferencesFile)
                SavePreferences();
        }

        /// <summary>This kills a string value and saves</summary>
        /// <param name="key">is the key to kill</param>
        public static void UnsetString(string key)
        {
            UnsetString(key, true);
        }
        /// <summary>This kills a boolean value</summary>
        /// <param name="key">is the key to kill</param>
        /// <param name="savePreferencesFile">set to false, if you only want to use this setting locally without saving it for the next startup</param>
        public static void UnsetString(string key, bool savePreferencesFile)
        {
            if (settings["string"] != null)
                settings["string"].Remove(key);

            // Save file, if user likes to
            if (savePreferencesFile)
                SavePreferences();
        }


        /// <summary>
        /// <para>Tells you, if a specific user preference is set in preference file.</para>
        /// <para>May also return false, if the preferences have not been loaded yet. Check "SettingsLoaded" for this.</para>
        /// <para>Note that this question generates some of garbage. Use carefully!</para>
        /// </summary>
        /// <param name="key">is the key you are looking at</param>
        /// <returns>True, if it exists. False, if it either does not exists or the preferences have not been loaded yet</returns>
        public bool HasKey(string key)
        {
            return (GetTypeOfKey(key) != null);
        }


        /// <summary>
        /// <para>This will get you the type of a key you only know the name of.</para>
        /// <para>Returns null, if either the key does not exist or the preferences have not been loaded yet. Check "SettingsLoaded" for this.</para>
        /// <para>Note that this question generates some of garbage. Use carefully!</para>
        /// </summary>
        /// <param name="key">is the key you are looking at</param>
        /// <returns>The type for the key, or null, if it does not exist or the preferences have not been loaded yet</returns>
        public System.Type GetTypeOfKey(string key)
        {
            if (!SettingsLoaded)
                return null;

            if (BoolKeys.Contains(key))
                return typeof(bool);
            if (IntKeys.Contains(key))
                return typeof(int);
            if (FloatKeys.Contains(key))
                return typeof(float);
            if (StringKeys.Contains(key))
                return typeof(string);

            return null;
        }



        /// <summary>This deletes ALL user preferences</summary>
        /// <param name="killFile">defines, if you want to kill the file as well</param>
        public static void DeleteAll(bool killFile)
        {
            if (string.IsNullOrEmpty(CurrentPreferencesFile))
                currentPreferencesFile = Application.persistentDataPath + "/settings.sve";

            if (killFile)
                DeleteAll(CurrentPreferencesFile);
            else
                DeleteAll("");
        }

        /// <summary>This deletes ALL user preferences and kills the file as well</summary>
        public static void DeleteAll(string filename)
        {
            // Remove settings
            settings = JSON.Parse(DEFAULTJSON);


            // Kill file, if desired
            if (!filename.Equals(""))
            {
                try
                {
                    System.IO.File.Delete(filename);
                }
                catch (System.IO.IOException err)
                {
                    Debug.LogWarning("File " + filename + " cannot be deleted! " + err.Message);
                }
            }
        }

        /// <summary>This returns the settings json (use for debugging only)</summary>
        public static new string ToString()
        {
            return settings.ToString();
        }
    }
}