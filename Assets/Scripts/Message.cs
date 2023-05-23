using Assets.Scripts;

using Newtonsoft.Json;

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Message
{
    public string Text;
    [JsonIgnore]
    public Sprite Image;

    public UDateTime SentAt;
    public UDateTime DeliveredAt => SentAt;
    public UDateTime ReceivedAt => DeliveredAt;

    public override string ToString()
    {
        return Text;
    }
}
