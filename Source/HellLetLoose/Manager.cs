using Offsets;
using HellLetLoose.Source.HellLetLoose.Features;

namespace HellLetLoose.Source.HellLetLoose
{
    public class Manager : IDisposable
    {
        public readonly ulong _playerController;
        public readonly bool _inGame;
        private CancellationTokenSource _cancellationTokenSource;
        
        protected ulong _cachedPlayerState = 0;
        protected ulong _cachedSoldierActor = 0;
        protected ulong _cachedInventoryComponent = 0;
        protected ulong _cachedCurrentWeapon = 0;
        protected ulong _cachedCharacterMovement = 0;
        protected DateTime _lastPointerUpdate = DateTime.MinValue;
        
        // Modules
        private QuickZoom _quickZoom;
        private Suppression _suppression;
        private NoRecoil _noRecoil;

        /// <summary>
        /// Constructor for feature classes that inherit from Manager
        /// </summary>
        /// <param name="playerController">The player controller address</param>
        /// <param name="inGame">Whether the game is currently active</param>
        public Manager(ulong playerController, bool inGame)
        {
            _playerController = playerController;
            _inGame = inGame;
            _cancellationTokenSource = null; // Features don't need this
            
            UpdateCachedPointers();
        }
        
        public Manager(ulong playerController, bool inGame, RegistredActors actors)
        {
            _playerController = playerController;
            _inGame = inGame;
            _cancellationTokenSource = new CancellationTokenSource();
            
            // Initialize cached pointers
            UpdateCachedPointers();
            
            // Initialize all feature modules
            InitializeFeatures();
            
            // Start a timer to apply features
            StartFeatureTimer();
        }
        
        /// <summary>
        /// Updates cached pointers to avoid redundant memory reads
        /// </summary>
        protected void UpdateCachedPointers()
        {
            try
            {
                if ((DateTime.Now - _lastPointerUpdate).TotalMilliseconds < 500 && 
                    _cachedPlayerState != 0 && _cachedSoldierActor != 0)
                    return;
                    
                if (!_inGame || _playerController == 0) return;
                _cachedPlayerState = Memory.ReadPtr(_playerController + Controller.PlayerState);
                if (_cachedPlayerState == 0) return;
                
                _cachedSoldierActor = Memory.ReadPtr(_playerController + Controller.Character);
                if (_cachedSoldierActor == 0) return;
                
                _cachedInventoryComponent = Memory.ReadPtr(_cachedSoldierActor + AShooterCharacter.Inventory);

                _cachedCurrentWeapon = Memory.ReadPtr(_cachedSoldierActor + AShooterCharacter.CurrentWeapon);
                _cachedCharacterMovement = Memory.ReadPtr(_cachedSoldierActor + Character.CharacterMovement);
                
                _lastPointerUpdate = DateTime.Now;
            }
            catch
            {
                // Reset pointers on error
                _cachedPlayerState = 0;
                _cachedSoldierActor = 0;
                _cachedInventoryComponent = 0;
                _cachedCurrentWeapon = 0;
                _cachedCharacterMovement = 0;
            }
        }
        
        private void InitializeFeatures()
        {
            _quickZoom = new QuickZoom(_playerController, _inGame);

            _noRecoil = new NoRecoil(_playerController, _inGame);
            _suppression = new Suppression(_playerController, _inGame);
        }

        /// <summary>
        /// Checks if the local player is valid (has a valid player state and soldier actor)
        /// </summary>
        /// <returns>True if local player is valid, false otherwise</returns>
        public bool IsLocalPlayerValid()
        {
            try
            {
                if (!_inGame || _playerController == 0)
                {
                    return false;
                }

                ulong playerState = _cachedPlayerState != 0 ? _cachedPlayerState : Memory.ReadPtr(_playerController + Controller.PlayerState);
                if (playerState == 0)
                {
                    return false;
                }

                ulong soldierActor = _cachedSoldierActor != 0 ? _cachedSoldierActor : Memory.ReadPtr(playerState + Controller.Pawn);
                if (soldierActor == 0)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Program.Log($"IsLocalPlayerValid: Exception - {ex.Message}");
                return false;
            }
        }

        // Start a timer to apply features every second
        // Simple fix for when the Localplayer respawns
        // Need to change it to a better solution
        private void StartFeatureTimer()
        {
            Task.Run(async () =>
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        UpdateCachedPointers();

                        if (_suppression._isSuppressionEnabled)
                        {
                            _suppression.Apply();
                        }
                        if (_noRecoil._isNoRecoilEnabled)
                        {
                            _noRecoil.Apply();
                        }

                        if (_quickZoom._isQuickZoomEnabled)
                        {
                            _quickZoom.Apply();
                        }

                    }
                    catch { /* Silently fail */ }
                    await Task.Delay(1000, _cancellationTokenSource.Token);
                }
            }, _cancellationTokenSource.Token);
        }
        
        /// <summary>
        /// Apply method to be implemented by features
        /// </summary>
        public virtual void Apply() { }
        
        #region Feature Control Methods
        
        public void SetQuickZoom(bool enable)
        {
            _quickZoom.SetEnabled(enable);
        }
        public void SetNoRecoil(bool enable)
        {
            _noRecoil.SetEnabled(enable);
        }
        public void SetNoSuppression(bool enable)
        {
            _suppression.SetEnabled(enable);
        }

        public void Dispose()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
            }
        }
        #endregion
    }
}