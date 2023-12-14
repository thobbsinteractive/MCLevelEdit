﻿namespace MCLevelEdit.Model.Domain;

public record TerrainGenerationParameters()
{
    public MapType MapType { get; set; } = MapType.Day;
    public ushort Seed { get; set; }
    public ushort Offset { get; set; }
    public ushort Raise { get; set; }
    public ushort Gnarl { get; set; }
    public ushort River { get; set; }
    public ushort Lriver { get; set; }
    public byte Source { get; set; }
    public ushort SnLin { get; set; }
    public byte SnFlt { get; set; }
    public byte BhLin { get; set; }
    public byte BhFlt { get; set; }
    public byte RkSte { get; set; }
}
