using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using System.IO;
#endif

/*
 * 
 *   IQScanner
 * 
 */
public class Main : MonoBehaviour
{
	// 每個Frame處理收到封包的次數
	private const int PROCESS_NETWORK_MESSAGE_COUNT = 2;

	#if USE_NGUI
	// 把NGUI的Root指定上去，之後所有UI邏輯由此物件管理
	// NOTE: Root必須有NGUI_Root的Script.
	public NGUI_Root mGUIRoot;
	public bool useNetWorkBase_Bool=false;
	public bool Google_X_Bool = false;

	private static NGUI_Root m_uiRoot;
	public static NGUI_Root g_uiRoot
	{
		get{ return m_uiRoot; }
	}
	#endif
	//Moving speed of bar
	private const int MOVE_BAR_SPEED = 1;
	//Moving bar execute times
	private const int MOVE_BAR_TIMES = 2;
	//for iOS iAd
	private GameObject m_iAd;
	private iAd m_iAdScript;
	//Player
	private GameObject m_PlayerPrefab;
	//Main Flow
	private int m_MainFlow = 0;
	//Try again button
	private GameObject m_TryAgain;
	//Move Bar 
	private GameObject m_MoveBar;
	//Moving bar count
	private int m_MoveBarTime = 0;
	//Flash Scanner 
	private GameObject m_FlashScanner;
	//Object of place finger hint
	private GameObject m_PlaceFinger;
	//Analyzing 
	private GameObject m_Analyzing;
	//Scanning 
	private GameObject m_Scanning;
	//Scanning finger page
	private GameObject m_AnalyStart;
	//IQNum 
	private GameObject m_IQNum;
	//YourIQis 
	private GameObject m_YourIQis;
	//TryAgain  press status
	private bool m_TryAgainPress = false;
	//Flash Scanner press status
	private bool m_FlashScannerPress = false;
	//Finger Bar is reach top or bottom 
	private bool m_FingerBarisFinished = false;
	//Scanner bar status : Top to bottom=1 -> Bottom to top=2 -> Ending=3
	private int m_FingerBarStatus = 0;
	//Frame Counter
	private int m_FrameCnt = 0;


	private static Main m_instance;
	public static Main getInstance
	{
		get{ return m_instance;}
	}


	void FixedUpdate()
	{

	}

	// Pause
	void OnApplicationPause(bool paused)
	{
#if USE_NGUI
		NGUIDebugPanel.Log("OnApplicationPause(): " + paused);
#endif
		// 在Android系統上某些機器(如HTC One)，當休眠或按下Home鈕回桌面後再進入遊戲，
		// opengl會重新建立，此種情況下，若是自行填入內容的貼圖(例如SysFontTexture_GUI)其內容會遺失，需要重新填入內容
		// 目前還沒有找到方法知道opengl是否有重新建立，所以不管有無重新建立都一律更新貼圖
		if (paused)
		{
			//Debug.Log("OnApplicationPause paused = true");
			
			// 中斷時設定需要更新SysFontTexture_GUI貼圖
			//SysFontTexture_GUI.SetNeedUpdateTexture();
		}
		else
		{
			
			//Debug.Log("OnApplicationPause paused = false");

			// 更新SysFontTexture_GUI貼圖
			//SysFontTexture_GUI.UpdateTextures();
		}

	}

	// On Every Time a Level loaded
	void OnLevelWasLoaded(int index)
	{
		// reset the BGM sound volume by current settings.
		//Settings.ApplyBGMVolume();
		//Settings.ApplySoundFxVolume ();
	}

	void OnEnable()
	{
#if UNITY_ANDROID
	#if GOOGLE_PLAY_IAB
		GoogleIABManager.queryInventorySucceededEvent += queryInventorySucceededEvent;
		GoogleIABManager.purchaseSucceededEvent += purchaseSucceededEvent;
	#endif
#endif
	}

	void OnDisable()
	{
#if UNITY_ANDROID
	#if GOOGLE_PLAY_IAB
		GoogleIABManager.queryInventorySucceededEvent -= queryInventorySucceededEvent;
		GoogleIABManager.purchaseSucceededEvent -= purchaseSucceededEvent;
	#endif
#endif
	}
	
	/**
	initial
	**/
	private IEnumerator Start()
	{



#if !ORI_FIXEDUPDATE
		// 目前FPS限制30，物理計算也限制30
		//Time.fixedDeltaTime = 0.0333f;
#endif



#if !UNITY_EDITOR
		// while VSync==false in Quality settings, 
		// we can use this to lock Framerate, that will lengthen battery life for this game.
		Application.targetFrameRate = 60;
#endif

		// 不進入休眠模式
		Screen.sleepTimeout = SleepTimeout.NeverSleep;


		
		// 一開始沒有攝影機，先清buffer，避免有些機器會有雜訊畫面
		GL.Clear(true, true, Color.black);

		// 設定LoadFromCacheOrDownload的Cache限制大小，目前為2GB
		Caching.maximumAvailableDiskSpace = 2147483648;

#if USE_NGUI
		// 使用NGUI時，會由下面的Loading畫面取代原先流程，在LoadingStartCallback中載入場景
		// 這裡只會先去抓NGUI_Root
		mGUIRoot = GetComponent<NGUI_Root>();
		m_uiRoot = GetComponent<NGUI_Root>();
		m_uiRoot.showGUI(NGUI_Root.eUIType.LOGIN);
		Localization.ReLoad=true;
#endif

		//set this gameObject always on scene 
		DontDestroyOnLoad( gameObject );
		//DontDestroyOnLoadToAllChildren(gameObject);
		//set singleton access
		m_instance = this;
		Debug.Log ("Loading");

		//Attching onClick event on Tryagain button
		m_TryAgain = GameObject.Find("UI Root/Camera/tryagain");
		UIEventListener.Get(m_TryAgain).onPress = OnPressTryAgain;
		m_TryAgain.SetActive (false);
		//Attching onClick event on flash scanner 
		m_FlashScanner = GameObject.Find("UI Root/Camera/flashscanner");

		UIEventListener.Get(m_FlashScanner).onPress = OnPressFlashScanner;
		//Place finger word
		m_PlaceFinger = GameObject.Find("UI Root/Camera/placefingeron");
		//MoveBar
		m_MoveBar = GameObject.Find("UI Root/Camera/movebar");

		m_AnalyStart = GameObject.Find ("UI Root/Camera/analy_start");

		m_Analyzing = GameObject.Find("UI Root/Camera/analyzing");
		m_Analyzing.SetActive (false);

		m_Scanning = GameObject.Find("UI Root/Camera/scanning");
		m_Scanning.SetActive(false);
		m_IQNum = GameObject.Find("UI Root/Camera/iqnum");
		m_IQNum.SetActive (false);
		m_YourIQis = GameObject.Find("UI Root/Camera/youriqis");
		m_YourIQis.SetActive (false);

		m_iAd = GameObject.Find ("iAd");
		m_iAdScript = m_iAd.GetComponent(typeof(iAd)) as iAd;
		yield return null;
	}

	//Try again
	private void OnPressTryAgain(GameObject go,bool isPress)
	{
		m_TryAgainPress = isPress;
		Debug.Log ("go:");
	}
	//Finger on scanner
	private void OnPressFlashScanner(GameObject go,bool isPress)
	{
		m_FlashScannerPress = isPress;
		Debug.Log ("flash:" + m_MainFlow);
	}


	private void OnDestroy()
	{
		
	}
	
	private void Update()
	{
		switch (m_MainFlow) {
		case 0:
			m_iAdScript.setAd (false);
			StartCoroutine (WaitTitleTime(2,20));
			m_MainFlow = 5;
			break;
		//Wait TitleScreen time
		case 5:
			break;
		//
		case 10:


			break;
		//Main screen
		case 20:
			
			//Flash the hint
			if ((m_FrameCnt & 0x20) == 0)
				m_PlaceFinger.SetActive (false);
			else
				m_PlaceFinger.SetActive (true);
			
			//Start Scanning !
			if (m_FlashScannerPress == true) {				
				m_PlaceFinger.SetActive (false);
				m_Scanning.SetActive (true);
				m_MainFlow = 30;
				m_MoveBarTime = 0;
			}
			break;
		//Finger on Scanner(Top to bottom)
		case 30:
			m_FingerBarisFinished = false;
			m_FingerBarStatus = 1;
			//Flip to nothing
			UI2DSprite u2d = m_MoveBar.GetComponent<UI2DSprite> ();
			u2d.flip = UIBasicSprite.Flip.Nothing;
			//Cycling play alpha 0.04<->1
			TweenAlpha twa = TweenAlpha.Begin (m_FlashScanner, 0.1f, 1f);
			twa.style = UITweener.Style.PingPong;
			TweenPosition twp = TweenPosition.Begin (m_MoveBar, MOVE_BAR_SPEED, new Vector3 (0, -773, 0));
			EventDelegate.Add (twp.onFinished, MoveBarFinished);
			PlaySfx(5);
			m_MainFlow = 31;
			break;
		//Wait bar to bottom
		case 31:
			if (m_FingerBarisFinished == true) {
				//if top to bottom
				if (m_FingerBarStatus == 1)
					m_MainFlow = 35;
				//if bottom to top,finish
				if (m_FingerBarStatus == 2) {
					//
					m_MoveBarTime++;
					if (m_MoveBarTime < MOVE_BAR_TIMES)
						m_MainFlow = 30;
					else {
						m_FlashScannerPress = false;
						//Reset to original status
						TweenAlpha.Begin (m_FlashScanner, 1f, 0.04f);
						m_FlashScanner.SetActive (false);
						m_Scanning.SetActive (false);
						m_Analyzing.SetActive (true);
						//Analyzing sound
						PlaySfx(6);
						m_MainFlow = 40;
					}
				}
			}
			break;
		//Finger on Scanner(Bottom to Top)
		case 35:
			m_FingerBarisFinished = false;
			m_FingerBarStatus = 2;
			//flip to vertical
			UI2DSprite u2dbt = m_MoveBar.GetComponent<UI2DSprite> ();
			u2dbt.flip = UIBasicSprite.Flip.Vertically;
			//Tween position
			TweenPosition twp2 = TweenPosition.Begin(m_MoveBar, MOVE_BAR_SPEED, new Vector3(0,773, 0));
			EventDelegate.Add (twp2.onFinished, MoveBarFinished);
			PlaySfx(5);
			m_MainFlow = 31;
			break;
		//Analyzing...
		case 40:
			
			StartCoroutine (WaitTitleTime(4,50));
			m_MainFlow = 45;
			break;
		//Wait time
		case 45:
			break;
		//Show result
		case 50:
			//turnon iAd
			m_iAdScript.setAd (true);

			TweenPosition.Begin (m_YourIQis, 0.25f, new Vector3 (10, 362, 1));
			int iq_num = Random.Range (50, 180);
			UILabel ul = m_IQNum.GetComponent<UILabel> ();
			ul.text = "" + iq_num;
			m_IQNum.SetActive (true);
			TweenScale twss = TweenScale.Begin (m_IQNum, 0.5f, new Vector3 (1.8f, 1.8f, 1f));
			EventDelegate.Add (twss.onFinished, IQNumFinished);
			m_YourIQis.SetActive (true);
			m_Analyzing.SetActive (false);
			//m_TryAgain.SetActive (true);
			//TweenAlpha.Begin (m_TryAgain, 3f, 1f);

			m_AnalyStart.SetActive (false);
			StartCoroutine (WaitTitleTime(2,52));
			m_MainFlow = 51;
			m_TryAgainPress = true;
			//IQ Mapping sound :)
			if (iq_num > 130)
				PlaySfx (4);
			else if (iq_num > 100 && iq_num <= 130)
				PlaySfx (2);
			else
				PlaySfx (3);
			break;
		//Wait
		case 51:
			break;
		case 52:
			m_TryAgain.SetActive (true);
			m_MainFlow = 55;
			break;
		//Press TryAgain button
		case 55:
			if (m_TryAgainPress == false) {
				TweenScale.Begin (m_IQNum, 0f, new Vector3 (0.01f, 0.01f, 1f));
				m_YourIQis.transform.localPosition = new Vector3 (10, 720, 1);
				m_IQNum.SetActive (false);
				m_YourIQis.SetActive (false);
				//TweenAlpha.Begin (m_TryAgain, 0f, 0f);
				m_TryAgain.SetActive (false);
				m_AnalyStart.SetActive (true);
				m_PlaceFinger.SetActive (true);
				m_FlashScanner.SetActive(true);
				m_MainFlow = 20;

				m_iAdScript.setAd (false);
			}
			break;
		}

		//Back key and exi
		if (Input.GetKeyDown (KeyCode.Escape)) {
			Application.Quit ();
		}
		m_FrameCnt++;
	}
	//IQNum scale done
	private void IQNumFinished()
	{
		TweenScale.Begin (m_IQNum, 0.1f, new Vector3 (1, 1, 1));
	}
	//Moving to specific position and reach target
	private void MoveBarFinished()
	{
		m_FingerBarisFinished = true;
	}
	//Set the wtime count down then go flow
	IEnumerator WaitTitleTime(float wtime,int flow)
	{
		yield return new WaitForSeconds(wtime);
		m_MainFlow = flow;
		Debug.Log ("wait done!" + flow);
	}

	void QuitAppCallback(object obj, System.EventArgs args)
	{
		Application.Quit ();
	}

	//Play sound
	//sfxid : sound index
	public static void PlaySfx(int sfxid)
	{
		NGUITools.soundVolume = 1f;
		AudioClip ac = Resources.Load ("sfx/"+sfxid)as AudioClip;
		NGUITools.PlaySound (ac);
	}
	public static void StopSfx()
	{		
		NGUITools.soundVolume = 0f;
	}
	/*
	private static NGUI_SoundManager SoundManager=null;
	/// play UI Sound 
	/// set sound name , and loop model
	public static void PlaySound(string soundname , bool loop=false)
	{
		if (SoundManager==null)
			SoundManager = GameObject.FindObjectOfType<NGUI_SoundManager>();
		if (SoundManager!=null)
		{
			SoundManager.PlaySound(soundname,loop);
		}
	}

	/// Stop the UI Sound 
	public static void SoundStop()
	{
		if (SoundManager==null)
			SoundManager = GameObject.FindObjectOfType<NGUI_SoundManager>();
		if (SoundManager!=null)
			SoundManager.Stop();
	}

	/// Update UI Sound Value from Setting
	public static void UpdateSoundValue(GameObject obj)
	{
		if (SoundManager==null)
			SoundManager = GameObject.FindObjectOfType<NGUI_SoundManager>();
		if (SoundManager!=null)
			SoundManager.SoundValueUpdate(obj);
	}

	/// play the object animation
	public static void GameObjectFxPlay(GameObject obj,bool enable, bool loop)
	{
		if (Main.CheckObject("ObjectAnimationPlay",obj,"NGUI_ButtonFx"))
		{
			NGUI_ButtonFx obj_fx = obj.GetComponent<NGUI_ButtonFx>();
			if (obj_fx!=null)
				obj_fx.FxPlay(enable,loop);
		}
	}
	*/




}