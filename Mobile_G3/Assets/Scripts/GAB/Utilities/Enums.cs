using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enums { }

public enum Language
{
    French, English
}

public enum PoolType
{
    ExampleTransform, ExampleRigidbody, ExampleMonoBehaviour
}

public enum CanvasType
{
    None, IntroductionCinematicCanvas, WorkshopCanvas, ControlCanvas, TimerCanvas, EndCinematicCanvas, EndGame, ConnectionCanvas
}

public enum InventoryObject
{
    None,
    CannonBall,
    Plank
}

public enum GameState
{
    Menu,
    Game
}

public enum BoatSide
{
    Deck, Hold
}

public enum WorkshopType
{
    Permanent, Temporary
}

public enum TileFilter
{
    Walkable
}

public enum EndGameReason
{
    TimerOver, ShipDestroyed
}

public enum SoundSettings
{
    All, MusicOnly, SoundOnly, None
}