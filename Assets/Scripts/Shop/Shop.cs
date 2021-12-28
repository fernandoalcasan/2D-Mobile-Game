using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;

[RequireComponent(typeof(Collider2D))]
public class Shop : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField]
    private Canvas _shopCanvas;
    private CanvasScaler _shopCanvasScaler;
    [SerializeField]
    private Canvas _shopWorldCanvas;
    private CanvasScaler _shopWorldCanvasScaler;
    [SerializeField]
    private Text _textDialog;
    [SerializeField]
    private Image _itemImg;
    [SerializeField]
    private Text _gemsCount;

    [Header("Ads properties")]
    [SerializeField]
    private string _androidGameID;
    [SerializeField]
    private bool _testMode = true;
    [SerializeField]
    private bool _enablePerPlacementMode = true;
    [SerializeField]
    private string _rewardedVideoID = "Rewarded_Android";
    [SerializeField]
    private Button _rewardBtn;
    [SerializeField]
    private int _diamondsPerAd;

    [SerializeField]
    private PlayerData _playerData;
    [SerializeField]
    private GameEvent _OnShopDisplayed;
    [SerializeField]
    private GameEvent _OnShopHidden;
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            _shopWorldCanvas.enabled = true;
            _shopWorldCanvasScaler.enabled = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _shopWorldCanvas.enabled = false;
            _shopWorldCanvasScaler.enabled = false;
        }
    }

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

    public void OnItemSelected(Item item)
    {
        _itemSelected = item;
        _textDialog.text = item.description;
        _itemImg.sprite = item.image;
    }

    public void UpdateSelection(Button but)
    {
        _btnSelected = but;
    }

    public void BuyItem()
    {
        if(_itemSelected is null || _btnSelected is null)
        {
            _textDialog.text = "If you want to buy an item ask me about it. Don't be afraid.";
            AudioManager.Instance.PlayOneShotSFX(_failSound, 1f);
            return;
        }

        if (_playerData.diamonds >= _itemSelected.cost)
        {
            _playerData.diamonds -= _itemSelected.cost;
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

    private void UpdateGems()
    {
        _gemsCount.text = _playerData.diamonds.ToString();
        UIManager.Instance.UpdateDiamonds();
    }

    private void InitializeAdsSDK()
    {
        Advertisement.Initialize(_androidGameID, _testMode, _enablePerPlacementMode, this);
    }

    public void OnInitializationComplete()
    {
        Debug.Log("Unity ads initialized");
        LoadAd();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log("Unity ads failed to initialize. " + error + ": " + message);
    }

    private void LoadAd()
    {
        Debug.Log("Loading Ad: " + _rewardedVideoID);
        Advertisement.Load(_rewardedVideoID, this);
    }






    public void OnUnityAdsAdLoaded(string placementId)
    {
        Debug.Log("Unity Ad loaded correctly");
        
        if(placementId.Equals(_rewardedVideoID))
        {
            _rewardBtn.interactable = true;
        }
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.Log("Unity Ad failed to load.  " + error + ": " + message);
    }





    public void ShowAd()
    {
        _rewardBtn.interactable = false;
        Advertisement.Show(_rewardedVideoID, this);
    }



    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.Log("Ad wasn't shown, reward wasn't given. " + error + ": " + message);
        LoadAd();
    }

    public void OnUnityAdsShowStart(string placementId){}

    public void OnUnityAdsShowClick(string placementId){}

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        if (placementId.Equals(_rewardedVideoID) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
        {
            Debug.Log("Ad was finished, reward was given");
            AudioManager.Instance.PlayOneShotSFX(_successSound, 1f);
            AudioManager.Instance.PlayOneShotSFX(_gemsSound, 1f);

            _playerData.diamonds += _diamondsPerAd;
            UpdateGems();

            LoadAd();
        }
    }
}
