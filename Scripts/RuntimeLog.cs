using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yorozu
{
    [DefaultExecutionOrder(int.MinValue)]
    public class RuntimeLog : MonoBehaviour
    {
        [SerializeField]
        private CaptureLogType _captureLogType = CaptureLogType.ErrorAll;

        private static class GUIStyles
        {
            internal static GUIStyle StyleBox;
            internal static GUIStyle StyleInfoButton;
            internal static GUIStyle StyleWarningButton;
            internal static GUIStyle StyleErrorButton;
            
            internal static GUIStyle StyleDetailInfoButton;
            internal static GUIStyle StyleDetailWarningButton;
            internal static GUIStyle StyleDetailErrorButton;
            
            static GUIStyles()
            {
                var boxTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                boxTexture.SetPixel(0, 0, new Color(1, 1, 1, 0.4f));
                boxTexture.Apply();
                StyleBox = new GUIStyle(GUI.skin.box);
                StyleBox.normal.background = boxTexture;

                StyleInfoButton = new GUIStyle(GUI.skin.label);
                StyleInfoButton.fontStyle = FontStyle.Bold;
                StyleInfoButton.alignment = TextAnchor.MiddleLeft;
                StyleInfoButton.padding.left = 20;
                StyleInfoButton.fontSize = 25;
                StyleInfoButton.normal.textColor = Color.black;
                StyleInfoButton.normal.background = boxTexture;

                StyleWarningButton = new GUIStyle(StyleInfoButton);
                StyleWarningButton.normal.textColor = Color.yellow;
                
                StyleErrorButton = new GUIStyle(StyleInfoButton);
                StyleErrorButton.normal.textColor = new Color(0.9f, 0f, 0f, 1f);

                StyleDetailInfoButton = new GUIStyle(StyleInfoButton);
                StyleDetailWarningButton = new GUIStyle(StyleWarningButton);
                StyleDetailErrorButton = new GUIStyle(StyleErrorButton);
                StyleDetailInfoButton.alignment = TextAnchor.UpperLeft;
                StyleDetailWarningButton.alignment = TextAnchor.UpperLeft;
                StyleDetailErrorButton.alignment = TextAnchor.UpperLeft;
            }

            internal static GUIStyle GetButtonStyle(LogType logType, bool isDetail)
            {
                switch (logType)
                {
                    case LogType.Error:
                    case LogType.Assert:
                    case LogType.Exception:
                        return isDetail ? StyleDetailErrorButton : StyleErrorButton;
                    case LogType.Warning:
                        return isDetail ? StyleDetailWarningButton : StyleWarningButton;
                }

                return isDetail ? StyleDetailInfoButton : StyleInfoButton;
            }
        }
        
        /// <summary>
        /// 何番目から見てないか
        /// </summary>
        private int _unreadIndex = -1;
        
        private List<CacheLog> _cacheLogs = new List<CacheLog>();
        private Rect _lastRect = new Rect();
        private Rect _drawRect = new Rect();
        private string _latestCondition;
        private LogType _latestLogType;

        private bool _detail;
        
        /// <summary>
        /// 5%をヘッダーサイズにする
        /// </summary>
        private const float MIN_HEIGHT_RATE = 0.04f;

        private void Awake()
        {
            Application.logMessageReceived += HandleLog;
            _lastRect = Screen.safeArea;
            Apply(_lastRect);
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void HandleLog(string condition, string stacktrace, LogType type)
        {
            if (!ValidCapture(type))
                return;
            
            if (_unreadIndex < 0)
                _unreadIndex = _cacheLogs.Count;
            
            _cacheLogs.Add(new CacheLog(type, condition, stacktrace));
            _latestLogType = type;
            _latestCondition = condition;
        }

        private bool ValidCapture(LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                    return (_captureLogType & CaptureLogType.Error) == CaptureLogType.Error;
                case LogType.Assert:
                    return (_captureLogType & CaptureLogType.Assert) == CaptureLogType.Assert;
                case LogType.Warning:
                    return (_captureLogType & CaptureLogType.Warning) == CaptureLogType.Warning;
                case LogType.Log:
                    return (_captureLogType & CaptureLogType.Log) == CaptureLogType.Log;
                case LogType.Exception:
                    return (_captureLogType & CaptureLogType.Exception) == CaptureLogType.Exception;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        
        internal void LateUpdate()
        {
            var safeArea = Screen.safeArea;
            if (_lastRect == safeArea)
                return;

            Apply(safeArea);
        }

        private void Apply(Rect safeArea)
        {
            _lastRect = safeArea;
			
            var display = Display.displays[0];
			
            // yは反転する必要がある
            var y = display.systemHeight - safeArea.size.y - safeArea.y;
            _drawRect = new Rect(safeArea)
            {
                y = y
            };
        }
		
        /// <summary>
        /// 描画
        /// </summary>
        private void OnGUI()
        {
            if (!_detail)
            {
                DrawShort();
            }
            else
            {
                DrawDetail();
            }
        }

        private void DrawShort()
        {
            if (_unreadIndex < 0 || string.IsNullOrEmpty(_latestCondition))
                return;

            var rect = _drawRect;
            rect.height *= MIN_HEIGHT_RATE;
            // クリックしたら詳細表示
            if (GUI.Button(rect, _latestCondition, GUIStyles.GetButtonStyle(_latestLogType, _detail)))
            {
                _detail = true;
            }
        }

        private void DrawDetail()
        {
            var log = _cacheLogs[_unreadIndex];
            if (GUI.Button(_drawRect, $"{log.Condition}\n{log.Stacktrace}", GUIStyles.GetButtonStyle(log.LogType, _detail)))
            {
                _unreadIndex++;
                if (_unreadIndex >= _cacheLogs.Count)
                    _unreadIndex = -1;

                _detail = false;
            }
        }
    }
}
