using UnityEngine;

namespace FPSGame
{
    public class FPSGameManager : MonoBehaviour
    {
        public static FPSGameManager Instance { get; private set; }

        public int score;
        public int killsToWin = 10;
        public Vector3 playerSpawnPosition = new Vector3(0f, 1f, 0f);

        FPSHealth _playerHealth;
        FPSWeapon _playerWeapon;
        bool _gameOver;

        static Texture2D _whitePixel;

        [Header("Crosshair")]
        public bool showCrosshair = true;
        public float crosshairSize = 10f;
        public float crosshairGap = 4f;
        public float crosshairThickness = 2f;

        public int Score => score;
        public bool GameOver => _gameOver;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void RegisterPlayer(FPSHealth health, FPSWeapon weapon)
        {
            _playerHealth = health;
            _playerWeapon = weapon;
        }

        public void RegisterKill()
        {
            if (_gameOver)
                return;

            score++;
            if (score >= killsToWin)
                _gameOver = true;
        }

        public void OnPlayerDied()
        {
            if (_gameOver)
                return;

            RespawnPlayer();
        }

        public void NotifyAmmoChanged(int current, int max)
        {
        }

        public void RespawnPlayer()
        {
            if (_playerHealth == null)
                return;

            Transform player = _playerHealth.transform;
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null)
                cc.enabled = false;

            player.position = playerSpawnPosition;
            _playerHealth.ResetHealth();

            if (cc != null)
                cc.enabled = true;
        }

        void OnGUI()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold
            };

            int health = _playerHealth != null ? _playerHealth.CurrentHealth : 0;
            int maxHealth = _playerHealth != null ? _playerHealth.maxHealth : 100;
            int ammo = _playerWeapon != null ? _playerWeapon.CurrentAmmo : 0;
            int mag = _playerWeapon != null ? _playerWeapon.magazineSize : 30;

            GUI.Label(new Rect(16, 12, 400, 30), $"Health: {health} / {maxHealth}", style);
            GUI.Label(new Rect(16, 36, 400, 30), $"Ammo: {ammo} / {mag}", style);
            GUI.Label(new Rect(16, 60, 400, 30), $"Score: {score} / {killsToWin}", style);

            if (showCrosshair && !_gameOver)
                DrawCrosshair();

            if (_gameOver)
            {
                GUIStyle big = new GUIStyle(style) { fontSize = 32, alignment = TextAnchor.MiddleCenter };
                GUI.Label(new Rect(0, Screen.height * 0.4f, Screen.width, 60), "You Win!", big);
                GUI.Label(new Rect(0, Screen.height * 0.4f + 50, Screen.width, 40), "Press Play again to restart", style);
            }
            else
            {
                GUIStyle hint = new GUIStyle(style) { fontSize = 14, fontStyle = FontStyle.Normal };
                GUI.Label(new Rect(16, Screen.height - 36, 600, 30),
                    "WASD move | Mouse look | LMB shoot | R reload | Shift sprint | Space jump",
                    hint);
            }
        }

        void DrawCrosshair()
        {
            float cx = Screen.width * 0.5f;
            float cy = Screen.height * 0.5f;
            float halfThick = crosshairThickness * 0.5f;

            Color outline = new Color(0f, 0f, 0f, 0.6f);
            Color fill = new Color(1f, 1f, 1f, 0.9f);

            // Horizontal bars
            DrawFilledRect(new Rect(cx - crosshairGap - crosshairSize - 1f, cy - halfThick - 1f, crosshairSize + 2f, crosshairThickness + 2f), outline);
            DrawFilledRect(new Rect(cx + crosshairGap - 1f, cy - halfThick - 1f, crosshairSize + 2f, crosshairThickness + 2f), outline);
            DrawFilledRect(new Rect(cx - crosshairGap - crosshairSize, cy - halfThick, crosshairSize, crosshairThickness), fill);
            DrawFilledRect(new Rect(cx + crosshairGap, cy - halfThick, crosshairSize, crosshairThickness), fill);

            // Vertical bars
            DrawFilledRect(new Rect(cx - halfThick - 1f, cy - crosshairGap - crosshairSize - 1f, crosshairThickness + 2f, crosshairSize + 2f), outline);
            DrawFilledRect(new Rect(cx - halfThick - 1f, cy + crosshairGap - 1f, crosshairThickness + 2f, crosshairSize + 2f), outline);
            DrawFilledRect(new Rect(cx - halfThick, cy - crosshairGap - crosshairSize, crosshairThickness, crosshairSize), fill);
            DrawFilledRect(new Rect(cx - halfThick, cy + crosshairGap, crosshairThickness, crosshairSize), fill);

            // Center dot
            float dot = crosshairThickness;
            DrawFilledRect(new Rect(cx - dot * 0.5f - 1f, cy - dot * 0.5f - 1f, dot + 2f, dot + 2f), outline);
            DrawFilledRect(new Rect(cx - dot * 0.5f, cy - dot * 0.5f, dot, dot), fill);
        }

        static void DrawFilledRect(Rect rect, Color color)
        {
            if (_whitePixel == null)
            {
                _whitePixel = new Texture2D(1, 1);
                _whitePixel.SetPixel(0, 0, Color.white);
                _whitePixel.Apply();
            }

            Color prev = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, _whitePixel);
            GUI.color = prev;
        }
    }
}
