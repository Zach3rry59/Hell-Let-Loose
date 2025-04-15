using squad_dma.Source.Squad.Features;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Collections.Generic;

namespace squad_dma
{
    /// <summary>
    /// Class containing Game instance.
    /// </summary>
    public class Game
    {
        #region Fields
        private readonly ulong _squadBase;
        private volatile bool _inGame = false;
        private RegistredActors _actors;
        private UActor _localUPlayer;
        private ulong _gameWorld;
        private ulong _gameInstance;
        private ulong _localPlayer;
        private ulong _playerController;
        private Vector3 _absoluteLocation;
        private string _currentLevel = string.Empty;
        private DateTime _lastTeamCheck = DateTime.MinValue;
        private const int TeamCheckInterval = 1000;

        private ulong _currentWeaponPtr;
        private bool _isAimingDownSights;
        private bool _hasPipScope;
        private float _currentFOV;
        private bool _isFiring = false;

        private Source.Squad.Manager _soldierManager;
        #endregion

        #region Properties
public bool InGame => _inGame;
        public string MapName => _currentLevel;
        public UActor LocalPlayer => _localUPlayer;
        public ReadOnlyDictionary<ulong, UActor> Actors => _actors?.Actors;
        public Vector3 AbsoluteLocation => _absoluteLocation;
        public bool IsAimingDownSights => _isAimingDownSights;
        public bool HasPipScope => _hasPipScope;
        public float CurrentFOV => _currentFOV;
        public bool IsFiring => _isFiring;
        #endregion

        #region Constructor
        public Game(ulong squadBase)
        {
            _squadBase = squadBase;
        }
        #endregion


        #region Public Methods
        
        public void SetQuickZoom(bool enable) => _soldierManager?.SetQuickZoom(enable);

        public void WaitForGame()
        {
            while (true)
            {
                try
                {
                    if (!Memory.GetModuleBase())
                    {
                        throw new GameNotRunningException("Process terminated during wait");
                    }

                    if (GetGameWorld() && GetGameInstance() && GetCurrentLevel() && InitActors() && GetLocalPlayer())
                    {
                        if (!Memory.GetModuleBase())
                        {
                            throw new GameNotRunningException("Process terminated during initialization");
                        }

                        Thread.Sleep(1000);
                        Program.Log("Game has started!!");
                        this._inGame = true;
                        Memory.GameStatus = GameStatus.InGame;

                        InitializeManagers();

                        return;
                    }
                }
                catch (GameNotRunningException)
                {
                    throw;
                }
                Thread.Sleep(500);
            }
        }

        public void GameLoop()
        {
            try
            {
                if (!this._inGame)
                {
                    throw new GameEnded("Game has ended!");
                }

                UpdateLocalPlayerInfo();
                this._actors.UpdateList();
                this._actors.UpdateAllPlayers();

                
            }
            catch (DMAShutdown)
            {
                HandleDMAShutdown();
            }
            catch (GameEnded e)
            {
                HandleGameEnded(e);
            }
            catch (Exception ex)
            {
                HandleUnexpectedException(ex);
            }
        }
        #endregion

        #region Private Methods
        private void InitializeManagers()
        {
            // todo: Initialize all managers here
        }

        private bool TryExecute(Action action)
        {
            try
            {
                action();
                return true;
            }
            catch { return false; }
        }

        private void HandleDMAShutdown()
        {
            Program.Log("DMA shutdown");
            this._inGame = false;
        }

        private void HandleGameEnded(GameEnded e)
        {
            Program.Log("Game has ended!");
            this._inGame = false;
            Memory.GameStatus = GameStatus.Menu;
            Memory.Restart();
        }

        private void HandleUnexpectedException(Exception ex)
        {
            Program.Log($"CRITICAL ERROR - Game ended due to unhandled exception: {ex}");
            this._inGame = false;
        }

        private bool GetGameWorld() =>
            TryExecute(() =>
            {
                var gWorldAddress = _squadBase + Offsets.GameObjects.GWorld;
                //Program.Log($"GWorld Address: {gWorldAddress:X}");
                _gameWorld = Memory.ReadPtr(gWorldAddress);
            });

        private bool GetGameInstance() =>
            TryExecute(() =>
            {
                var gameInstanceAddress = _gameWorld + Offsets.World.OwningGameInstance;
                //Program.Log($"GameInstance Address: {gameInstanceAddress:X}");
                _gameInstance = Memory.ReadPtr(gameInstanceAddress);
            });

        private bool GetCurrentLevel() =>
            TryExecute(() =>
            {
                var currentMapAddress = _gameInstance + Offsets.GameInstance.MapLoadingData;
                //Program.Log($"CurrentLayer Address: {currentLayerAddress:X}");
                var currentMap = Memory.ReadPtr(currentMapAddress);

                var currentmap = currentMapAddress + Offsets.UMapLoadingScreenData.MapName;
                //Program.Log($"CurrentLevelId Address: {currentLevelIdAddress:X}");
                string mapName = Memory.ReadString(currentMapAddress);

                _currentLevel = mapName;
                Program.Log($"Current level is {_currentLevel}");
            });

        private bool InitActors() =>
            TryExecute(() =>
            {
                var persistentLevelAddress = _gameWorld + Offsets.World.PersistentLevel;
                //Program.Log($"PersistentLevel Address: {persistentLevelAddress:X}");
                var persistentLevel = Memory.ReadPtr(persistentLevelAddress);
                _actors = new RegistredActors(persistentLevel);
            });

        private bool GetLocalPlayer() =>
            TryExecute(() =>
            {
                var localPlayersAddress = _gameInstance + Offsets.GameInstance.LocalPlayers;
                //Program.Log($"LocalPlayers Address: {localPlayersAddress:X}");
                var localPlayers = Memory.ReadPtr(localPlayersAddress);

                Program.Log($"LocalPlayer Address: {localPlayers:X}");
                _localPlayer = Memory.ReadPtr(localPlayers);

                _localUPlayer = new UActor(_localPlayer);
                _localUPlayer.Team = Team.Unknown;
                GetPlayerController();
            });

        private bool GetPlayerController() =>
            TryExecute(() =>
            {
                var playerControllerAddress = _localPlayer + Offsets.UPlayer.PlayerController;
                _playerController = Memory.ReadPtr(playerControllerAddress);
                Program.Log($"PlayerController Address: {_playerController:X}");
            });

        private bool UpdateLocalPlayerInfo()
        {
            try
            {
                if ((DateTime.Now - _lastTeamCheck).TotalMilliseconds > TeamCheckInterval)
                {
                    _lastTeamCheck = DateTime.Now;

                    try
                    {
                        ulong playerState = Memory.ReadPtr(_playerController + Offsets.Controller.PlayerState);

                        if (playerState == 0)
                        {
                            Program.Log("UpdateLocalPlayerInfo: PlayerState is null");
                            return false;
                        }

                        byte teamId = Memory.ReadValue<byte>(playerState + Offsets.AShooterPlayerState.RepPlayerInfo + Offsets.FHLLPlayerInfo.PlayerTeam);
                        int squadId = Memory.ReadValue<int>(playerState + Offsets.AShooterPlayerState.RepPlayerInfo + Offsets.FHLLPlayerInfo.PlatoonIndex);

                        if (_localUPlayer.TeamID != teamId || _localUPlayer.SquadID != squadId)
                        {
                            _localUPlayer.TeamID = teamId;
                            _localUPlayer.SquadID = squadId;
                            Program.Log($"LocalPlayer updated: TeamID={teamId}, SquadID={squadId}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Program.Log($"UpdateLocalPlayerInfo: Error reading PlayerState - {ex.Message}");
                        return false;
                    }
                }

                GetCameraCache();
                return true;
            }
            catch (Exception ex)
            {
                Program.Log($"UpdateLocalPlayerInfo: Critical error - {ex.Message}");
                return false;
            }
        }

        private bool GetCameraCache()
        {
            try
            {
                if (_playerController == 0 || _gameWorld == 0)
                {
                    Program.Log("GetCameraCache: PlayerController or GameWorld is null");
                    return false;
                }

                var cameraInfoScatterMap = new ScatterReadMap(1);
                var cameraManagerRound = cameraInfoScatterMap.AddRound();

                cameraManagerRound.AddEntry<ulong>(0, 0, _playerController + Offsets.PlayerController.PlayerCameraManager);
                cameraManagerRound.AddEntry<float>(0, 11, _gameWorld + Offsets.World.WorldOrigin);
                cameraManagerRound.AddEntry<float>(0, 12, _gameWorld + Offsets.World.WorldOrigin + 0x4);
                cameraManagerRound.AddEntry<float>(0, 13, _gameWorld + Offsets.World.WorldOrigin + 0x8);

                cameraInfoScatterMap.Execute();

                if (!cameraInfoScatterMap.Results[0][0].TryGetResult<ulong>(out var cameraManagerAddr) || cameraManagerAddr == 0)
                {
                    Program.Log("GetCameraCache: Invalid PlayerCameraManager");
                    return false;
                }

                var viewTargetPtr = cameraManagerAddr + Offsets.Camera.ViewTarget;
                var povPtr = viewTargetPtr + Offsets.FTViewTarget.POV;

                var viewInfoScatterMap = new ScatterReadMap(1);
                var viewInfoRound = viewInfoScatterMap.AddRound();

                viewInfoRound.AddEntry<float>(0, 14, povPtr + Offsets.FMinimalViewInfo.Location_X);
                viewInfoRound.AddEntry<float>(0, 15, povPtr + Offsets.FMinimalViewInfo.Location_Y);
                viewInfoRound.AddEntry<float>(0, 16, povPtr + Offsets.FMinimalViewInfo.Location_Z);
                viewInfoRound.AddEntry<float>(0, 17, povPtr + Offsets.FMinimalViewInfo.Rotation_Pitch);
                viewInfoRound.AddEntry<float>(0, 18, povPtr + Offsets.FMinimalViewInfo.Rotation_Yaw);
                viewInfoRound.AddEntry<float>(0, 19, povPtr + Offsets.FMinimalViewInfo.Rotation_Roll);
                viewInfoRound.AddEntry<float>(0, 20, povPtr + Offsets.FMinimalViewInfo.FOV);

                viewInfoScatterMap.Execute();

                if (cameraInfoScatterMap.Results[0][11].TryGetResult<float>(out var absoluteX) &&
                    cameraInfoScatterMap.Results[0][12].TryGetResult<float>(out var absoluteY) &&
                    cameraInfoScatterMap.Results[0][13].TryGetResult<float>(out var absoluteZ))
                {
                    _absoluteLocation = new Vector3(absoluteX, absoluteY, absoluteZ);
                }
                else
                {
                    _absoluteLocation = Vector3.Zero;
                    Program.Log("GetCameraCache: Failed to read WorldOrigin");
                }

                if (viewInfoScatterMap.Results[0][14].TryGetResult<float>(out var x) &&
                    viewInfoScatterMap.Results[0][15].TryGetResult<float>(out var y) &&
                    viewInfoScatterMap.Results[0][16].TryGetResult<float>(out var z))
                {
                    _localUPlayer.Position = new Vector3(
                        x + _absoluteLocation.X,
                        y + _absoluteLocation.Y,
                        z + _absoluteLocation.Z
                    );
                }
                else
                {
                    Program.Log("GetCameraCache: Failed to read Location");
                    return false;
                }

                if (viewInfoScatterMap.Results[0][17].TryGetResult<float>(out var rotX) &&
                    viewInfoScatterMap.Results[0][18].TryGetResult<float>(out var rotY) &&
                    viewInfoScatterMap.Results[0][19].TryGetResult<float>(out var rotZ))
                {
                    var rotation = new Vector3(rotX, rotY, rotZ);
                    _localUPlayer.Rotation = new Vector2(rotation.Y, rotation.X);
                    _localUPlayer.Rotation3D = rotation;
                }
                else
                {
                    Program.Log("GetCameraCache: Failed to read Rotation");
                    return false;
                }

                if (viewInfoScatterMap.Results[0][20].TryGetResult<float>(out var cameraFOV))
                {
                    _currentFOV = cameraFOV;
                }
                else
                {
                    _currentFOV = 90.0f;
                    Program.Log("GetCameraCache: Failed to read FOV, using default 90.0");
                }

                return true;
            }
            catch (Exception ex)
            {
                Program.Log($"GetCameraCache: Error - {ex.Message}");
                return false;
            }
        }
        #endregion
    }

    #region Exceptions
    public class GameNotRunningException : Exception
    {
        public GameNotRunningException() { }
        public GameNotRunningException(string message) : base(message) { }
        public GameNotRunningException(string message, Exception inner) : base(message, inner) { }
    }

    public class GameEnded : Exception
    {
        public GameEnded() { }
        public GameEnded(string message) : base(message) { }
        public GameEnded(string message, Exception inner) : base(message, inner) { }
    }
    #endregion
}