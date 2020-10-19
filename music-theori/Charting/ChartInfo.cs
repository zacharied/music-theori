﻿using System;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;
using theori.Database;
using theori.GameModes;

namespace theori.Charting
{
    public class ChartInfo : IEquatable<ChartInfo>, IHasPrimaryKey
    {
        public static bool operator ==(ChartInfo? a, ChartInfo? b) => a is null ? b is null : a.Equals(b);
        public static bool operator !=(ChartInfo? a, ChartInfo? b) => !(a == b);

        /// <summary>
        /// The database primary key.
        /// </summary>
        public long ID { get; set; }

        public long LastWriteTime { get; set; }

        public long SetID => Set.ID;
        public ChartSetInfo Set { get; set; }

        public GameMode? GameMode { get; set; } = null;
        public string? ChartFileType => Path.GetExtension(FileName ?? string.Empty);

        /// <summary>
        /// The name of the chart file inside of the Set directory.
        /// </summary>
        public string? FileName { get; set; } = null;

        public string SongTitle { get; set; } = "Unknown";
        public string SongArtist { get; set; } = "Unknown";
        public string SongFileName { get; set; } = "song.ogg";
        public int SongVolume { get; set; } = 80;

        public time_t ChartOffset { get; set; } = 0;

        private string m_charterBacking = "Unknown";
        public string Charter
        {
            get => m_charterBacking;
            set => m_charterBacking = value ?? "Unknown";
        }

        public string? JacketFileName { get; set; } = null;
        public string? JacketArtist { get; set; } = null;

        public string? BackgroundFileName { get; set; } = null;
        public string? BackgroundArtist { get; set; } = null;

        public double DifficultyLevel { get; set; } = 1.0;
        public int? DifficultyIndex { get; set; } = null;

        public string? DifficultyName { get; set; } = null;
        public string? DifficultyNameShort { get; set; } = null;

        public Vector3? DifficultyColor { get; set; } = null;

        public time_t ChartDuration { get; set; } = 0;

        public string Tags { get; set; } = "";

        // TODO(neko) Decide upon a consistent and safe way to store this information
        public string? SourceFileHash { get; set; } = null;

        public override bool Equals(object obj) => obj is ChartInfo info && Equals(info);
        bool IEquatable<ChartInfo>.Equals(ChartInfo other)
        {
            if (other is null) return false;
            // TODO(local): This isn't TECHNICALLY true
            //  but should it be considered true?
            return ID == other.ID;
        }

        public override int GetHashCode() => HashCode.Combine(ID);
    }
}
