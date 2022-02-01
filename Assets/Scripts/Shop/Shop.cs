/*
 * This script contains the behavior of the shop of the game.
 * It also handles the Unity ads and the respective UI mechanics.
 */

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;

[RequireComponent(typeof(Collider2D))]
public class Shop : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [Header("UI assets")]
    [SerializeField]
    private Canvas _shopCanvas;
    private CanvasScaler _shopCanvasScaler;
    [SerializeField]
    private Canvas _shopWorldCanvas;
    private CanvasScaler _shopWorldCanvasScaler;
    
    [Header("Shop elements")]
    [SerializeField]
    private Text _textDialog;
    [SerializeField]
    private Image _itemImg;
    [SerializeField]
    private Text _gemsCount;
    [SerializeField]
    private Button[] _itemBtns;

    [Header("Ads properties")]
    [SerializeField]
    private string _androidGameID;
    [SerializeField]
    private bool _testMode;
    [SerializeField]
    private bool _enablePerPlacementMode = true;
    [SerializeField]
    private string _rewardedVideoID = "Rewarded_Android";
    [SerializeField]
    private Button _rewardBtn;
    [SerializeField]
    private int _diamondsPerAd;

    [Header("Player Data")]
    [SerializeField]
    private PlayerData _playerData;

    [Header("Shop Events")]
    [SerializeField]
    private GameEvent _OnShopDisplayed;
    [SerializeField]
    private GameEvent _OnShopHidden;

    //Help variables, to cache references and behavior
    private Item _itemSelected;
    private Button _btnSelected;

    [Header("Shop Audio")]
    [SerializeField]
    private AudioClip _buySound;
    [SerializeField]
    private AudioClip _failSound;
    [SerializeField]
    private AudioClip _gemsSound;
    [SerializeField]
    private AudioClip _successSound;

    private void Awake()
    {

#if !UNITY_EDITOR
        Debug.unityLogger.logEnabled = false;
#endif
        
        if(Advertisement.isInitialized)
            _rewardBtn.interactable = true;
        else
            InitializeAdsSDK();

        if (_shopCanvas is null || _shopWorldCanvas is null)
            Debug.LogError("Please assign the shop Canvas and/or the world canvas");

        _shopCanvasScaler = _shopCanvas.GetComponent<CanvasScaler>();

        if(_shopCanvasScaler is null)
            Debug.LogError("The shop canvas scaler is NULL");

        _shopWorldCanvasScaler = _shopWorldCanvas.GetComponent<CanvasScaler>();

        if (_shopWorldCanvasScaler is null)
            Debug.LogError("The shop world canvas scaler is NULL");
    }

    private void Start()
    {
        UpdateData();
    }

    //Enable world UI of the shop
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            _shopWorldCanvas.enabled = true;
            _shopWorldCanvasScaler.enabled = true;
        }
    }

    //Disable world UI of the shop
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _shopWorldCanvas.enabled = false;
            _shopWorldCanvasScaler.enabled = false;
        }
    }

    //Update items that were acquired already
    private void UpdateData()
    {
        if (_playerData.data.gotAttackUpgrade)
            _itemBtns[0].interactable = false;
        if (_playerData.data.gotWindBoots)
            _itemBtns[1].interactable = false;
        if (_playerData.data.gotCastleKey)
            _itemBtns[2].interactable = false;
    }

    //Enable/Disable UI of the shop and raise its events
    public void DisplayOrHideShop(bool enable)
    {
        if (enable)
            _OnShopDisplayed.Raise();
        else
            _OnShopHidden.Raise();

        UpdateGems();
        _shopCanvas.enabled = enable;
        _shopCanvasScaler.enabled = enable;
        _textDialog.text = "Greetings strange traveler... Hoy may I be of service?";
    }

    //Method called by UI buttons of the shop
    public void OnItemSelected(Item item)
    {
        _itemSelected = item;
        _textDialog.text = item.description;
        _itemImg.sprite = item.image;
    }

    //Method called by UI buttons of the shop
    public void UpdateSelection(Button but)
    {
        _btnSelected = but;
    }

    //Method called by UI button of the shop
    public void BuyItem()
    {
        if(_itemSelected is null || _btnSelected is null)
        {
            _textDialog.text = "If you want to buy an item ask me about it. Don't be afraid.";
            AudioManager.Instance.PlayOneShotSFX(_failSound, 1f);
            return;
        }

        if (_playerData.data.diamonds >= _itemSelected.cost)
        {
            _playerData.data.diamonds -= _itemSelected.cost;
            _itemSelected.OnItemBought.Raise();
            _btnSelected.interactable = false;
            _textDialog.text = _itemSelected.buyPhrase;
            UpdateGems();
            AudioManager.Instance.PlayOneShotSFX(_buySound, 1f);
        }
        else
        {
            AudioManager.Instance.PlayOneShotSFX(_failSound, 1f);
            _textDialog.text = "It seems that you don't have enough diamonds to buy this item... Sorry, I don't do discounts.";
        }

        _itemSelected = null;
        _btnSelected = null;
    }

    //Method to update respective UI of the shop
    private void UpdateGems()
    {
        _gemsCount.text = _playerData.data.diamonds.ToString();
        UIManager.Instance.UpdateDiamonds();
    }

    //Method to initialize Unity Ads
    private void InitializeAdsSDK()
    {
        Advertisement.Initialize(_androidGameID, _testMode, _enablePerPlacementMode, this);
    }

    //Method called when Unity Ads are initialized (implemented by interface)
    public void OnInitializationComplete()
    {
        Debug.Log("Unity ads initialized");
        LoadAd();
    }

    //Method called when Unity Ads fail to initialize (implemented by interface)
    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log("Unity ads failed to initialize. " + error + ": " + message);
    }

    //Method called to load a Unity Ad when needed
    private void LoadAd()
    {
        Debug.Log("Loading Ad: " + _rewardedVideoID);
        Advertisement.Load(_rewardedVideoID, this);
    }

    //Method called when the Unity Ad gets loaded (implemented by interface)
    public void OnUnityAdsAdLoaded(string placementId)
    {
        Debug.Log("Unity Ad loaded correctly");
        
        if(placementId.Equals(_rewardedVideoID))
        {
            _rewardBtn.interactable = true;
        }
    }

    //Method called when the Unity Ad failed to load (implemented by interface)
    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.Log("Unity Ad failed to load.  " + error + ": " + message);
    }

    //Method called to show the respective ad
    public void ShowAd()
    {
        _rewardBtn.interactable = false;
        Advertisement.Show(_rewardedVideoID, this);
    }

    //Method called when Unity Ad failed to show (implemented by interface)
    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.Log("Ad wasn't shown, reward wasn't given. " + error + ": " + message);
        LoadAd();
    }

    //Method called when Unity Ad starts to show (implemented by interface)
    public void OnUnityAdsShowStart(string placementId){}

    //Method called when Unity Ad gets clicked (implemented by interface)
    public void OnUnityAdsShowClick(string placementId){}

    //Method called when Unity Ad was successfully watched (implemented by interface)
    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        if (placementId.Equals(_rewardedVideoID) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
        {
            Debug.Log("Ad was finished, reward was given");
            AudioManager.Instance.PlayOneShotSFX(_successSound, 1f);
            AudioManager.Instance.PlayOneShotSFX(_gemsSound, 1f);

            _playerData.data.diamonds += _diamondsPerAd;
            UpdateGems();

            LoadAd();
        }
    }
}
