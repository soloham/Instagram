using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DMScreenNotesManager : MonoBehaviour
{
    public Image AddNoteProfileImage;

    // Start is called before the first frame update
    void Start()
    {
        AddNoteProfileImage.sprite = ProfileManager.Instance.LoggedInProfile.PictureBorderless;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
