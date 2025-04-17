using Offsets;
using System;

namespace squad_dma.Source.Squad.Features
{
    public class Suppression : Manager
    {
        public bool _isSuppressionEnabled = false;
        private bool _isSuppressionApplied = false;
        private ulong _lastSoldierActor = 0;

        public Suppression(ulong playerController, bool inGame)
            : base(playerController, inGame)
        {
        }

        public void SetEnabled(bool enable)
        {
            if (!IsLocalPlayerValid())
            {
                Program.Log("NoSuppression.SetEnabled: Invalid local player");
                return;
            }
            if (_isSuppressionEnabled != enable)
            {
                Program.Log($"NoSuppression.SetEnabled: enable={enable}");
                _isSuppressionEnabled = enable;
                _isSuppressionApplied = false;
                Apply();
            }
        }

        public override void Apply()
        {
            try
            {
                if (!IsLocalPlayerValid())
                {
                    Program.Log("NoSuppression.Apply: Invalid local player");
                    return;
                }

                UpdateCachedPointers();
                ulong soldierActor = _cachedSoldierActor;
                if (soldierActor == 0)
                {
                    return;
                }

                if (soldierActor != _lastSoldierActor)
                {
                    Program.Log($"NoSuppression.Apply: Soldier actor changed to 0x{soldierActor:X}, resetting suppression values");
                    _isSuppressionApplied = false;
                    _lastSoldierActor = soldierActor;
                }

                // Read current suppression values for logging
                float currentMaxSuppression = Memory.ReadValue<float>(soldierActor + AShooterCharacter.MaxSuppression);
                float currentRecoveryRate = Memory.ReadValue<float>(soldierActor + AShooterCharacter.SuppressionRecoveryRate);
                //Program.Log($"NoSuppression.Apply: Current MaxSuppression={currentMaxSuppression:F2}, SuppressionRecoveryRate={currentRecoveryRate:F2}, Enabled={_isSuppressionEnabled}, Applied={_isSuppressionApplied}, SoldierActor=0x{soldierActor:X}");

                if (_isSuppressionEnabled && !_isSuppressionApplied)
                {
                    // Apply no suppression values
                    if (Math.Abs(currentMaxSuppression - 0.0f) > 0.01f)
                    {
                        Memory.WriteValue<float>(soldierActor + AShooterCharacter.MaxSuppression, 0.0f);
                        Program.Log("NoSuppression.Apply: Set MaxSuppression to 0.0f");
                    }
                    if (Math.Abs(currentRecoveryRate - 100.0f) > 0.01f)
                    {
                        Memory.WriteValue<float>(soldierActor + AShooterCharacter.SuppressionRecoveryRate, 100.0f);
                        Program.Log("NoSuppression.Apply: Set SuppressionRecoveryRate to 100.0f");
                    }
                    _isSuppressionApplied = true;
                }
                else if (!_isSuppressionEnabled && _isSuppressionApplied)
                {
                    // Restore default suppression values
                    if (Math.Abs(currentMaxSuppression - 1.0f) > 0.01f)
                    {
                        Memory.WriteValue<float>(soldierActor + AShooterCharacter.MaxSuppression, 1.0f);
                        Program.Log("NoSuppression.Apply: Restored MaxSuppression to 1.0f");
                    }
                    if (Math.Abs(currentRecoveryRate - 1.0f) > 0.01f)
                    {
                        Memory.WriteValue<float>(soldierActor + AShooterCharacter.SuppressionRecoveryRate, 1.0f);
                        Program.Log("NoSuppression.Apply: Restored SuppressionRecoveryRate to 1.0f");
                    }
                    _isSuppressionApplied = false;
                }
            }
            catch (Exception ex)
            {
                Program.Log($"Error setting no suppression: {ex.Message}, Address=0x{(_cachedSoldierActor + AShooterCharacter.MaxSuppression):X}");
            }
        }
    }
}