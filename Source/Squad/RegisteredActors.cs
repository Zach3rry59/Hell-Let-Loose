using Offsets;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Numerics;
using System.Linq;

namespace squad_dma
{
    public class RegistredActors
    {
        private readonly ulong _persistentLevel;
        private ulong _actorsArray;
        private readonly Stopwatch _regSw = new();
        private readonly ConcurrentDictionary<ulong, UActor> _actors = new();
        private Dictionary<ulong, int> _squadCache = new();
        private DateTime _lastSquadUpdate = DateTime.MinValue;
        private const int SquadUpdateInterval = 1000;

        public IEnumerable<uint> GetActorNameIds()
        {
            return _actors.Values.Select(actor => actor.NameId).Where(id => id != 0);
        }

        #region Getters
        public ReadOnlyDictionary<ulong, UActor> Actors { get; }

        public int ActorCount
        {
            get
            {
                const int maxAttempts = 5;
                for (int attempt = 0; attempt < maxAttempts; attempt++)
                {
                    try
                    {
                        if (_persistentLevel == 0)
                        {
                            Program.Log("Skipping ActorCount read: Game state not ready.");
                            return -1;
                        }
                        var count = Memory.ReadValue<int>(_persistentLevel + Offsets.Level.MaxPacket);
                        if (count < 1)
                        {
                            this._actors.Clear();
                            return -1;
                        }
                        return count;
                    }
                    catch (DMAShutdown)
                    {
                        throw;
                    }
                    catch (Exception ex) when (attempt < maxAttempts - 1)
                    {
                        if (ex.InnerException is ArgumentOutOfRangeException)
                        {
                            Program.Log($"CRITICAL - Invalid memory address for ActorCount: {ex}");
                            return -1;
                        }
                        Program.Log($"ERROR - PlayerCount attempt {attempt + 1} failed: {ex}");
                        Thread.Sleep(700);
                    }
                }
                return -1;
            }
        }
        #endregion

        public RegistredActors(ulong persistentLevelAddr)
        {
            this._persistentLevel = persistentLevelAddr;
            this.Actors = new(this._actors);
            this._actorsArray = Memory.ReadPtr(_persistentLevel + Offsets.Level.Actors);
            this._regSw.Start();
            //Program.Log($"RegistredActors initialized with persistentLevel: 0x{persistentLevelAddr:X}");
        }

        #region Update List/Player Functions
        public Dictionary<ulong, uint> GetActorBaseWithName()
        {
            var count = this.ActorCount;
            if (count < 1)
                return new Dictionary<ulong, uint>();

            var initialActorScatterMap = new ScatterReadMap(count);
            var actorRound = initialActorScatterMap.AddRound();
            var playerObjectIdRound = initialActorScatterMap.AddRound();

            for (int i = 0; i < count; i++)
            {
                var actorAddr = actorRound.AddEntry<ulong>(i, 0, _actorsArray + (uint)(i * 0x8));
                var playerObjectId = playerObjectIdRound.AddEntry<uint>(i, 1, actorAddr, null, Offsets.Actor.ID);
            }

            initialActorScatterMap.Execute();

            var actorBaseWithName = new Dictionary<ulong, uint>();
            for (int i = 0; i < count; i++)
            {
                if (!initialActorScatterMap.Results[i][0].TryGetResult<ulong>(out var actorAddr) || actorAddr == 0)
                    continue;
                if (!initialActorScatterMap.Results[i][1].TryGetResult<uint>(out var actorNameId) || actorNameId == 0)
                    continue;
                actorBaseWithName[actorAddr] = actorNameId;
            }

            return actorBaseWithName;
        }

        private static Random rng = new Random();
        public void UpdateList()
        {
            if (this._regSw.ElapsedMilliseconds < 500)
                return;

            try
            {
                var count = this.ActorCount;
                if (count < 6)
                    throw new GameEnded();

                var initialActorScatterMap = new ScatterReadMap(count);
                var actorRound = initialActorScatterMap.AddRound();
                var playerObjectIdRound = initialActorScatterMap.AddRound();

                for (int i = 0; i < count; i++)
                {
                    var actorAddr = actorRound.AddEntry<ulong>(i, 0, _actorsArray + (uint)(i * 0x8));
                    var playerObjectId = playerObjectIdRound.AddEntry<uint>(i, 1, actorAddr, null, Offsets.Actor.ID);
                }

                initialActorScatterMap.Execute();

                var actorBaseWithName = new Dictionary<ulong, uint>();
                for (int i = 0; i < count; i++)
                {
                    if (!initialActorScatterMap.Results[i][0].TryGetResult<ulong>(out var actorAddr) || actorAddr == 0)
                    {
                        continue;
                    }
                    if (!initialActorScatterMap.Results[i][1].TryGetResult<uint>(out var actorNameId) || actorNameId == 0)
                    {
                        continue;
                    }
                    actorBaseWithName[actorAddr] = actorNameId;
                    //Program.Log($"UpdateList: Actor 0x{actorAddr:X}, NameId={actorNameId}");
                }

                var notUpdated = new HashSet<ulong>(_actors.Keys);
                foreach (var item in actorBaseWithName)
                {
                    if (_actors.ContainsKey(item.Key) && _actors[item.Key].NameId == item.Value)
                    {
                        notUpdated.Remove(item.Key);
                        actorBaseWithName.Remove(item.Key);
                    }
                }

                var names = Memory.GetNamesById([.. actorBaseWithName.Values.Distinct()]);
                foreach (var item in actorBaseWithName)
                {
                    if (!names.TryGetValue(item.Value, out var actorName))
                    {
                        actorName = "Unknown";
                    }
                    //Program.Log($"UpdateList: Actor 0x{item.Key:X}, NameId={item.Value}, Name={actorName}");
                }

                var playersNameIDs = names.Where(x => x.Value.Contains("PlayerPawn") || Names.TechNames.ContainsKey(x.Value)).ToDictionary();
                var filteredActors = actorBaseWithName.Where(actor => playersNameIDs.ContainsKey(actor.Value)).Select(actor => actor.Key).ToList();
                count = filteredActors.Count;

                for (int i = 0; i < count; i++)
                {
                    var actorAddr = filteredActors[i];
                    var nameId = actorBaseWithName[actorAddr];
                    var actorName = playersNameIDs[nameId];
                    var team = Team.Unknown;
                    var actorType = Names.TechNames.GetValueOrDefault(actorName, ActorType.Player);
                    if (actorType == ActorType.Player)
                        team = Team.Unknown;

                    if (_actors.TryGetValue(actorAddr, out var actor))
                    {
                        if (actor.ErrorCount > 50)
                        {
                            Program.Log($"Existing player '{actor.Base}' being reallocated due to excessive errors...");
                            reallocateActor(actorAddr, team, actorType, nameId);
                        }
                        else if (actor.Base != actorAddr)
                        {
                            Program.Log($"Existing player '{actor.Base}' being reallocated due to new base address...");
                            reallocateActor(actorAddr, team, actorType, nameId);
                        }
                    }
                    else
                    {
                        reallocateActor(actorAddr, team, actorType, nameId);
                    }
                    _actors[actorAddr].Name = actorName;
                    notUpdated.Remove(actorAddr);
                }

                foreach (var actorIdToRemove in notUpdated)
                {
                    _actors.TryRemove(actorIdToRemove, out var _);
                }
            }
            catch (DMAShutdown)
            {
                throw;
            }
            catch (GameEnded)
            {
                throw;
            }
            catch (Exception ex)
            {
                Program.Log($"CRITICAL ERROR - RegisteredActors Loop FAILED: {ex}");
            }
            finally
            {
                this._regSw.Restart();
            }
        }

        UActor reallocateActor(ulong actorBase, Team team, ActorType actorType, uint nameId)
            {
                try
                {
                    _actors[actorBase] = new UActor(actorBase)
                    {
                        Team = team,
                        ActorType = actorType,
                        NameId = nameId,
                    };
                    return _actors[actorBase];
                }
                catch (Exception ex)
                {
                    throw new Exception($"ERROR re-allocating player: ", ex);
                }
            }
        
        public void UpdateAllPlayers()
        {
            try
            {
                var count = _actors.Count;

                if (count < 6)
                    throw new GameEnded();

                var actorBases = _actors.Values.Select(actor => actor.Base).Order().ToArray();
                var playerInfoScatterMap = new ScatterReadMap(count);
                var playerInstanceInfoRound = playerInfoScatterMap.AddRound();
                var instigatorAndRootRound = playerInfoScatterMap.AddRound();
                var teamInfoRound = playerInfoScatterMap.AddRound();
                var meshRound = playerInfoScatterMap.AddRound();
                var boneInfoRound = playerInfoScatterMap.AddRound();

                int[] boneIds = { 11, 9, 8, 7, 5, 14, 15, 16, 17, 38, 39, 40, 41, 68, 63, 64, 66, 69, 70, 71, 73 };

                for (int i = 0; i < count; i++)
                {
                    var actorAddr = actorBases[i];
                    var actorType = _actors[actorAddr].ActorType;

                    var rootComponent = playerInstanceInfoRound.AddEntry<ulong>(i, 1, actorAddr + Offsets.Actor.RootComponent);

                    if (actorType == ActorType.Player)
                    {
                        // Existing Player logic remains unchanged
                        playerInstanceInfoRound.AddEntry<float>(i, 2, actorAddr + Offsets.AShooterCharacter.Health);
                        var pawnPlayerState = playerInstanceInfoRound.AddEntry<ulong>(i, 6, actorAddr + Offsets.Pawn.PlayerState);
                        var controller = playerInstanceInfoRound.AddEntry<ulong>(i, 7, actorAddr + Offsets.Pawn.Controller);
                        var controllerPlayerState = teamInfoRound.AddEntry<ulong>(i, 8, controller, null, Offsets.Controller.PlayerState);

                        teamInfoRound.AddEntry<byte>(i, 9, pawnPlayerState, null, Offsets.AShooterPlayerState.RepPlayerInfo + Offsets.FHLLPlayerInfo.PlayerTeam);
                        teamInfoRound.AddEntry<int>(i, 10, pawnPlayerState, null, Offsets.AShooterPlayerState.RepPlayerInfo + Offsets.FHLLPlayerInfo.PlatoonIndex);
                        teamInfoRound.AddEntry<ulong>(i, 16, pawnPlayerState, null, 0x300);
                        teamInfoRound.AddEntry<int>(i, 17, pawnPlayerState, null, 0x300 + 8);

                        var meshPtr = playerInstanceInfoRound.AddEntry<ulong>(i, 11, actorAddr + Offsets.AShooterCharacter.Mesh);
                        meshRound.AddEntry<FTransform>(i, 12, meshPtr, null, Offsets.USceneComponent.ComponentToWorld);
                        var boneArrayPtr = meshRound.AddEntry<ulong>(i, 13, meshPtr, null, Offsets.USkeletalMeshComponent.BonesArray);

                        for (int j = 0; j < boneIds.Length; j++)
                        {
                            boneInfoRound.AddEntry<FTransform>(i, 25 + j, boneArrayPtr, null, (uint)(boneIds[j] * 0x30));
                        }
                    }
                    else if (Names.Deployables.Contains(actorType))
                    {
                        if (actorType == ActorType.Mine)
                        {
                            teamInfoRound.AddEntry<byte>(i, 5, actorAddr + Offsets.AHLLExplosive.Team);
                        }

                        else if (actorType == ActorType.Hab || actorType == ActorType.RallyPoint)
                        {
                            teamInfoRound.AddEntry<byte>(i, 5, actorAddr + ADynamicSpawn.Team);
                        }
                        else
                        {
                            var healthComponentPtr = playerInstanceInfoRound.AddEntry<ulong>(i, 2, actorAddr + Offsets.AHLLDispenseStructure.HealthComponent);
                            playerInstanceInfoRound.AddEntry<float>(i, 3, healthComponentPtr, null, Offsets.UHLLSimpleHealthComponent.HealthInfo + Offsets.FRepHealthInfo.Health);
                            teamInfoRound.AddEntry<byte>(i, 5, actorAddr + Offsets.AHLLDispenseStructure.Team);
                        }
                    }
                    
                    else if (Names.Tanks.Contains(actorType))
                    {
                        var armourHealthPtr = playerInstanceInfoRound.AddEntry<ulong>(i, 2, actorAddr + Offsets.ABaseTank.ArmourHealth);
                        playerInstanceInfoRound.AddEntry<ushort>(i, 3, armourHealthPtr, null, Offsets.UHLLArmourHealthComponent.ArmourInfo + Offsets.FHLLArmourHealthData.CurrentHealth);
                        playerInstanceInfoRound.AddEntry<ushort>(i, 4, armourHealthPtr, null, Offsets.UHLLArmourHealthComponent.ArmourInfo);
                        teamInfoRound.AddEntry<byte>(i, 5, actorAddr + Offsets.ABaseTank.Team);
                    }
                    else // Vehicle
                    {
                        var armourHealthPtr = playerInstanceInfoRound.AddEntry<ulong>(i, 2, actorAddr + Offsets.ABaseVehicle.ArmourHealth);
                        playerInstanceInfoRound.AddEntry<ushort>(i, 3, armourHealthPtr, null, Offsets.UHLLArmourHealthComponent.ArmourInfo + Offsets.FHLLArmourHealthData.CurrentHealth);
                        playerInstanceInfoRound.AddEntry<ushort>(i, 4, armourHealthPtr, null, Offsets.UHLLArmourHealthComponent.ArmourInfo);
                        teamInfoRound.AddEntry<byte>(i, 5, actorAddr + Offsets.ABaseVehicle.Team);
                    }

                    instigatorAndRootRound.AddEntry<Vector3>(i, 14, rootComponent, null, Offsets.USceneComponent.RelativeLocation);
                    instigatorAndRootRound.AddEntry<Vector3>(i, 15, rootComponent, null, Offsets.USceneComponent.RelativeRotation);
                }

                playerInfoScatterMap.Execute();

                bool updatePlatoons = (DateTime.Now - _lastSquadUpdate).TotalMilliseconds > SquadUpdateInterval;

                for (int i = 0; i < count; i++)
                {
                    var actor = _actors[actorBases[i]];
                    var results = playerInfoScatterMap.Results[i];

                    if (actor.ActorType == ActorType.Player)
                    {
                        if (results.TryGetValue(2, out var healthResult) && healthResult.TryGetResult<float>(out var hp))
                        {
                            if (actor.Health > 0 && hp <= 0)
                            {
                                actor.DeathPosition = actor.Position;
                                actor.TimeOfDeath = DateTime.Now;
                            }
                            actor.Health = hp;
                            actor.MaxHealth = 100.0f;
                        }
                        else
                        {
                            actor.Health = -1;
                            actor.MaxHealth = -1;
                        }

                        bool teamIdFound = false;

                        if (results.TryGetValue(9, out var teamResult) &&
                            teamResult.TryGetResult<byte>(out var teamId))
                        {
                            actor.TeamID = teamId; // ETeam
                            teamIdFound = true;
                        }

                        if (results.TryGetValue(10, out var platoonResult) &&
                            platoonResult.TryGetResult<int>(out var platoonId))
                        {
                            actor.SquadID = platoonId; // PlatoonIndex
                        }
                        else
                        {
                            actor.SquadID = -1;
                        }

                        if (!teamIdFound && results.TryGetValue(6, out var playerStateResult) &&
                            playerStateResult.TryGetResult<ulong>(out var playerStateAddr) &&
                            playerStateAddr != 0)
                        {
                            try
                            {
                                actor.TeamID = Memory.ReadValue<byte>(playerStateAddr + Offsets.AShooterPlayerState.RepPlayerInfo + Offsets.FHLLPlayerInfo.PlayerTeam);
                                actor.SquadID = Memory.ReadValue<int>(playerStateAddr + Offsets.AShooterPlayerState.RepPlayerInfo + Offsets.FHLLPlayerInfo.PlatoonIndex);
                                teamIdFound = true;
                            }
                            catch { /* Silently fail */ }
                        }

                        if (!teamIdFound)
                        {
                            actor.TeamID = -1;
                            actor.SquadID = -1;
                        }

                        if (actor.IsFriendly())
                        {
                            if (_squadCache.TryGetValue(actor.Base, out var cachedPlatoonId))
                            {
                                actor.SquadID = cachedPlatoonId;
                            }
                        }

                        if (results.TryGetValue(11, out var meshResult) && meshResult.TryGetResult<ulong>(out var meshAddr))
                        {
                            actor.Mesh = meshAddr;
                            if (meshAddr == 0)
                            {
                                actor.BoneScreenPositions = new Vector2[boneIds.Length];
                                Array.Clear(actor.BoneScreenPositions, 0, actor.BoneScreenPositions.Length);
                                continue;
                            }

                            if (results.TryGetValue(12, out var ctwResult) && ctwResult.TryGetResult<FTransform>(out var ctw))
                            {
                                actor.ComponentToWorld = ctw;
                            }
                            else
                            {
                                actor.BoneScreenPositions = new Vector2[boneIds.Length];
                                Array.Clear(actor.BoneScreenPositions, 0, actor.BoneScreenPositions.Length);
                                continue;
                            }

                            if (results.TryGetValue(13, out var boneArrayResult) && boneArrayResult.TryGetResult<ulong>(out var boneArrayPtr))
                            {
                                if (boneArrayPtr == 0)
                                {
                                    actor.BoneScreenPositions = new Vector2[boneIds.Length];
                                    Array.Clear(actor.BoneScreenPositions, 0, actor.BoneScreenPositions.Length);
                                    continue;
                                }

                                actor.BoneTransforms.Clear();
                                var viewInfo = new MinimalViewInfo
                                {
                                    Location = Memory._game.LocalPlayer.Position,
                                    Rotation = Memory._game.LocalPlayer.Rotation3D,
                                    FOV = Memory._game.CurrentFOV
                                };
                                actor.BoneScreenPositions = new Vector2[boneIds.Length];

                                bool anyBoneSuccess = false;
                                for (int j = 0; j < boneIds.Length; j++)
                                {
                                    if (results.TryGetValue(25 + j, out var boneResult) &&
                                        boneResult.TryGetResult<FTransform>(out var boneTransform))
                                    {
                                        actor.BoneTransforms[boneIds[j]] = boneTransform;
                                        Vector3 boneWorldPos = TransformToWorld(boneTransform, actor.ComponentToWorld);
                                        actor.BoneScreenPositions[j] = Camera.WorldToScreen(viewInfo, boneWorldPos);
                                        if (actor.BoneScreenPositions[j] != Vector2.Zero)
                                        {
                                            anyBoneSuccess = true;
                                        }
                                    }
                                    else
                                    {
                                        actor.BoneScreenPositions[j] = Vector2.Zero;
                                    }
                                }

                                if (!anyBoneSuccess)
                                {
                                    Array.Clear(actor.BoneScreenPositions, 0, actor.BoneScreenPositions.Length);
                                }
                            }
                            else
                            {
                                actor.BoneScreenPositions = new Vector2[boneIds.Length];
                                Array.Clear(actor.BoneScreenPositions, 0, actor.BoneScreenPositions.Length);
                            }
                            if (results.TryGetValue(16, out var dataPtrResult) && dataPtrResult.TryGetResult<ulong>(out var dataPtr) &&
                                results.TryGetValue(17, out var numResult) && numResult.TryGetResult<int>(out var num))
                            {
                                if (dataPtr != 0 && num > 0)
                                {
                                    try
                                    {
                                        actor.PrivateName = Memory.ReadString(dataPtr, (uint)(num * 2), true);
                                    }
                                    catch
                                    {
                                        actor.PrivateName = string.Empty;
                                    }
                                }
                                else
                                {
                                    actor.PrivateName = string.Empty;
                                }
                            }
                            else
                            {
                                actor.PrivateName = string.Empty;
                            }

                        }

                        else
                        {
                            actor.Mesh = 0;
                            actor.BoneScreenPositions = new Vector2[boneIds.Length];
                            Array.Clear(actor.BoneScreenPositions, 0, actor.BoneScreenPositions.Length);
                            continue;
                        }
                    }
                    else // Vehicle
                    {
                        if (results.TryGetValue(3, out var healthResult) && healthResult.TryGetResult<ushort>(out var currentHealth))
                        {
                            actor.Health = currentHealth;
                        }
                        else
                        {
                            actor.Health = -1;
                        }

                        if (results.TryGetValue(4, out var maxHealthResult) && maxHealthResult.TryGetResult<ushort>(out var maxHealth))
                        {
                            actor.MaxHealth = maxHealth;
                        }
                        else
                        {
                            actor.MaxHealth = -1;
                        }

                        if (actor.Health > 0 && actor.MaxHealth > 0)
                        {
                            actor.Health = (actor.Health / actor.MaxHealth) * 100;
                        }

                        if (results.TryGetValue(5, out var teamResult) &&
                            teamResult.TryGetResult<byte>(out var teamId))
                        {
                            actor.TeamID = teamId;
                        }
                        else
                        {
                            actor.TeamID = -1;
                        }
                    }

                    if (results.TryGetValue(14, out var locResult) &&
                        locResult.TryGetResult<Vector3>(out var location))
                    {
                        actor.Position = location;
                    }

                    if (results.TryGetValue(15, out var rotResult) &&
                        rotResult.TryGetResult<Vector3>(out var rotation))
                    {
                        actor.Rotation = new Vector2(rotation.Y, rotation.X);
                        actor.Rotation3D = rotation;
                    }
                }

                if (updatePlatoons)
                {
                    _lastSquadUpdate = DateTime.Now;
                    _squadCache = _squadCache.Where(kv => _actors.ContainsKey(kv.Key))
                                             .ToDictionary(kv => kv.Key, kv => kv.Value);
                }
            }
            catch (GameEnded)
            {
                throw;
            }
            catch (Exception ex)
            {
                Program.Log($"CRITICAL ERROR - UpdateAllPlayers Loop FAILED: {ex}");
            }
        }
        private Vector3 TransformToWorld(FTransform boneTransform, FTransform componentToWorld)
        {
            boneTransform.Scale3D = new Vector3(1, 1, 1);
            componentToWorld.Scale3D = new Vector3(1, 1, 1);
            Matrix4x4 boneMatrix = boneTransform.ToMatrix();
            Matrix4x4 worldMatrix = componentToWorld.ToMatrix();
            Matrix4x4 finalMatrix = boneMatrix * worldMatrix;
            return new Vector3(finalMatrix.M41, finalMatrix.M42, finalMatrix.M43);
        }
    }
    #endregion
}