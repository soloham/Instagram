using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TestPlayerHolder
{
    public TestDropdown Source;

    [Dropdown("Source.Players", "Name")]
    public Player Player;
}

public class TestPlayer : MonoBehaviour
{
    public TestDropdown Source;
    public TestPlayerHolder Player;

    private void Awake()
    {
        Source = TestDropdown.Instance;
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
