using System.Collections;
using System.Collections.Generic;
using TMPro;

using UnityEngine;

public class VirtualScrollRectInspector : MonoBehaviour
{

    [SerializeField]
    private int m_elementsBeforeVisible = 30;
    public int elementsBeforeVisible { get { return m_elementsBeforeVisible; } set { m_elementsBeforeVisible = value; } }

    [SerializeField]
    private int m_elementsAfterVisible = 30;
    public int elementsAfterVisible { get { return m_elementsAfterVisible; } set { m_elementsAfterVisible = value; } }

    public bool AllowAntistalling = false;

    public TextMeshProUGUI ActiveTMP;
    public TextMeshProUGUI LastIterationsTMP;
    public TextMeshProUGUI FirstIterationsTMP;
    public TextMeshProUGUI VelocityTMP;
    public TextMeshProUGUI StutteringTMP;
    public TextMeshProUGUI UpdateCountTMP;
    public TextMeshProUGUI FirstItemIndexTMP;

    public float ScrollIncrementDivisor = 1;

    public ChatAreaManager ChatAreaManager;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
