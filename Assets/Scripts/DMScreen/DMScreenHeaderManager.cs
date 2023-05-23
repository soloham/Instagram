using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;

public class DMScreenHeaderManager : MonoBehaviour
{
    public static DMScreenHeaderManager Instance;
    public TextMeshProUGUI UsernameField;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    public void Initialise()
    {
        UsernameField.text = ProfileManager.Instance.LoggedInProfile.Handle;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
