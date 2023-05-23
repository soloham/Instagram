using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Unity.VisualScripting.Member;

[Serializable]
public class Player
{
    public string Name;
    public string UserName;
}

public class TestDropdown : MonoBehaviour
{
    public static TestDropdown Instance;
    public List<Player> Players;

    [Dropdown("Players", "Name")]
    public Player MainPlayer;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
