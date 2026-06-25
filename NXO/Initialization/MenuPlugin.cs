using BepInEx;

namespace NXO.Initialization;

[BepInPlugin("com.nxo.nxomodmenu.org", "NXO", "5.2")]
public class MenuPlugin : BaseUnityPlugin
{
	private void Awake()
	{
		Loader.Load();
	}
}
