using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUpdateWorkshop
{
    public void Setup(); // Must be called in Awake
    public void StartGameLoopHostOnly();
    public void UpdateGameLoopHostOnly();
}
