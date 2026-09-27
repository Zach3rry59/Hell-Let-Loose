using Offsets;

namespace HellLetLoose.Source.HellLetLoose.Features
{
    public class NoRecoil : Manager
    {
        public bool _isNoRecoilEnabled = false;
        private float _originalRecoil = 0.0f;
        private float _originalRecoilADS = 0.0f;
        private ulong _lastWeapon = 0;
        private bool _isRecoilApplied = false;

        public NoRecoil(ulong playerController, bool inGame)
            : base(playerController, inGame)
        {
        }

        public void SetEnabled(bool enable)
        {
            if (!IsLocalPlayerValid())
            {
                Program.Log("NoRecoil.SetEnabled: Invalid local player");
                return;
            }
            if (_isNoRecoilEnabled != enable)
            {
                Program.Log($"NoRecoil.SetEnabled: enable={enable}");
                _isNoRecoilEnabled = enable;
                _isRecoilApplied = false;
                Apply();
            }
        }

        public override void Apply()
        {
            try
            {
                if (!IsLocalPlayerValid())
                {
                    Program.Log("NoRecoil.Apply: Invalid local player");
                    return;
                }

                UpdateCachedPointers();
                ulong currentWeapon = _cachedCurrentWeapon;
                if (currentWeapon == 0)
                {
                    return;
                }

                if (currentWeapon != _lastWeapon)
                {
                    Program.Log($"NoRecoil.Apply: Weapon changed to 0x{currentWeapon:X}, resetting recoil values");
                    if (_originalRecoil != 0.0f && _isNoRecoilEnabled)
                    {
                        Memory.WriteValue<float>(_lastWeapon + AShooterWeapon.RecoilAndSpread, _originalRecoil);
                        Memory.WriteValue<float>(_lastWeapon + AShooterWeapon.RecoilAndSpread_ADS, _originalRecoilADS);
                        Program.Log($"NoRecoil.Apply: Restored previous weapon Recoil={_originalRecoil}, RecoilADS={_originalRecoilADS}");
                    }
                    _originalRecoil = 0.0f;
                    _originalRecoilADS = 0.0f;
                    _isRecoilApplied = false;
                    _lastWeapon = currentWeapon;
                }

                float currentRecoil = Memory.ReadValue<float>(currentWeapon + AShooterWeapon.RecoilAndSpread);
                float currentRecoilADS = Memory.ReadValue<float>(currentWeapon + AShooterWeapon.RecoilAndSpread_ADS);

                if (_isNoRecoilEnabled && !_isRecoilApplied)
                {
                    if (_originalRecoil == 0.0f)
                    {
                        _originalRecoil = currentRecoil;
                        _originalRecoilADS = currentRecoilADS;
                        Program.Log($"NoRecoil.Apply: Stored original Recoil={_originalRecoil}, RecoilADS={_originalRecoilADS}");
                    }

                    if (Math.Abs(currentRecoil - 0.0f) > 0.01f)
                    {
                        Memory.WriteValue<float>(currentWeapon + AShooterWeapon.RecoilAndSpread, 0.0f);
                        Program.Log("NoRecoil.Apply: Set Recoil to 0.0f");
                    }
                    if (Math.Abs(currentRecoilADS - 0.0f) > 0.01f)
                    {
                        Memory.WriteValue<float>(currentWeapon + AShooterWeapon.RecoilAndSpread_ADS, 0.0f);
                        Program.Log("NoRecoil.Apply: Set RecoilADS to 0.0f");
                    }
                    _isRecoilApplied = true;
                }
                else if (!_isNoRecoilEnabled && _originalRecoil != 0.0f)
                {
                    // Restore original recoil values
                    if (Math.Abs(currentRecoil - _originalRecoil) > 0.01f)
                    {
                        Memory.WriteValue<float>(currentWeapon + AShooterWeapon.RecoilAndSpread, _originalRecoil);
                        Program.Log($"NoRecoil.Apply: Restored Recoil to {_originalRecoil}");
                    }
                    if (Math.Abs(currentRecoilADS - _originalRecoilADS) > 0.01f)
                    {
                        Memory.WriteValue<float>(currentWeapon + AShooterWeapon.RecoilAndSpread_ADS, _originalRecoilADS);
                        Program.Log($"NoRecoil.Apply: Restored RecoilADS to {_originalRecoilADS}");
                    }
                    _originalRecoil = 0.0f;
                    _originalRecoilADS = 0.0f;
                    _isRecoilApplied = false;
                }
            }
            catch (Exception ex)
            {
                Program.Log($"Error setting no recoil: {ex.Message}, Address=0x{(_cachedCurrentWeapon + AShooterWeapon.RecoilAndSpread):X}");
            }
        }
    }
}