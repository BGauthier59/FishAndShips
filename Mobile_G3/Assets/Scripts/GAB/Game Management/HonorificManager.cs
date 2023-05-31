using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HonorificManager : NetworkMonoSingleton<HonorificManager>
{
    private Dictionary<Honorifics, uint> honorificsDictionary;

    private List<(uint[], ulong)> allDictionaries = new List<(uint[], ulong)>();

    [TextArea(1, 4)] public string[] messages;

    public void StartGameLoop()
    {
        // Might be in game loop after

        honorificsDictionary = new Dictionary<Honorifics, uint>
        {
            {Honorifics.Captain, 0},
            {Honorifics.Sailor, 0},
            {Honorifics.Explorer, 0},
            {Honorifics.HeadChef, 0},
            {Honorifics.Firefighter, 0},
            {Honorifics.Carpenter, 0},
            {Honorifics.TeamSpirit, 0},
            {Honorifics.CardioTrainer, 0},
            {Honorifics.Gunner, 0},
        };
    }

    public void AddHonorific(params Honorifics[] honorifics)
    {
        foreach (var honorific in honorifics)
        {
            honorificsDictionary[honorific]++;
        }
    }

    [ClientRpc]
    public void InitiateHonorificsResumeClientRpc(bool victory, int starCount)
    {
        Debug.Log("Honorifics");
        LevelManager.instance.UpdateCurrentLevel(true, true, starCount);
        SendPlayerDictionaryToHost(victory, starCount);
    }

    private void SendPlayerDictionaryToHost(bool victory, int starCount)
    {
        uint[] data = new uint[honorificsDictionary.Count];

        int i = 0;
        foreach (var kvp in honorificsDictionary)
        {
            data[i] = kvp.Value;
            i++;
        }

        // Data are sent in the same order than enum order
        SendPlayerDictionaryServerRpc(data, NetworkManager.Singleton.LocalClientId, victory, starCount);
    }

    private int hostReadyClientCount = 0;

    [ServerRpc(RequireOwnership = false)]
    private void SendPlayerDictionaryServerRpc(uint[] data, ulong id, bool victory, int starCount)
    {
        allDictionaries.Add((data, id));

        hostReadyClientCount++;
        if (hostReadyClientCount != ConnectionManager.instance.players.Count) return;

        // Calculate data and determine winners
        //Dictionary<Honorifics, ulong> winners = new Dictionary<Honorifics, ulong>();
        List<long> winners = new List<long>();

        Honorifics[] allHonorifics = new[]
        {
            Honorifics.Captain, Honorifics.Sailor, Honorifics.Explorer, Honorifics.HeadChef,
            Honorifics.Firefighter, Honorifics.Carpenter, Honorifics.TeamSpirit, Honorifics.CardioTrainer,
            Honorifics.Gunner
        };

        for (int i = 0; i < allHonorifics.Length; i++)
        {
            Honorifics current = allHonorifics[i];

            int highest = 0;
            long winnerId = -1;

            for (int j = 0; j < allDictionaries.Count; j++)
            {
                var couple = allDictionaries[j];
                if (couple.Item1[i] > highest)
                {
                    highest = (int) couple.Item1[i];
                    winnerId = (long) couple.Item2;
                }
            }
            winners.Add(winnerId);
        }

        GameManager.instance.GameEndsClientRpc(victory, starCount, winners.ToArray());
    }
}

public enum Honorifics
{
    Captain, // A chaque mini-jeu
    Sailor, // A chaque gouvernail
    Explorer, // A chaque map
    HeadChef, // A chaque crevette
    Firefighter, // A chaque feu
    Carpenter, // A chaque reparation
    TeamSpirit, // A chaque mini-jeu coop
    CardioTrainer, // A chaque escalier traversé
    Gunner // Cannon shoot
}