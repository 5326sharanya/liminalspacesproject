using System;
using System.Collections.Generic;
using UnityEngine;

namespace LiminalSpace
{
    public enum RealmType
    {
        SilentField = 1,
        Poolrooms = 2,
        YellowBackrooms = 3,
        CrimsonStairwell = 4,
        VelvetCinema = 5,
        WhiteVoid = 6
    }

    public enum AnomalyType
    {
        None = 0,
        // Door 1 (Silent Field) Anomalies
        WhiteFlowers,
        CrimsonSky,
        TwinWatchers,
        HollowGaze,
        MissingBed,
        // General Realm Anomalies
        BloodWater,
        ReversedLights,
        StaticScreen
    }

    public enum FateEffect
    {
        None = 0,
        SereneMind,     // From obeying Door 1: slows anomaly tension & improves clarity
        EchoCurse,      // From breaking silence: causes lights to flicker & lowers anomaly resistance
        Waterlogged,    // From Poolrooms: damp footsteps, resistant to Crimson static
        Foresight,      // From White Void Payphone: reveals true anomaly presence when listening at doors
        CrimsonMarked   // From Crimson Stairwell: faster sprint, higher tension
    }

    [Serializable]
    public class DoorAssignment
    {
        public int doorNumber; // 1..6
        public RealmType realmType;
        public AnomalyType anomaly;
        public bool hasRelic;
        public string displayTitle;
        public string listenClue;
        public Color frameColor;
        public bool isAnomaly => anomaly != AnomalyType.None;
    }

    /// <summary>
    /// Holds procedural run state across Cycles: current Cycle, seeded door generation,
    /// carry-over Fate buffs/curses, collected Relics, and persistent high score.
    /// </summary>
    public class LiminalRunState
    {
        public int RunSeed { get; private set; }
        public int CurrentCycle { get; private set; } = 1;
        public int RelicsCollected { get; private set; } = 0;
        public int RelicsRequired { get; private set; } = 3;
        public int BestCycle { get; private set; } = 1;
        public FateEffect ActiveFate { get; set; } = FateEffect.None;

        public List<DoorAssignment> CurrentDoors { get; private set; } = new List<DoorAssignment>();

        private const string BestCyclePrefKey = "Liminal_BestCycle";

        public LiminalRunState()
        {
            RunSeed = Environment.TickCount & 0x7FFFFFFF;
            BestCycle = PlayerPrefs.GetInt(BestCyclePrefKey, 1);
            GenerateCycleDoors(1);
        }

        public void StartNewRun()
        {
            RunSeed = Environment.TickCount & 0x7FFFFFFF;
            CurrentCycle = 1;
            RelicsCollected = 0;
            ActiveFate = FateEffect.None;
            GenerateCycleDoors(1);
        }

        public void AdvanceToNextCycle()
        {
            CurrentCycle++;
            if (CurrentCycle > BestCycle)
            {
                BestCycle = CurrentCycle;
                PlayerPrefs.SetInt(BestCyclePrefKey, BestCycle);
                PlayerPrefs.Save();
            }
            GenerateCycleDoors(CurrentCycle);
        }

        public void CollectRelic()
        {
            RelicsCollected++;
        }

        public void GenerateCycleDoors(int cycle)
        {
            CurrentDoors.Clear();
            System.Random rng = new System.Random(RunSeed + cycle * 997);

            // Exactly 2 Doors: Door 1 = The Celestial Horizon, Door 2 = The Subterranean Poolrooms
            float anomalyChance = (cycle == 1) ? 0f : Mathf.Clamp(0.25f + (cycle - 1) * 0.1f, 0.25f, 0.6f);
            if (ActiveFate == FateEffect.EchoCurse) anomalyChance = Mathf.Min(0.75f, anomalyChance + 0.15f);

            int relicDoorIdx = rng.Next(0, 2);

            // Door 1: The Ethereal Celestial Horizon
            AnomalyType d1Anomaly = AnomalyType.None;
            if (cycle > 1 && rng.NextDouble() < anomalyChance)
            {
                d1Anomaly = PickAnomalyForRealm(RealmType.SilentField, rng);
            }

            DoorAssignment d1 = new DoorAssignment
            {
                doorNumber = 1,
                realmType = RealmType.SilentField,
                anomaly = d1Anomaly,
                hasRelic = (relicDoorIdx == 0) && (RelicsCollected < RelicsRequired),
                displayTitle = "The Celestial Horizon",
                frameColor = new Color(0.85f, 0.78f, 0.98f),
                listenClue = BuildListenClue(RealmType.SilentField, d1Anomaly, rng)
            };
            CurrentDoors.Add(d1);

            // Door 2: The Subterranean Poolrooms
            AnomalyType d2Anomaly = AnomalyType.None;
            if (cycle > 1 && rng.NextDouble() < anomalyChance)
            {
                d2Anomaly = PickAnomalyForRealm(RealmType.Poolrooms, rng);
            }

            DoorAssignment d2 = new DoorAssignment
            {
                doorNumber = 2,
                realmType = RealmType.Poolrooms,
                anomaly = d2Anomaly,
                hasRelic = (relicDoorIdx == 1) && (RelicsCollected < RelicsRequired),
                displayTitle = "The Subterranean Poolrooms",
                frameColor = new Color(0.32f, 0.76f, 0.86f),
                listenClue = BuildListenClue(RealmType.Poolrooms, d2Anomaly, rng)
            };
            CurrentDoors.Add(d2);
        }

        private AnomalyType PickAnomalyForRealm(RealmType realm, System.Random rng)
        {
            switch (realm)
            {
                case RealmType.SilentField:
                    AnomalyType[] fieldAnomalies = new[]
                    {
                        AnomalyType.WhiteFlowers,
                        AnomalyType.CrimsonSky,
                        AnomalyType.TwinWatchers,
                        AnomalyType.HollowGaze,
                        AnomalyType.MissingBed
                    };
                    return fieldAnomalies[rng.Next(fieldAnomalies.Length)];

                case RealmType.Poolrooms:
                    return rng.NextDouble() < 0.6 ? AnomalyType.BloodWater : AnomalyType.ReversedLights;

                case RealmType.YellowBackrooms:
                    return AnomalyType.ReversedLights;

                case RealmType.VelvetCinema:
                    return AnomalyType.StaticScreen;

                case RealmType.CrimsonStairwell:
                    return AnomalyType.ReversedLights;

                case RealmType.WhiteVoid:
                    return AnomalyType.HollowGaze;

                default:
                    return AnomalyType.None;
            }
        }

        private string GetRealmTitle(RealmType r, int cycle)
        {
            // In Cycle 1, show clear names; in Cycle 2+, keep labels cryptic so the player must listen!
            if (cycle > 1)
            {
                switch (r)
                {
                    case RealmType.SilentField: return "Meadow Sector";
                    case RealmType.Poolrooms: return "Sublimity Basin";
                    case RealmType.YellowBackrooms: return "Level Zero";
                    case RealmType.CrimsonStairwell: return "Sub-Level Descent";
                    case RealmType.VelvetCinema: return "Screening Room";
                    case RealmType.WhiteVoid: return "Horizon Mist";
                }
            }

            switch (r)
            {
                case RealmType.SilentField: return "The Silent Green Field";
                case RealmType.Poolrooms: return "The Poolrooms";
                case RealmType.YellowBackrooms: return "Mono-Yellow Corridors";
                case RealmType.CrimsonStairwell: return "The Crimson Stairwell";
                case RealmType.VelvetCinema: return "The Velvet Cinema";
                case RealmType.WhiteVoid: return "The White Mist Void";
                default: return "Unknown";
            }
        }

        private Color GetRealmFrameColor(RealmType r)
        {
            switch (r)
            {
                case RealmType.SilentField: return new Color(0.92f, 0.96f, 0.92f);
                case RealmType.Poolrooms: return new Color(0.35f, 0.85f, 0.92f);
                case RealmType.YellowBackrooms: return new Color(0.92f, 0.82f, 0.35f);
                case RealmType.CrimsonStairwell: return new Color(0.75f, 0.14f, 0.14f);
                case RealmType.VelvetCinema: return new Color(0.62f, 0.12f, 0.28f);
                case RealmType.WhiteVoid: return new Color(0.94f, 0.94f, 0.98f);
                default: return Color.white;
            }
        }

        private string BuildListenClue(RealmType r, AnomalyType anomaly, System.Random rng)
        {
            string baseClue = r switch
            {
                RealmType.SilentField => "Ethereal celestial harmonies, crystal chimes, and distant cascading waters.",
                RealmType.Poolrooms => "Slow water droplets echoing against cold ceramic tiles and deep metallic groans.",
                RealmType.YellowBackrooms => "A steady 60Hz fluorescent buzz behind damp wallpaper.",
                RealmType.CrimsonStairwell => "Heavy metallic hum and a slow rotating beacon.",
                RealmType.VelvetCinema => "Faint projector clicking in velvet darkness.",
                RealmType.WhiteVoid => "A distant rotary telephone ringing through thick mist.",
                _ => "Silence."
            };

            if (anomaly != AnomalyType.None)
            {
                string anomalyHint = ActiveFate == FateEffect.Foresight
                    ? " [FORESIGHT: ANOMALY DETECTED INSIDE!]"
                    : " ...Something sounds slightly distorted beneath it.";
                return baseClue + anomalyHint;
            }

            if (ActiveFate == FateEffect.Foresight)
            {
                return baseClue + " [FORESIGHT: Clean. No anomaly.]";
            }

            return baseClue;
        }

        public static string GetFateDisplayName(FateEffect fate)
        {
            return fate switch
            {
                FateEffect.SereneMind => "Serene Mind (+Calm)",
                FateEffect.EchoCurse => "Echo Curse (High Anomaly Risk)",
                FateEffect.Waterlogged => "Waterlogged (Damp Steps)",
                FateEffect.Foresight => "Foresight (Senses Anomalies)",
                FateEffect.CrimsonMarked => "Crimson Marked (+Speed)",
                _ => "Unbound"
            };
        }
    }
}

