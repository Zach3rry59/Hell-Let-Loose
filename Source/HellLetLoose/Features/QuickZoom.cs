using Offsets;

namespace HellLetLoose.Source.HellLetLoose.Features
{
    public class QuickZoom : Manager
    {
        public bool _isQuickZoomEnabled = false;
        private float _originalFov = 0.0f;
        
        public QuickZoom(ulong playerController, bool inGame)
            : base(playerController, inGame)
        {
        }
        
        public void SetEnabled(bool enable)
        {
            if (!IsLocalPlayerValid()) return;
            _isQuickZoomEnabled = enable;
            Apply();
        }
        
        public override void Apply()
        {
            try
            {
                if (!IsLocalPlayerValid()) return;
                
                UpdateCachedPointers();
                ulong cameraManager = Memory.ReadPtr(_playerController + PlayerController.PlayerCameraManager);
                if (cameraManager == 0) return;
                
                if (_isQuickZoomEnabled)
                {
                    _originalFov = Memory.ReadValue<float>(cameraManager + Offsets.Camera.DefaultFOV);
                    ulong UCameraAnimInst = Memory.ReadPtr(cameraManager + 0x2688);
                    ulong UCameraAnim = Memory.ReadPtr(UCameraAnimInst + 0x28);

                    Program.Log($"{Memory.ReadValue<float>(UCameraAnimInst + 0x50)}");
                    Memory.WriteValue<float>(UCameraAnimInst + 0x50, 2000.0f);

                    Program.Log($"QuickZoom.Apply: Set CameraCache FOV to 20f");
                    Memory.WriteValue<float>(cameraManager + Offsets.Camera.DefaultFOV, 20.0f);
                }
                else if (_originalFov != 0.0f)
                {
                    Memory.WriteValue<float>(cameraManager + Offsets.Camera.DefaultFOV, _originalFov);
                    _originalFov = 0.0f;
                }
            }
            catch (Exception ex)
            {
                Program.Log($"Error setting Quick Zoom: {ex.Message}");
            }
        }
    }
} 