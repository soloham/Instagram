using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DMScreenNotesManager : MonoBehaviour
{
    public Image AddNoteProfileImage;
    public static DMScreenNotesManager Instance;

    private void Start()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    public void Initialise()
    {
        AddNoteProfileImage.sprite = ProfileManager.Instance.LoggedInProfile.PictureBorderless;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
