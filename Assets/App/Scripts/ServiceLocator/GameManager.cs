using UnityEngine;
using MagicTile.TileSystem;
using MagicTile.Booster;
using MagicTile.CameraEffect;
using MagicTile.UI;

namespace MagicTile.ServiceLocator
{
    public class GameManager : MonoBehaviour
    {
        private const float RefWidth = 1080f;
        private const float RefHeight = 1920f;
        private const float RefOrthoSize = 10.6f;

        [SerializeField] private Camera mainCamera;

        private CameraShakePresenter _cameraShakePresenter;

        private void Awake()
        {
            Init();
            SetupCameraSize();

            Application.targetFrameRate = 60;
        }

        private void SetupCameraSize()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null || !mainCamera.orthographic) return;

            float refAspect = RefWidth / RefHeight;
            float currentAspect = (float)Screen.width / Screen.height;
            mainCamera.orthographicSize = RefOrthoSize * (refAspect / currentAspect);
        }

        private void Start()
        {
            _cameraShakePresenter = new CameraShakePresenter(mainCamera != null ? mainCamera : Camera.main);

            StartCoroutine(CRStartLevel());
        }

        private System.Collections.IEnumerator CRStartLevel()
        {
            yield return null;

            var levelSevice = ServiceLocator.Get<ILevelService>();
            levelSevice.InitLevel(0);

            ServiceLocator.Get<CanvasManager>()?.Spawn(UIName.IngameMenu, null);
        }

        public void Init()
        {
            var boosterManager = new BoosterManager();
            ServiceLocator.Register<IBoosterService>(boosterManager);
        }

        private void OnDestroy()
        {
            _cameraShakePresenter?.Dispose();
            ServiceLocator.Unregister<ILevelService>();
        }
    }
}
