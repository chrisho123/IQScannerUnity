using UnityEngine;
using System.Collections;
//using UnityEngine.Advertisements;
public class iAd : MonoBehaviour {
	

	public bool showOnTop = true;
	public bool dontDestroy = false;

	#if UNITY_IPHONE
	private const string UNITY_ADS_GAMEID = "1004088";
	private ADBannerView banner;
	// Use this for initialization
	void Start () 
	{
		if(dontDestroy)
		{
			GameObject.DontDestroyOnLoad(gameObject);
		}
		//iAd initialize
		banner = new ADBannerView(ADBannerView.Type.Banner, 
			showOnTop? ADBannerView.Layout.Top : ADBannerView.Layout.Bottom);

		ADBannerView.onBannerWasLoaded += onBannerLoaded;
		/*
		//Unity Ads initialize
		if (Advertisement.isSupported) {
			Advertisement.Initialize (UNITY_ADS_GAMEID);
		} else {
			Debug.Log("Platform not supported");
		}
		*/
	}

	// iAd onBannerLoaded is declared here
	void onBannerLoaded () 
	{
		//banner.visible = true;
	}
	//Set the Ad visible status
	public void setAd(bool sw)
	{
		//iAd
		banner.visible = sw;
		/*
		if (sw == true) {
			//Unity ads
			Advertisement.Show (null, new ShowOptions {
				resultCallback = result => {
					Debug.Log (result.ToString ());
				}
			});
		}
		*/
	}


	//Unity Ads(Video ads only)

	#else
	//iAd no need except iOS
	// iAd onBannerLoaded is declared here
	void onBannerLoaded () 
	{
		
	}
	public void setAd(bool sw)
	{

	}
	#endif
	
	// Update is called once per frame
	void Update () {

	}
}
