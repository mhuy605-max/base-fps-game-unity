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
void Start()
{
    if (_playerHealth == null)
    {
        foreach (FPSHealth health in FindObjectsByType<FPSHealth>(FindObjectsSortMode.None))
        {
            if (health.isPlayer)
            {
                _playerHealth = health;
                _playerWeapon = health.GetComponent<FPSWeapon>();
                playerSpawnPosition = health.transform.position;
                break;
            }
        }
    }
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
            int health    = _playerHealth != null ? _playerHealth.CurrentHealth : 0;
            int maxHealth = _playerHealth != null ? _playerHealth.maxHealth     : 100;
            int ammo      = _playerWeapon != null ? _playerWeapon.CurrentAmmo   : 0;
            int mag       = _playerWeapon != null ? _playerWeapon.magazineSize  : 30;

            if (showCrosshair && !_gameOver)
                DrawCrosshair();

            DrawHUD(health, maxHealth, ammo, mag);

            if (_gameOver)
                DrawGameOver();
        }

        void DrawHUD(int health, int maxHealth, int ammo, int mag)
        {
            float sw = Screen.width;
            float sh = Screen.height;

            Color panel    = new Color(0f, 0f, 0f, 0.55f);
            Color barBg    = new Color(0f, 0f, 0f, 0.7f);
            Color barGreen = new Color(0.18f, 0.78f, 0.22f, 1f);
            Color barYellow= new Color(0.9f,  0.75f, 0.1f,  1f);
            Color barRed   = new Color(0.85f, 0.15f, 0.15f, 1f);
            Color white    = Color.white;
            Color dimWhite = new Color(1f, 1f, 1f, 0.55f);

            GUIStyle bigNum = new GUIStyle(GUI.skin.label)
            {
                fontSize   = 36,
                fontStyle  = FontStyle.Bold,
                alignment  = TextAnchor.MiddleLeft,
                normal     = { textColor = white }
            };
            GUIStyle smallNum = new GUIStyle(bigNum)
            {
                fontSize  = 20,
                alignment = TextAnchor.MiddleLeft,
                normal    = { textColor = dimWhite }
            };
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal    = { textColor = dimWhite }
            };
            GUIStyle scoreStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal    = { textColor = white }
            };

            // ── HEALTH (bottom-left) ──────────────────────────────────
            float hpPanelW = 220f;
            float hpPanelH = 70f;
            float hpX      = 16f;
            float hpY      = sh - hpPanelH - 16f;

            DrawFilledRect(new Rect(hpX, hpY, hpPanelW, hpPanelH), panel);

            GUI.Label(new Rect(hpX + 10f, hpY + 4f, 80f, 18f), "HEALTH", labelStyle);

            float hpFrac  = Mathf.Clamp01((float)health / maxHealth);
            Color hpColor = hpFrac > 0.5f ? barGreen : hpFrac > 0.25f ? barYellow : barRed;

            float barX = hpX + 10f;
            float barY = hpY + 24f;
            float barW = hpPanelW - 20f;
            float barH = 10f;
            DrawFilledRect(new Rect(barX, barY, barW, barH), barBg);
            DrawFilledRect(new Rect(barX, barY, barW * hpFrac, barH), hpColor);

            GUI.Label(new Rect(hpX + 10f, hpY + 34f, 100f, 34f), health.ToString(), bigNum);

            // ── AMMO (bottom-right) ───────────────────────────────────
            float amPanelW = 180f;
            float amPanelH = 70f;
            float amX      = sw - amPanelW - 16f;
            float amY      = sh - amPanelH - 16f;

            DrawFilledRect(new Rect(amX, amY, amPanelW, amPanelH), panel);

            GUIStyle ammoLabel = new GUIStyle(labelStyle) { alignment = TextAnchor.MiddleRight };
            GUI.Label(new Rect(amX, amY + 4f, amPanelW - 10f, 18f), "AMMO", ammoLabel);

            GUIStyle amBig = new GUIStyle(bigNum) { alignment = TextAnchor.MiddleRight };
            GUIStyle amSml = new GUIStyle(smallNum) { alignment = TextAnchor.MiddleRight };

            GUI.Label(new Rect(amX, amY + 22f, amPanelW - 10f, 40f), ammo.ToString(), amBig);
            GUI.Label(new Rect(amX, amY + 46f, amPanelW - 10f, 22f), $"/ {mag}", amSml);

            // ── SCORE (top-center) ────────────────────────────────────
            float scW = 160f;
            float scH = 36f;
            float scX = (sw - scW) * 0.5f;
            float scY = 12f;

            DrawFilledRect(new Rect(scX, scY, scW, scH), panel);
            GUI.Label(new Rect(scX, scY, scW, scH), $"KILLS  {score} / {killsToWin}", scoreStyle);
        }

        void DrawGameOver()
        {
            float sw = Screen.width;
            float sh = Screen.height;

            Color panel = new Color(0f, 0f, 0f, 0.75f);
            DrawFilledRect(new Rect(sw * 0.3f, sh * 0.3f, sw * 0.4f, sh * 0.35f), panel);

            GUIStyle title = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 40,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal    = { textColor = new Color(0.9f, 0.75f, 0.1f, 1f) }
            };
            GUIStyle sub = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 18,
                alignment = TextAnchor.MiddleCenter,
                normal    = { textColor = new Color(1f, 1f, 1f, 0.8f) }
            };
            GUIStyle statStyle = new GUIStyle(sub) { fontSize = 16 };

            GUI.Label(new Rect(0f, sh * 0.33f, sw, 50f), "GAME OVER", title);
            GUI.Label(new Rect(0f, sh * 0.33f + 55f, sw, 30f), $"Final Score: {score} kills", sub);
            GUI.Label(new Rect(0f, sh * 0.33f + 90f, sw, 26f), "Press Play to restart", statStyle);
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
