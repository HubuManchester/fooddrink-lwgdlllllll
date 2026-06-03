package crc645759c937ba45c653;


public class AndroidSpeechRecognitionService_ActivityResultCallback
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		androidx.activity.result.ActivityResultCallback
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onActivityResult:(Ljava/lang/Object;)V:GetOnActivityResult_Ljava_lang_Object_Handler:AndroidX.Activity.Result.IActivityResultCallbackInvoker, Xamarin.AndroidX.Activity\n" +
			"";
		mono.android.Runtime.register ("FoodNutritionApp.Platforms.Android.AndroidSpeechRecognitionService+ActivityResultCallback, FoodNutritionApp", AndroidSpeechRecognitionService_ActivityResultCallback.class, __md_methods);
	}

	public AndroidSpeechRecognitionService_ActivityResultCallback ()
	{
		super ();
		if (getClass () == AndroidSpeechRecognitionService_ActivityResultCallback.class) {
			mono.android.TypeManager.Activate ("FoodNutritionApp.Platforms.Android.AndroidSpeechRecognitionService+ActivityResultCallback, FoodNutritionApp", "", this, new java.lang.Object[] {  });
		}
	}

	public void onActivityResult (java.lang.Object p0)
	{
		n_onActivityResult (p0);
	}

	private native void n_onActivityResult (java.lang.Object p0);

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
