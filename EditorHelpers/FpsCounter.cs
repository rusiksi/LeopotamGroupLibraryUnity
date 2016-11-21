﻿//-------------------------------------------------------
// LeopotamGroupLibrary for unity3d
// Copyright (c) 2012-2016 Leopotam <leopotam@gmail.com>
//-------------------------------------------------------

using LeopotamGroup.Common;
using UnityEngine;

namespace LeopotamGroup.EditorHelpers {
    /// <summary>
    /// Fps counter.
    /// </summary>
    sealed class FpsCounter : UnitySingleton<FpsCounter> {
        const int UpdateFrequency = 2;

        const float _invUpdatesPerSecond = 1 / (float) UpdateFrequency;

        const float BaseFontSize = 16 / 768f;

        readonly byte[] FontData =
            {
                0, 1, 1, 0, 0, 0, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 0, 0, 1, // 0-4
                1, 1, 1, 1, 0, 1, 1, 0, 1, 1, 1, 1, 0, 1, 1, 0, 0, 1, 1, 0, // 5-9

                1, 0, 0, 1, 0, 1, 1, 0, 0, 0, 0, 1, 0, 0, 0, 1, 1, 0, 0, 1, // 0-4
                1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, // 5-9

                1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 1, 1, 1, 1, // 0-4
                1, 1, 1, 0, 1, 1, 1, 0, 0, 0, 1, 0, 0, 1, 1, 0, 0, 1, 1, 1, // 5-9

                1, 0, 0, 1, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, // 0-4
                0, 0, 0, 1, 1, 0, 0, 1, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0, 1, // 5-9

                0, 1, 1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, // 0-4
                1, 1, 1, 0, 0, 1, 1, 0, 0, 1, 0, 0, 0, 1, 1, 0, 0, 1, 1, 0, // 5-9
            };

        int _frameCount;

        float _lastTime;

        int _cameraIndex;

        float _xOffset;

        float _yOffset;

        float _fontSize;

        Texture2D _tex;

        Material _mtrl;

        protected override void OnConstruct () {
            DontDestroyOnLoad (gameObject);

            _tex = new Texture2D (40, 5, TextureFormat.ARGB32, false);
            _tex.hideFlags = HideFlags.DontSave;
            _tex.filterMode = FilterMode.Point;
            _tex.wrapMode = TextureWrapMode.Clamp;
            var pixels = new Color32[FontData.Length];
            var pix0 = new Color32 (0, 0, 0, 0);
            var pix1 = new Color32 (255, 255, 255, 255);
            for (var i = FontData.Length - 1; i >= 0; i--) {
                pixels[i] = FontData[i] > 0 ? pix1 : pix0;
            }
            _tex.SetPixels32 (pixels);
            _tex.Apply (false, true);

            _mtrl = new Material (Shader.Find ("Unlit/Transparent"));
            _mtrl.hideFlags = HideFlags.DontSave;
            _mtrl.mainTexture = _tex;
#if !UNITY_EDITOR
            _fontSize = Screen.height * BaseFontSize;
#endif
        }

        protected override void OnDestruct () {
            Destroy (_mtrl);
            _mtrl = null;
            Destroy (_tex);
            _tex = null;
            base.OnDestruct ();
        }

        void OnRenderObject () {
            if (Camera.current.gameObject.scene.IsValid ()) {
                _cameraIndex++;
            }

            if (_cameraIndex == Camera.allCamerasCount) {
#if UNITY_EDITOR
                _fontSize = Screen.height * BaseFontSize;
#endif
                GL.PushMatrix ();
                GL.LoadPixelMatrix ();
                _mtrl.SetPass (0);
                GL.Color (Color.white);
                GL.Begin (GL.QUADS);
                _xOffset = 0;
                _yOffset = Screen.height;

                var v0 = CurrentFps;
                var v1 = 0;
                var count = 0;
                while (v0 > 0) {
                    count++;
                    v1 = v1 * 10 + (v0 % 10);
                    v0 /= 10;
                }
                if (count == 0) {
                    count++;
                }

                int charID;
                for (var i = count - 1; i >= 0; i--) {
                    charID = v1 % 10;

                    GL.TexCoord2 (charID * 0.1f, 1f);
                    GL.Vertex3 (_xOffset, _yOffset - _fontSize, 0f);

                    GL.TexCoord2 (charID * 0.1f, 0f);
                    GL.Vertex3 (_xOffset, _yOffset, 0f);

                    GL.TexCoord2 ((charID + 1) * 0.1f, 0f);
                    GL.Vertex3 (_xOffset + _fontSize, _yOffset, 0f);

                    GL.TexCoord2 ((charID + 1) * 0.1f, 1f);
                    GL.Vertex3 (_xOffset + _fontSize, _yOffset - _fontSize, 0f);

                    _xOffset += _fontSize + 1;
                    v1 /= 10;
                }

                GL.End ();
                GL.PopMatrix ();
            }
        }

        void LateUpdate () {
            _cameraIndex = 0;
            var currTime = Time.realtimeSinceStartup;
            if ((currTime - _lastTime) > _invUpdatesPerSecond) {
                CurrentFps = _frameCount * UpdateFrequency;
                _frameCount = 1;
                _lastTime = currTime;
            } else {
                _frameCount++;
            }
        }

        /// <summary>
        /// Get current fps.
        /// </summary>
        /// <value>The current fps.</value>
        public int CurrentFps { get; private set; }
    }
}