using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.Assertions;

namespace RenEditor
{
    public class SceneCameraSyncer
    {
        private const string path = "RenLib/Level Build/";
        private static Camera gameCamera;
        private static bool isTracking = false;
        public static Camera MainCamera
        {
            get
            {
                if (gameCamera != null)
                {
                    return gameCamera;
                }
                else
                {
                    gameCamera = Camera.main;
                    Assert.IsNotNull(gameCamera, "[SceneCameraSyncer] ReAssign camera");
                    return gameCamera;
                }
            }
        }

        [Shortcut(path + "Sync main camera with scene view", typeof(SceneView), KeyCode.Q, ShortcutModifiers.Alt)]
        private static void SyncCameraToogle()
        {
            if (isTracking)
                StopSync();
            else
                StartSync();
        }

        private static void StartSync()
        {
            isTracking = true;
            EditorApplication.update -= UpdateCamera;
            EditorApplication.update += UpdateCamera;
        }

        private static void StopSync()
        {
            isTracking = false;
            EditorApplication.update -= UpdateCamera;
        }

        private static void UpdateCamera()
        {
            var cam = SceneView.lastActiveSceneView?.camera;
            if (cam == null)
                return;

            if (EditorApplication.isPlaying || MainCamera == null)
            {
                StopSync();
                return;
            }

            var pos = cam.transform.position;
            var rot = cam.transform.rotation;
            var view = cam.fieldOfView;
            var ortho = cam.orthographic;

            MainCamera.transform.position = pos;
            MainCamera.transform.rotation = rot;
            MainCamera.fieldOfView = view;
            MainCamera.orthographic = ortho;
        }
    }
}