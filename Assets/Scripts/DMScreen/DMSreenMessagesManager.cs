using Cysharp.Threading.Tasks;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DMSreenMessagesManager : MonoBehaviour
{
    public static DMSreenMessagesManager Instance;

    public GameObject ChatPrefab;
    public Transform ChatsHolder;

    public RectTransform DMScreenScrollContainer;

    public List<Chat> Chats;

    [HideInInspector]
    public List<Profile> Profiles;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    public async UniTask Initialise()
    {
        DMScreenNotesManager.Instance.Initialise();

        Profiles = ProfileManager.Instance.Profiles;
        foreach (Transform child in ChatsHolder)
        {
            Destroy(child.gameObject);
        }

        Chats = ProfileManager.Instance.LoggedInProfile.Chats;

        foreach (var chat in Chats)
        {
            var chatUIObject = Instantiate(ChatPrefab, ChatsHolder);
            var chatUI = chatUIObject.GetComponent<ChatUI>();
            chatUI.Initialise(chat);
        }

        var vlg = DMScreenScrollContainer.GetComponent<VerticalLayoutGroup>();
        vlg.enabled = false;

        await UniTask.WaitForEndOfFrame();

        vlg.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
