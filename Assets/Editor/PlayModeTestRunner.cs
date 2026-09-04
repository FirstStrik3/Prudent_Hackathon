using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Unity.AI.Assistant.PlayModeTest
{
    [InitializeOnLoad]
    internal static class PlayModeTestRunner
    {
        private const string StateKey = "PlayModeTest.State";
        private const string ResultKey = "PlayModeTest.Result";
        private const string ScriptPathKey = "PlayModeTest.ScriptPath";
        private const string SentinelLog = "PLAY_MODE_TEST_COMPLETE";

        private static readonly int WaitFrames = SessionState.GetInt("PlayModeTest.WaitFrames", 3);
        private static readonly float TestTimeout = SessionState.GetFloat("PlayModeTest.TestTimeout", 10.0f);

        private static List<string> _capturedLogs = new List<string>();
        private const int MaxCapturedLogs = 50;

        private static bool _initialIdleVerified = false;
        private static bool _dissembleTriggered = false;
        private static bool _dissembleVerified = false;
        private static bool _assembleTriggered = false;
        private static bool _assembleVerified = false;
        private static string _testFailureReason = "";

        static PlayModeTestRunner()
        {
            string state = SessionState.GetString(StateKey, "Idle");

            switch (state)
            {
                case "Idle":
                    break;

                case "WaitingForCompile":
                    Debug.Log("[PlayModeTest] Bootstrap compiled. Scheduling Play Mode entry.");
                    EditorApplication.delayCall += () =>
                    {
                        SessionState.SetString(StateKey, "EnteringPlayMode");
                        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                        EditorApplication.isPlaying = true;
                    };
                    break;

                case "EnteringPlayMode":
                    EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                    if (EditorApplication.isPlaying)
                    {
                        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                        SessionState.SetString(StateKey, "InPlayMode");
                        EditorApplication.update += WaitFramesThenRun;
                    }
                    break;

                case "InPlayMode":
                    if (EditorApplication.isPlaying)
                    {
                        EditorApplication.update += WaitFramesThenRun;
                    }
                    break;

                case "Done":
                    Debug.Log(SentinelLog);
                    if (string.IsNullOrEmpty(SessionState.GetString("PlayModeTest.ScreenshotPath", "")))
                    {
                        EditorApplication.delayCall += SelfDestruct;
                    }
                    break;
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredPlayMode)
            {
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                SessionState.SetString(StateKey, "InPlayMode");
                EditorApplication.update += WaitFramesThenRun;
            }
        }

        private static int _frameCount = 0;
        private static bool _setupDone = false;
        private static bool _testDone = false;
        private static double _testStartTime = 0;

        private static void WaitFramesThenRun()
        {
            _frameCount++;
            if (_frameCount < WaitFrames) return;

            if (_testDone) return;

            if (!_setupDone)
            {
                _setupDone = true;
                Application.logMessageReceived += OnLogMessage;
                _testStartTime = EditorApplication.timeSinceStartup;
                try
                {
                    Setup();
                }
                catch (System.Exception e)
                {
                    Debug.LogError("[PlayModeTest] Setup threw exception: " + e);
                    FinishTest(true, e.Message);
                    return;
                }
                return;
            }

            float elapsed = (float)(EditorApplication.timeSinceStartup - _testStartTime);
            bool timedOut = elapsed >= TestTimeout;

            try
            {
                bool complete = Tick(elapsed);
                if (complete || timedOut)
                {
                    if (timedOut && !complete)
                    {
                        Debug.LogWarning("[PlayModeTest] Test timed out after " + elapsed + "s");
                    }
                    FinishTest(timedOut && !complete, timedOut ? "Test timed out after " + TestTimeout + "s. Failure: " + _testFailureReason : null);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("[PlayModeTest] Tick threw exception: " + e);
                FinishTest(true, e.Message);
            }
        }

        private static void FinishTest(bool isError, string errorMessage)
        {
            _testDone = true;
            EditorApplication.update -= WaitFramesThenRun;
            Application.logMessageReceived -= OnLogMessage;

            string resultJson;
            try
            {
                resultJson = GetResult();
            }
            catch (System.Exception e)
            {
                resultJson = JsonUtility.ToJson(new TestResult
                {
                    success = false,
                    error = "GetResult() threw: " + e.Message,
                    logs = _capturedLogs.ToArray()
                });
            }

            if (isError && errorMessage != null)
            {
                resultJson = JsonUtility.ToJson(new TestResult
                {
                    success = false,
                    error = errorMessage,
                    logs = _capturedLogs.ToArray()
                });
            }

            SessionState.SetString(ResultKey, resultJson);
            SessionState.SetString(StateKey, "Done");
            EditorApplication.isPlaying = false;
        }

        private static void OnLogMessage(string message, string stackTrace, LogType type)
        {
            if (_capturedLogs.Count >= MaxCapturedLogs) return;
            if (type == LogType.Error || type == LogType.Exception ||
                message.Contains("[Test]") || message.Contains("TEST_RESULT"))
            {
                _capturedLogs.Add("[" + type + "] " + message);
            }
        }

        private static void SelfDestruct()
        {
            string scriptPath = SessionState.GetString(ScriptPathKey, "");
            if (!string.IsNullOrEmpty(scriptPath) && AssetDatabase.AssetPathExists(scriptPath))
            {
                AssetDatabase.DeleteAsset(scriptPath);
            }
            SessionState.EraseString(StateKey);
            SessionState.EraseString(ScriptPathKey);
        }

        [System.Serializable]
        private class TestResult
        {
            public bool success;
            public string error;
            public string[] logs;
        }

        private static void Setup()
        {
            var explodeViewGo = GameObject.Find("ExplodeView");
            if (explodeViewGo == null)
            {
                _testFailureReason = "ExplodeView not found";
                Debug.LogError("[Test] " + _testFailureReason);
                return;
            }

            var anim = explodeViewGo.GetComponent<Animator>();
            var state = anim.GetCurrentAnimatorStateInfo(0);
            if (state.IsName("Idle"))
            {
                _initialIdleVerified = true;
                Debug.Log("[Test] Verified: Animator is initially in Idle state. No animation playing automatically on start.");
            }
            else
            {
                _testFailureReason = "Animator was not in Idle initially; state is: " + state.fullPathHash;
                Debug.LogError("[Test] " + _testFailureReason);
                return;
            }

            var explodeBtnGo = GameObject.Find("Canvas/Panel/ExplodeBtn");
            if (explodeBtnGo != null && explodeBtnGo.activeInHierarchy)
            {
                Debug.Log("[Test] ExplodeBtn is active on start. Simulating click on ExplodeBtn.");
                var btn = explodeBtnGo.GetComponent<Button>();
                btn.onClick.Invoke();
                _dissembleTriggered = true;
            }
            else
            {
                _testFailureReason = "ExplodeBtn not found or not active";
                Debug.LogError("[Test] " + _testFailureReason);
            }
        }

        private static bool Tick(float elapsed)
        {
            if (!_initialIdleVerified || !_dissembleTriggered) return false;

            var explodeViewGo = GameObject.Find("ExplodeView");
            if (explodeViewGo == null) return false;
            var anim = explodeViewGo.GetComponent<Animator>();

            if (!_dissembleVerified && elapsed >= 0.2f)
            {
                var state = anim.GetCurrentAnimatorStateInfo(0);
                if (state.IsName("Dissemble"))
                {
                    _dissembleVerified = true;
                    Debug.Log("[Test] Verified: Dissemble animation is playing after clicking ExplodeBtn.");

                    var assembleBtnGo = GameObject.Find("Canvas/Panel/AssembleBtn");
                    if (assembleBtnGo != null && assembleBtnGo.activeInHierarchy)
                    {
                        Debug.Log("[Test] AssembleBtn is now active. Simulating click on AssembleBtn.");
                        var btn = assembleBtnGo.GetComponent<Button>();
                        btn.onClick.Invoke();
                        _assembleTriggered = true;
                    }
                    else
                    {
                        _testFailureReason = "AssembleBtn was not activated after Dissemble click";
                        Debug.LogError("[Test] " + _testFailureReason);
                        return true;
                    }
                }
            }

            if (_assembleTriggered && !_assembleVerified && elapsed >= 0.5f)
            {
                var state = anim.GetCurrentAnimatorStateInfo(0);
                if (state.IsName("Assemble"))
                {
                    _assembleVerified = true;
                    Debug.Log("[Test] Verified: Assemble animation is playing after clicking AssembleBtn.");

                    var explodeBtnGo = GameObject.Find("Canvas/Panel/ExplodeBtn");
                    if (explodeBtnGo != null && explodeBtnGo.activeInHierarchy)
                    {
                        Debug.Log("[Test] ExplodeBtn is active again after Assemble click.");
                        return true; // Test succeeded!
                    }
                }
            }

            return false;
        }

        private static string GetResult()
        {
            bool passed = _initialIdleVerified && _dissembleVerified && _assembleVerified;
            var result = new TestResult
            {
                success = passed,
                error = passed ? "" : _testFailureReason,
                logs = _capturedLogs.ToArray()
            };
            return JsonUtility.ToJson(result);
        }
    }
}
