using UnityEngine;
using UnityEngine.UI;

public class ColorPicker : MonoBehaviour
{
	public Slider Rslider;
	public Slider Gslider;
	public Slider Bslider;
	
	public Color color;

	public UnityEngine.UI.Image img;

	public gravity_Csharp gravscript;

	public void UpdateSliders(){
	
		color = new Color(Rslider.value, Gslider.value, Bslider.value);
		
		img.color = color;
		gravscript.ChangeColorPreset(color);
	}


	void Awake(){

		UpdateSliders();
	}

}
