using BepInEx;

namespace NXO.Initialization;

[BepInPlugin("com.nxo.nxomodmenu.org", "NXO", "7.0")]
public class MenuPlugin : BaseUnityPlugin
{
	private void Awake()
	{
		Loader.Load();
	}
}
