namespace Offsets
{
    public struct GameObjects
    {
        public const uint GObjects = 0x44b2a80;
        public const uint GNames = 0x4486a80;
        public const uint GWorld = 0x45f3970;
    }
    public struct World
    {
        public const uint PersistentLevel = 0x30;
        public const uint AuthorityGameMode = 0x128;
        public const uint GameState = 0x130;
        public const uint Levels = 0x148;
        public const uint OwningGameInstance = 0x188;
        public const uint WorldOrigin = 0x5B8; // 0x5B8 or 0x5C4 on Squad :/
    }

    public struct GameInstance
    {
        public const uint LocalPlayers = 0x38;
        public const uint MapLoadingData = 0x688; // UMapLoadingScreenData*

    }

    public struct UMapLoadingScreenData
    {
        public const uint MapName = 0x30; // FText
    }

    public struct Level
    {
        public const uint Actors = 0x98;
        public const uint MaxPacket = 0xA0;
    }

    public struct Actor
    {
        public const uint Instigator = 0x118;
        public const uint RootComponent = 0x130;
        public const uint ID = 0x18;
    }

    public struct USceneComponent
    {
        public const uint RelativeLocation = 0x11C;
        public const uint RelativeRotation = 0x128;
        public const uint ComponentToWorld = 0x1C0; // Relative Offset Guess
        public const uint RelativeScale3D = 0x134;
    }

    public struct UPlayer
    {
        public const uint PlayerController = 0x30;
    }

    public struct ULocalPlayer
    {
        public const uint ViewportClient = 0x70;
    }

    public struct Pawn
    {
        public const uint PlayerState = 0x240;
        public const uint Controller = 0x258;
    }
    public struct Controller
    {
        public const uint PlayerState = 0x228;
        public const uint Pawn = 0x250;
        public const uint Character = 0x260;
    }

    public struct PlayerController
    {
        public const uint Player = 0x298;
        public const uint AcknowledgedPawn = 0x2a0;
        public const uint PlayerCameraManager = 0x2b8;
    }
    public struct Camera // PlayerCameraManager
    {
        public const uint PCOwner = 0x220;
        public const uint DefaultFOV = 0x238;
        public const uint CameraCache = 0x290; // CachePrivate :: FCameraCacheEntry > 0x10 = FMinimalViewInfo
        public const uint ViewTarget = 0xe50;
    }

    public struct FTViewTarget
    {
        public const uint POV = 0x10; // FMinimalViewInfo
    }
    public static class FMinimalViewInfo
    {
        public const int Location_X = 0x0;  // float
        public const int Location_Y = 0x4;  // float
        public const int Location_Z = 0x8;  // float
        public const int Rotation_Pitch = 0xC;  // float
        public const int Rotation_Yaw = 0x10;   // float
        public const int Rotation_Roll = 0x14;  // float
        public const int FOV = 0x18;            // float
    }
    public static class AShooterCharacter
    {
        public const int Health = 0x9d4;
        public const int Mesh = 0x280;
    }

    public static class APlayerState
    {
        public const int Pawn = 0x280; // APawn*
    }

    public static class AShooterPlayerState
    {
        public const int RepPlayerInfo = 0x49c; // FHLLPlayerInfo
    }

    public static class FHLLPlayerInfo
    {
        public const int PlayerTeam = 0x0;    // ETeam (uint8)
        public const int PlatoonIndex = 0x4;  // int32
    }

    public static class AHLLDispenseStructure
    {
        public const int HealthComponent = 0x298; // UHLLSimpleHealthComponent*
        public const int Team = 0x2dc;           // ETeam (uint8)
    }

    public static class UHLLSimpleHealthComponent
    {
        public const int HealthInfo = 0x148; // FRepHealthInfo
    }

    public static class FRepHealthInfo
    {
        public const int Health = 0x0; // float
    }

    public static class ABaseVehicle
    {
        public const int ArmourHealth = 0x428; // UHLLArmourHealthComponent*
        public const int Team = 0x2e0;         // ETeam (uint8)
    }

    public static class UHLLArmourHealthComponent
    {
        public const int ArmourInfo = 0xC8; // FHLLArmourHealthData
    }

    public static class FHLLArmourHealthData
    {
        public const int MaxHealth = 0x0;     // uint16
        public const int CurrentHealth = 0x8; // uint16
    }

}
