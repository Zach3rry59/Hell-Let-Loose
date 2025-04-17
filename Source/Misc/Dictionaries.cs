using SkiaSharp;
using squad_dma;

namespace squad_dma {
    public class Names {
        public static readonly Dictionary<string, ActorType> TechNames = new()
        {
                        // HLL Recon 
                        
            {"BP_Greyhound_C", ActorType.IFV },
            {"BP_GreyhoundWinter_C", ActorType.IFV },
                        // HLL Axis
                        
            {"BP_Kubelwagen_C", ActorType.JeepTransport },
            {"BP_Halftrack_GER_C", ActorType.TrackedLogistics },
            {"BP_Panther_max_C", ActorType.Tank },
            {"BP_PanzerIV_C", ActorType.TankMGS },
            {"BP_Luchs_C", ActorType.TrackedIFV },
            {"BP_OpelBlitz_Transport_C", ActorType.TruckTransport },
            {"BP_OpelBlitz_Supply_C", ActorType.TruckLogistics },
            {"GER_Outpost_C", ActorType.RallyPoint},
            {"GER_Garrison_C", ActorType.Hab},
            {"Mine_SMine_C", ActorType.Mine},
            {"Mine_Tellermine43_C", ActorType.Mine},


            {"BP_Puma_C", ActorType.IFV },
            {"BP_PanzerIV_NA_C", ActorType.TankMGS },
            {"BP_Tiger_NA_C", ActorType.Tank },
            {"BP_Luchs_NA_C", ActorType.TrackedIFV },
            {"BP_Puma_NA_C", ActorType.IFV },
            {"BP_OpelBlitz_Transport_NA_C", ActorType.TruckTransport },
            {"BP_OpelBlitz_Supply_NA_C", ActorType.TruckLogistics },

            {"BP_GER_SFH18_C", ActorType.DeployableMortars },

            {"BP_PantherWinter_max_C", ActorType.Tank },
            {"BP_PanzerIVWinter_C", ActorType.TankMGS },
            {"BP_LuchsWinter_C", ActorType.TrackedIFV },
            {"BP_OpelBlitzWinter_Transport_C", ActorType.TruckTransport },
            {"BP_OpelBlitzWinter_Supply_C", ActorType.TruckLogistics },

                          // HLL USA
            {"BP_JeepUS_C", ActorType.JeepTransport },
            {"BP_Halftrack_US_C", ActorType.TrackedLogistics },
            {"BP_Sherman_Jumbo_76mm_C", ActorType.Tank },
            {"BP_Sherman_M4A3_75_W_C", ActorType.TankMGS },
            {"BP_Stuart_C", ActorType.TrackedIFV },
            {"BP_GMCDeuce_Transport_C", ActorType.TruckTransport },
            {"BP_GMCDeuce_Supply_C", ActorType.TruckLogistics },


            {"BP_ShermanWinter_Jumbo_76mm_C", ActorType.Tank },
            {"BP_ShermanWinter_M4A3_75_W_C", ActorType.TankMGS },
            {"BP_StuartWinter_C", ActorType.TrackedIFV },
            {"BP_GMCDeuceWinter_Transport_C", ActorType.TruckTransport },
            {"BP_GMCDeuceWinter_Supply_C", ActorType.TruckLogistics },

            {"Mine_M2AP_C", ActorType.Mine},
            {"Mine_M1A1_C", ActorType.Mine},

                          // HLL RU
            {"BP_Gaz67_C", ActorType.JeepTransport },
            {"BP_Halftrack_RU_C", ActorType.TrackedLogistics },
            {"BP_IS_1_C", ActorType.Tank },
            {"BP_T34_76_C", ActorType.TankMGS },
            {"BP_T70_C", ActorType.TrackedIFV },
            {"BP_Zis_5_Transport_C", ActorType.TruckTransport },
            {"BP_Zis_5_Supply_C", ActorType.TruckLogistics },

            {"BP_ISWinter_1_C", ActorType.Tank },
            {"BP_T34Winter_76_C", ActorType.TankMGS },
            {"BP_T70Winter_C", ActorType.TrackedIFV },
            {"BP_ZisWinter_5_Transport_C", ActorType.TruckTransport },
            {"BP_ZisWinter_5_Supply_C", ActorType.TruckLogistics },

            {"RU_Outpost_C", ActorType.RallyPoint},
            {"RU_Garrison_C", ActorType.Hab},
            {"Mine_POMZ_C", ActorType.Mine},
            {"Mine_TM-35_C", ActorType.Mine},

            {"RU_RepairStation_C", ActorType.FOBRadio},

                          // HLL Allies
            {"BP_JeepCOM_C", ActorType.JeepTransport },
            {"BP_Halftrack_COM_C", ActorType.TrackedLogistics },
            {"BP_ChurchillMk7_C", ActorType.Tank },
            {"BP_Cromwell_C", ActorType.TankMGS },
            {"BP_Tetrarch_C", ActorType.TrackedIFV },
            {"BP_Bedford_Transport_C", ActorType.TruckTransport },
            {"BP_Bedford_Supply_C", ActorType.TruckLogistics },

            {"BP_ChurchillMk7Winter_C", ActorType.Tank },
            {"BP_CromwellWinter_C", ActorType.TankMGS },
            {"BP_TetrarchWinter_C", ActorType.TrackedIFV },
            {"BP_BedfordWinter_Transport_C", ActorType.TruckTransport },
            {"BP_BedfordWinter_Supply_C", ActorType.TruckLogistics },


            {"BP_JeepCOM_NA_C", ActorType.JeepTransport },
            {"BP_Bedford_Supply_NA_C", ActorType.TruckLogistics },
            {"BP_Bedford_Transport_NA_C", ActorType.TruckTransport },
            {"BP_Daimler_NA_C", ActorType.IFV },

            {"BP_Churchill_C", ActorType.Tank },
            {"BP_Crusader_C", ActorType.TankMGS},
            {"BP_M3_Stuart_Honey_C", ActorType.TrackedIFV },

            {"COM_Outpost_C", ActorType.RallyPoint},
            {"COM_Garrison_C", ActorType.Hab},
            {"Mine_ShrapnelMK2_C", ActorType.Mine},
            {"Mine_GSMkV_C", ActorType.Mine},

        };

        public static readonly Dictionary<ActorType, SKBitmap> BitMaps = new(){
            {ActorType.FOBRadio, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.FOBRadio)},
            {ActorType.Hab, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.Hab)},
            {ActorType.AntiAir, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.AntiAir)},
            {ActorType.APC, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.APC)},
            {ActorType.AttackHelicopter, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.AttackHelicopter)},
            {ActorType.LoachCAS, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.LoachCAS)},
            {ActorType.LoachScout, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.LoachScout)},
            {ActorType.Boat, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.Boat)},
            {ActorType.BoatLogistics, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.BoatLogistics)},
            {ActorType.DeployableAntiAir, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.DeployableAntiAir)},
            {ActorType.DeployableAntitank, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.DeployableAntitank)},
            {ActorType.DeployableAntitankGun, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.DeployableAntitankGun)},
            {ActorType.DeployableGMG, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.DeployableGMG)},
            {ActorType.DeployableHellCannon, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.DeployableHellCannon)},
            {ActorType.DeployableHMG, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.DeployableHMG)},
            {ActorType.DeployableMortars, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.DeployableMortars)},
            {ActorType.DeployableRockets, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.DeployableRockets)},
            {ActorType.Drone, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.Drone)},
            {ActorType.IFV, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.IFV)},
            {ActorType.JeepAntiAir, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.JeepAntiAir)},
            {ActorType.JeepAntitank, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.JeepAntitank)},
            {ActorType.JeepArtillery, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.JeepArtillery)},
            {ActorType.JeepLogistics, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.JeepLogistics)},
            {ActorType.JeepTransport, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.JeepTransport)},
            {ActorType.JeepRWSTurret, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.JeepRWSTurret)},
            {ActorType.JeepTurret, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.JeepTurret)},
            {ActorType.Mine, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.Mine)},
            {ActorType.Motorcycle, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.Motorcycle)},
            {ActorType.RallyPoint, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.RallyPoint)},
            {ActorType.Tank, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.Tank)},
            {ActorType.TankMGS, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.TankMGS)},
            {ActorType.TrackedAPC, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.TrackedAPC)},
            {ActorType.TrackedLogistics, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.TrackedLogistics)},
            {ActorType.TrackedAPCArtillery, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.TrackedAPCArtillery)},
            {ActorType.TrackedIFV, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.TrackedIFV)},
            {ActorType.TrackedJeep, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.TrackedJeep)},
            {ActorType.TransportHelicopter, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.TransportHelicopter)},
            {ActorType.TruckAntiAir, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.TruckAntiAir)},
            {ActorType.TruckArtillery, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.TruckArtillery)},
            {ActorType.TruckLogistics, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.TruckLogistics)},
            {ActorType.TruckTransport, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.TruckTransport)},
            {ActorType.TruckTransportArmed, SkiaSharp.Views.Desktop.Extensions.ToSKBitmap(Properties.Resources.TruckTransportArmed)},
        };

        public static readonly HashSet<ActorType> RotateBy45Degrees = [
            ActorType.DeployableAntiAir,
            ActorType.DeployableAntitank,
            ActorType.DeployableAntitankGun,
            ActorType.DeployableGMG,
            ActorType.DeployableHellCannon,
            ActorType.DeployableHMG,
            ActorType.DeployableMortars,
            ActorType.DeployableRockets,
        ];

        public static readonly HashSet<ActorType> DoNotRotate = [
            ActorType.Hab,
            ActorType.FOBRadio,
            ActorType.Mine,
            ActorType.RallyPoint,
        ];

        public static readonly HashSet<ActorType> Tanks = [
            ActorType.Tank,
            ActorType.TankMGS,
            ActorType.TrackedIFV,
        ];

        public static readonly HashSet<ActorType> Deployables = [
            ActorType.DeployableAntiAir,
            ActorType.DeployableAntitank,
            ActorType.DeployableAntitankGun,
            ActorType.DeployableGMG,
            ActorType.DeployableHellCannon,
            ActorType.DeployableHMG,
            ActorType.DeployableMortars,
            ActorType.DeployableRockets,
            ActorType.Hab,
            ActorType.FOBRadio,
            ActorType.Mine,
            ActorType.RallyPoint,
        ];
    }
    
}