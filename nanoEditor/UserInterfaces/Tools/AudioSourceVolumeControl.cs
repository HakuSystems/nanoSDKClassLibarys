using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace nanoEditor.UserInterfaces.Tools;

public class AudioSourceVolumeControl : EditorWindow
{
    private static readonly List<AudioSource> AudioSources = new();
    private Vector2 _scrollPosition;
    private string _searchBar = "";
    private int _showIndex = 10;

    public static void ShowWindow()
    {
        GetWindow(typeof(AudioSourceVolumeControl), false, "AudioSourceVolumeControl");
    }

    private void OnEnable()
    {
        minSize = new Vector2(1000, 300);
        maxSize = new Vector2(1000, 300);
        RefreshAudioSources();
    }

    private void RefreshAudioSources()
    {
        AudioSources.Clear();
        AudioSources.AddRange(GameObject.FindObjectsOfType<AudioSource>());
    }

    private void OnDestroy()
    {
        foreach (var audioSource in AudioSources) audioSource.Pause();
    }

    private void Update()
    {
        Repaint();
        if (AudioSources.Count != FindObjectsOfType<AudioSource>().Length) RefreshAudioSources();

        var currentlyPlaying = AudioSources.Count(audioSource => audioSource.isPlaying);
        if (currentlyPlaying > 1)
            foreach (var audioSource in AudioSources.Where(audioSource => audioSource.isPlaying))
                audioSource.Pause();
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        {
            EditorGUILayout.BeginHorizontal();
            {
                var audioSourcesCountLabel = AudioSources.Count == 0
                    ? "No AudioSources found"
                    : $"AudioSources found: {AudioSources.Count}";
                EditorGUILayout.LabelField(audioSourcesCountLabel, EditorStyles.toolbarButton);
                EditorGUILayout.LabelField("AudioSources in Hierarchy", EditorStyles.toolbarButton);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            {
                _showIndex = AudioSources.Count == 1
                    ? 1
                    : EditorGUILayout.IntSlider("Show Amount: ", _showIndex, 1, AudioSources.Count);
                _searchBar = EditorGUILayout.TextField(_searchBar, EditorStyles.toolbarSearchField);
            }
            EditorGUILayout.EndHorizontal();
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, false, false,
                GUILayout.Width(EditorGUIUtility.currentViewWidth));
            {
                EditorGUILayout.BeginVertical();
                {
                    var resultCount = 0;
                    foreach (var audioSource in AudioSources)
                    {
                        resultCount++;
                        if (!audioSource.name.ToLower().Contains(_searchBar.ToLower()) || _showIndex <= resultCount - 1)
                        {
                            resultCount--;
                            continue;
                        }

                        EditorGUILayout.BeginHorizontal();
                        {
                            switch (audioSource.volume)
                            {
                                case float v when v > 0.5f:
                                    GUI.color = Color.green;
                                    break;
                                case float v when v > 0.25f:
                                    GUI.color = Color.yellow;
                                    break;
                                default:
                                    GUI.color = Color.red;
                                    break;
                            }

                            if (audioSource != null)
                            {
                                if (GUILayout.Button(audioSource.name, EditorStyles.toolbarButton))
                                    Selection.activeObject = audioSource.gameObject;

                                var clip = audioSource.clip;
                                var clipFileNameExtension = Path.GetFileName(AssetDatabase.GetAssetPath(clip));
                                EditorGUILayout.LabelField(clipFileNameExtension, EditorStyles.boldLabel);
                                if (audioSource.isPlaying)
                                {
                                    var audioTime = audioSource.time;
                                    var audioLength = audioSource.clip.length;
                                    var audioTimeLabel =
                                        $"{(int)(audioTime / 60)}:{(int)(audioTime % 60)} / {(int)(audioLength / 60)}:{(int)(audioLength % 60)}";
                                    EditorGUI.ProgressBar(GUILayoutUtility.GetRect(100, 20), audioTime / audioLength,
                                        audioTimeLabel);
                                }

                                if (clip != null)
                                {
                                    if (GUILayout.Button("Play", EditorStyles.miniButton)) audioSource.Play();

                                    if (GUILayout.Button("Pause", EditorStyles.miniButton)) audioSource.Pause();
                                }
                                
                                //lmao so many if else
                                if (GUI.color == Color.green)
                                {
                                    EditorGUILayout.LabelField("GOOD TO HEAR", EditorStyles.toolbarButton);
                                }
                                else if (GUI.color == Color.yellow)
                                {
                                    EditorGUILayout.LabelField("LISTEN CLOSELY", EditorStyles.toolbarButton);
                                }
                                else if (GUI.color == Color.red)
                                {
                                    EditorGUILayout.LabelField("MUTE/ALMOST MUTE", EditorStyles.toolbarButton);
                                }

                                audioSource.volume = EditorGUILayout.Slider(audioSource.volume, 0, 1);
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
        }
        EditorGUILayout.EndVertical();
    }
}